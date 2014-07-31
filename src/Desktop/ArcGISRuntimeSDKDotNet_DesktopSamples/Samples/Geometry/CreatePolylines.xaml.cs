using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to create polyline geometries, attach them to graphics and display them on the map. Polyline geometry objects are used to store geographic lines.
    /// </summary>
    /// <title>Create Polylines</title>
	/// <category>Geometry</category>
	public partial class CreatePolylines : UserControl
    {
        /// <summary>Construct Create Polylines sample control</summary>
        public CreatePolylines()
        {
            InitializeComponent();

            var task = CreatePolylineGraphics();
        }

        // Create polyline graphics on the map in the center and the center of four equal quadrants
        private async Task CreatePolylineGraphics()
        {
            await MyMapView.LayersLoadedAsync();

            var height = MyMapView.Extent.Height / 4;
            var width = MyMapView.Extent.Width / 4;
            var length = width / 4;
            var center = MyMapView.Extent.GetCenter();
			var topLeft = new MapPointBuilder(center.X - width, center.Y + height, MyMapView.SpatialReference).ToGeometry();
			var topRight = new MapPointBuilder(center.X + width, center.Y + height, MyMapView.SpatialReference).ToGeometry();
			var bottomLeft = new MapPointBuilder(center.X - width, center.Y - height, MyMapView.SpatialReference).ToGeometry();
			var bottomRight = new MapPointBuilder(center.X + width, center.Y - height, MyMapView.SpatialReference).ToGeometry();

            var redSymbol = new SimpleLineSymbol() { Color = System.Windows.Media.Colors.Red, Width = 4, Style = SimpleLineStyle.Solid };
            var blueSymbol = new SimpleLineSymbol() { Color = System.Windows.Media.Colors.Blue, Width = 4, Style = SimpleLineStyle.Solid };

            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(center, length), Symbol = blueSymbol });
            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(topLeft, length), Symbol = redSymbol });
            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(topRight, length), Symbol = redSymbol });
            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(bottomLeft, length), Symbol = redSymbol });
            graphicsLayer.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(bottomRight, length), Symbol = redSymbol });
        }

        // Creates a polyline with two paths in the shape of an 'X' centered at the given point
        private Polyline CreatePolylineX(MapPoint center, double length)
        {
            var halfLen = length / 2.0;

            PointCollection coords1 = new PointCollection();
            coords1.Add(new MapPointBuilder(center.X - halfLen, center.Y + halfLen).ToGeometry());
			coords1.Add(new MapPointBuilder(center.X + halfLen, center.Y - halfLen).ToGeometry());

            PointCollection coords2 = new PointCollection();
			coords2.Add(new MapPointBuilder(center.X + halfLen, center.Y + halfLen).ToGeometry());
			coords2.Add(new MapPointBuilder(center.X - halfLen, center.Y - halfLen).ToGeometry());

            return new Polyline(new PartCollection { coords1, coords2 }, MyMapView.SpatialReference);
        }
    }
}
