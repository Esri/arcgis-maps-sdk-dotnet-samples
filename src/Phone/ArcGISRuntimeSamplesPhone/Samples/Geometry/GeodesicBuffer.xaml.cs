using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates use of the GeometryEngine to calculate a geodesic buffer.
	/// </summary>
	/// <title>Geodesic Buffer</title>
	/// <category>Geometry</category>
	public sealed partial class GeodesicBuffer : Page
    {
        public GeodesicBuffer()
        {
            this.InitializeComponent();
			mapView.Map.InitialViewpoint = new Viewpoint(new Envelope(
				-13047918, 
				4036008, 
				-13045480, 
				4037866, 
				SpatialReferences.WebMercator));

	        DrawShapes.ItemsSource = new DrawShape[]
            {
                DrawShape.Point, DrawShape.Polygon, DrawShape.Polyline
            };
            DrawShapes.SelectedIndex = 0;
        }

        private async void GenerateGeodesicBuffer_Click(object sender, RoutedEventArgs e)
        {
             string message = null;
            try
            {
                
                GraphicsLayer inputGraphicsLayer = null;
                var drawShape = (DrawShape)DrawShapes.SelectedItem;
                inputGraphicsLayer = drawShape == DrawShape.Point ? mapView.Map.Layers["PointInputLayer"] as GraphicsLayer :
					((drawShape == DrawShape.Polyline) ? mapView.Map.Layers["LineInputLayer"] as GraphicsLayer : mapView.Map.Layers["PolygonInputLayer"] as GraphicsLayer);

                if (inputGraphicsLayer.Graphics.Count == 0)
                    throw new Exception("No input shape. Please draw shape to generate buffer");
                Esri.ArcGISRuntime.Geometry.Geometry geom = inputGraphicsLayer.Graphics.FirstOrDefault().Geometry;

                if (geom != null)
                {
                    string json = geom.ToJson();
                    var buffer = GeometryEngine.GeodesicBuffer(geom, 10, LinearUnits.Meters);
                    if (buffer != null)
                    {
						GraphicsLayer resultGraphicsLayer = mapView.Map.Layers["GeometryResultGraphicsLayer"] as GraphicsLayer;
                        resultGraphicsLayer.Graphics.Add(new Graphic() { Geometry = buffer });
						mapView.SetView(resultGraphicsLayer.Graphics.First().Geometry.Extent);
                    }
                }
            }
            catch (Exception ex)
            {

                message = ex.Message;
            }
             if (message != null)
                await new MessageDialog(message).ShowAsync();
        }



        private async void Draw_Click(object sender, RoutedEventArgs e)
        {
            string message = null;

			var editor = mapView.Editor;
            editor.EditorConfiguration = new EditorConfiguration()
            {
                AllowAddVertex = true,
                VertexSymbol =
                    new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Diamond, Color = Colors.Yellow, Size = 15 }
            };
            try
            {
                GraphicsLayer inputGraphicsLayer = null;
                var drawShape = (DrawShape)DrawShapes.SelectedItem;

				inputGraphicsLayer = drawShape == DrawShape.Point ? mapView.Map.Layers["PointInputLayer"] as GraphicsLayer :
					((drawShape == DrawShape.Polyline) ? mapView.Map.Layers["LineInputLayer"] as GraphicsLayer : mapView.Map.Layers["PolygonInputLayer"] as GraphicsLayer);

                var r = await editor.RequestShapeAsync(drawShape, null);
                inputGraphicsLayer.Graphics.Add(new Graphic() { Geometry = r });

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (message != null)
                await new MessageDialog(message).ShowAsync();
        }

        private void OnClearButtonClicked(object sender, RoutedEventArgs e)
        {
			IEnumerable<Layer> graphicLayers = mapView.Map.Layers.Where(l => l is GraphicsLayer);
            foreach (Layer l in graphicLayers)
                (l as GraphicsLayer).Graphics.Clear();

        }
    }
}
