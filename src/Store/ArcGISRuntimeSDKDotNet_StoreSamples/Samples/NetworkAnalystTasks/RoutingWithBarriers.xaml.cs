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

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Shows routing and re-routing between stops and around user defined barriers using the OnlineLocatorTask.
    /// </summary>
    /// <title>Routing Around Barriers</title>
    /// <category>Network Analyst Tasks</category>
    public partial class RoutingWithBarriers : Windows.UI.Xaml.Controls.Page
    {
        private const string OnlineRoutingService = "http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route";

        private GraphicsLayer _routeGraphicsLayer;
        private GraphicsLayer _stopGraphicsLayer;
        private GraphicsLayer _barrierGraphicsLayer;

        private OnlineRouteTask _routeTask;
        private RouteParameters _routeParams;

        public RoutingWithBarriers()
        {
            InitializeComponent();

            MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-13637527, 4545455, -13622931, 4552890, SpatialReferences.WebMercator));

            _routeGraphicsLayer = MyMapView.Map.Layers["RouteGraphicsLayer"] as GraphicsLayer;
            _stopGraphicsLayer = MyMapView.Map.Layers["StopGraphicsLayer"] as GraphicsLayer;
            _barrierGraphicsLayer = MyMapView.Map.Layers["BarrierGraphicsLayer"] as GraphicsLayer;

            var _ = SetupRouteTask();
        }

        private async Task SetupRouteTask()
        {
            _routeTask = new OnlineRouteTask(new Uri(OnlineRoutingService));
            if (_routeTask != null)
                _routeParams = await _routeTask.GetDefaultParametersAsync();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _routeGraphicsLayer.Graphics.Clear();
            _stopGraphicsLayer.Graphics.Clear();
            _barrierGraphicsLayer.Graphics.Clear();
        }

        private async void MyMapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            try
            {
                if (rbStops.IsChecked == true)
                {
                    var graphicIdx = _stopGraphicsLayer.Graphics.Count + 1;
                    _stopGraphicsLayer.Graphics.Add(CreateStopGraphic(e.Location, graphicIdx));
                }
                else
                {
                    _barrierGraphicsLayer.Graphics.Add(new Graphic(e.Location));
                }

                await SolveRoute();
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        public async Task SolveRoute()
        {
            if (_stopGraphicsLayer.Graphics.Count < 2)
                return;

            try
            {
                progress.Visibility = Visibility.Visible;

                _routeParams.Stops = new FeaturesAsFeature(_stopGraphicsLayer.Graphics);
                _routeParams.PointBarriers = new FeaturesAsFeature(_barrierGraphicsLayer.Graphics);
                _routeParams.OutSpatialReference = MyMapView.SpatialReference;

                RouteResult routeResult = await _routeTask.SolveAsync(_routeParams);

                if (routeResult.Routes.Count > 0)
                {
                    _routeGraphicsLayer.Graphics.Clear();

                    var route = routeResult.Routes.First().RouteFeature;
                    _routeGraphicsLayer.Graphics.Add(new Graphic(route.Geometry));
                }
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
