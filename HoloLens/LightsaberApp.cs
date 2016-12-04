using System;
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

			Renderer.HDRRendering = true;
			var rp = Renderer.GetViewport(1).RenderPath.Clone();
			rp.Append(ResourceCache.GetXmlFile("PostProcess/HoloBloomHDR.xml"));
			rp.Append(CoreAssets.PostProcess.FXAA3);
			Renderer.GetViewport(1).RenderPath = rp;
			
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

			handleNode.RunActions(new RepeatForever(new RotateBy(0.3f, 30, 30, 30)));

			//blade.Toggle();
			await handleNode.RunActionsAsync(new DelayTime(3));
			blade.SetColor(Color.Red);


			//while (!await ConnectAsync()) { }
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
		Node bladeNode;
		StaticModel bladeModel;

		[Preserve] public Blade() { }
		[Preserve] public Blade(IntPtr ptr) : base(ptr) { }

		public bool Active { get; private set; }

		public void SetColor(Color c)
		{
			float glowF = 18;
			bladeModel.GetMaterial(0).SetShaderParameter("MatDiffColor", new Color(c.R * glowF + 1, c.G * glowF + 1, c.B * glowF + 1));
			Toggle();
		}

		public void Toggle()
		{
			Active = !Active;
			var to = new Vector3(0.7f, 0.7f, 7) / 1.75f;
			bladeNode.Scale = new Vector3(1, 1, 0.1f) / 1.75f;
			bladeNode.Position = new Vector3(0f, 0f, 0.6f);

			if (Active)
			{
				bladeNode.RunActions(new ScaleTo(0.5f, to.X, to.Y, to.Z));
				bladeNode.RunActions(new MoveTo(0.5f, new Vector3(0f, 0f, 2.5f)));
			}
		}

		public override void OnAttachedToNode(Node node)
		{
			base.OnAttachedToNode(node);

			bladeNode = node.CreateChild();
			bladeNode.SetScale(0);
			bladeModel = bladeNode.CreateComponent<StaticModel>();
			bladeModel.Model = CoreAssets.Models.Box;
			bladeModel.SetMaterial(Material.FromColor(Color.White));
		}
	}
}