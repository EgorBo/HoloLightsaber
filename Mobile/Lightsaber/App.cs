﻿using Lightsaber;
using Xamarin.Forms;

namespace Lightsaber
{
	public class App : Application
	{
		public App ()
		{
			// The root page of your application
			MainPage = new NavigationPage(new QrPage());
			//MainPage = new NavigationPage(new LightSaberPage(null));
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}