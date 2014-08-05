using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates using GeometryEngine.Densify to take an input polygon and return a densified polygon.
    /// </summary>
    /// <title>Densify</title>
    /// <category>Geometry</category>
    public partial class Densify : Windows.UI.Xaml.Controls.Page
    {
        private Symbol _polySymbol;
        private Symbol _origVertexSymbol;
        private Symbol _newVertexSymbol;

        private GraphicsLayer _inputGraphics;
        private GraphicsLayer _resultGraphics;

        /// <summary>Construct Densify sample control</summary>
        public Densify()
        {
            InitializeComponent();

            _polySymbol = LayoutRoot.Resources["PolySymbol"] as Symbol;
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
                _inputGraphics.Graphics.Clear();
                _resultGraphics.Graphics.Clear();

                // Request polygon from the user
                var poly = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon, _polySymbol) as Polygon;

                // Add original polygon and vertices to input graphics layer
                _inputGraphics.Graphics.Add(new Graphic(poly, _polySymbol));
				foreach (var coord in poly.Parts.First())
                {
                    _inputGraphics.Graphics.Add(new Graphic(new MapPointBuilder(coord).ToGeometry(), _origVertexSymbol));
                }

                // Densify the polygon
                var densify = GeometryEngine.Densify(poly, MyMapView.Extent.Width / 100) as Polygon;

                // Add new vertices to result graphics layer
				foreach (var coord in densify.Parts.First())
                {
                    _resultGraphics.Graphics.Add(new Graphic(new MapPointBuilder(coord).ToGeometry(), _newVertexSymbol));
                }
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Densify Error: " + ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
