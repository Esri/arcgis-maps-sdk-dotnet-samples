using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Collections.Generic;
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
        private GraphicsLayer _originalGraphics;
        private GraphicsLayer _movedGraphics;

        /// <summary>Construct Geodesic Move sample control</summary>
        public GeodesicMove()
        {
            InitializeComponent();

            _origSymbol = LayoutRoot.Resources["OriginalSymbol"] as Symbol;
            _originalGraphics = mapView.Map.Layers["OriginalGraphics"] as GraphicsLayer;
            _movedGraphics = mapView.Map.Layers["MovedGraphics"] as GraphicsLayer;
                
            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        // Start map interaction once the mapview extent is set
        private async void mapView_ExtentChanged(object sender, EventArgs e)
        {
            mapView.ExtentChanged -= mapView_ExtentChanged;

            mapView.Editor.EditorConfiguration.MidVertexSymbol = null;
            mapView.Editor.EditorConfiguration.VertexSymbol = null;
            mapView.Editor.EditorConfiguration.SelectedVertexSymbol = new SimpleMarkerSymbol() { Color = Colors.Blue, Size = 6 };

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

                var polygon = await mapView.Editor.RequestShapeAsync(DrawShape.Polygon, _origSymbol);

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

                var coords = _originalGraphics.Graphics[0].Geometry as IEnumerable<CoordinateCollection>;
                if (coords == null)
                    throw new Exception("Digitize a polygon to move.");

                var points = coords.First().Select(c => new MapPoint(c, mapView.SpatialReference));
                var distance = (double)comboDistance.SelectedItem;
                var azimuth = (double)sliderAngle.Value;
                var movedPoints = GeometryEngine.GeodesicMove(points, distance, LinearUnits.Miles, azimuth);

                Polygon movedPoly = new Polygon(movedPoints.Select(p => p.Coordinate), mapView.SpatialReference);
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
