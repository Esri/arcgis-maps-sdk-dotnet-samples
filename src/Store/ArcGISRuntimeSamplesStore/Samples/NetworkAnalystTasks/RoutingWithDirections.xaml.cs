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

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates retrieving a route and driving directions between input locations with OnlineLocatorTask.
    /// </summary>
    /// <title>Routing with Directions</title>
    /// <category>Network Analyst Tasks</category>
    public partial class RoutingWithDirections : Windows.UI.Xaml.Controls.Page
    {
		private const string OnlineRoutingService = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route";

        private OnlineRouteTask _routeTask;
        private Symbol _directionPointSymbol;
        private GraphicsOverlay _stopsOverlay;
        private GraphicsOverlay _routesOverlay;
        private GraphicsOverlay _directionsOverlay;

        public RoutingWithDirections()
        {
            InitializeComponent();
      
            _directionPointSymbol = LayoutRoot.Resources["directionPointSymbol"] as Symbol;
			_stopsOverlay = MyMapView.GraphicsOverlays["StopsOverlay"];
			_routesOverlay = MyMapView.GraphicsOverlays["RoutesOverlay"];
			_directionsOverlay = MyMapView.GraphicsOverlays["DirectionsOverlay"];

            _routeTask = new OnlineRouteTask(new Uri(OnlineRoutingService));
        }

        // Get user Stop points
        private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                e.Handled = true;

                if (_directionsOverlay.Graphics.Count() > 0)
                {
                    panelResults.Visibility = Visibility.Collapsed;

                    _stopsOverlay.Graphics.Clear();
                    _routesOverlay.Graphics.Clear();
                    _directionsOverlay.GraphicsSource = null;
                }

                var graphicIdx = _stopsOverlay.Graphics.Count + 1;
                _stopsOverlay.Graphics.Add(CreateStopGraphic(e.Location, graphicIdx));
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Calculate the route
        private async void MyMapView_MapViewDoubleTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            if (_stopsOverlay.Graphics.Count() < 2)
                return;

            try
            {
                e.Handled = true;

                panelResults.Visibility = Visibility.Collapsed;
                progress.Visibility = Visibility.Visible;

                RouteParameters routeParams = await _routeTask.GetDefaultParametersAsync();
                routeParams.OutSpatialReference = MyMapView.SpatialReference;
                routeParams.ReturnDirections = true;
                routeParams.DirectionsLengthUnit = LinearUnits.Miles;
				routeParams.DirectionsLanguage = new CultureInfo("en-Us"); // CultureInfo.CurrentCulture;
                routeParams.SetStops(_stopsOverlay.Graphics);

                var routeResult = await _routeTask.SolveAsync(routeParams);
                if (routeResult == null || routeResult.Routes == null || routeResult.Routes.Count() == 0)
                    throw new Exception("No route could be calculated");

                var route = routeResult.Routes.First();
                _routesOverlay.Graphics.Add(new Graphic(route.RouteFeature.Geometry));

                _directionsOverlay.GraphicsSource = route.RouteDirections.Select(rd => GraphicFromRouteDirection(rd));

                var totalTime = route.RouteDirections.Select(rd => rd.Time).Aggregate(TimeSpan.Zero, (p, v) => p.Add(v));
                var totalLength = route.RouteDirections.Select(rd => rd.GetLength(LinearUnits.Miles)).Sum();
                txtRouteTotals.Text = string.Format("Time: {0:h':'mm':'ss} / Length: {1:0.00} mi", totalTime, totalLength);

                await MyMapView.SetViewAsync(route.RouteFeature.Geometry.Extent.Expand(1.25));
            }
            catch (AggregateException ex)
            {
                var message = ex.Message;
                var innermostExceptions = ex.Flatten().InnerExceptions;
                if (innermostExceptions != null && innermostExceptions.Count > 0)
                    message = innermostExceptions[0].Message;
                
                var _x = new MessageDialog(message, "Sample Error").ShowAsync();
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
                if (_directionsOverlay.Graphics.Count() > 0)
                    panelResults.Visibility = Visibility.Visible;
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

        private Graphic GraphicFromRouteDirection(RouteDirection rd)
        {
            var graphic = new Graphic(rd.Geometry);
            graphic.Attributes.Add("Direction", rd.Text);
            graphic.Attributes.Add("Time", string.Format("{0:h\\:mm\\:ss}", rd.Time));
            graphic.Attributes.Add("Length", string.Format("{0:0.00}", rd.GetLength(LinearUnits.Miles)));
            if (rd.Geometry is MapPoint)
                graphic.Symbol = _directionPointSymbol;

            return graphic;
        }

        private void listDirections_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            _directionsOverlay.ClearSelection();

            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                var graphic = e.AddedItems[0] as Graphic;
                graphic.IsSelected = true;
            }
        }
    }
}
