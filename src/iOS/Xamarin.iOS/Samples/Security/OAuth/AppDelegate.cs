using ArcGISRuntimeXamarin.Samples.OpenMapOAuth;
using Foundation;
using UIKit;

namespace OAuth
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // create a new window instance based on the screen size
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            // If you have defined a root view controller, set it here:
             Window.RootViewController = new UINavigationController(new OpenMapOAuth());

            // make the window visible
            Window.MakeKeyAndVisible();

            return true;
        }        
    }
}


