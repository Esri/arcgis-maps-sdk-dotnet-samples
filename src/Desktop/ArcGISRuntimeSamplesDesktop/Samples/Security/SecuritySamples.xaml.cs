using Esri.ArcGISRuntime.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop.Samples
{
	/// <summary>
	/// This page contains instructions for accessing the Security samples
	/// </summary>
	/// <title>Security samples</title>
	/// <category>Security</category>
	public partial class SecuritySamples : UserControl
	{
		public SecuritySamples()
		{
			InitializeComponent();
		}

		private void GitHubSiteLink_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://github.com/Esri/arcgis-runtime-samples-dotnet/tree/master/src/Desktop/ArcGISRuntimeSamplesDesktop/Samples/Security"));
		}
	}
}
