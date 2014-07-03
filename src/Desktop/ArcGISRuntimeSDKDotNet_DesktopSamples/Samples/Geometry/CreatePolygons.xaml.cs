using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to create polygon geometries, attach them to graphics and display them on the map. Polygon geometry objects are used to store geographic polygons.
    /// </summary>
    /// <title>Create Polygons</title>
	/// <category>Geometry</category>
	public partial class CreatePolygons : UserControl
    {
        /// <summary>Construct Create Polygons sample control</summary>
        public CreatePolygons()
        {
            InitializeComponent();

            var task = CreatePolygonGraphics();
        }

        // Create Polygon graphics on the map in the center and the center of four equal quadrants
        private async Task CreatePolygonGraphics()
        {
            await mapView.LayersLoadedAsync();

            var height = mapView.Extent.Height / 4;
            var width = mapView.Extent.Width / 4;
            var length = width / 4;
            var center = mapView.Extent.GetCenter();
            var topLeft = new MapPointBuilder(center.X - width, center.Y + height, mapView.SpatialReference).ToGeometry();
			var topRight = new MapPointBuilder(center.X + width, center.Y + height, mapView.SpatialReference).ToGeometry();
			var bottomLeft = new MapPointBuilder(center.X - width, center.Y - height, mapView.SpatialReference).ToGeometry();
			var bottomRight = new MapPointBuilder(center.X + width, center.Y - height, mapView.SpatialReference).ToGeometry();

            var redSymbol = new SimpleFillSymbol() { Color = System.Windows.Media.Colors.Red };
            var blueSymbol = new SimpleFillSymbol() { Color = System.Windows.Media.Colors.Blue };

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

            PointCollection points = new PointCollection();
			points.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());
			points.Add(new MapPointBuilder(center.X + halfLen, center.Y + halfLen).ToGeometry());
			points.Add(new MapPointBuilder(center.X + halfLen, center.Y - halfLen).ToGeometry());
			points.Add(new MapPointBuilder(center.X - halfLen, center.Y - halfLen).ToGeometry());
			points.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());

            halfLen /= 3;
			PointCollection pointsHole = new PointCollection();
			pointsHole.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());
			pointsHole.Add(new MapPointBuilder(center.X - halfLen, center.Y - halfLen).ToGeometry());
			pointsHole.Add(new MapPointBuilder(center.X + halfLen, center.Y - halfLen).ToGeometry());
			pointsHole.Add(new MapPointBuilder(center.X + halfLen, center.Y + halfLen).ToGeometry());
			pointsHole.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());

			return new Polygon(new List<PointCollection> { points, pointsHole }, mapView.SpatialReference);
        }
    }
}
