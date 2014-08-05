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

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates retrieving a route and driving directions between input locations with OnlineLocatorTask.
    /// </summary>
    /// <title>Routing with Directions</title>
    /// <category>Network Analyst Tasks</category>
    public partial class RoutingWithDirections : Windows.UI.Xaml.Controls.Page
    {
        private const string OnlineRoutingService = "http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route";

        private OnlineRouteTask _routeTask;
        private Symbol _directionPointSymbol;
        private GraphicsLayer _stopsLayer;
        private GraphicsLayer _routesLayer;
        private GraphicsLayer _directionsLayer;

        public RoutingWithDirections()
        {
            InitializeComponent();

            MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-7917189, 5211428, -7902815, 5218750, SpatialReferences.WebMercator));

            _directionPointSymbol = LayoutRoot.Resources["directionPointSymbol"] as Symbol;
            _stopsLayer = MyMapView.Map.Layers["StopsLayer"] as GraphicsLayer;
            _routesLayer = MyMapView.Map.Layers["RoutesLayer"] as GraphicsLayer;
            _directionsLayer = MyMapView.Map.Layers["DirectionsLayer"] as GraphicsLayer;

            _routeTask = new OnlineRouteTask(new Uri(OnlineRoutingService));
        }

        // Get user Stop points
        private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                e.Handled = true;

                if (_directionsLayer.Graphics.Count() > 0)
                {
                    panelResults.Visibility = Visibility.Collapsed;

                    _stopsLayer.Graphics.Clear();
                    _routesLayer.Graphics.Clear();
                    _directionsLayer.GraphicsSource = null;
                }

                var graphicIdx = _stopsLayer.Graphics.Count + 1;
                _stopsLayer.Graphics.Add(CreateStopGraphic(e.Location, graphicIdx));
            }
            catch (System.Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Calculate the route
        private async void MyMapView_MapViewDoubleTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            if (_stopsLayer.Graphics.Count() < 2)
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
                routeParams.DirectionsLanguage = CultureInfo.CurrentCulture;
                routeParams.Stops = new FeaturesAsFeature(_stopsLayer.Graphics) { SpatialReference = MyMapView.SpatialReference };

                var routeResult = await _routeTask.SolveAsync(routeParams);
                if (routeResult == null || routeResult.Routes == null || routeResult.Routes.Count() == 0)
                    throw new Exception("No route could be calculated");

                var route = routeResult.Routes.First();
                _routesLayer.Graphics.Add(new Graphic(route.RouteFeature.Geometry));

                _directionsLayer.GraphicsSource = route.RouteDirections.Select(rd => GraphicFromRouteDirection(rd));

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
                
                var _ = new MessageDialog(message, "Sample Error").ShowAsync();
            }
            catch (System.Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
                if (_directionsLayer.Graphics.Count() > 0)
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
            _directionsLayer.ClearSelection();

            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                var graphic = e.AddedItems[0] as Graphic;
                graphic.IsSelected = true;
            }
        }
    }
}
