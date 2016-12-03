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
			await detector.StartListening();

			while (true)
			{
				await Task.Delay(20);
				var q = detector.GetRotation();
				Device.BeginInvokeOnMainThread(() => {
					label.Text = $"{Math.Round(q.X, 1)};  {Math.Round(q.Y, 1)};  {Math.Round(q.Z, 1)}";
				});
			}
		}
	}
}
