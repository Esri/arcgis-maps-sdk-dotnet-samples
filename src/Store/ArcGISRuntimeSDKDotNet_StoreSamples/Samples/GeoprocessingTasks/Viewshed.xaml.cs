using Esri.ArcGISRuntime.Controls;
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

        private GraphicsOverlay _inputOverlay;
        private Geoprocessor _gpTask;

        /// <summary>Construct Viewshed sample control</summary>
        public Viewshed()
        {
            InitializeComponent();

			_inputOverlay = MyMapView.GraphicsOverlays[0];
                
            _gpTask = new Geoprocessor(new Uri(ViewshedServiceUrl));
        }

        // Get the users click point on the map and fire off a GP Job to calculate the viewshed
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _inputOverlay.Graphics.Clear();
                MyMapView.Map.Layers.Remove("ViewshedResultsLayer");

                //get the user's input point
                var inputPoint = await MyMapView.Editor.RequestPointAsync();

                progress.Visibility = Visibility.Visible;
                _inputOverlay.Graphics.Add(new Graphic() { Geometry = inputPoint });

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
                        resultLayer.ID = "ViewshedResultsLayer";
                        MyMapView.Map.Layers.Add(resultLayer);
                        await MyMapView.LayersLoadedAsync(new List<Layer> { resultLayer });
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
