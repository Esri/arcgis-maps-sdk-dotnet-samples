using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates using the OnlineLocatorTask.ReverseGeocodeAsync method to get address information from a location on the map.
    /// </summary>
    /// <title>Reverse Geocode</title>
    /// <category>Geocode Tasks</category>
    public partial class ReverseGeocode : Page
    {
        private const string OnlineLocatorUrl = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";

        private GraphicsLayer _graphicsLayer;
        private LocatorTask _locator;

        /// <summary>Construct reverse geocode sample control</summary>
        public ReverseGeocode()
        {
            InitializeComponent();

            Envelope extent = new Envelope(-117.387, 33.97, -117.355, 33.988, SpatialReferences.Wgs84);
            mapView.Map.InitialExtent = GeometryEngine.Project(extent, SpatialReferences.WebMercator) as Envelope;

            _graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
                
            _locator = new OnlineLocatorTask(new Uri(OnlineLocatorUrl));
        }

        // Reverse geocode the clicked point and add a graphic and map tip to the map
        private async void mapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            try
            {
                _graphicsLayer.Graphics.Add(new Graphic(e.Location));

                var result = await _locator.ReverseGeocodeAsync(e.Location, 50, SpatialReferences.Wgs84, CancellationToken.None);

                var overlay = new ContentControl() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
                overlay.Template = layoutGrid.Resources["MapTipTemplate"] as ControlTemplate;
                overlay.DataContext = result;
                MapView.SetMapOverlayAnchor(overlay, e.Location);
                mapView.Overlays.Add(overlay);
            }
            catch (AggregateException aex)
            {
                var _ = new MessageDialog(aex.InnerExceptions[0].Message, "Reverse Geocode").ShowAsync();
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Reverse Geocode").ShowAsync();
            }
        }

        // Clear current graphcis and overlay map tips
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            mapView.Overlays.Clear();
            _graphicsLayer.Graphics.Clear();
        }
    }
}
