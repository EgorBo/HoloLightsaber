using System;
using System.Threading.Tasks;
using Shared;
using Xamarin.Forms;

namespace Lightsaber
{
	public class LightSaberPage : ContentPage
	{
		readonly ScannerConnection connection;
		MotionDetector motionDetector;

		public LightSaberPage(ScannerConnection connection)
		{
			this.connection = connection;

			Button redBt = new Button();
			redBt.Clicked += OnColorClick;
			redBt.HorizontalOptions = LayoutOptions.FillAndExpand;
			redBt.BackgroundColor = Color.Red;

			Button greenBt = new Button();
			greenBt.Clicked += OnColorClick;
			greenBt.HorizontalOptions = LayoutOptions.FillAndExpand;
			greenBt.BackgroundColor = Color.Green;

			Button blueBt = new Button();
			blueBt.Clicked += OnColorClick;
			blueBt.HorizontalOptions = LayoutOptions.FillAndExpand;
			blueBt.BackgroundColor = Color.Blue;

			StackLayout hStack = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Children = { redBt, greenBt, blueBt }
				};

			Content = hStack;
		}

		protected override async void OnAppearing()
		{
			motionDetector = new MotionDetector();
			motionDetector.StartListening();
			while (true)
			{
				await Task.Delay(10);
				connection?.Send(new MotionDto { Rotation = motionDetector.GetLastQuaternion() });
			}
		}

		void OnColorClick(object sender, EventArgs eventArgs)
		{
			var color = ((Button) sender).BackgroundColor;
			var colorVector = new Vector3Dto { X = (float) color.R, Y = (float) color.G, Z = (float) color.B };
			connection?.Send(new ColorChangedDto { Color = colorVector });
		}
	}
}
