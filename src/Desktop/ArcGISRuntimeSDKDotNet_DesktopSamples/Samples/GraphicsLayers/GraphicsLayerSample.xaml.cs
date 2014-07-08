using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates how to add a GraphicsLayer with Graphics and Symbols to the map in XAML.  The sample also shows how to add Polyline graphics to a GraphicsLayer from the code-behind.
    /// </summary>
    /// <title>Graphics Layer</title>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class GraphicsLayerSample : UserControl
    {
        /// <summary>Construct graphics layer sample control</summary>
        public GraphicsLayerSample()
        {
            InitializeComponent();
			AddPointGraphics();
			AddPolyLineGraphics();
        }

		private void AddPointGraphics()
		{
			var symbols = this.Resources.OfType<MarkerSymbol>();
			double x = -7000000;
			foreach (var symbol in symbols)
			{
				Graphic g = new Graphic(new MapPoint(x, 3900000), symbol);
				graphicsLayer.Graphics.Add(g);
				x += 1000000;
			}

			graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-7000000, 3900000), (Symbol)Resources["RedMarkerSymbolCircle"]));
			graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-6000000, 3900000), (Symbol)Resources["RedMarkerSymbolCross"]));
			graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-5000000, 3900000), (Symbol)Resources["RedMarkerSymbolDiamond"]));
			graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-4000000, 3900000), (Symbol)Resources["RedMarkerSymbolSquare"]));
			graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-3000000, 3900000), (Symbol)Resources["RedMarkerSymbolTriangle"]));
			graphicsLayer.Graphics.Add(new Graphic(new MapPoint(-2000000, 3900000), (Symbol)Resources["RedMarkerSymbolX"]));
		}

		private void AddPolyLineGraphics()
		{
			MapPoint ptStart = (MapPoint)graphicsLayer.Graphics[0].Geometry;
			MapPoint ptEnd = (MapPoint)graphicsLayer.Graphics[5].Geometry;

			var blueLineBuilder = new PolylineBuilder();
			blueLineBuilder.AddPoint(ptStart.X, ptStart.Y + 1000000);
			blueLineBuilder.AddPoint(ptEnd.X, ptEnd.Y + 1000000);

			// Solid Blue line above point graphics
			Graphic blueLine = new Graphic()
			{
				Symbol = new SimpleLineSymbol() { Color = Colors.Blue, Style = SimpleLineStyle.Solid, Width = 4 },
				Geometry = blueLineBuilder.ToGeometry()
			};

			var greenLineBuilder = new PolylineBuilder();
			greenLineBuilder.AddPoint(ptStart.X, ptStart.Y - 1000000);
			greenLineBuilder.AddPoint(ptEnd.X, ptEnd.Y - 1000000);

			// Dashed Green line below point graphics
			Graphic greenLine = new Graphic()
			{
				Symbol = new SimpleLineSymbol() { Color = Colors.Green, Style = SimpleLineStyle.Dash, Width = 4 },
				Geometry = greenLineBuilder.ToGeometry()
			};

			graphicsLayer.Graphics.Add(blueLine);
			graphicsLayer.Graphics.Add(greenLine);
		}
    }
}
