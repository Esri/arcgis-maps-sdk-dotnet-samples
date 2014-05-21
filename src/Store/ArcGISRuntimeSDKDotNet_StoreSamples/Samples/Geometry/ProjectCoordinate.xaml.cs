using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Sample shows how to project a coordinate from the current map projection (in this case Web Mercator) to a different projection.
    /// </summary>
    /// <title>Project</title>
	/// <category>Geometry</category>
	public partial class ProjectCoordinate : Windows.UI.Xaml.Controls.Page
    {
        private GraphicsLayer _graphicsLayer;

        /// <summary>Construct Project sample control</summary>
        public ProjectCoordinate()
        {
            InitializeComponent();

            _graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
                
            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        // Start map interaction
        private async void mapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                mapView.ExtentChanged -= mapView_ExtentChanged;
                await AcceptPointsAsync();
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Accept user map clicks and add points to the graphics layer with the selected symbol
        // - collected point is in the coordinate system of the current map
        private async Task AcceptPointsAsync()
        {
            while (mapView.Extent != null)
            {
                var point = await mapView.Editor.RequestPointAsync();

                _graphicsLayer.Graphics.Clear();
                _graphicsLayer.Graphics.Add(new Graphic(point));

                // Convert from web mercator to WGS84
                var projectedPoint = GeometryEngine.Project(point, SpatialReferences.Wgs84);

                gridXY.Visibility = gridLatLon.Visibility = Visibility.Visible;
                gridXY.DataContext = point;
                gridLatLon.DataContext = projectedPoint;
            }
        }
    }
}
