using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Controls;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to position map tips on graphics in a graphics layer.
	/// </summary>
	/// <title>Map Tips</title>
	/// <category>Graphics Layers</category>
	public sealed partial class GraphicsMapTips : Page
	{
		private const int MAX_GRAPHICS = 50;

		private Random _random = new Random();
		private GraphicsLayer _graphicsLayer;
		private FrameworkElement _mapTip;
		private bool _isMapReady;

		public GraphicsMapTips()
		{
			this.InitializeComponent();

			_graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
			_mapTip = MyMapView.Overlays.Items[0] as FrameworkElement;

			MyMapView.PointerMoved += MyMapView_PointerMoved;

			_isMapReady = false;
			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
		}

		private void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			CreateGraphics();
			_isMapReady = true;
		}

		// HitTest the graphics and position the map tip
		private async void MyMapView_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isMapReady)
				return;

			try
			{
				_isMapReady = false;

				Point screenPoint = e.GetCurrentPoint(MyMapView).Position;
				var graphic = await _graphicsLayer.HitTestAsync(MyMapView, screenPoint);
				if (graphic != null)
				{
					_mapTip.DataContext = graphic;
					_mapTip.Visibility = Visibility.Visible;
				}
				else
					_mapTip.Visibility = Visibility.Collapsed;
			}
			catch
			{
				_mapTip.Visibility = Visibility.Collapsed;
			}
			finally
			{
				_isMapReady = true;
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
				var _x = new MessageDialog("Error occurred " + ex.ToString(), "Sample error").ShowAsync();
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
