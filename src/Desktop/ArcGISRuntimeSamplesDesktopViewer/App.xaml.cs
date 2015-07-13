using System;
using System.Diagnostics;
using System.Windows;

namespace ArcGISRuntime.Samples.DesktopViewer
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Debug.WriteLine("An unhandled exception occured!");
            Debug.WriteLine(string.Format("Error : {0}", exception));
            MessageBox.Show(string.Format(exception.Message), "An exception occured");
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
            Debug.WriteLine("An unhandled exception occured!");
            Debug.WriteLine(string.Format("Error : {0}", e.Exception));
            MessageBox.Show(string.Format(e.Exception.Message), "An exception occured");
            e.Handled = true;
		}
	}
}
