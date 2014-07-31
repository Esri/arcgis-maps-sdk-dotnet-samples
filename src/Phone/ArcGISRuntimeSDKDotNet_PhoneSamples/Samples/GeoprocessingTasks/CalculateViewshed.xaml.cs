using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Geoprocessing Tasks</category>
	public sealed partial class CalculateViewshed : Page
    {
        GraphicsLayer inputLayer;
        public CalculateViewshed()
        {
            InitializeComponent();
            InitializePMS();
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-12004036, 4652780, -11735714, 4808810));
            inputLayer = mapView1.Map.Layers["InputLayer"] as GraphicsLayer;
        }

        private async void InitializePMS()
        {
            try
            {
                var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/i_pushpin.png"));
                var imageSource = await imageFile.OpenReadAsync();
                var pms = LayoutRoot.Resources["StartMarkerSymbol"] as PictureMarkerSymbol;
                await pms.SetSourceAsync(imageSource);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }



        private void ClearResultsButton_Click(object sender, RoutedEventArgs e)
        {

            //remove all previous results
            inputLayer.Graphics.Clear();

            var resultsLayer = mapView1.Map.Layers["MyResultsLayer"];
            if (resultsLayer != null)
                mapView1.Map.Layers.Remove(resultsLayer);

        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            ClearResultsButton.Visibility = Visibility.Collapsed;

            //get the user's input point
            var inputPoint = await mapView1.Editor.RequestPointAsync();

            //update UI elements
            MyProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MyProgressRing.IsActive = true;

            inputLayer.Graphics.Clear();
            inputLayer.Graphics.Add(new Graphic() { Geometry = inputPoint });

            Geoprocessor task = new Geoprocessor(new Uri("http://serverapps101.esri.com/arcgis/rest/services/ProbabilisticViewshedModel/GPServer/ProbabilisticViewshedModel"));

            var parameter = new GPInputParameter()
            {
                OutSpatialReference = new SpatialReference(102100)
            };
            parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Features", inputPoint));
            parameter.GPParameters.Add(new GPString("Height", HeightTextBox.Text));
            parameter.GPParameters.Add(new GPLinearUnit("Distance", LinearUnits.Miles, Convert.ToDouble(MilesTextBox.Text)));


            var result = await task.SubmitJobAsync(parameter);

            //Poll the server for results every 2 seconds.
            while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted &&
                 result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut)
            {
                result = await task.CheckJobStatusAsync(result.JobID);

                //show the status
                StatusTextBlock.Text = string.Join(Environment.NewLine, result.Messages.Select(x => x.Description));


                await Task.Delay(2000);
            }
            if (result.JobStatus == GPJobStatus.Succeeded)
            {
                //get the results as a ArcGISDynamicMapServiceLayer
                StatusTextBlock.Text = "Finished processing. Retrieving results...";
                var resultLayer = task.GetResultMapServiceLayer(result.JobID);

                if (resultLayer != null)
                {
                    //Add an ID so that we can reference this layer (to remove it)
                    resultLayer.ID = "MyResultsLayer";

                    //Insert the results layer just beneath the input graphics layer.
                    //This allows us to see the input point at all times.
                    mapView1.Map.Layers.Insert(mapView1.Map.Layers.IndexOf(inputLayer), resultLayer);

                }

                //Reset the UI
                StatusTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                StartButton.IsEnabled = true;
                ClearResultsButton.Visibility = Visibility.Visible;
                MyProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MyProgressRing.IsActive = false;
            }
        }

    }
}
