using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates using GeometryEngine.Densify to take an input polygon and return a densified polygon.
	/// </summary>
	/// <title>Densify</title>
	/// <category>Geometry</category>
	public sealed partial class DensifyPolygons : Page
	{
		GraphicsLayer graphicsLayerVertices;
		GraphicsLayer graphicsLayerPolygon;
		Polygon densifyPolygonGeometry;
		SimpleFillSymbol polygonSymbol;
		Esri.ArcGISRuntime.Symbology.Symbol defaultVertexMarkerSymbol;
		Esri.ArcGISRuntime.Symbology.Symbol densifyVertexMarkerSymbol;

		public DensifyPolygons()
		{
			InitializeComponent();

			polygonSymbol = LayoutRoot.Resources["MySimpleFillSymbol"] as SimpleFillSymbol;
			graphicsLayerVertices = MyMapView.Map.Layers["MyVerticesGraphicsLayer"] as GraphicsLayer;
			graphicsLayerPolygon = MyMapView.Map.Layers["MyPolygonGraphicsLayer"] as GraphicsLayer;
			defaultVertexMarkerSymbol = LayoutRoot.Resources["MyDefaultVertexMarkerSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol;
			densifyVertexMarkerSymbol = LayoutRoot.Resources["MyDensifyVertexMarkerSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol;
		}

		private async void StartButton_Click(object sender, RoutedEventArgs e)
		{
			SetupUI();

			// InstructionsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
			StartButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

			//Get the user's input
			densifyPolygonGeometry = (await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon)) as Polygon;

			//Create a graphic and add it to the PolygonGraphicsLayer
			var thePolygonGraphic = new Graphic
			{
				Geometry = densifyPolygonGeometry,
				Symbol = polygonSymbol
			};
			graphicsLayerPolygon.Graphics.Add(thePolygonGraphic);

			//show the vertices for the polygon
			foreach (var vert in densifyPolygonGeometry.Parts.FirstOrDefault().GetPoints())
			{
				var graphic = new Graphic { Geometry = new MapPoint(vert.X, vert.Y), Symbol = defaultVertexMarkerSymbol };
				graphicsLayerVertices.Graphics.Add(graphic);
			}

			DensifyPolygonsButton.IsEnabled = true;
		}

		private void DensifyPolygonsButton_Click(object sender, RoutedEventArgs e)
		{
			//hide/show ui elements
			try
			{
				var maxSegLength = GetMaxSegmentLength();
				var results = GeometryEngine.Densify(densifyPolygonGeometry, maxSegLength);
				//show the vertices for the polygon
				foreach (var vert in (results as Polygon).Parts.FirstOrDefault().GetPoints())
				{
					var graphic = new Graphic { Geometry = new MapPoint(vert.X, vert.Y), Symbol = densifyVertexMarkerSymbol };
					graphicsLayerVertices.Graphics.Add(graphic);
				}
			}
			catch (Exception ex)
			{
				var dlg = new MessageDialog(ex.Message, "Geometry Engine Failed!");
				var _x = dlg.ShowAsync();
			}

			//Hide/show ui elements
			ResetUI();
		}


		private double GetMaxSegmentLength()
		{
			//return MyMapView.Scale / 96 * 10;

			var tiledLayer = MyMapView.Map.Layers["StreetMapLayer"] as ArcGISTiledMapServiceLayer;
			//find the resolution of the LoD whose scale is the next highest than the map's current scale
			var resolution = tiledLayer.TileInfo.Lods.OrderBy(x => x.Scale).FirstOrDefault(x => x.Scale >= MyMapView.Scale).Resolution;
			return resolution * 10;

		}


		private void ResetUI()
		{
			DensifyPolygonsButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			DensifyPolygonsButton.IsEnabled = false;
			StartButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

		}

		private void SetupUI()
		{
			graphicsLayerVertices.Graphics.Clear();
			graphicsLayerPolygon.Graphics.Clear();

			DensifyPolygonsButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
			DensifyPolygonsButton.IsEnabled = false;
		}

	}
}
