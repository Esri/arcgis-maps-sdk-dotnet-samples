using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Controls;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates how to position map tips on graphics in a graphics layer. In this example, a random set of graphics is added to a graphics layer on the map and the MapView.MouseMove event is handled to hit test the graphics layer and position the map tip.
	/// </summary>
	/// <title>Map Tips</title>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class GraphicsMapTips : UserControl
	{
		private Random _random = new Random();
		private GraphicsLayer _graphicsLayer;
		private bool _isHitTesting;

		/// <summary>Construct Graphics Map Tips sample control</summary>
		public GraphicsMapTips()
		{
			InitializeComponent();

			_graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

			_isHitTesting = true;
			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
		}

		private void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			CreateGraphics();
			_isHitTesting = false;
		}

		// HitTest the graphics and position the map tip
		private async void MyMapView_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isHitTesting)
				return;

			try
			{
				_isHitTesting = true;

				System.Windows.Point screenPoint = e.GetPosition(MyMapView);
				var graphic = await _graphicsLayer.HitTestAsync(MyMapView, screenPoint);
				if (graphic != null)
				{
					mapTip.DataContext = graphic;
					mapTip.Visibility = System.Windows.Visibility.Visible;
				}
				else
					mapTip.Visibility = System.Windows.Visibility.Collapsed;
			}
			catch
			{
				mapTip.Visibility = System.Windows.Visibility.Collapsed;
			}
			finally
			{
				_isHitTesting = false;
			}
		}

		// Create three List<Graphic> objects with random graphics to serve as layer GraphicsSources
		private void CreateGraphics()
		{
			for (int n = 1; n <= 20; ++n)
			{
				_graphicsLayer.Graphics.Add(CreateRandomGraphic(n));
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
