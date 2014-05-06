using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to add graphics to a GraphicsLayer using markup or code
    /// </summary>
    /// <title>Graphics Layer</title>
    /// <category>Graphics Layers</category>
    public sealed partial class GraphicsLayerSample : Page
    {
        private GraphicsLayer _graphicsLayer;

        public GraphicsLayerSample()
        {
            this.InitializeComponent();

            _graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

            AddPolyLineGraphics();
        }

        private void AddPolyLineGraphics()
        {
            MapPoint ptStart = (MapPoint)_graphicsLayer.Graphics[0].Geometry;
            MapPoint ptEnd = (MapPoint)_graphicsLayer.Graphics[5].Geometry;

            // Solid Blue line above point graphics
            Graphic blueLine = new Graphic()
            {
                Symbol = new SimpleLineSymbol() { Color = Colors.Blue, Style = SimpleLineStyle.Solid, Width = 4 },
                Geometry = new Polyline(new CoordinateCollection() 
                { 
                    new Coordinate(ptStart.X, ptStart.Y + 1000000),
                    new Coordinate(ptEnd.X, ptEnd.Y + 1000000)
                })
            };

            // Dashed Green line below point graphics
            Graphic greenLine = new Graphic()
            {
                Symbol = new SimpleLineSymbol() { Color = Colors.Green, Style = SimpleLineStyle.Dash, Width = 4 },
                Geometry = new Polyline(new CoordinateCollection() 
                { 
                    new Coordinate(ptStart.X, ptStart.Y - 1000000),
                    new Coordinate(ptEnd.X, ptEnd.Y - 1000000)
                })
            };

            _graphicsLayer.Graphics.Add(blueLine);
            _graphicsLayer.Graphics.Add(greenLine);
        }
    }
}
