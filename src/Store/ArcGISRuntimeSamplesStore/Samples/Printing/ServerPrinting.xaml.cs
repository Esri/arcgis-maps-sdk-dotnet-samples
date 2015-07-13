using Esri.ArcGISRuntime.Tasks.Printing;
using System;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates printing a map using a PrintTask.
    /// </summary>
    /// <title>Server Printing</title>
    /// <category>Printing</category>
    public partial class ServerPrinting : Page
    {
		private const string PrintTaskUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task";
        private PrintTask _printTask;

        /// <summary>Construct Server Printing sample control</summary>
        public ServerPrinting()
        {
            InitializeComponent();
			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
        }

		private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			try
			{
				_printTask = new PrintTask(new Uri(PrintTaskUrl));
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
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

        private async void ExportMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                PrintParameters printParameters = new PrintParameters(MyMapView)
                {
                    ExportOptions = new ExportOptions() { Dpi = 96, OutputSize = new Size(MyMapView.ActualWidth, MyMapView.ActualHeight) },
                    LayoutTemplate = (string)comboLayout.SelectedItem ?? string.Empty,
                    Format = (string)comboFormat.SelectedItem,
                };

                var result = await _printTask.PrintAsync(printParameters);

				await Windows.System.Launcher.LaunchUriAsync(result.Uri);
            }
            catch (Exception ex)
            {
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
