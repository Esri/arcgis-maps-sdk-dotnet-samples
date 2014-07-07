using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates use of the Geoprocessor to call an asynchronous ViewShed geoprocessing service.
    /// </summary>
    /// <title>Viewshed</title>
	/// <category>Tasks</category>
	/// <subcategory>Geoprocessing</subcategory>
	public partial class Viewshed : UserControl
    {
        private Geoprocessor _gpTask;

        /// <summary>Construct Viewshed sample control</summary>
        public Viewshed()
        {
            InitializeComponent();

            mapView.Map.InitialViewpoint = new Envelope(-12004035.9462375, 4652780.19374956, -11735714.4261546, 4808810.41937776);

            _gpTask = new Geoprocessor(
                new Uri("http://serverapps101.esri.com/arcgis/rest/services/ProbabilisticViewshedModel/GPServer/ProbabilisticViewshedModel"));
        }

        // Get the users click point on the map and fire off a GP Job to calculate the viewshed
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uiPanel.IsEnabled = false;
                inputLayer.Graphics.Clear();
                mapView.Map.Layers.Remove("ViewshedResultsLayer");

                //get the user's input point
                var inputPoint = await mapView.Editor.RequestPointAsync();

                progress.Visibility = Visibility.Visible;
                inputLayer.Graphics.Add(new Graphic() { Geometry = inputPoint });

                var parameter = new GPInputParameter() { OutSpatialReference = SpatialReferences.WebMercator };
                parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Features", inputPoint));
                parameter.GPParameters.Add(new GPString("Height", txtHeight.Text));
                parameter.GPParameters.Add(new GPLinearUnit("Distance", LinearUnits.Miles, Convert.ToDouble(txtMiles.Text)));

                var result = await SubmitAndPollStatusAsync(parameter);

                if (result.JobStatus == GPJobStatus.Succeeded)
                {
                    txtStatus.Text = "Finished processing. Retrieving results...";

                    //get the results as a ArcGISDynamicMapServiceLayer
                    var resultLayer = _gpTask.GetResultMapServiceLayer(result.JobID);
                    if (resultLayer != null)
                    {
                        //Insert the results layer just beneath the input graphics layer.
                        //This allows us to see the input point at all times.
                        resultLayer.ID = "ViewshedResultsLayer";
                        mapView.Map.Layers.Insert(mapView.Map.Layers.IndexOf(inputLayer), resultLayer);
                        await mapView.LayersLoadedAsync(new List<Layer> { resultLayer });
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
                progress.Visibility = Visibility.Collapsed;
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
    }
}
