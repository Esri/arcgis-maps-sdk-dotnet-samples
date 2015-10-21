using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OAuthAuthentication
{
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			try
			{
				// Deployed applications must be licensed at the Basic level or greater (https://developers.arcgis.com/licensing).
				// To enable Basic level functionality set the Client ID property before initializing the ArcGIS Runtime.
				// ArcGISRuntimeEnvironment.ClientId = "<Your Client ID>";

				// Initialize the ArcGIS Runtime before any components are created.
				ArcGISRuntimeEnvironment.Initialize();

				// Standard level functionality can be enabled once the ArcGIS Runtime is initialized.                
				// To enable Standard level functionality you must either:
				// 1. Allow the app user to authenticate with ArcGIS Online or Portal for ArcGIS then call the set license method with their license info.
				// ArcGISRuntimeEnvironment.License.SetLicense(LicenseInfo object returned from an ArcGIS Portal instance)
				// 2. Call the set license method with a license string obtained from Esri Customer Service or your local Esri distributor.
				// ArcGISRuntimeEnvironment.License.SetLicense("<Your License String or Strings (extensions) Here>");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "ArcGIS Runtime initialization failed.");

				// Exit application
				this.Shutdown();
			}
		}
	}
}
