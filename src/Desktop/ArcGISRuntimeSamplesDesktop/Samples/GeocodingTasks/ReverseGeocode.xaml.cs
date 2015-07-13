using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates using the OnlineLocatorTask.ReverseGeocodeAsync method to get address information from a location on the map.
    /// </summary>
    /// <title>Reverse Geocode</title>
	/// <category>Tasks</category>
	/// <subcategory>Geocoding</subcategory>
	public partial class ReverseGeocode : UserControl
    {
        private LocatorTask _locator;

        /// <summary>Construct reverse geocode sample control</summary>
        public ReverseGeocode()
        {
            InitializeComponent();
            _locator = new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"));
        }

        // Reverse geocode the clicked point and add a graphic and map tip to the map
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                graphicsOverlay.Graphics.Add(new Graphic(e.Location));

                var result = await _locator.ReverseGeocodeAsync(e.Location, 50, SpatialReferences.Wgs84, CancellationToken.None);

                var overlay = new ContentControl() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
                overlay.Template = layoutGrid.Resources["MapTipTemplate"] as ControlTemplate;
                overlay.DataContext = result;
                MapView.SetViewOverlayAnchor(overlay, e.Location);
                MyMapView.Overlays.Items.Add(overlay);
            }
            catch (AggregateException aex)
            {
                MessageBox.Show(aex.InnerExceptions[0].Message, "Reverse Geocode");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reverse Geocode");
            }
        }

        // Clear current graphcis and overlay map tips
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.Overlays.Items.Clear();
			graphicsOverlay.Graphics.Clear();
        }
    }
}
