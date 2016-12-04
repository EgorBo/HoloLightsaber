using System;
using System.Threading.Tasks;
using Shared;
using Xamarin.Forms;

namespace Lightsaber
{
	public class LightSaberPage : ContentPage
	{
		readonly HoloLensConnection connection;
		MotionDetector motionDetector;

		public LightSaberPage(HoloLensConnection connection)
		{
			NavigationPage.SetHasNavigationBar(this, false);
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

			Button purpleBt = new Button();
			purpleBt.Clicked += OnColorClick;
			purpleBt.HorizontalOptions = LayoutOptions.FillAndExpand;
			purpleBt.BackgroundColor = Color.Purple;

			StackLayout hStack = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.Center,
					HeightRequest = 200,
					Children = { redBt, greenBt, blueBt, purpleBt }
				};

			Content = hStack;
		}

		protected override void OnAppearing()
		{
			motionDetector = new MotionDetector();
			motionDetector.StartListening(rot => connection?.Send(new MotionDto { Rotation = rot }));
		}

		void OnColorClick(object sender, EventArgs eventArgs)
		{
			var color = ((Button) sender).BackgroundColor;
			var colorVector = new Vector3Dto { X = (float) color.R, Y = (float) color.G, Z = (float) color.B };
			connection?.Send(new ColorChangedDto { Color = colorVector });
		}
	}
}
