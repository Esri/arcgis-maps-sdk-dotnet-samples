using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Windows.Controls;
using System.Windows.Media;

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
            AddPolyLineGraphics();
        }

        private void AddPolyLineGraphics()
        {
            MapPoint ptStart = (MapPoint)graphicsLayer.Graphics[0].Geometry;
            MapPoint ptEnd = (MapPoint)graphicsLayer.Graphics[5].Geometry;

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

            graphicsLayer.Graphics.Add(blueLine);
            graphicsLayer.Graphics.Add(greenLine);
        }
    }
}
