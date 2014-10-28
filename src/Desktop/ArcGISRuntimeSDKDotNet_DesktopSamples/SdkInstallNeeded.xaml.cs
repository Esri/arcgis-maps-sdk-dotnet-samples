using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples
{
	public partial class SdkInstallNeeded : UserControl
	{
		public SdkInstallNeeded()
		{
			InitializeComponent();
		}

		private void DeveloperSiteLink_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("http://esriurl.com/dotnetsdk"));
		}

		private void ReadMore_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://github.com/Esri/arcgis-runtime-samples-dotnet/blob/master/README.md"));
		}
	}
}
