using System.Threading.Tasks;
using Shared;
using Urho;
using Urho.Gui;
using Urho.HoloLens;
using Urho.Shapes;

namespace Lightsaber.HoloLens
{
	public class LightsaberApp : HoloApplication
	{
		Blade blade;
		Node handleNode;
		Node environmentNode;
		ClientConnection clientConnection;
		Vector3 manipulationPos;
		Material spatialMaterial;

		public LightsaberApp(ApplicationOptions opts) : base(opts) { }
		
		public override void OnGestureManipulationStarted() => manipulationPos = handleNode.Position;
		public override void OnGestureManipulationUpdated(Vector3 hand) => handleNode.Position = hand * 1.3f + manipulationPos;
		public override Vector3 FocusWorldPoint => handleNode.WorldPosition;

		protected override async void Start()
		{
			base.Start();

			//Renderer.HDRRendering = true;
			//var rp = Renderer.GetViewport(1).RenderPath.Clone();
			//rp.Append(ResourceCache.GetXmlFile("PostProcess/HoloBloomHDR.xml"));
			//rp.Append(CoreAssets.PostProcess.FXAA2);
			//Renderer.GetViewport(1).RenderPath = rp;
			
			clientConnection = new ClientConnection();
			clientConnection.RegisterFor<MotionDto>(OnMotion);
			clientConnection.RegisterFor<ColorChangedDto>(OnColorChanged);

			Zone.AmbientColor = new Color(0.5f, 0.5f, 0.5f);
			DirectionalLight.Brightness = 1f;

			EnableGestureManipulation = true;

			environmentNode = Scene.CreateChild();
			
			handleNode = Scene.CreateChild();
			handleNode.Position = new Vector3(0, 0, 1.5f);
			handleNode.Scale = new Vector3(1, 1, 7) / 60;
			var handleModel = handleNode.CreateComponent<Box>();
			handleModel.Color = Color.Gray;

			blade = handleNode.CreateComponent<Blade>();

			var handleRingTop = handleNode.CreateChild();
			var handleRingTopModel = handleRingTop.CreateComponent<Box>();
			handleRingTopModel.Color = Color.White;
			handleRingTop.Scale = new Vector3(1, 1, 1 / 10f) * 1.2f;
			handleRingTop.Position = new Vector3(0, 0, 0.55f);

			var handleRingBottom = handleNode.CreateChild();
			var handleRingBottomModel = handleRingBottom.CreateComponent<Box>();
			handleRingBottomModel.Color = handleRingTopModel.Color;
			handleRingBottom.Scale = handleRingTop.Scale;
			handleRingBottom.Position = -handleRingTop.Position;

			// Material for spatial surfaces
			spatialMaterial = new Material();
			spatialMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);

			//handleNode.RunActions(new RepeatForever(new RotateBy(0.3f, 30, 30, 30)));
			//blade.SetColor(Color.Red);

			await StartSpatialMapping(Vector3.One * 50);

			while (!await ConnectAsync()) { }
		}

		/* Assets don't look like I can distribute them for free :(
		void CreateBB8(Node node)
		{
			var bb8BodyNode = node.CreateChild();
			bb8BodyNode.SetScale(0.005f);
			bb8BodyNode.Position = new Vector3(0, -0.25f, 0);
			var bb8BodyModel = bb8BodyNode.CreateComponent<StaticModel>();
			bb8BodyModel.Model = ResourceCache.GetModel("Models/BB8Body.mdl");
			bb8BodyModel.ApplyMaterialList("Models/BB8Body.txt");

			var bb8HeadNode = node.CreateChild();
			bb8HeadNode.SetScale(0.00035f);
			var bb8HeadModel = bb8HeadNode.CreateComponent<StaticModel>();
			bb8HeadModel.Model = ResourceCache.GetModel("Models/BB8Head.mdl");
			bb8HeadModel.ApplyMaterialList("Models/BB8Head.txt");

			node.RunActions(new RepeatForever(
					new MoveBy(1f, new Vector3(1f, 0, 0)), new DelayTime(3f),
					new MoveBy(1f, new Vector3(-1f, 0, 0)), new DelayTime(3f)));

			bb8BodyNode.RunActions(new RepeatForever(
					new RotateBy(1f, 0, 0, -90), new DelayTime(3f),
					new RotateBy(1f, 0, 0, 90), new DelayTime(3f)));

			bb8HeadNode.RunActions(new RepeatForever(
				new RotateBy(1f, 0, 120, 0), new DelayTime(2f),
				new RotateBy(1f, 0, -120, 0), new DelayTime(2f)));
		}*/

