using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates use of the Geoprocessor to call a Viewshed geoprocessing service.
    /// </summary>
    /// <title>Viewshed</title>
	/// <category>Tasks</category>
	/// <subcategory>Geoprocessing</subcategory>
	public partial class Viewshed : UserControl
    {
		private const string ViewshedServiceUrl =
		   "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/ESRI_Elevation_World/GPServer/Viewshed";

		private GraphicsOverlay _inputOverlay;
		private GraphicsOverlay _viewshedOverlay;
		private Geoprocessor _gpTask;

        /// <summary>Construct Viewshed sample control</summary>
        public Viewshed()
        {
            InitializeComponent();

			_inputOverlay = MyMapView.GraphicsOverlays["inputOverlay"];
			_viewshedOverlay = MyMapView.GraphicsOverlays["ViewshedOverlay"];

            _gpTask = new Geoprocessor(new Uri(ViewshedServiceUrl));
        }

        // Get the users click point on the map and fire off a GP Job to calculate the viewshed
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uiPanel.IsEnabled = false;
				_inputOverlay.Graphics.Clear();
				_viewshedOverlay.Graphics.Clear();

                //get the user's input point
                var inputPoint = await MyMapView.Editor.RequestPointAsync();

                progress.Visibility = Visibility.Visible;
				_inputOverlay.Graphics.Add(new Graphic() { Geometry = inputPoint });

                var parameter = new GPInputParameter() { OutSpatialReference = SpatialReferences.WebMercator };
				parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Observation_Point", inputPoint));
				parameter.GPParameters.Add(new GPLinearUnit("Viewshed_Distance", LinearUnits.Miles, Convert.ToDouble(txtMiles.Text)));

				txtStatus.Text = "Processing on server...";
				var result = await _gpTask.ExecuteAsync(parameter);
                if (result == null || result.OutParameters == null || !(result.OutParameters[0] is GPFeatureRecordSetLayer))
					throw new ApplicationException("No viewshed graphics returned for this start point.");

                txtStatus.Text = "Finished processing. Retrieving results...";
				var viewshedLayer = result.OutParameters[0] as GPFeatureRecordSetLayer;
				_viewshedOverlay.Graphics.AddRange(viewshedLayer.FeatureSet.Features.OfType<Graphic>());
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
    }
}
