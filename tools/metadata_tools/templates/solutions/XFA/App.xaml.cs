using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ArcGISRuntime
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

			var navigationPage = new NavigationPage(new ArcGISRuntime.MainPage() 
		    {
		        Title = "$$friendly_name$$"
		    });

		    MainPage = navigationPage;
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
