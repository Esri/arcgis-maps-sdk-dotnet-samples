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

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to find the nearest polygon point or vertex of a geometry using the NearestCoordinateInGeometry or NearestVertexInGeometry methods of the GeometryEngine class.
    /// </summary>
    /// <title>Nearest Coordinate in Geometry</title>
	/// <category>Geometry</category>
	public partial class NearestCoordinateInGeometry : Windows.UI.Xaml.Controls.Page
    {
        private SimpleMarkerSymbol _vertexSymbol;
        private SimpleMarkerSymbol _userPointSymbol;
        private GraphicsLayer _graphicsLayer;
        private GraphicsLayer _targetLayer;
        private GraphicsLayer _coordinateLayer;

        /// <summary>Construct Nearest Coordinate sample control</summary>
        public NearestCoordinateInGeometry()
        {
            InitializeComponent();

            _vertexSymbol = new SimpleMarkerSymbol() { Color = Colors.LightGreen, Size = 8, Style = SimpleMarkerStyle.Circle };
            _userPointSymbol = new SimpleMarkerSymbol() { Color = Colors.Black, Size = 10, Style = SimpleMarkerStyle.Circle };

            _graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
            _targetLayer = mapView.Map.Layers["TargetLayer"] as GraphicsLayer;
            _coordinateLayer = mapView.Map.Layers["CoordinateLayer"] as GraphicsLayer;
                
            mapView.Map.InitialExtent = (Envelope)GeometryEngine.Project(
                new Envelope(-83.3188395774275, 42.61428312652851, -83.31295664068958, 42.61670913269855, SpatialReferences.Wgs84),
                SpatialReferences.WebMercator);

            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        private async void mapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                mapView.ExtentChanged -= mapView_ExtentChanged;

                await LoadParcelsAsync();
                await mapView.LayersLoadedAsync();

                await ProcessUserPointsAsync(false);
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Error loading parcels: " + ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void SelectTargetButton_Click(object sender, RoutedEventArgs e)
        {
            _coordinateLayer.Graphics.Clear();
            await ProcessUserPointsAsync(false);
        }

        private async void cboVertexOnly_Click(object sender, RoutedEventArgs e)
        {
            _coordinateLayer.Graphics.Clear();
            await ProcessUserPointsAsync(_targetLayer.Graphics.Count > 0);
        }

        // Process user selection and point clicks
        private async Task ProcessUserPointsAsync(bool isTargetSelected)
        {
            try
            {
                txtResult.Visibility = Visibility.Collapsed;
                txtResult.Text = string.Empty;

                if (mapView.Editor.IsActive)
                    mapView.Editor.Cancel.Execute(null);

                if (!isTargetSelected)
                {
                    await SelectTargetGeometryAsync();
                }

                while (mapView.Extent != null)
                {
                    await GetNearestCoordAsync((bool)cboVertexOnly.IsChecked);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Load parcels from map service
        private async Task LoadParcelsAsync()
        {
            var queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));
            var query = new Query(mapView.Extent) { ReturnGeometry = true, OutSpatialReference = mapView.SpatialReference, OutFields = OutFields.All };
            var result = await queryTask.ExecuteAsync(query);

            _graphicsLayer.Graphics.Clear();
            _graphicsLayer.Graphics.AddRange(result.FeatureSet.Features);
        }

        // Accept user click point and select the underlying target polygon
        private async Task SelectTargetGeometryAsync()
        {
            txtInstruct.Text = "Click to select a target geometry";
            _coordinateLayer.Graphics.Clear();
            _targetLayer.Graphics.Clear();

            Graphic graphic = null;
            while (graphic == null)
            {
                var point = await mapView.Editor.RequestPointAsync();

                graphic = await _graphicsLayer.HitTestAsync(mapView, mapView.LocationToScreen(point));
                if (graphic == null)
                    continue;

                _targetLayer.Graphics.Add(graphic);

                var poly = graphic.Geometry as Polygon;
                foreach (var coord in poly.Parts.First())
                {
                    _targetLayer.Graphics.Add(new Graphic(new MapPointBuilder(coord).ToGeometry(), _vertexSymbol));
                }
            }
        }

        // Accept user click point and find nearest target geometry point
        private async Task GetNearestCoordAsync(bool vertexOnly)
        {
            var target = _targetLayer.Graphics.Select(g => g.Geometry).FirstOrDefault();
            if (target == null)
                return;

            txtInstruct.Text = "Click the map to find the nearest coordinate in the selected geometry";
            var point = await mapView.Editor.RequestPointAsync();

            ProximityResult result = null;
            if (vertexOnly)
                result = GeometryEngine.NearestVertexInGeometry(target, point);
            else
                result = GeometryEngine.NearestCoordinateInGeometry(target, point);

            _coordinateLayer.Graphics.Clear();
            _coordinateLayer.Graphics.Add(new Graphic(point, _userPointSymbol));
            _coordinateLayer.Graphics.Add(new Graphic(result.Point));

            txtResult.Visibility = Visibility.Visible;
            txtResult.Text = string.Format("Nearest Point: Index: {0}, Distance: {1:0.000}", result.PointIndex, result.Distance);
        }
    }
}
