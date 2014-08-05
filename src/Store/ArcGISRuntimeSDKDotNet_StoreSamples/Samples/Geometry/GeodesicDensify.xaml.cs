using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates using GeometryEngine.GeodesicDensify to take an input shape and return a geodesic densified shape.
    /// </summary>
    /// <title>Geodesic Densify</title>
	/// <category>Geometry</category>
	public partial class GeodesicDensify : Windows.UI.Xaml.Controls.Page
    {
        private const double METERS_TO_MILES = 0.000621371192;
        private const double SQUARE_METERS_TO_MILES = 3.86102159e-7;

        private Symbol _lineSymbol;
        private Symbol _fillSymbol;
        private Symbol _origVertexSymbol;
        private Symbol _newVertexSymbol;
        private GraphicsLayer _inputGraphics;
        private GraphicsLayer _resultGraphics;

        /// <summary>Construct Densify sample control</summary>
        public GeodesicDensify()
        {
            InitializeComponent();

            _lineSymbol = LayoutRoot.Resources["LineSymbol"] as Symbol;
            _fillSymbol = LayoutRoot.Resources["FillSymbol"] as Symbol;
            _origVertexSymbol = LayoutRoot.Resources["OrigVertexSymbol"] as Symbol;
            _newVertexSymbol = LayoutRoot.Resources["NewVertexSymbol"] as Symbol;

            _inputGraphics = MyMapView.Map.Layers["InputGraphics"] as GraphicsLayer;
            _resultGraphics = MyMapView.Map.Layers["ResultGraphics"] as GraphicsLayer;
        }

        // Draw and densify a user defined polygon
        private async void DensifyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                resultsPanel.Visibility = Visibility.Collapsed;
                _inputGraphics.Graphics.Clear();
                _resultGraphics.Graphics.Clear();

                // Request polygon or polyline from the user
                DrawShape drawShape = (DrawShape)comboShapeType.SelectedValue;
                var original = await MyMapView.Editor.RequestShapeAsync(drawShape, _fillSymbol);

                // Add original shape vertices to input graphics layer
				var coordsOriginal = (original as Multipart).Parts.First();
                foreach (var coord in coordsOriginal)
                    _inputGraphics.Graphics.Add(new Graphic(new MapPointBuilder(coord).ToGeometry(), _origVertexSymbol));

                // Densify the shape
                var densify = GeometryEngine.GeodesicDensify(original, MyMapView.Extent.Width / 100, LinearUnits.Meters);
                _inputGraphics.Graphics.Add(new Graphic(densify, _fillSymbol));

                // Add new vertices to result graphics layer
				var coordsDensify = (densify as Multipart).Parts.First();
                foreach (var coord in coordsDensify)
                    _resultGraphics.Graphics.Add(new Graphic(new MapPointBuilder(coord).ToGeometry(), _newVertexSymbol));

                // Results
                var results = new List<Tuple<string, object>>()
                {
                    new Tuple<string, object>("Length", GeometryEngine.GeodesicLength(densify) * METERS_TO_MILES),
                    new Tuple<string, object>("Area", 
                        (original is Polygon) ? (GeometryEngine.GeodesicArea(densify) * SQUARE_METERS_TO_MILES).ToString("0.000") : "N/A"),
                    new Tuple<string, object>("Vertices Before", coordsOriginal.Count()),
                    new Tuple<string, object>("Vertices After", coordsDensify.Count())
                };

                resultsListView.ItemsSource = results;
                resultsPanel.Visibility = Visibility.Visible;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Densify Error: " + ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
