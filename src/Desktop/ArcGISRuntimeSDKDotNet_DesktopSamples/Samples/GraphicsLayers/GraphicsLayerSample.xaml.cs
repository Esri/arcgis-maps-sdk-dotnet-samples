using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates how to add a GraphicsOverlay with Graphics and Symbols to the map in XAML.  The sample also shows how to add Polyline graphics to a GraphicsOverlay from the code-behind.
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
				graphicsOverlay.Graphics.Add(g);
				x += 1000000;
			}

			graphicsOverlay.Graphics.Add(new Graphic(new MapPoint(-7000000, 3900000), (Symbol)Resources["RedMarkerSymbolCircle"]));
			graphicsOverlay.Graphics.Add(new Graphic(new MapPoint(-6000000, 3900000), (Symbol)Resources["RedMarkerSymbolCross"]));
			graphicsOverlay.Graphics.Add(new Graphic(new MapPoint(-5000000, 3900000), (Symbol)Resources["RedMarkerSymbolDiamond"]));
			graphicsOverlay.Graphics.Add(new Graphic(new MapPoint(-4000000, 3900000), (Symbol)Resources["RedMarkerSymbolSquare"]));
			graphicsOverlay.Graphics.Add(new Graphic(new MapPoint(-3000000, 3900000), (Symbol)Resources["RedMarkerSymbolTriangle"]));
			graphicsOverlay.Graphics.Add(new Graphic(new MapPoint(-2000000, 3900000), (Symbol)Resources["RedMarkerSymbolX"]));
		}

		private void AddPolyLineGraphics()
		{
			MapPoint ptStart = (MapPoint)graphicsOverlay.Graphics[0].Geometry;
			MapPoint ptEnd = (MapPoint)graphicsOverlay.Graphics[5].Geometry;

			var blueLineGeometry = new Polyline(
				new List<MapPoint> 
				{
					new MapPoint(ptStart.X, ptStart.Y + 1000000),
					new MapPoint(ptEnd.X, ptEnd.Y + 1000000)
				}, 
				ptStart.SpatialReference);
				
			// Solid Blue line above point graphics
			Graphic blueLine = new Graphic()
			{
				Symbol = new SimpleLineSymbol() { Color = Colors.Blue, Style = SimpleLineStyle.Solid, Width = 4 },
				Geometry = blueLineGeometry
			};

			var greenLineGeometry = new Polyline(
				new List<MapPoint> 
				{
					new MapPoint(ptStart.X, ptStart.Y - 1000000),
					new MapPoint(ptEnd.X, ptEnd.Y - 1000000)
				},
				ptStart.SpatialReference);

			// Dashed Green line below point graphics
			Graphic greenLine = new Graphic()
			{
				Symbol = new SimpleLineSymbol() { Color = Colors.Green, Style = SimpleLineStyle.Dash, Width = 4 },
				Geometry = greenLineGeometry
			};

			graphicsOverlay.Graphics.Add(blueLine);
			graphicsOverlay.Graphics.Add(greenLine);
		}
    }
}
