using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to find the nearest polygon point or vertex of a geometry using the NearestCoordinateInGeometry or NearestVertexInGeometry methods of the GeometryEngine class. Here, the user selects a target polygon graphic and then clicks points on the map to show the nearest point or vertex on the target polygon.
    /// </summary>
    /// <title>Nearest Coordinate in Geometry</title>
	/// <category>Geometry</category>
	public partial class NearestCoordinateInGeometry : UserControl
    {
        private SimpleMarkerSymbol _vertexSymbol;
        private SimpleMarkerSymbol _userPointSymbol;

        /// <summary>Construct Nearest Coordinate sample control</summary>
        public NearestCoordinateInGeometry()
        {
            InitializeComponent();

            _vertexSymbol = new SimpleMarkerSymbol() { Color = Colors.LightGreen, Size = 8, Style = SimpleMarkerStyle.Circle };
            _userPointSymbol = new SimpleMarkerSymbol() { Color = Colors.Black, Size = 10, Style = SimpleMarkerStyle.Circle };

			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-83.3188396, 42.6142831, -83.3129566, 42.6167091, SpatialReferences.Wgs84));
			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;

            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        private async void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

                await LoadParcelsAsync();
                await MyMapView.LayersLoadedAsync();

                layoutGrid.IsEnabled = true;
                await ProcessUserPointsAsync(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading parcels: " + ex.Message, "Nearest Coordinate Sample");
            }
        }

        private async void SelectTargetButton_Click(object sender, RoutedEventArgs e)
        {
            coordinateLayer.Graphics.Clear();
            await ProcessUserPointsAsync(false);
        }

        private async void cboVertexOnly_Click(object sender, RoutedEventArgs e)
        {
            coordinateLayer.Graphics.Clear();
            await ProcessUserPointsAsync(targetLayer.Graphics.Count > 0);
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

                while (MyMapView.Extent != null)
                {
                    await GetNearestCoordAsync((bool)cboVertexOnly.IsChecked);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Nearest Coordinate Sample");
            }
        }

        // Load parcels from map service
        private async Task LoadParcelsAsync()
        {
            var queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));
            var query = new Query(MyMapView.Extent) { ReturnGeometry = true, OutSpatialReference = MyMapView.SpatialReference, OutFields = OutFields.All };
            var result = await queryTask.ExecuteAsync(query);

            graphicsLayer.Graphics.Clear();
            graphicsLayer.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
        }

        // Accept user click point and select the underlying target polygon
        private async Task SelectTargetGeometryAsync()
        {
            txtInstruct.Text = "Click to select a target geometry";
            coordinateLayer.Graphics.Clear();
            targetLayer.Graphics.Clear();

            Graphic graphic = null;
            while (graphic == null)
            {
                var point = await MyMapView.Editor.RequestPointAsync();

                graphic = await graphicsLayer.HitTestAsync(MyMapView, MyMapView.LocationToScreen(point));
                if (graphic == null)
                    continue;

                targetLayer.Graphics.Add(graphic);

                var poly = graphic.Geometry as Polygon;
                foreach (var coord in poly.Parts.First())
                {
					targetLayer.Graphics.Add(new Graphic(new MapPointBuilder(coord).ToGeometry(), _vertexSymbol));
                }
            }
        }

        // Accept user click point and find nearest target geometry point
        private async Task GetNearestCoordAsync(bool vertexOnly)
        {
            var target = targetLayer.Graphics.Select(g => g.Geometry).FirstOrDefault();
            if (target == null)
                return;

            txtInstruct.Text = "Click the map to find the nearest coordinate in the selected geometry";
            var point = await MyMapView.Editor.RequestPointAsync();

            ProximityResult result = null;
            if (vertexOnly)
                result = GeometryEngine.NearestVertexInGeometry(target, point);
            else
                result = GeometryEngine.NearestCoordinateInGeometry(target, point);

            coordinateLayer.Graphics.Clear();
            coordinateLayer.Graphics.Add(new Graphic(point, _userPointSymbol));
            coordinateLayer.Graphics.Add(new Graphic(result.Point));

            txtResult.Visibility = Visibility.Visible;
            txtResult.Text = string.Format("Nearest Point: Index: {0}, Distance: {1:0.000}", result.PointIndex, result.Distance);
        }
    }
}
