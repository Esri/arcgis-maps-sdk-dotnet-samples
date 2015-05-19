using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates using an QueryTask to Query an ArcGISImageServiceLayer to find the outlines of image tiles and display them in a GraphicOverlay. MapOverlay attribute information is displayed for selected tile graphics when they are clicked on the map.
    /// </summary>
    /// <title>Get Samples</title>
    /// <category>Tasks</category>
    /// <subcategory>Imagery</subcategory>
    public partial class GetSamples : UserControl
    {
		private GraphicsOverlay _graphicsOverlay;
		private FrameworkElement _mapTip;

        /// <summary>Construct Get Image Samples sample control</summary>
        public GetSamples()
        {
            InitializeComponent();
			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			_mapTip = MyMapView.Overlays.Items.First() as FrameworkElement;
            MyMapView.LayerLoaded += MyMapView_LayerLoaded;
        }

        // Zoom to the image service extent
        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.Layer is ArcGISImageServiceLayer)
            {
                if (e.Layer.FullExtent != null)
                    await MyMapView.SetViewAsync(e.Layer.FullExtent);
            }
        }

        // Start query process on user button click
        private async void GetSamplesButton_Click(object sender, RoutedEventArgs e)
        {
            _graphicsOverlay.Graphics.Clear();
			_mapTip.Visibility = System.Windows.Visibility.Collapsed;
			await QueryImageTiles();
        }

        // Query the image service for sample tiles
        private async Task QueryImageTiles()
        {
            try
            {
                var envelope = await MyMapView.Editor.RequestShapeAsync(DrawShape.Envelope) as Envelope;

                QueryTask queryTask = new QueryTask(
					new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Portland/Aerial/ImageServer/"));

                Query query = new Query(envelope)
                {
                    OutFields = new OutFields(new string[] { "Name", "LowPS" }),
                    ReturnGeometry = true,
                    OutSpatialReference = MyMapView.SpatialReference,
                    Where = "Category = 1"
                };

                var result = await queryTask.ExecuteAsync(query);

				_graphicsOverlay.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        // Hittest the graphics layer and show the map tip for the selected graphic
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
				_graphicsOverlay.ClearSelection();

				var graphic = await _graphicsOverlay.HitTestAsync(MyMapView, e.Position);
                if (graphic != null)
                {
                    graphic.IsSelected = true;
					MapView.SetViewOverlayAnchor(mapTip, e.Location);
					_mapTip.DataContext = graphic;
					_mapTip.Visibility = System.Windows.Visibility.Visible;
                }
                else
					_mapTip.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch
            {
				_mapTip.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
