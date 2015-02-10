using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates using the OnlineLocatorTask.ReverseGeocodeAsync method to get address information from a location on the map.
    /// </summary>
    /// <title>Reverse Geocode</title>
    /// <category>Geocode Tasks</category>
    public partial class ReverseGeocode : Page
    {
        private const string OnlineLocatorUrl = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";

        private GraphicsOverlay _graphicsOverlay;
        private LocatorTask _locator;

        /// <summary>Construct reverse geocode sample control</summary>
        public ReverseGeocode()
        {
            InitializeComponent();
			_graphicsOverlay = MyMapView.GraphicsOverlays[0];
            _locator = new OnlineLocatorTask(new Uri(OnlineLocatorUrl));
        }

        // Reverse geocode the clicked point and add a graphic and map tip to the map
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                _graphicsOverlay.Graphics.Add(new Graphic(e.Location));

                var result = await _locator.ReverseGeocodeAsync(e.Location, 50, SpatialReferences.Wgs84, CancellationToken.None);

                var overlay = new ContentControl() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
                overlay.Template = layoutGrid.Resources["MapTipTemplate"] as ControlTemplate;
                overlay.DataContext = result;
				MapView.SetViewOverlayAnchor(overlay, e.Location);
                MyMapView.Overlays.Items.Add(overlay);
            }
            catch (AggregateException aex)
            {
                var _x = new MessageDialog(aex.InnerExceptions[0].Message, "Reverse Geocode").ShowAsync();
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Reverse Geocode").ShowAsync();
            }
        }

        // Clear current graphics and overlay map tips
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.Overlays.Items.Clear();
            _graphicsOverlay.Graphics.Clear();
        }
    }
}
