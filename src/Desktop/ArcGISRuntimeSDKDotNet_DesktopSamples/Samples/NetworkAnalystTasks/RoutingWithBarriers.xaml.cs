using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Shows routing and re-routing around user defined point, polyline, and polygon barriers.
    /// </summary>
	/// <category>Tasks</category>
    /// <subcategory>Network Analyst</subcategory>
	public partial class RoutingWithBarriers : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region IsOnline Property

        private bool _isOnline = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is online.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is online; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnline
        {
            get
            {
                return _isOnline;
            }
            set
            {
                _isOnline = value;

                SetupRouteTask();

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsOnline"));
                }
            }
        }

        #endregion

        #region IsMapReady Property

        private bool _isMapReady = true;

        /// <summary>
        /// Gets or sets a value indicating whether the map is initialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if the map is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsMapReady
        {
            get
            {
                return _isMapReady;
            }
            set
            {
                _isMapReady = !value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsMapReady"));
                }
            }
        }

        #endregion

        GraphicsLayer _routeGraphicsLayer;
        GraphicsLayer _stopsGraphicsLayer;
        GraphicsLayer _pointBarriersGraphicsLayer;
        GraphicsLayer _polylineBarriersGraphicsLayer;
        GraphicsLayer _polygonBarriersGraphicsLayer;

        RouteTask _routeTask;
        RouteParameters _routeParams;

        string _onlineRoutingService = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route";
        string _localRoutingDatabase = @"..\..\..\..\..\samples-data\networks\san-diego\san-diego-network.geodatabase";
        string _networkName = "Streets_ND";

        private async Task SetupRouteTask()
        {
            if (!IsOnline)
            {
                _routeTask = new LocalRouteTask(_localRoutingDatabase, _networkName);
            }
            else
            {
                _routeTask = new OnlineRouteTask(new Uri(_onlineRoutingService));
            }

            if (_routeTask != null)
                _routeParams = await _routeTask.GetDefaultParametersAsync();
        }

        public RoutingWithBarriers()
        {
            InitializeComponent();

            // Min X,Y = -13,044,000  3,855,000 Meters
            // Max X,Y = -13,040,000  3,858,000 Meters
            map1.InitialExtent = new Envelope(-13044000, 3855000, -13040000, 3858000, new SpatialReference(102100));

            DataContext = this;

            var _ = SetupRouteTask();
            CreateGraphicsLayers();

            HandleLayersInitialized();
        }

        private async void HandleLayersInitialized()
        {
            try
            {
                await mapView1.LayersLoadedAsync();
                IsMapReady = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

          public void CreateGraphicsLayers()
        {
            _routeGraphicsLayer = new GraphicsLayer();
            _routeGraphicsLayer.Renderer = new SimpleRenderer()
            {
                Symbol = new SimpleLineSymbol()
                {
                    Color = Colors.Blue,
                    Style = SimpleLineStyle.Dot,
                    Width = 2
                }
            };
            map1.Layers.Add(_routeGraphicsLayer);

            _stopsGraphicsLayer = new GraphicsLayer();
            _stopsGraphicsLayer.Renderer = new SimpleRenderer()
            {
                Symbol = new SimpleMarkerSymbol()
                {
                    Color = Colors.Green,
                    Size = 12,
                    Style = SimpleMarkerStyle.Circle
                }
            };
            map1.Layers.Add(_stopsGraphicsLayer);

            _pointBarriersGraphicsLayer = new GraphicsLayer();
            _pointBarriersGraphicsLayer.Renderer = new SimpleRenderer()
            {
                Symbol = new SimpleMarkerSymbol()
                {
                    Color = Colors.Red,
                    Size = 12,
                    Style = SimpleMarkerStyle.Square
                }
            };
            map1.Layers.Add(_pointBarriersGraphicsLayer);

            _polylineBarriersGraphicsLayer = new GraphicsLayer();
            _polylineBarriersGraphicsLayer.Renderer = new SimpleRenderer()
            {
                Symbol = new SimpleLineSymbol()
                {
                    Color = Colors.Red,
                    Style = SimpleLineStyle.Solid,
                    Width = 3
                }
            };
            map1.Layers.Add(_polylineBarriersGraphicsLayer);

            _polygonBarriersGraphicsLayer = new GraphicsLayer();
            _polygonBarriersGraphicsLayer.Renderer = new SimpleRenderer()
            {
                Symbol = new SimpleFillSymbol()
                {
                    Color = Colors.Red,
                    Outline = new SimpleLineSymbol()
                    {
                        Color = Colors.Red,
                        Style = SimpleLineStyle.Solid,
                        Width = 2
                    },
                    Style = SimpleFillStyle.DiagonalCross,
                }
            };
            map1.Layers.Add(_polygonBarriersGraphicsLayer);
        }

        public async Task SolveRoute()
        {
            if (_stopsGraphicsLayer.Graphics.Count > 1)
            {
                try
                {
                    _routeParams.ReturnPointBarriers = true;
                    _routeParams.ReturnPolylineBarriers = true;
                    _routeParams.ReturnPolygonBarriers = true;
                    _routeParams.ReturnStops = true;
                    _routeParams.ReturnZ = true;

                    _routeParams.Stops = new FeaturesAsFeature(_stopsGraphicsLayer.Graphics);
                    _routeParams.PointBarriers = new FeaturesAsFeature(_pointBarriersGraphicsLayer.Graphics);
                    _routeParams.PolylineBarriers = new FeaturesAsFeature(_polylineBarriersGraphicsLayer.Graphics);
                    _routeParams.PolygonBarriers = new FeaturesAsFeature(_polygonBarriersGraphicsLayer.Graphics);

                    _routeParams.OutSpatialReference = mapView1.SpatialReference;
                    _routeParams.DirectionsLengthUnit = LinearUnits.Kilometers;
                    
                    RouteResult routeResult = await _routeTask.SolveAsync(_routeParams);
                    if (routeResult.Routes.Count > 0)
                    {
                        _routeGraphicsLayer.Graphics.Clear();
                        Graphic graphic = new Graphic(routeResult.Routes[0].RouteGraphic.Geometry);
                        _routeGraphicsLayer.Graphics.Add(graphic);
                    }
                }
                catch (AggregateException ex)
                {
                    var innermostExceptions = ex.Flatten().InnerExceptions;
                    if (innermostExceptions != null && innermostExceptions.Count > 0)
                        MessageBox.Show(innermostExceptions[0].Message);
                    else
                        MessageBox.Show(ex.Message);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (Layer layer in map1.Layers)
                if (layer is GraphicsLayer)
                    (layer as GraphicsLayer).Graphics.Clear();
        }

        private async void CommandBinding_AddStopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                MapPoint mapPoint;
                mapPoint = await mapView1.Editor.RequestPointAsync();
                _stopsGraphicsLayer.Graphics.Add(new Graphic(mapPoint));

                await SolveRoute();
            }
			catch (TaskCanceledException) { return; }
			catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private async void CommandBinding_AddPointBarrierExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                MapPoint mapPoint;
                mapPoint = await mapView1.Editor.RequestPointAsync();
                _pointBarriersGraphicsLayer.Graphics.Add(new Graphic(mapPoint));

                await SolveRoute();
            }
			catch (TaskCanceledException) { return; }
			catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private async void CommandBinding_AddPolylineBarrierExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                //Polyline polyLine;
                //polyLine = await map1.Editor.RequestShape(Esri.ArcGISRuntime.Controls.DrawShape.Polyline) as Polyline;
                //_polylineBarriersGraphicsLayer.Graphics.Add(new Graphic(polyLine));

                Polyline polyLine = new Polyline();
                // Assign Spatial Reference (no SR conveyed from Coordinates to PolyLine)
                polyLine.SpatialReference = mapView1.SpatialReference;
                CoordinateCollection coords = new CoordinateCollection();
                
                // 1st Point:
                MapPoint mapPoint;
                mapPoint = await mapView1.Editor.RequestPointAsync();
                coords.Add(mapPoint.Coordinate);
                Graphic g1 = new Graphic(mapPoint);
                _pointBarriersGraphicsLayer.Graphics.Add(g1);

                // 2nd Point:
                mapPoint = await mapView1.Editor.RequestPointAsync();
                coords.Add(mapPoint.Coordinate);           
                Graphic g2 = new Graphic(mapPoint);
                _pointBarriersGraphicsLayer.Graphics.Add(g2); 
                // Return to UI thread to allow 2nd graphic to appear.
                await Task.Delay(100);

                polyLine.Paths.Add(coords);
                _polylineBarriersGraphicsLayer.Graphics.Add(new Graphic(polyLine));
                _pointBarriersGraphicsLayer.Graphics.Remove(g1);
                _pointBarriersGraphicsLayer.Graphics.Remove(g2);

                await SolveRoute();
            }
			catch (TaskCanceledException) { return; }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private async void CommandBinding_AddPolygonBarrierExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                //Polygon polygon;
                //polygon = await map1.Editor.RequestShape(Esri.ArcGISRuntime.Controls.DrawShape.Polygon) as Polygon;
                //_polygonBarriersGraphicsLayer.Graphics.Add(new Graphic(polygon));

                // Polygon Barrier:
                Polygon polygon = new Polygon();
                // Assign Spatial Reference (no SR conveyed from Coordinates to PolyLine)
                polygon.SpatialReference = mapView1.SpatialReference;
                CoordinateCollection coords = new CoordinateCollection();

                // 1st Point:
                MapPoint mapPoint;
                mapPoint = await mapView1.Editor.RequestPointAsync();
                coords.Add(mapPoint.Coordinate);
                Graphic g1 = new Graphic(mapPoint);
                _pointBarriersGraphicsLayer.Graphics.Add(g1);

                // 2nd Point:
                mapPoint = await mapView1.Editor.RequestPointAsync();
                coords.Add(mapPoint.Coordinate);
                Graphic g2 = new Graphic(mapPoint);
                _pointBarriersGraphicsLayer.Graphics.Add(g2);
                // Return to UI thread to allow 2nd graphic to appear.
                await Task.Delay(100);

                // 3rd Point:
                mapPoint = await mapView1.Editor.RequestPointAsync();
                coords.Add(mapPoint.Coordinate);
                Graphic g3 = new Graphic(mapPoint);
                _pointBarriersGraphicsLayer.Graphics.Add(g3);
                // Return to UI thread to allow 2nd graphic to appear.
                await Task.Delay(100);

                polygon.Rings.Add(coords);
                _polygonBarriersGraphicsLayer.Graphics.Add(new Graphic(polygon));
                _pointBarriersGraphicsLayer.Graphics.Remove(g1);
                _pointBarriersGraphicsLayer.Graphics.Remove(g2);
                _pointBarriersGraphicsLayer.Graphics.Remove(g3);

                await SolveRoute();
            }
			catch (TaskCanceledException) { return; }
			catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }

    public static class NetworkCommands
    {
        public static readonly RoutedUICommand AddStopCommand = new RoutedUICommand("Add Stop", "AddStopCommand", typeof(RoutingWithBarriers));
        public static readonly RoutedUICommand AddPointBarrierCommand = new RoutedUICommand("Add Point Barrier", "AddPointBarrierCommand", typeof(RoutingWithBarriers));
        public static readonly RoutedUICommand AddPolylineBarrierCommand = new RoutedUICommand("Add Polyline Barrier", "AddPolylineBarrierCommand", typeof(RoutingWithBarriers));
        public static readonly RoutedUICommand AddPolygonBarrierCommand = new RoutedUICommand("Add Polygon Barrier", "AddPolygonBarrierCommand", typeof(RoutingWithBarriers));
    }
    
}
