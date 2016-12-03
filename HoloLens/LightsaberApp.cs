using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.HoloLens;
using Urho.Shapes;

namespace Lightsaber.HoloLens
{
	public class LightsaberApp : HoloApplication
	{
		Node handleNode;
		ClientConnection clientConnection;
		Blade blade;

		public LightsaberApp(ApplicationOptions opts) : base(opts) { }

		protected override async void Start()
		{
			base.Start();

			//Renderer.HDRRendering = true;
			//Renderer.GetViewport(0).RenderPath.Append(ResourceCache.GetXmlFile("PostProcess/BloomHDR.xml"));

			clientConnection = new ClientConnection();
			clientConnection.RegisterFor<MotionDto>(OnMotion);
			clientConnection.RegisterFor<ColorChangedDto>(OnColorChanged);

			Zone.AmbientColor = new Color(0.4f, 0.4f, 0.4f); // Color.Transparent;
			DirectionalLight.Brightness = 1f;
			
			handleNode = Scene.CreateChild();
			handleNode.Position = new Vector3(0, 0, 1.5f);
			handleNode.Scale = new Vector3(1, 1, 7) / 60;
			var handleModel = handleNode.CreateComponent<Box>();
			handleModel.Color = Color.Gray;

			blade = handleNode.CreateComponent<Blade>();
			blade.SetColor(Color.Blue);

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
			blade.Toggle();

			while (!await ConnectAsync()) { }
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

	public class Blade : Component
	{
		[Preserve] public Blade() { ReceiveSceneUpdates = true; }
		[Preserve] public Blade(IntPtr ptr) : base(ptr) { ReceiveSceneUpdates = true; }

		Box glowModel;
		Node bladeNode;

		public void SetColor(Color c)
		{
			Toggle();
			glowModel.GetMaterial(0).SetShaderParameter("MatDiffColor", new Color(c, 0.4f));
		}

		public void Toggle()
		{
			var to = new Vector3(1, 1, 7) / 1.75f;
			bladeNode.Scale = new Vector3(1, 1, 0.1f) / 1.75f;
			bladeNode.Position = new Vector3(0f, 0f, 0.6f);
			bladeNode.RunActions(new ScaleTo(0.5f, to.X, to.Y, to.Z));
			bladeNode.RunActions(new MoveTo(0.5f, new Vector3(0f, 0f, 2.5f)));
		}

		public override void OnAttachedToNode(Node node)
		{
			base.OnAttachedToNode(node);

			bladeNode = node.CreateChild();
			bladeNode.Position = new Vector3(0f, 0f, 2.5f);
			bladeNode.Scale = new Vector3(1, 1, 0.1f) / 1.75f;
			var bladeModel = bladeNode.CreateComponent<StaticModel>();
			bladeModel.Model = CoreAssets.Models.Box;
			bladeModel.SetMaterial(Material.FromColor(Color.White));

			var glowNode = bladeNode.CreateChild();
			glowNode.Scale = new Vector3(3f, 3f, 1f);
			glowNode.Position = new Vector3(0.02f, 0, 0.02f);
			glowModel = glowNode.CreateComponent<Box>();
			glowModel.Color = new Color(0, 1, 0, 0.5f);
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
		}
	}
}