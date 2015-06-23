using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample shows how to hit test a graphics layer using the GraphicsOverlay.HitTestAsync method. Here, the user may sketch a point on the map to initiate the hit testing process - the results of the hit test are then displayed in the UI.
	/// </summary>
	/// <title>Hit Testing</title>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class GraphicsHitTesting : UserControl
	{
		private const int MAX_GRAPHICS = 50;

		private Random _random = new Random();
		private GraphicsLayer _graphicsLayer;

		/// <summary>Construct Graphics Hit Testing sample control</summary>
		public GraphicsHitTesting()
		{
			InitializeComponent();

			_graphicsLayer = MyMapView.Map.Layers["graphicsLayer"] as GraphicsLayer;
			MyMapView.ExtentChanged += MyMapView_ExtentChanged;
		}

		private async void MyMapView_ExtentChanged(object sender, EventArgs e)
		{
			try
			{
				MyMapView.ExtentChanged -= MyMapView_ExtentChanged;
				await CreateGraphics();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Sample Exception");
			}
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
				MessageBox.Show("HitTest Error: " + ex.Message, "Graphics Layer Hit Testing");
			}
		}

		// Create three List<Graphic> objects with random graphics to serve as layer GraphicsSources
		private async Task CreateGraphics()
		{
			try
			{
				await MyMapView.LayersLoadedAsync();

				for (int n = 1; n <= MAX_GRAPHICS; ++n)
					_graphicsLayer.Graphics.Add(CreateRandomGraphic(n));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occurred : " + ex.ToString(), "Sample error");
			}
		}

		// Create a random graphic
		private Graphic CreateRandomGraphic(int id)
		{
			var symbol = new CompositeSymbol();
			symbol.Symbols.Add(new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Circle, Color = Colors.Red, Size = 17 });
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
