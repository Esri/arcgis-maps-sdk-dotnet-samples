using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates how to spatially search your data using a QueryTask with its geometry attribute set. The sample spatially queries a map service using the buffer geometry around a user's click point and displays the results in a graphics layer on the map.
    /// </summary>
    /// <title>Spatial Query</title>
    /// <category>Tasks</category>
    /// <subcategory>Query</subcategory>
    public partial class SpatialQuery : UserControl
    {
        /// <summary>Construct Spatial Query sample control</summary>
        public SpatialQuery()
        {
            InitializeComponent();

            InitializePictureMarkerSymbol();
        }

        // Initialize PushPin symbol
        private async void InitializePictureMarkerSymbol()
        {
            try
            {
                var pms = layoutGrid.Resources["DefaultMarkerSymbol"] as PictureMarkerSymbol;
				await pms.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSamplesDesktop;component/Assets/RedPushpin.png"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // buffer the click point, query the map service with the buffer geometry as the filter and add graphics to the map
        private async void MyMapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            try
            {
                var graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
                if (!(graphicsOverlay.Graphics.Count == 0))
                    graphicsOverlay.Graphics.Clear();

                graphicsOverlay.Graphics.Add(new Graphic() { Geometry = e.Location });

                var bufferOverlay = MyMapView.GraphicsOverlays["bufferOverlay"];
                if (!(bufferOverlay.Graphics.Count == 0))
                    bufferOverlay.Graphics.Clear();

                var bufferResult = GeometryEngine.Buffer(e.Location, 100);
                bufferOverlay.Graphics.Add(new Graphic() { Geometry = bufferResult });

                var queryTask = new QueryTask(
                    new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2"));
                var query = new Query("1=1")
                {
                    ReturnGeometry = true,
                    OutSpatialReference = MyMapView.SpatialReference,
                    Geometry = bufferResult
                };
                query.OutFields.Add("OWNERNME1");

                var queryResult = await queryTask.ExecuteAsync(query);
                if (queryResult != null && queryResult.FeatureSet != null)
                {
                    var resultOverlay = MyMapView.GraphicsOverlays["parcelOverlay"];
                    if (!(resultOverlay.Graphics.Count == 0))
                        resultOverlay.Graphics.Clear();

                    resultOverlay.Graphics.AddRange(queryResult.FeatureSet.Features.OfType<Graphic>());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Spatial Query Sample");
            }
        }
    }
}