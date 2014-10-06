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

            MyMapView.ExtentChanged += MyMapView_ExtentChanged;

			esriOverlay.DataContext = new MapPoint(-117.19568, 34.056601, SpatialReferences.Wgs84);
        }

		private void MyMapView_ExtentChanged(object sender, System.EventArgs e)
		{
			var normalizedPoint = GeometryEngine.NormalizeCentralMeridian(MyMapView.Extent.GetCenter());
			var projectedCenter = GeometryEngine.Project(normalizedPoint, SpatialReferences.Wgs84) as MapPoint;

			if (!(clickOverlay.DataContext is MapPoint))
				clickOverlay.DataContext = projectedCenter;

			centerOverlay.DataContext = projectedCenter;
		}

		private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			var normalizedPoint = GeometryEngine.NormalizeCentralMeridian(e.Location);
			var projectedCenter = GeometryEngine.Project(normalizedPoint, SpatialReferences.Wgs84) as MapPoint;
			clickOverlay.DataContext = projectedCenter;
		}
    }
}
