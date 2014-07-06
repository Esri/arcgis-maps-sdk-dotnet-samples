using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates use of the Geoprocessor to call an asynchronous ViewShed geoprocessing service.
    /// </summary>
    /// <title>Viewshed</title>
    /// <category>Geoprocessing Tasks</category>
    public partial class Viewshed : Windows.UI.Xaml.Controls.Page
    {
        private const string ViewshedServiceUrl =
            "http://serverapps101.esri.com/arcgis/rest/services/ProbabilisticViewshedModel/GPServer/ProbabilisticViewshedModel";

        private GraphicsLayer _inputLayer;
        private Geoprocessor _gpTask;

        /// <summary>Construct Viewshed sample control</summary>
        public Viewshed()
        {
            InitializeComponent();

            _inputLayer = mapView.Map.Layers["InputLayer"] as GraphicsLayer;
                
            _gpTask = new Geoprocessor(new Uri(ViewshedServiceUrl));
        }

        // Get the users click point on the map and fire off a GP Job to calculate the viewshed
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _inputLayer.Graphics.Clear();
                mapView.Map.Layers.Remove("ViewshedResultsLayer");

                //get the user's input point
                var inputPoint = await mapView.Editor.RequestPointAsync();

                progress.Visibility = Visibility.Visible;
                _inputLayer.Graphics.Add(new Graphic() { Geometry = inputPoint });

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
                        mapView.Map.Layers.Insert(mapView.Map.Layers.IndexOf(_inputLayer), resultLayer);
                        await mapView.LayersLoadedAsync(new List<Layer> { resultLayer });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
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
