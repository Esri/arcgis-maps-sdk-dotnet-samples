using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates how to display map overlay elements in the MapView. Here, three map overlays are included in the MapView XAML. The first is an overlay that maintains a constant X,Y anchor point on the map. The second overlay is always anchored to the center point of the current extent. The last overlay changes its anchor point when the user clicks the map.
    /// </summary>
    /// <title>Map Overlays</title>
	/// <category>Mapping</category>
	public partial class MapOverlays : UserControl
    {
        /// <summary>Construct Map Overlays sample control</summary>
        public MapOverlays()
        {
            InitializeComponent();

            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        private void mapView_ExtentChanged(object sender, System.EventArgs e)
        {
            var center = mapView.Extent.GetCenter();

            if (!(clickOverlay.DataContext is MapPoint))
                clickOverlay.DataContext = center;

            centerOverlay.DataContext = center;
        }

        private void mapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            clickOverlay.DataContext = e.Location;
        }
    }
}
