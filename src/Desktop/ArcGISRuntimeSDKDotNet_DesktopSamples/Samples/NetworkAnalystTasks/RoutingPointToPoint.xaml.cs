using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates simple point to point routing between two input locations.
    /// </summary>
	/// <category>Tasks</category>
	/// <subcategory>Network Analyst</subcategory>
	public partial class RoutingPointToPoint : UserControl, INotifyPropertyChanged
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

        private bool _isMapReady = false;

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
                _isMapReady = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsMapReady"));
                }
            }
        }

        #endregion

        #region Routing Start and End Location Properties
        private string _startLocationText = "";
        /// <summary>
        /// Gets or sets the start location text.
        /// </summary>
        /// <value>
        /// The start location text.
        /// </value>
        public string StartLocationText
        {
            get
            {
                return _startLocationText;
            }
            set
            {
                _startLocationText = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StartLocationText"));
                }
            }
        }

        private string _endLocationText = "";

        /// <summary>
        /// Gets or sets the end location text.
        /// </summary>
        /// <value>
        /// The end location text.
        /// </value>
        public string EndLocationText
        {
            get
            {
                return _endLocationText;
            }
            set
            {
                _endLocationText = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("EndLocationText"));
                }
            }
        }
        #endregion Routing Start and End Locations

        GraphicsLayer _routeGraphicsLayer;
        GraphicsLayer _stopsGraphicsLayer;
        RouteTask _routeTask;
        string _onlineRoutingService = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route";
        string _localRoutingDatabase = @"..\..\..\..\..\samples-data\networks\san-diego\san-diego-network.geodatabase";
        string _networkName = "Streets_ND";

        private void SetupRouteTask()
        {
            if (!IsOnline)
            {
                _routeTask = new LocalRouteTask(_localRoutingDatabase, _networkName);
            }
            else
            {
                _routeTask = new OnlineRouteTask(new Uri(_onlineRoutingService));
            }
        }

        public RoutingPointToPoint()
        {
            InitializeComponent();

            // Min X,Y = -13,044,000  3,855,000 Meters
            // Max X,Y = -13,040,000  3,858,000 Meters
            map1.InitialExtent = new Envelope(-13044000, 3855000, -13040000, 3858000, new SpatialReference(102100));

            DataContext = this;

            SetupRouteTask();

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

        private async Task CalculateRoute()
        {
            _routeGraphicsLayer.Graphics.Clear();
            _stopsGraphicsLayer.Graphics.Clear();

            try
            {
                // Mouseclick 1:
                MapPoint startLocation = await mapView1.Editor.RequestPointAsync();
                _stopsGraphicsLayer.Graphics.Add(new Graphic() { Geometry = startLocation });

                StartLocationText = " X: " + Math.Round(startLocation.X, 0).ToString() + " Y: " + Math.Round(startLocation.Y, 0).ToString();

                // Mouseclick 2:
                MapPoint endLocation = await mapView1.Editor.RequestPointAsync();
                _stopsGraphicsLayer.Graphics.Add(new Graphic() { Geometry = endLocation });

                EndLocationText = " X: " + Math.Round(endLocation.X, 0).ToString() + " Y: " + Math.Round(endLocation.Y, 0).ToString();

                Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteParameters routeParams = await _routeTask.GetDefaultParametersAsync();

                routeParams.OutSpatialReference = mapView1.SpatialReference;
                routeParams.ReturnDirections = false;
                routeParams.DirectionsLengthUnit = LinearUnits.Kilometers;
                
                List<Graphic> graphicsStops = new List<Graphic>();

                graphicsStops.Add(new Graphic(startLocation));
                graphicsStops.Add(new Graphic(endLocation));
                FeaturesAsFeature stops = new FeaturesAsFeature(graphicsStops);
                stops.SpatialReference = mapView1.SpatialReference;
                routeParams.Stops = stops;

                RouteResult routeResult = await _routeTask.SolveAsync(routeParams);
                
                if (routeResult.Routes.Count > 0)
                {
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
            finally
            {
            }

        }

        private async void SolveRouteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await CalculateRoute();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }
    }
}
