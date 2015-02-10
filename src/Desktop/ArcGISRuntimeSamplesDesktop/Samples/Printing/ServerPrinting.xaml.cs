using Esri.ArcGISRuntime.Tasks.Printing;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
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

            _printTask = new PrintTask(
                new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task"));

            MyMapView.Loaded += MyMapView_Loaded;
        }

        private async void MyMapView_Loaded(object sender, RoutedEventArgs e)
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

                PrintParameters printParameters = new PrintParameters(MyMapView)
                {
                    ExportOptions = new ExportOptions() { Dpi = 96, OutputSize = new Size(MyMapView.ActualWidth, MyMapView.ActualHeight) },
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
