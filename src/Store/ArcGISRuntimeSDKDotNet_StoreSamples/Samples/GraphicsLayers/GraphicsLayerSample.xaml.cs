using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Windows.UI;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to add graphics to a GraphicsLayer using markup or code
    /// </summary>
    /// <title>Graphics Layer</title>
    /// <category>Graphics Layers</category>
    public sealed partial class GraphicsLayerSample : Windows.UI.Xaml.Controls.Page
    {
        private GraphicsLayer _graphicsLayer;

        public GraphicsLayerSample()
        {
            this.InitializeComponent();

            _graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
			AddPointGraphics();
            AddPolyLineGraphics();
        }

		private void AddPointGraphics()
		{
			var symbols = this.Resources.OfType<MarkerSymbol>();
			double x = -7000000;
			foreach(var symbol in symbols)
			{
				Graphic g = new Graphic(new MapPoint(x, 3900000), symbol);
				_graphicsLayer.Graphics.Add(g);
				x += 1000000;
			}

			_graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-7000000, 3900000), (Symbol)Resources["RedMarkerSymbolCircle"]));
			_graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-6000000, 3900000), (Symbol)Resources["RedMarkerSymbolCross"]));
			_graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-5000000, 3900000), (Symbol)Resources["RedMarkerSymbolDiamond"]));
			_graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-4000000, 3900000), (Symbol)Resources["RedMarkerSymbolSquare"]));
			_graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-3000000, 3900000), (Symbol)Resources["RedMarkerSymbolTriangle"]));
			_graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-2000000, 3900000), (Symbol)Resources["RedMarkerSymbolX"]));
		}

        private void AddPolyLineGraphics()
        {
            MapPoint ptStart = (MapPoint)_graphicsLayer.Graphics[0].Geometry;
            MapPoint ptEnd = (MapPoint)_graphicsLayer.Graphics[5].Geometry;

            // Solid Blue line above point graphics
            Graphic blueLine = new Graphic()
            {
                Symbol = new SimpleLineSymbol() { Color = Colors.Blue, Style = SimpleLineStyle.Solid, Width = 4 },
                Geometry = new Polyline(new PointCollection() 
                { 
                    new MapPoint(ptStart.X, ptStart.Y + 1000000),
                    new MapPoint(ptEnd.X, ptEnd.Y + 1000000)
                })
            };

            // Dashed Green line below point graphics
            Graphic greenLine = new Graphic()
            {
                Symbol = new SimpleLineSymbol() { Color = Colors.Green, Style = SimpleLineStyle.Dash, Width = 4 },
                Geometry = new Polyline(new PointCollection() 
                { 
                    new MapPoint(ptStart.X, ptStart.Y - 1000000),
                    new MapPoint(ptEnd.X, ptEnd.Y - 1000000)
                })
            };

            _graphicsLayer.Graphics.Add(blueLine);
            _graphicsLayer.Graphics.Add(greenLine);
        }
    }
}
