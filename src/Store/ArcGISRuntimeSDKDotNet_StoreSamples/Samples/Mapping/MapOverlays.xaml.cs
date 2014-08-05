using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
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

			(MyMapView.Overlays[0] as Grid).DataContext = new MapPointBuilder(-117.19568, 34.056601, SpatialReferences.Wgs84).ToGeometry();

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
            var center = GeometryEngine.Project(MyMapView.Extent.GetCenter(), SpatialReferences.Wgs84);

            if (!(_clickOverlay.DataContext is MapPoint))
                _clickOverlay.DataContext = center;

            _centerOverlay.DataContext = center;
        }

        private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            _clickOverlay.DataContext = GeometryEngine.Project(e.Location, SpatialReferences.Wgs84);
        }
    }
}
