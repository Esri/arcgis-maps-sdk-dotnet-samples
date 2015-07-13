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
    /// This sample demonstrates use of the Geoprocessor to call a MessageInABottle geoprocessing service. To use the sample, specify the number of days and click a point in the ocean. The path of a bottle dropped at the click point over the specified number of days will be drawn on the map.
    /// </summary>
    /// <title>Message in a Bottle</title>
	/// <category>Tasks</category>
	/// <subcategory>Geoprocessing</subcategory>
	public partial class MessageInABottle : UserControl
    {
        private const string MessageInABottleServiceUrl =
            "http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_Currents_World/GPServer/MessageInABottle";

		private GraphicsOverlay _inputOverlay;
		private GraphicsOverlay _resultsOverlay;

        /// <summary>Construct Message In A Bottle sample control</summary>
        public MessageInABottle()
        {
            InitializeComponent();

			_inputOverlay = MyMapView.GraphicsOverlays["inputOverlay"];
			_resultsOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];
        }

        // Begin geoprocessing with a user tap on the map
        private async void MyMapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            try
            {
                Progress.Visibility = Visibility.Visible;

				_resultsOverlay.Graphics.Clear();
                _inputOverlay.Graphics.Clear();
				_inputOverlay.Graphics.Add(new Graphic() { Geometry = e.Location });

                Geoprocessor geoprocessorTask = new Geoprocessor(new Uri(MessageInABottleServiceUrl));

                var parameter = new GPInputParameter() { OutSpatialReference = MyMapView.SpatialReference };
				var ptNorm = GeometryEngine.NormalizeCentralMeridian(e.Location);
                var ptGeographic = GeometryEngine.Project(ptNorm, SpatialReferences.Wgs84) as MapPoint;

                parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Point", ptGeographic));
                parameter.GPParameters.Add(new GPDouble("Days", Convert.ToDouble(DaysTextBox.Text)));

                var result = await geoprocessorTask.ExecuteAsync(parameter);

                foreach (GPParameter gpParameter in result.OutParameters)
                {
                    if (gpParameter is GPFeatureRecordSetLayer)
                    {
                        GPFeatureRecordSetLayer gpLayer = gpParameter as GPFeatureRecordSetLayer;
						_resultsOverlay.Graphics.AddRange(gpLayer.FeatureSet.Features.OfType<Graphic>());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geoprocessor service failed: " + ex.Message, "Sample Error");
            }
            finally
            {
                Progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
