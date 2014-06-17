using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates using an QueryTask to Query an ArcGISImageServiceLayer to find the outlines of image tiles and display them in a GraphicsLayer. MapOverlay attribute information is displayed for selected tile graphics when they are clicked on the map.
    /// </summary>
    /// <title>Get Samples</title>
    /// <category>Tasks</category>
    /// <subcategory>Imagery</subcategory>
    public partial class GetSamples : UserControl
    {
        /// <summary>Construct Get Image Samples sample control</summary>
        public GetSamples()
        {
            InitializeComponent();
            mapView.LayerLoaded += mapView_LayerLoaded;
        }

        // Zoom to the image service extent
        private async void mapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.Layer is ArcGISImageServiceLayer)
            {
                if (e.Layer.FullExtent != null)
                    await mapView.SetViewAsync(e.Layer.FullExtent);
            }
        }

        // Start query process on user button click
        private async void GetSamplesButton_Click(object sender, RoutedEventArgs e)
        {
            graphicsLayer.Graphics.Clear();
            await QueryImageTiles();
        }

        // Query the image service for sample tiles
        private async Task QueryImageTiles()
        {
            try
            {
                var envelope = await mapView.Editor.RequestShapeAsync(DrawShape.Envelope) as Envelope;

                QueryTask queryTask = new QueryTask(
                    new Uri("http://servicesbeta.esri.com/ArcGIS/rest/services/Portland/PortlandAerial/ImageServer/query"));

                Query query = new Query(envelope)
                {
                    OutFields = new OutFields(new string[] { "Name", "LowPS" }),
                    ReturnGeometry = true,
                    OutSpatialReference = mapView.SpatialReference,
                    Where = "Category = 1"
                };

                var result = await queryTask.ExecuteAsync(query);
                
                graphicsLayer.Graphics.AddRange(result.FeatureSet.Features);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        // Hittest the graphics layer and show the map tip for the selected graphic
        private async void mapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                graphicsLayer.ClearSelection();

                var graphic = await graphicsLayer.HitTestAsync(mapView, e.Position);
                if (graphic != null)
                {
                    graphic.IsSelected = true;
                    MapView.SetMapOverlayAnchor(mapTip, e.Location);
                    mapTip.DataContext = graphic;
                    mapTip.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    mapTip.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch
            {
                mapTip.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
