using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to create polygon geometries, attach them to graphics and display them on the map.
    /// Polygon geometry objects are used to store geographic polygons.
    /// </summary>
    /// <title>Create Polygons</title>
	/// <category>Geometry</category>
	public partial class CreatePolygons : Page
    {
        private GraphicsLayer graphicsLayer;

        /// <summary>Construct Create Polygons sample control</summary>
        public CreatePolygons()
        {
            InitializeComponent();

            graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
            var task = CreatePolygonGraphics();
        }

        // Create Polygon graphics on the map in the center and the center of four equal quadrants
        private async Task CreatePolygonGraphics()
        {
            await MyMapView.LayersLoadedAsync();

            var height = MyMapView.Extent.Height / 4;
            var width = MyMapView.Extent.Width / 4;
            var length = width / 4;
            var center = MyMapView.Extent.GetCenter();
            var topLeft = new MapPoint(center.X - width, center.Y + height, MyMapView.SpatialReference);
            var topRight = new MapPoint(center.X + width, center.Y + height, MyMapView.SpatialReference);
            var bottomLeft = new MapPoint(center.X - width, center.Y - height, MyMapView.SpatialReference);
            var bottomRight = new MapPoint(center.X + width, center.Y - height, MyMapView.SpatialReference);

            var redSymbol = new SimpleFillSymbol() { Color = Colors.Red };
            var blueSymbol = new SimpleFillSymbol() { Color = Colors.Blue };

            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(center, length), Symbol = blueSymbol });
            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(topLeft, length), Symbol = redSymbol });
            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(topRight, length), Symbol = redSymbol });
            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(bottomLeft, length), Symbol = redSymbol });
            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(bottomRight, length), Symbol = redSymbol });
        }

        // Creates a square polygon with a hole centered at the given point
        private Polygon CreatePolygonBox(MapPoint center, double length)
        {
            var halfLen = length / 2.0;

            PointCollection coords = new PointCollection();
			coords.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());
			coords.Add(new MapPointBuilder(center.X + halfLen, center.Y + halfLen).ToGeometry());
			coords.Add(new MapPointBuilder(center.X + halfLen, center.Y - halfLen).ToGeometry());
			coords.Add(new MapPointBuilder(center.X - halfLen, center.Y - halfLen).ToGeometry());
			coords.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());

            halfLen /= 3;
            PointCollection coordsHole = new PointCollection();
			coordsHole.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());
			coordsHole.Add(new MapPointBuilder(center.X - halfLen, center.Y - halfLen).ToGeometry());
			coordsHole.Add(new MapPointBuilder(center.X + halfLen, center.Y - halfLen).ToGeometry());
			coordsHole.Add(new MapPointBuilder(center.X + halfLen, center.Y + halfLen).ToGeometry());
			coordsHole.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());

            return new Polygon(new List<PointCollection> { coords, coordsHole }, MyMapView.SpatialReference);
        }
    }
}
