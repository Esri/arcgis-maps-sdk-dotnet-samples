using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates use of the GeometryEngine.GeodesicMove method to move a geometry be a specified distance.
    /// </summary>
    /// <title>Geodesic Move</title>
	/// <category>Geometry</category>
	public partial class GeodesicMove : Windows.UI.Xaml.Controls.Page
    {
        private Symbol _origSymbol;
        private GraphicsOverlay _originalGraphics;
        private GraphicsOverlay _movedGraphics;

        /// <summary>Construct Geodesic Move sample control</summary>
        public GeodesicMove()
        {
            InitializeComponent();

            _origSymbol = LayoutRoot.Resources["OriginalSymbol"] as Symbol;
			_originalGraphics = MyMapView.GraphicsOverlays[0];
			_movedGraphics = MyMapView.GraphicsOverlays[1];
                
            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        // Start map interaction once the mapview extent is set
        private async void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

            MyMapView.Editor.EditorConfiguration.MidVertexSymbol = null;
            MyMapView.Editor.EditorConfiguration.VertexSymbol = null;
            MyMapView.Editor.EditorConfiguration.SelectedVertexSymbol = new SimpleMarkerSymbol() { Color = Colors.Blue, Size = 6 };

            await AcceptUserPolygon();
        }

        // Digitize a new original polygon to move
        private async void DigitizeButton_Click(object sender, RoutedEventArgs e)
        {
            await AcceptUserPolygon();
        }

        // Get the polygon from the user
        private async Task AcceptUserPolygon()
        {
            try
            {
                _movedGraphics.Graphics.Clear();
                _originalGraphics.Graphics.Clear();

                var polygon = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon, _origSymbol);

                _originalGraphics.Graphics.Add(new Graphic(polygon));
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Move the polygon the specified distance and angle
        private void GeodesicMoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_originalGraphics.Graphics.Count == 0)
                    throw new Exception("Digitize a polygon to move.");

                var coords = _originalGraphics.Graphics[0].Geometry as Multipart;
                if (coords == null)
                    throw new Exception("Digitize a polygon to move.");

                var points = coords.Parts.First();
                var distance = (double)comboDistance.SelectedItem;
                var azimuth = (double)sliderAngle.Value;
                var movedPoints = GeometryEngine.GeodesicMove(points, distance, LinearUnits.Miles, azimuth);

                Polygon movedPoly = new PolygonBuilder(movedPoints, MyMapView.SpatialReference).ToGeometry();
                _movedGraphics.Graphics.Clear();
                _movedGraphics.Graphics.Add(new Graphic(movedPoly));
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
