using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates routing between stops and around user-defined barriers using the OnlineLocatorTask.
	/// </summary>
	/// <title>Routing Around Barriers</title>
	/// <category>Network Analyst Tasks</category>
	public sealed partial class RoutingWithBarriers : Page
	{
		private const string OnlineRoutingService = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route";

		private GraphicsOverlay _routeGraphicsOverlay;
		private GraphicsOverlay _stopGraphicsOverlay;
		private GraphicsOverlay _barrierGraphicsOverlay;

		private OnlineRouteTask _routeTask;
		private RouteParameters _routeParams;

		public RoutingWithBarriers()
		{
			InitializeComponent();

			_routeGraphicsOverlay = MyMapView.GraphicsOverlays["RouteGraphicsOverlay"];
			_stopGraphicsOverlay = MyMapView.GraphicsOverlays["StopGraphicsOverlay"];
			_barrierGraphicsOverlay = MyMapView.GraphicsOverlays["BarrierGraphicsOverlay"];

			SetupRouteTask();
		}

		private async void SetupRouteTask()
		{
			try
			{
				_routeTask = new OnlineRouteTask(new Uri(OnlineRoutingService));
				_routeParams = await _routeTask.GetDefaultParametersAsync();
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			_routeGraphicsOverlay.Graphics.Clear();
			_stopGraphicsOverlay.Graphics.Clear();
			_barrierGraphicsOverlay.Graphics.Clear();
		}

		private async void MyMapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			try
			{
				if (rbStops.IsChecked == true)
				{
					var graphicIdx = _stopGraphicsOverlay.Graphics.Count + 1;
					_stopGraphicsOverlay.Graphics.Add(CreateStopGraphic(e.Location, graphicIdx));
				}
				else
				{
					_barrierGraphicsOverlay.Graphics.Add(new Graphic(e.Location));
				}

				await SolveRoute();
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		public async Task SolveRoute()
		{
			if (_stopGraphicsOverlay.Graphics.Count < 2)
				return;

			try
			{
				progress.Visibility = Visibility.Visible;

				_routeParams.SetStops(_stopGraphicsOverlay.Graphics);
				_routeParams.SetPointBarriers(_barrierGraphicsOverlay.Graphics);
				_routeParams.OutSpatialReference = MyMapView.SpatialReference;

				RouteResult routeResult = await _routeTask.SolveAsync(_routeParams);

				if (routeResult.Routes.Count > 0)
				{
					_routeGraphicsOverlay.Graphics.Clear();

					var route = routeResult.Routes.First().RouteFeature;
					_routeGraphicsOverlay.Graphics.Add(new Graphic(route.Geometry));
				}
			}
			catch (AggregateException ex)
			{
				var message = ex.Message;
				var innermostExceptions = ex.Flatten().InnerExceptions;
				if (innermostExceptions != null && innermostExceptions.Count > 0)
					message = innermostExceptions[0].Message;

				var _x = new MessageDialog(message, "Sample Error").ShowAsync();
			}
			catch (System.Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				progress.Visibility = Visibility.Collapsed;
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
	}
}
