using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample shows how to work with an asynchronous Extract Data geoprocessing service. The Extract Data Task lets users specify an area of interest then download a zip file that contains data for that area from one or more layers.
    /// </summary>
    /// <title>Extract Data</title>
    /// <category>Tasks</category>
    /// <subcategory>Geoprocessing</subcategory>
    public partial class ExtractData : UserControl
    {
		private const string ExtractDataServiceUrl =
				"http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/HomelandSecurity/Incident_Data_Extraction/GPServer/Extract%20Data%20Task";

        private Geoprocessor _gpTask;
		private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Extract Data sample control</summary>
        public ExtractData()
        {
            InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			_gpTask = new Geoprocessor(new Uri(ExtractDataServiceUrl));

            SetupUI();
        }

        // Sets up UI choices from the extract data service information
        private async void SetupUI()
        {
            try
            {
                var info = await _gpTask.GetTaskInfoAsync();

                listLayers.ItemsSource = info.Parameters.First(p => p.Name == "Layers_to_Clip").ChoiceList;

                comboFormat.ItemsSource = info.Parameters.First(p => p.Name == "Feature_Format").ChoiceList;
                if (comboFormat.ItemsSource != null && comboFormat.Items.Count > 0)
                    comboFormat.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        // Gets the users digitized area of interest polygon
        private async void AreaOfInterestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				_graphicsOverlay.Graphics.Clear();

                Polygon aoi = null;
                if (chkFreehand.IsChecked == true)
                {
                    var boundary = await MyMapView.Editor.RequestShapeAsync(DrawShape.Freehand) as Polyline;
                    if (boundary.Parts.First().Count <= 1)
                        return;

                    aoi = new Polygon(boundary.Parts, MyMapView.SpatialReference);
                    aoi = GeometryEngine.Simplify(aoi) as Polygon;
                }
                else
                {
                    aoi = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon) as Polygon;
                }

				_graphicsOverlay.Graphics.Add(new Graphic(aoi));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        // Calls the ExtractData service and prompts the user for saving the results
        private async void ExtractDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uiPanel.IsEnabled = false;
                progress.Visibility = txtStatus.Visibility = Visibility.Visible;

                var layersToClip = listLayers.SelectedItems.OfType<string>().Select(s => new GPString(s, s)).ToList();
                if (layersToClip == null || layersToClip.Count == 0)
                    throw new ApplicationException("Please select layers to extract data from.");

				if (_graphicsOverlay.Graphics.Count == 0)
                    throw new ApplicationException("Please digitize an area of interest polygon on the map.");

                var parameter = new GPInputParameter() { OutSpatialReference = SpatialReferences.WebMercator };
                parameter.GPParameters.Add(new GPMultiValue<GPString>("Layers_to_Clip", layersToClip));
				parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Area_of_Interest", _graphicsOverlay.Graphics[0].Geometry));
                parameter.GPParameters.Add(new GPString("Feature_Format", (string)comboFormat.SelectedItem));

                var result = await SubmitAndPollStatusAsync(parameter);

                if (result.JobStatus == GPJobStatus.Succeeded)
                {
                    txtStatus.Text = "Finished processing. Retrieving results...";

                    var outParam = await _gpTask.GetResultDataAsync(result.JobID, "Output_Zip_File") as GPDataFile;
                    if (outParam != null && outParam.Uri != null)
                    {
                        await SaveResultsToFile(outParam);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                uiPanel.IsEnabled = true;
                progress.Visibility = txtStatus.Visibility = Visibility.Collapsed;
            }
        }

        // Submit GP Job and Poll the server for results every 2 seconds.
        private async Task<GPJobInfo> SubmitAndPollStatusAsync(GPInputParameter parameter)
        {
            // Submit gp service job
            var result = await _gpTask.SubmitJobAsync(parameter);

            // Poll for the results async
            while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted
                && result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut)
            {
                result = await _gpTask.CheckJobStatusAsync(result.JobID);

                txtStatus.Text = string.Join(Environment.NewLine, result.Messages.Select(x => x.Description));

                await Task.Delay(2000);
            }

            return result;
        }

        // Saves results to a local file - with user prompt for file name
        private async Task SaveResultsToFile(GPDataFile gpDataFile)
        {
            MessageBoxResult res = MessageBox.Show("Data file created. Would you like to download the file?", "Success", MessageBoxButton.OKCancel);
            if (res == MessageBoxResult.OK)
            {
                WebClient webClient = new WebClient();
                var stream = await webClient.OpenReadTaskAsync(gpDataFile.Uri);

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = "Output.zip";
                dialog.Filter = "Zip file (*.zip)|*.zip";

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        using (Stream fs = (Stream)dialog.OpenFile())
                        {
                            stream.CopyTo(fs);
                            fs.Flush();
                            fs.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving file :" + ex.Message);
                    }
                }
            }
        }

		// Show error if loading layers fail
		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			MessageBox.Show(
					string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
					"Error loading layer");
		}
    }
}
