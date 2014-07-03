using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Printing;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates printing a map using a PrintTask. To use the sample, specify the print settings (Layout Template and Format) using the dropdowns and click the Export Map button to generate a printout of the map. The printout will be displayed using the systems default browser.
    /// </summary>
    /// <title>Server Printing</title>
    /// <category>Printing</category>
    public partial class ServerPrinting : UserControl
    {
        private PrintTask _printTask;

        /// <summary>Construct Server Printing sample control</summary>
        public ServerPrinting()
        {
            InitializeComponent();

			// Create initial extend and set it
			var envelopeBuilder = new EnvelopeBuilder(SpatialReference.Create(102100));
			envelopeBuilder.XMin = -10929488.234;
			envelopeBuilder.YMin = 4525208.388;
			envelopeBuilder.XMax = -10906776.553;
			envelopeBuilder.YMax = 4535252.104;

			mapView.Map.InitialExtent = envelopeBuilder.ToGeometry();

            _printTask = new PrintTask(
                new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task"));

            mapView.Loaded += mapView_Loaded;
        }

        private async void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var info = await _printTask.GetTaskInfoAsync();
                
                comboLayout.ItemsSource = info.LayoutTemplates;
                if (info.LayoutTemplates != null && info.LayoutTemplates.Count > 0)
                    comboLayout.SelectedIndex = 0;

                comboFormat.ItemsSource = info.Formats;
                if (info.Formats != null && info.Formats.Count > 0)
                    comboFormat.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        private async void ExportMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                PrintParameters printParameters = new PrintParameters(mapView)
                {
                    ExportOptions = new ExportOptions() { Dpi = 96, OutputSize = new Size(mapView.ActualWidth, mapView.ActualHeight) },
                    LayoutTemplate = (string)comboLayout.SelectedItem ?? string.Empty,
                    Format = (string)comboFormat.SelectedItem,
                };

                var result = await _printTask.PrintAsync(printParameters);

                Process.Start(result.Uri.AbsoluteUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
