using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// Demonstrates using a QueryTask to Query an ArcGISImageServiceLayer to find image tile outlines and display them in a GraphicOverlay. MapOverlay attribute information is displayed for selected tile graphics when they are clicked on the map.
    /// </summary>
    /// <title>Get Samples</title>
    /// <category>Imagery Tasks</category>
    public partial class GetSamples : Page
    {
		private GraphicsOverlay _graphicsOverlay;
		private FrameworkElement _mapTip;

        /// <summary>Construct Get Image Samples sample control</summary>
        public GetSamples()
        {
            InitializeComponent();
			_graphicsOverlay = MyMapView.GraphicsOverlays.First();
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
			_mapTip.Visibility = Visibility.Collapsed;
            _graphicsOverlay.Graphics.Clear();
            await QueryImageTiles();
        }

        // Query the image service for sample tiles
        private async Task QueryImageTiles()
        {
            try
            {
                var envelope = await MyMapView.Editor.RequestShapeAsync(DrawShape.Envelope) as Envelope;

				var imageLayer = MyMapView.Map.Layers["ImageLayer"] as ArcGISImageServiceLayer;
                QueryTask queryTask = new QueryTask(new Uri(imageLayer.ServiceUri));
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
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
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
					MapView.SetViewOverlayAnchor(_mapTip, e.Location);
                    _mapTip.DataContext = graphic;
                    _mapTip.Visibility = Visibility.Visible;
                }
                else
                    _mapTip.Visibility = Visibility.Collapsed;
            }
            catch
            {
                _mapTip.Visibility = Visibility.Collapsed;
            }
        }
    }
}
