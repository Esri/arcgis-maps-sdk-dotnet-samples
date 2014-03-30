using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

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
            await mapView.LayersLoadedAsync();

            var height = mapView.Extent.Height / 4;
            var width = mapView.Extent.Width / 4;
            var length = width / 4;
            var center = mapView.Extent.GetCenter();
            var topLeft = new MapPoint(center.X - width, center.Y + height, mapView.SpatialReference);
            var topRight = new MapPoint(center.X + width, center.Y + height, mapView.SpatialReference);
            var bottomLeft = new MapPoint(center.X - width, center.Y - height, mapView.SpatialReference);
            var bottomRight = new MapPoint(center.X + width, center.Y - height, mapView.SpatialReference);

            var redSymbol = new SimpleLineSymbol() { Color = Colors.Red, Width = 4, Style = SimpleLineStyle.Solid };
            var blueSymbol = new SimpleLineSymbol() { Color = Colors.Blue, Width = 4, Style = SimpleLineStyle.Solid };

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

            CoordinateCollection coords1 = new CoordinateCollection();
            coords1.Add(new Coordinate(center.X - halfLen, center.Y + halfLen));
            coords1.Add(new Coordinate(center.X + halfLen, center.Y - halfLen));

            CoordinateCollection coords2 = new CoordinateCollection();
            coords2.Add(new Coordinate(center.X + halfLen, center.Y + halfLen));
            coords2.Add(new Coordinate(center.X - halfLen, center.Y - halfLen));

            return new Polyline(new List<CoordinateCollection> { coords1, coords2 }, mapView.SpatialReference);
        }
    }
}
