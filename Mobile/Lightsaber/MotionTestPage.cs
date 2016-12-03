using System;
using Urho;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Lightsaber
{
	public class MotionTestPage : ContentPage
	{
		MotionDetector detector;
		Label label;

		public MotionTestPage()
		{
			label = new Label();
			label.Text = "test";
			label.VerticalOptions = LayoutOptions.Center;
			label.HorizontalOptions = LayoutOptions.Center;
			Content = label;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			detector = new MotionDetector();
			detector.StartListening(rot => 
				Device.BeginInvokeOnMainThread(() => 
					label.Text = $"{Math.Round(rot.X, 1)};  {Math.Round(rot.Y, 1)};  {Math.Round(rot.Z, 1)}"));
		}
	}
}
