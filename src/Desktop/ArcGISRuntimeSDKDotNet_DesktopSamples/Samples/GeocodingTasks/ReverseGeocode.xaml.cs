using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates using the OnlineLocatorTask.ReverseGeocodeAsync method to attempt to address information from a location on the map.
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

            Envelope extent = new Envelope(-117.387, 33.97, -117.355, 33.988, SpatialReferences.Wgs84);
            mapView.Map.InitialExtent = GeometryEngine.Project(extent, SpatialReferences.WebMercator) as Envelope;

            _locator = new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"));
        }

        // Reverse geocode the clicked point and add a graphic and map tip to the map
        private async void mapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            try
            {
                graphicsLayer.Graphics.Add(new Graphic(e.Location));

                var result = await _locator.ReverseGeocodeAsync(e.Location, 50, SpatialReferences.Wgs84, CancellationToken.None);

                var overlay = new ContentControl() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
                overlay.Template = layoutGrid.Resources["MapTipTemplate"] as ControlTemplate;
                overlay.DataContext = result;
                MapView.SetMapOverlayAnchor(overlay, e.Location);
                mapView.Overlays.Add(overlay);
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
            mapView.Overlays.Clear();
            graphicsLayer.Graphics.Clear();
        }
    }
}
