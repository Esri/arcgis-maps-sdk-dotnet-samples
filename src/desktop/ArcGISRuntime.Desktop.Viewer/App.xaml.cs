using ArcGISRuntime.Desktop.Viewer.Managers;
using ArcGISRuntime.Samples.Models;
using System.Windows;

namespace ArcGISRuntime.Desktop.Viewer
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var selectedLanguage = Language.CSharp;

            // Check application parameters:
            // parameter definitions:
            //     /vb = launch application using VBNet samples, defaults to C#
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/vb")
                {
                    selectedLanguage = Language.VBNet;
                }
            }

            ApplicationManager.Current.Initialize(selectedLanguage);
        }
    }
}
