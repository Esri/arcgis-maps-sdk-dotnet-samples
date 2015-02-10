using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates simple point to point routing between two input locations with either OnlineLocatorTask or LocalLocatorTask.
    /// </summary>
    /// <title>Routing</title>
    /// <category>Network Analyst Tasks</category>
    public partial class Routing : Windows.UI.Xaml.Controls.Page
    {
        private const string OnlineRoutingService = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route";
        private const string LocalRoutingDatabase = @"networks\san-diego\san-diego-network.geodatabase";
        private const string LocalNetworkName = "Streets_ND";

        private GraphicsOverlay _extentGraphicsOverlay;
        private GraphicsOverlay _routeGraphicsOverlay;
        private GraphicsOverlay _stopsGraphicsOverlay;
        private RouteTask _routeTask;

        public Routing()
        {
            InitializeComponent();

			_extentGraphicsOverlay = MyMapView.GraphicsOverlays["ExtentGraphicsOverlay"];
			_routeGraphicsOverlay = MyMapView.GraphicsOverlays["RouteGraphicsOverlay"];
			_stopsGraphicsOverlay = MyMapView.GraphicsOverlays["StopsGraphicsOverlay"];

            var extent = new Envelope(-117.2595, 32.5345, -116.9004, 32.8005, SpatialReferences.Wgs84);
            _extentGraphicsOverlay.Graphics.Add(new Graphic(GeometryEngine.Project(extent, SpatialReferences.WebMercator)));

            _routeTask = new OnlineRouteTask(new Uri(OnlineRoutingService));
        }

        private async void MyMapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            try
            {
                var graphicIdx = _stopsGraphicsOverlay.Graphics.Count + 1;
                _stopsGraphicsOverlay.Graphics.Add(CreateStopGraphic(e.Location, graphicIdx));

                if (graphicIdx > 1)
                {
                    await CalculateRoute();
                }
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void chkOnline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _routeGraphicsOverlay.Graphics.Clear();
                _stopsGraphicsOverlay.Graphics.Clear();
                panelRouteInfo.Visibility = Visibility.Collapsed;

                if (((Windows.UI.Xaml.Controls.CheckBox)sender).IsChecked == true)
                    _routeTask = await Task.Run<RouteTask>(() => new OnlineRouteTask(new Uri(OnlineRoutingService)));
                else
                {
                    try
                    {
                        var path = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, LocalRoutingDatabase);
                        _routeTask = await Task.Run<LocalRouteTask>(() => new LocalRouteTask(path, LocalNetworkName));
                    }
                    catch
                    {
                        chkOnline.IsChecked = true;
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
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

        private async Task CalculateRoute()
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                RouteParameters routeParams = await _routeTask.GetDefaultParametersAsync();

                routeParams.OutSpatialReference = MyMapView.SpatialReference;
                routeParams.ReturnDirections = false;

				routeParams.SetStops(_stopsGraphicsOverlay.Graphics);

                RouteResult routeResult = await _routeTask.SolveAsync(routeParams);

                if (routeResult.Routes.Count > 0)
                {
                    _routeGraphicsOverlay.Graphics.Clear();

                    var route = routeResult.Routes.First().RouteFeature;
                    _routeGraphicsOverlay.Graphics.Add(new Graphic(route.Geometry));

                    var meters = GeometryEngine.GeodesicLength(route.Geometry, GeodeticCurveType.Geodesic);
                    txtDistance.Text = string.Format("{0:0.00} miles", LinearUnits.Miles.ConvertFromMeters(meters));

                    panelRouteInfo.Visibility = Visibility.Visible;
                }
            }
            catch (AggregateException ex)
            {
                var innermostExceptions = ex.Flatten().InnerExceptions;
                if (innermostExceptions != null && innermostExceptions.Count > 0)
                {
                    var _x = new MessageDialog(innermostExceptions[0].Message, "Sample Error").ShowAsync();
                }
                else
                {
                    var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
                }
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
    }
}