		public override void OnSurfaceAddedOrUpdated(SpatialMeshInfo surface, Model generatedModel)
		{
			bool isNew = false;
			StaticModel staticModel = null;
			Node node = environmentNode.GetChild(surface.SurfaceId, false);
			if (node != null)
			{
				isNew = false;
				staticModel = node.GetComponent<StaticModel>();
			}
			else
			{
				isNew = true;
				node = environmentNode.CreateChild(surface.SurfaceId);
				staticModel = node.CreateComponent<StaticModel>();
			}

			node.Position = surface.BoundsCenter;
			node.Rotation = surface.BoundsRotation;
			staticModel.Model = generatedModel;

			if (isNew)
			{
				staticModel.SetMaterial(spatialMaterial);
			}
		}

		void OnColorChanged(ColorChangedDto dto)
		{
			if (blade == null) return;
			InvokeOnMain(() => blade.SetColor(new Color(dto.Color.X, dto.Color.Y, dto.Color.Z)));
		}

		void OnMotion(MotionDto e)
		{
			if (handleNode == null) return;
			InvokeOnMain(() => handleNode.Rotation = new Quaternion(e.Rotation.X, e.Rotation.Y, e.Rotation.Z));
		}

		public async Task<bool> ConnectAsync()
		{
			var textNode = LeftCamera.Node.CreateChild();
			textNode.Position = new Vector3(0, 0, 1);
			textNode.SetScale(0.1f);
			var text = textNode.CreateComponent<Text3D>();
			text.Text = "Look at the QR code\nopened in Android/iOS/UWP app...";
			text.HorizontalAlignment = HorizontalAlignment.Center;
			text.VerticalAlignment = VerticalAlignment.Center;
			text.TextAlignment = HorizontalAlignment.Center;
			text.SetFont(CoreAssets.Fonts.AnonymousPro, 20);
			text.SetColor(Color.Green);

			string ipAddressString = "", ip = "";
			int port;
			while (!Utils.TryParseIpAddress(ipAddressString, out ip, out port))
			{
#if VIDEO_RECORDING //see OnGestureDoubleTapped for comments
				ipAddressString = await fakeQrCodeResultTaskSource.Task; 
#else
				ipAddressString = await QrCodeReader.ReadAsync();
#endif
			}

			InvokeOnMain(() => text.Text = "Connecting...");

			if (await clientConnection.ConnectAsync(ip, port))
			{
				InvokeOnMain(() => textNode.Remove());
				return true;
			}
			return false;
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			DirectionalLight.Node.SetDirection(LeftCamera.Node.Direction);
		}

#if VIDEO_RECORDING
		TaskCompletionSource<string> fakeQrCodeResultTaskSource = new TaskCompletionSource<string>();
		public override void OnGestureDoubleTapped()
		{
			// Unfortunately, it's not allowed to record a video ("Hey Cortana, start recording")
			// and grab frames (in order to read a QR) at the same time - it will crash.
			// so I use a fake QR code result for the demo purposes
			// it is emulated by a double tap gesture
			Task.Run(() => fakeQrCodeResultTaskSource.TrySetResult("192.168.1.6:5206"));
		}
#endif
	}
}