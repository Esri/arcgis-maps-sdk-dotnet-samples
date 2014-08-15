using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to display map overlay elements in the MapView.
    /// </summary>
    /// <title>Map Overlays</title>
    /// <category>Mapping</category>
	public sealed partial class MapOverlays : Page
    {
        private FrameworkElement _clickOverlay;
        private FrameworkElement _centerOverlay;

        public MapOverlays()
        {
            this.InitializeComponent();

			(MyMapView.Overlays.Items[0] as Grid).DataContext = new MapPoint(-117.19568, 34.056601, SpatialReferences.Wgs84);

            MyMapView.Loaded += MyMapView_Loaded;
            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        private void MyMapView_Loaded(object sender, RoutedEventArgs e)
        {
            _clickOverlay = FindName("clickOverlay") as FrameworkElement;
            _centerOverlay = FindName("centerOverlay") as FrameworkElement;
        }

        private void MyMapView_ExtentChanged(object sender, System.EventArgs e)
        {
			var normalizedPoint = GeometryEngine.NormalizeCentralMeridian(MyMapView.Extent.GetCenter());
			var projectedCenter = GeometryEngine.Project(normalizedPoint, SpatialReferences.Wgs84) as MapPoint;

            if (!(_clickOverlay.DataContext is MapPoint))
				_clickOverlay.DataContext = projectedCenter;

			_centerOverlay.DataContext = projectedCenter;
        }

        private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
			var normalizedPoint = GeometryEngine.NormalizeCentralMeridian(e.Location);
			var projectedCenter = GeometryEngine.Project(normalizedPoint, SpatialReferences.Wgs84) as MapPoint;
			_clickOverlay.DataContext = projectedCenter;
        }
    }
}
