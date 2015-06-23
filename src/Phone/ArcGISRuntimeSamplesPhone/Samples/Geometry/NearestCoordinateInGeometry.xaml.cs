using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// This sample demonstrates how to find the nearest polygon point or vertex of a geometry using the NearestCoordinateInGeometry or NearestVertexInGeometry methods of the GeometryEngine class.
    /// </summary>
    /// <title>Nearest Coordinate in Geometry</title>
    /// <category>Geometry</category>
    public sealed partial class NearestCoordinateInGeometry : Page
    {
        private SimpleMarkerSymbol _vertexSymbol;
        private SimpleMarkerSymbol _userPointSymbol;
        private GraphicsOverlay _graphicsOverlay;
        private GraphicsOverlay _targetOverlay;
        private GraphicsOverlay _coordinateOverlay;

        /// <summary>Construct Nearest Coordinate sample control</summary>
        public NearestCoordinateInGeometry()
        {
            InitializeComponent();

            _vertexSymbol = new SimpleMarkerSymbol() { Color = Colors.LightGreen, Size = 8, Style = SimpleMarkerStyle.Circle };
            _userPointSymbol = new SimpleMarkerSymbol() { Color = Colors.Black, Size = 10, Style = SimpleMarkerStyle.Circle };

            _graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
            _targetOverlay = MyMapView.GraphicsOverlays["targetOverlay"];
            _coordinateOverlay = MyMapView.GraphicsOverlays["coordinateOverlay"];

            MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
        }

        private async void MyMapView_NavigationCompleted(object sender, EventArgs e)
        {
            try
            {
                MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;

                await LoadParcelsAsync();
                await MyMapView.LayersLoadedAsync();

                await ProcessUserPointsAsync(false);
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Error loading parcels: " + ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void SelectTargetButton_Click(object sender, RoutedEventArgs e)
        {
            _coordinateOverlay.Graphics.Clear();
            await ProcessUserPointsAsync(false);
        }

        private async void cboVertexOnly_Click(object sender, RoutedEventArgs e)
        {
            _coordinateOverlay.Graphics.Clear();
            await ProcessUserPointsAsync(_targetOverlay.Graphics.Count > 0);
        }

        // Process user selection and point clicks
        private async Task ProcessUserPointsAsync(bool isTargetSelected)
        {
            try
            {
                txtResult.Visibility = Visibility.Collapsed;
                txtResult.Text = string.Empty;

                if (MyMapView.Editor.IsActive)
                    MyMapView.Editor.Cancel.Execute(null);

                if (!isTargetSelected)
                {
                    await SelectTargetGeometryAsync();
                }

                while (MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent != null)
                {
                    await GetNearestCoordAsync((bool)cboVertexOnly.IsChecked);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Load parcels from map service
        private async Task LoadParcelsAsync()
        {
            var queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));
            
            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

            var query = new Query(viewpointExtent) { ReturnGeometry = true, OutSpatialReference = MyMapView.SpatialReference, OutFields = OutFields.All };
            var result = await queryTask.ExecuteAsync(query);

            _graphicsOverlay.Graphics.Clear();
            _graphicsOverlay.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
        }

        // Accept user click point and select the underlying target polygon
        private async Task SelectTargetGeometryAsync()
        {
            txtInstruct.Text = "Click to select a target geometry";
            _coordinateOverlay.Graphics.Clear();
            _targetOverlay.Graphics.Clear();

            Graphic graphic = null;
            while (graphic == null)
            {
                var point = await MyMapView.Editor.RequestPointAsync();

                graphic = await _graphicsOverlay.HitTestAsync(MyMapView, MyMapView.LocationToScreen(point));
                if (graphic == null)
                    continue;

                _targetOverlay.Graphics.Add(graphic);

                var poly = graphic.Geometry as Polygon;
                foreach (var coord in poly.Parts.First().GetPoints())
                {
                    _targetOverlay.Graphics.Add(new Graphic(coord, _vertexSymbol));
                }
            }
        }

        // Accept user click point and find nearest target geometry point
        private async Task GetNearestCoordAsync(bool vertexOnly)
        {
            var target = _targetOverlay.Graphics.Select(g => g.Geometry).FirstOrDefault();
            if (target == null)
                return;

            txtInstruct.Text = "Click the map to find the nearest coordinate";
            var point = await MyMapView.Editor.RequestPointAsync();

            ProximityResult result = null;
            if (vertexOnly)
                result = GeometryEngine.NearestVertex(target, point);
            else
                result = GeometryEngine.NearestCoordinate(target, point);

            _coordinateOverlay.Graphics.Clear();
            _coordinateOverlay.Graphics.Add(new Graphic(point, _userPointSymbol));
            _coordinateOverlay.Graphics.Add(new Graphic(result.Point));

            txtResult.Visibility = Visibility.Visible;
            txtResult.Text = string.Format("Nearest Point: Index: {0}, Distance: {1:0.000}", result.PointIndex, result.Distance);
        }
    }
}
