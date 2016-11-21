using Xamarin.Forms;

namespace XamarinForms.IWA
{
	public class App : Application
	{
		public App ()
		{
            // Create a NavigationPage with the map page (IWAForm) as the root
            var navPage = new NavigationPage(new IWAForm());

            // Set the app's main page with the navigation page (to enable navigation to a login screen)
            MainPage = navPage;
		}
	}
}
