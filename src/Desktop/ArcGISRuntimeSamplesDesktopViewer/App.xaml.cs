using System.Diagnostics;
using System.Windows;

namespace ArcGISRuntime.Samples.DesktopViewer
{
    public partial class App : Application
    {
    	private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
            Debug.WriteLine("An unhandled exception occured!");
            Debug.WriteLine(string.Format("Error : {0}", e.Exception));
            MessageBox.Show(string.Format(e.Exception.Message), "An exception occured");
		}
	}
}
