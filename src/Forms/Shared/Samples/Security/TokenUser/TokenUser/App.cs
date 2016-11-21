using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace TokenSecuredKnownUser
{
	public class App : Application
	{
		public App ()
		{
            // Create a NavigationPage with the map page as the root
            var navPage = new NavigationPage(new TokenKnownUserPage());

            // Set the app's main page with the navigation page (to enable navigation to a login screen)
            MainPage = navPage;
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
