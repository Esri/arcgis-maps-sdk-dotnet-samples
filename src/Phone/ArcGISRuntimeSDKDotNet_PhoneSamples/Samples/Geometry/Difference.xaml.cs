using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geometry</category>
	public sealed partial class Difference : Page
    {
        GraphicsLayer inputGraphicsLayer;
        GraphicsLayer outputGraphicsLayer;
        GraphicsLayer drawGraphicsLayer;
        Polygon inputDifferencePolygonGeometry;
        public Difference()
        {
            InitializeComponent();

			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-117.5, 32.5, -116.5, 35.5, SpatialReferences.Wgs84));
            inputGraphicsLayer = mapView1.Map.Layers["InputGraphicsLayer"] as GraphicsLayer;
            outputGraphicsLayer = mapView1.Map.Layers["OutputGraphicsLayer"] as GraphicsLayer;
            drawGraphicsLayer = mapView1.Map.Layers["DrawGraphicsLayer"] as GraphicsLayer;

            AddInputGraphics();

        }
        private void AddInputGraphics()
        {
            var g = new Graphic
            {
                Geometry = new Polygon(new List<MapPoint> {
                new MapPoint(-116.5,33),
                new MapPoint(-116.5,34),
                new MapPoint(-116,34),
                new MapPoint(-116,33)
            }, mapView1.SpatialReference),
            };

            inputGraphicsLayer.Graphics.Add(g);

            g = new Graphic
            {
                Geometry = new Polygon(new List<MapPoint> {
                new MapPoint(-118,34),
                new MapPoint(-118,35),
                new MapPoint(-117.5,35),
                new MapPoint(-117.5,34)
                
            }, mapView1.SpatialReference)
            };
            inputGraphicsLayer.Graphics.Add(g);

            g = new Graphic
            {
                Geometry = new Polygon(new List<MapPoint> {
                new MapPoint(-117.3,34),
                new MapPoint(-116.3,34),
                new MapPoint(-116.3,33.5),
                new MapPoint(-117.3,33.5)
                
            }, mapView1.SpatialReference)
            };
            inputGraphicsLayer.Graphics.Add(g);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {

            outputGraphicsLayer.Graphics.Clear();

            InstructionsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;

            InstructionsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            StartButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ResetButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

			// wait for user to draw difference polygon
			Polygon inputDifferencePolygonGeometry = await mapView1.Editor.RequestShapeAsync(DrawShape.Polygon) as Polygon;

			// Take account of WrapAround
			Polygon polygon = GeometryEngine.NormalizeCentralMeridian(inputDifferencePolygonGeometry) as Polygon;

			// Get the user input geometry and add it to the map
            drawGraphicsLayer.Graphics.Clear();
            drawGraphicsLayer.Graphics.Add(new Graphic { Geometry = inputDifferencePolygonGeometry });

			// Adjust user polygon for backward digitization
			var simplifyGeometry = GeometryEngine.Simplify(polygon);

            //Generate the difference geometries
            var inputGeometries1 = inputGraphicsLayer.Graphics.Select(x => x.Geometry).ToList();
            var inputGeometries2 = new List<Geometry> { simplifyGeometry };
            var differenceOutputGeometries = GeometryEngine.Difference(inputGeometries1, inputGeometries2);

            //Add the difference geometries to the amp
            foreach (var geom in differenceOutputGeometries)
            {
                outputGraphicsLayer.Graphics.Add(new Graphic { Geometry = geom });
            }

            ResetButton.IsEnabled = true;
        }



        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            drawGraphicsLayer.Graphics.Clear();
            outputGraphicsLayer.Graphics.Clear();
            InstructionsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            StartButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            ResetButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ResetButton.IsEnabled = false;
        }

    }
}
