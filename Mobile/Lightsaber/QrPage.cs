using Lightsaber;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Lightsaber
{
	public class QrPage : ContentPage
	{
		public QrPage()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			Initialize();
		}

		async void Initialize()
		{
			var ip = await HoloLensConnection.GetLocalIp() ?? "ERROR";
			var qrSize = 320;
			var barcode = new ZXingBarcodeImageView
			{
				WidthRequest = qrSize,
				HeightRequest = qrSize,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BarcodeFormat = ZXing.BarcodeFormat.QR_CODE,
				BarcodeOptions =
					{
						Width = qrSize,
						Height = qrSize,
					},
				BarcodeValue = ip,
			};
			BackgroundColor = Color.White;
			var stack = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						TextColor = Color.Black,
						HorizontalTextAlignment = TextAlignment.Center,
						Text = $"Waiting for the HoloLens ({ip}):"
					},
					barcode,
				}
			};

			Content = stack;
			var connection = new HoloLensConnection();
			await connection.WaitForCompanion();
			await Navigation.PushAsync(new LightSaberPage(connection));
		}
	}
}
