using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to hit test a graphics layer using the GraphicsLayer.HitTestAsync method.
	/// </summary>
	/// <title>Hit Testing</title>
	/// <category>Graphics Layers</category>
	public sealed partial class GraphicsHitTesting : Page
	{
		private const int MAX_GRAPHICS = 50;

		private Random _random = new Random();
		private GraphicsLayer _graphicsLayer;

		public GraphicsHitTesting()
		{
			this.InitializeComponent();

			_graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

			MyMapView.MapViewTapped += MyMapView_MapViewTapped;
			CreateGraphics();
		}

		// Hit Test the graphics layer by single point
		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			try
			{
				var graphics = await _graphicsLayer.HitTestAsync(MyMapView, e.Position, MAX_GRAPHICS);

				string results = "Hit: ";
				if (graphics == null || graphics.Count() == 0)
					results += "None";
				else
					results += string.Join(", ", graphics.Select(g => g.Attributes["ID"].ToString()).ToArray());
				txtResults.Text = results;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("HitTest Error: " + ex.Message, "Graphics Layer Hit Testing").ShowAsync();
			}
		}

		// Create three List<Graphic> objects with random graphics to serve as layer GraphicsSources
		private async void CreateGraphics()
		{
			try
			{
				await MyMapView.LayersLoadedAsync();

				for (int n = 1; n <= MAX_GRAPHICS; ++n)
					_graphicsLayer.Graphics.Add(CreateRandomGraphic(n));
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.ToString(), "Sample error").ShowAsync();
			}
		}

		// Create a random graphic
		private Graphic CreateRandomGraphic(int id)
		{
			var symbol = new CompositeSymbol();
			symbol.Symbols.Add(new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Circle, Color = Colors.Red, Size = 16 });
			symbol.Symbols.Add(new TextSymbol()
			{
				Text = id.ToString(),
				Color = Colors.White,
				VerticalTextAlignment = VerticalTextAlignment.Middle,
				HorizontalTextAlignment = HorizontalTextAlignment.Center,
				YOffset = -1
			});

			var graphic = new Graphic()
			{
				Geometry = GetRandomMapPoint(),
				Symbol = symbol
			};

			graphic.Attributes["ID"] = id;

			return graphic;
		}

		// Utility: Generate a random MapPoint within the current extent
		private MapPoint GetRandomMapPoint()
		{
            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

            double x = viewpointExtent.XMin + (_random.NextDouble() * viewpointExtent.Width);
            double y = viewpointExtent.YMin + (_random.NextDouble() * viewpointExtent.Height);
            return new MapPoint(x, y, MyMapView.SpatialReference);
		}
	}
}
