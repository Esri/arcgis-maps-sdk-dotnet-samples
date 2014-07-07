using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates offline routing with a network analyst LocalRouteTask.
    /// </summary>
    /// <title>Routing</title>
    /// <category>Offline</category>
    public partial class OfflineRouting : UserControl
    {
        private const string _localRoutingDatabase = @"..\..\..\..\..\samples-data\networks\san-diego\san-diego-network.geodatabase";
        private const string _networkName = "Streets_ND";

        private LocalRouteTask _routeTask;
        private Symbol _directionPointSymbol;
        private bool _isMapReady;

        public OfflineRouting()
        {
            InitializeComponent();

            _isMapReady = false;
            _directionPointSymbol = layoutGrid.Resources["directionPointSymbol"] as Symbol;
            mapView.Map.InitialViewpoint = new Envelope(-13044000, 3855000, -13040000, 3858000, SpatialReferences.WebMercator);
            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        // Make sure the map is ready for user interaction
        private async void mapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                mapView.ExtentChanged -= mapView_ExtentChanged;

                await mapView.LayersLoadedAsync();
                _isMapReady = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        // Get user Stop points
        private void mapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            if (!_isMapReady)
                return;

            try
            {
                e.Handled = true;

                if (directionsLayer.Graphics.Count() > 0)
                {
                    panelResults.Visibility = Visibility.Collapsed;

                    stopsLayer.Graphics.Clear();
                    routesLayer.Graphics.Clear();
                    directionsLayer.GraphicsSource = null;
                }

                stopsLayer.Graphics.Add(new Graphic(e.Location));
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        // Calculate the route
        private async void mapView_MapViewDoubleTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            if (!_isMapReady || stopsLayer.Graphics.Count() < 2)
                return;

            try
            {
                e.Handled = true;

                panelResults.Visibility = Visibility.Collapsed;
                progress.Visibility = Visibility.Visible;

                if (_routeTask == null)
                    _routeTask = await Task.Run<LocalRouteTask>(() => new LocalRouteTask(_localRoutingDatabase, _networkName));

                RouteParameters routeParams = await _routeTask.GetDefaultParametersAsync();
                routeParams.OutSpatialReference = mapView.SpatialReference;
                routeParams.ReturnDirections = true;
                routeParams.DirectionsLengthUnit = LinearUnits.Miles;
                routeParams.DirectionsLanguage = new CultureInfo("en");
                routeParams.Stops = new FeaturesAsFeature(stopsLayer.Graphics) { SpatialReference = mapView.SpatialReference };

                var routeResult = await _routeTask.SolveAsync(routeParams);
                if (routeResult == null || routeResult.Routes == null || routeResult.Routes.Count() == 0)
                    throw new ApplicationException("No route could be calculated");

                var route = routeResult.Routes.First();
                routesLayer.Graphics.Add(new Graphic(route.RouteGraphic.Geometry));

                directionsLayer.GraphicsSource = route.RouteDirections.Select(rd => GraphicFromRouteDirection(rd));

                var totalTime = route.RouteDirections.Select(rd => rd.Time).Aggregate(TimeSpan.Zero, (p, v) => p.Add(v));
                var totalLength = route.RouteDirections.Select(rd => rd.GetLength(LinearUnits.Miles)).Sum();
                txtRouteTotals.Text = string.Format("Time: {0:h':'mm':'ss} / Length: {1:0.00} mi", totalTime, totalLength);

                await mapView.SetViewAsync(route.RouteGraphic.Geometry.Extent.Expand(1.2));
            }
            catch (AggregateException ex)
            {
                var innermostExceptions = ex.Flatten().InnerExceptions;
                if (innermostExceptions != null && innermostExceptions.Count > 0)
                    MessageBox.Show(innermostExceptions[0].Message, "Sample Error");
                else
                    MessageBox.Show(ex.Message, "Sample Error");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
                if (directionsLayer.Graphics.Count() > 0)
                    panelResults.Visibility = Visibility.Visible;
            }
        }

        private Graphic GraphicFromRouteDirection(RouteDirection rd)
        {
            var graphic = new Graphic(rd.Geometry);
            graphic.Attributes.Add("Direction", rd);
            graphic.Attributes["Length"] = rd.GetLength(LinearUnits.Miles);
            if (rd.Geometry is MapPoint)
                graphic.Symbol = _directionPointSymbol;

            return graphic;
        }
    }
}
