using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ArcGISRuntime
{
	public partial class App2 : Application
	{
		public App2 ()
		{
			InitializeComponent();

		    var navigationPage = new NavigationPage(new ArcGISRuntime.FormsMainPage() 
		    {
		        Title = "ArcGIS Runtime SDK for .NET"
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
