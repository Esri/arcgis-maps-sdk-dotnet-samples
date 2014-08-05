using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates offline routing with a network analyst LocalRouteTask.
    /// </summary>
    /// <title>Routing</title>
    /// <category>Offline</category>
    public partial class OfflineRouting : Windows.UI.Xaml.Controls.Page
    {
        private const string _localRoutingDatabase = @"networks\san-diego\san-diego-network.geodatabase";
        private const string _networkName = "Streets_ND";

        private LocalRouteTask _routeTask;
        private Symbol _directionPointSymbol;
        private GraphicsLayer _stopsLayer;
        private GraphicsLayer _routesLayer;
        private GraphicsLayer _directionsLayer;
        private bool _isMapReady;

        public OfflineRouting()
        {
            InitializeComponent();

            _isMapReady = false;
            _directionPointSymbol = LayoutRoot.Resources["directionPointSymbol"] as Symbol;
            _stopsLayer = MyMapView.Map.Layers["StopsLayer"] as GraphicsLayer;
            _routesLayer = MyMapView.Map.Layers["RoutesLayer"] as GraphicsLayer;
            _directionsLayer = MyMapView.Map.Layers["DirectionsLayer"] as GraphicsLayer;
                
            MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-13044000, 3855000, -13040000, 3858000, SpatialReferences.WebMercator));
            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        // Make sure the map is ready for user interaction
        private async void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

                await MyMapView.LayersLoadedAsync();
                _isMapReady = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        // Get user Stop points
        private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            if (!_isMapReady)
                return;

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

                _stopsLayer.Graphics.Add(new Graphic(e.Location));
            }
            catch (System.Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Calculate the route
        private async void MyMapView_MapViewDoubleTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            if (!_isMapReady || _stopsLayer.Graphics.Count() < 2)
                return;

            try
            {
                e.Handled = true;

                panelResults.Visibility = Visibility.Collapsed;
                progress.Visibility = Visibility.Visible;

                if (_routeTask == null)
                {
                    var path = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, _localRoutingDatabase);
                    _routeTask = await Task.Run<LocalRouteTask>(() => new LocalRouteTask(path, _networkName));
                }

                RouteParameters routeParams = await _routeTask.GetDefaultParametersAsync();
                routeParams.OutSpatialReference = MyMapView.SpatialReference;
                routeParams.ReturnDirections = true;
                routeParams.DirectionsLengthUnit = LinearUnits.Miles;
                routeParams.DirectionsLanguage = new CultureInfo("en-US");
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

                await MyMapView.SetViewAsync(route.RouteFeature.Geometry.Extent.Expand(1.2));
            }
            catch (AggregateException ex)
            {
                var innermostExceptions = ex.Flatten().InnerExceptions;
                if (innermostExceptions != null && innermostExceptions.Count > 0)
                {
                    var _ = new MessageDialog(innermostExceptions[0].Message, "Sample Error").ShowAsync();
                }
                else
                {
                    var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
                }
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
