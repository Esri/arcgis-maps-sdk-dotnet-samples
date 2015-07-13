using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Globalization;
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates simple point to point routing between two input locations using the OnlineLocatorTask.
	/// </summary>
	/// <title>Routing</title>
	/// <category>Network Analyst Tasks</category>
	public sealed partial class Routing : Page
	{
		private GraphicsOverlay _routeGraphicsOverlay;
		private GraphicsOverlay _stopsGraphicsOverlay;

		public Routing()
		{
			this.InitializeComponent();
		}

		private async void MyMapView_Tapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			_routeGraphicsOverlay = MyMapView.GraphicsOverlays["RouteGraphicsOverlay"];
			_stopsGraphicsOverlay = MyMapView.GraphicsOverlays["StopsGraphicsOverlay"];

			var graphicIdx = _stopsGraphicsOverlay.Graphics.Count + 1;
			_stopsGraphicsOverlay.Graphics.Add(CreateStopGraphic(e.Location, graphicIdx));

			if (_stopsGraphicsOverlay.Graphics.Count > 1)
			{
				try
				{
					progress.Visibility = Visibility.Visible;

					var routeTask = new OnlineRouteTask(
						new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route"));
					var routeParams = await routeTask.GetDefaultParametersAsync();

					routeParams.SetStops(_stopsGraphicsOverlay.Graphics);
					routeParams.UseTimeWindows = false;
					routeParams.OutSpatialReference = MyMapView.SpatialReference;
					routeParams.DirectionsLanguage = new CultureInfo("en-Us"); // CultureInfo.CurrentCulture;

					var result = await routeTask.SolveAsync(routeParams);
					if (result.Routes.Count > 0)
					{
						_routeGraphicsOverlay.Graphics.Clear();

						var route = result.Routes.First().RouteFeature;
						_routeGraphicsOverlay.Graphics.Add(new Graphic(route.Geometry));

						var meters = GeometryEngine.GeodesicLength(route.Geometry, GeodeticCurveType.Geodesic);
						txtDistance.Text = string.Format("{0:0.00} miles", LinearUnits.Miles.ConvertFromMeters(meters));

						panelRouteInfo.Visibility = Visibility.Visible;
					}
				}
				catch (Exception ex)
				{
					var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
				}
				finally
				{
					progress.Visibility = Visibility.Collapsed;
				}
			}
		}

		private Graphic CreateStopGraphic(MapPoint location, int id)
		{
			var symbol = new CompositeSymbol();
			symbol.Symbols.Add(new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Circle, Color = Colors.Blue, Size = 16 });
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
				Geometry = location,
				Symbol = symbol
			};

			return graphic;
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_routeGraphicsOverlay.Graphics.Clear();
				_stopsGraphicsOverlay.Graphics.Clear();
				panelRouteInfo.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}
	}
}
