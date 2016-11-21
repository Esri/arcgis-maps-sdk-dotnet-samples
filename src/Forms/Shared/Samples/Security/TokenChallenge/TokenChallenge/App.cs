using Xamarin.Forms;

namespace TokenChallenge
{
	public class App : Application
	{
		public App ()
		{
            // Create a NavigationPage with the map page as the root
            var navPage = new NavigationPage(new TokenChallengePage());

            // Set the app's main page with the navigation page (to enable navigation to a login screen)
            MainPage = navPage;
        }        
	}
}
