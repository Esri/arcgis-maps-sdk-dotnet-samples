// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.ShowLocationHistory
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Show location history",
        category: "Location",
        description: "Display your location history on the map.",
        instructions: "Click 'Start tracking' to start tracking your location, which will appear as points on the map. A line will connect the points for easier visualization. Click 'Stop tracking' to stop updating the location history. This sample uses a simulated data source to allow the sample to be useful on desktop/non-mobile devices. To track a user's real position, use the `SystemLocationDataSource` instead.",
        tags: new[] { "GPS", "bread crumb", "breadcrumb", "history", "movement", "navigation", "real-time", "trace", "track", "trail" })]
    public partial class ShowLocationHistory
    {
        // Track whether location tracking is enabled.
        private bool _isTrackingEnabled;

        // Location data source provides location data updates.
        private LocationDataSource _locationDataSource;

        // Graphics overlay to display the location history (points).
        private GraphicsOverlay _locationHistoryOverlay;

        // Graphics overlay to display the line created by the location points.
        private GraphicsOverlay _locationHistoryLineOverlay;

        // Polyline builder to more efficiently manage large location history graphic.
        private PolylineBuilder _polylineBuilder;

        // Track previous location to ensure the route line appears behind the animating location symbol.
        private MapPoint _lastPosition;

        public ShowLocationHistory()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Add event handler for when this sample is unloaded.
            Unloaded += SampleUnloaded;

            // Create new Map with basemap.
            Map myMap = new Map(BasemapStyle.ArcGISDarkGray);

            // Display the map.
            MyMapView.Map = myMap;

            // Create and add graphics overlay for displaying the trail.
            _locationHistoryLineOverlay = new GraphicsOverlay();
            SimpleLineSymbol locationLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Lime, 2);
            _locationHistoryLineOverlay.Renderer = new SimpleRenderer(locationLineSymbol);
            MyMapView.GraphicsOverlays.Add(_locationHistoryLineOverlay);

            // Create and add graphics overlay for showing points.
            _locationHistoryOverlay = new GraphicsOverlay();
            SimpleMarkerSymbol locationPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 3);
            _locationHistoryOverlay.Renderer = new SimpleRenderer(locationPointSymbol);
            MyMapView.GraphicsOverlays.Add(_locationHistoryOverlay);

            // Create the polyline builder.
            _polylineBuilder = new PolylineBuilder(SpatialReferences.WebMercator);

            // Start location services.
            _ = HandleLocationReady();
        }

        private async Task HandleLocationReady()
        {
            // Create a polyline of mock data.
            var simulatedLocations = (Polyline)Geometry.FromJson("{\"paths\":[[[-13185646.046666779,4037971.5966668758],[-13185586.780000051,4037827.6633333955], [-13185514.813333312,4037709.1299999417],[-13185569.846666701,4037522.8633330846], [-13185591.01333339,4037378.9299996048],[-13185629.113333428,4037283.6799995075], [-13185770.93000024,4037425.4966663187],[-13185821.730000293,4037546.146666442], [-13185880.996667018,4037704.8966666036],[-13185948.730000421,4037874.2300001099], [-13185974.130000448,4037946.1966668498],[-13186120.180000596,4037958.896666863], [-13186264.113334076,4037984.296666889],[-13186336.080000836,4038001.2300002342], [-13186314.91333415,4037757.8133333195],[-13186272.580000773,4037560.9633331187], [-13186187.913334005,4037463.59666635],[-13186431.33000092,4037404.3299996229], [-13186676.863334503,4037290.0299995062],[-13187625.130002158,4038589.6633341513], [-13187333.030001862,4038756.8800009824],[-13187091.730001617,4038617.1800008402], [-13186791.163334643,4038805.5633343654],[-13186721.313334571,4038801.3300010278], [-13186833.49666802,4038195.9633337436],[-13186977.677439401,4037699.8176972247], [-13186784.921301765,4037820.4541915278],[-13186749.517113185,4038150.8932846226], [-13186649.860878762,4038288.5762400767],[-13186556.760975549,4038323.9804286221], [-13186472.839936033,4038481.3323777127],[-13186373.183701571,4038489.1999751539], [-13186344.335844241,4038242.6819215398],[-13186126.665647998,4038308.245233661], [-13185814.584282301,4038358.0733508728],[-13185651.987268206,4038116.8003622484], [-13185203.534213299,4038048.6145176427],[-13184576.748949422,4038150.8932845518], [-13184251.55492135,4037833.5668537691],[-13184146.653621957,4037524.1080205571], [-13183949.963685593,4037621.1417224966],[-13183687.71043711,4037781.1162040718], [-13183480.530370807,4037875.5273735262],[-13182307.629999243,4037859.7460188437], [-13181376.039484169,4037820.9297473822],[-13180716.162869323,4038364.3575478429], [-13180182.439136729,4038810.7446696493],[-13178474.523192419,4040237.2426458625], [-13178321.134040033,4039740.6894803117],[-13177958.020228144,4039140.1550991111], [-13177073.512224896,4037459.5898928214],[-13177757.842101147,4037589.9384406791], [-13178386.308314031,4037799.427178307],[-13180095.012208173,4037811.6550856642], [-13180126.165447287,4036845.9046731163],[-13179806.844746364,4036324.0879179495], [-13180928.361354485,4035887.9425703473],[-13181598.155995468,4035428.432293402], [-13182984.47513606,4034447.105261297],[-13182229.264383668,4033222.8051626245], [-13182058.735615831,4033339.8690072047],[-13181939.035180708,4033691.2477038498], [-13182116.65518121,4033861.1450956347],[-13181792.305615077,4034085.1007484416], [-13182027.845180977,4034467.3698799671],[-13181877.254310986,4034644.9898804692], [-13181630.130832028,4034517.5668366305],[-13181386.868657427,4034424.8955320208], [-13181228.555178719,4034652.7124891868],[-13181379.14604871,4034942.3103160923], [-13181267.168222306,4035189.4337950516],[-13181074.103004368,4035015.6750989081], [-13180807.673003616,4034934.5877073747],[-13180618.469090037,4034814.8872722536], [-13180599.162568243,4035374.7764042714],[-13181047.073873857,4035494.476839392], [-13181317.365178969,4035413.3894478586],[-13180765.198655669,4035143.0981427468], [-13180328.871263131,4034892.1133594285],[-13180270.951697765,4035258.9372735149], [-13180325.009958787,4035718.4324922049],[-13180707.279090302,4035695.2646660525], [-13181413.897788007,4035536.9511873648],[-13181618.54691902,4035807.2424924765], [-13181884.976919774,4036065.949884512],[-13182159.129529245,4035861.3007534989], [-13182174.57474668,4035668.2355355616],[-13182417.83692128,4035664.374231203], [-13182784.660835361,4035409.5281435261],[-13182997.032575091,4035255.0759691764], [-13182618.624747934,4034679.7416197238]]], \"spatialReference\":{\"wkid\":102100,\"latestWkid\":3857}}");

            // Create a simulated location data source using the mock data.
            var parameters = new SimulationParameters(DateTimeOffset.Now, 150);
            var simulatedSource = new SimulatedLocationDataSource();
            simulatedSource.SetLocationsWithPolyline(simulatedLocations, parameters);

            // Set the location data source to the simulated source.
            _locationDataSource = simulatedSource;
            // Use this instead if you want real location: _locationDataSource = new SystemLocationDataSource();

            try
            {
                // Start the data source.
                await _locationDataSource.StartAsync();

                if (_locationDataSource.Status == LocationDataSourceStatus.Started)
                {
                    // Set the location display data source and enable location display.
                    MyMapView.LocationDisplay.DataSource = _locationDataSource;
                    MyMapView.LocationDisplay.IsEnabled = true;
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    MyMapView.LocationDisplay.InitialZoomScale = 10000;

                    // Enable the button to start location tracking.
                    LocationTrackingButton.IsEnabled = true;
                }
                else
                {
                    ShowMessage("There was a problem enabling location", "Error");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                ShowMessage("There was a problem enabling location", "Error");
            }
        }

        private void ToggleLocationTracking()
        {
            // Toggle location tracking.
            _isTrackingEnabled = !_isTrackingEnabled;

            // Apply new configuration.
            if (_isTrackingEnabled)
            {
                // Configure new symbology first.
                _locationDataSource.LocationChanged += LocationDataSourceOnLocationChanged;

                // Update the UI.
                LocationTrackingButton.Content = "Stop tracking";
            }
            else
            {
                // Stop updating.
                _locationDataSource.LocationChanged -= LocationDataSourceOnLocationChanged;

                // Update the UI.
                LocationTrackingButton.Content = "Start tracking";
            }
        }

        private void LocationDataSourceOnLocationChanged(object sender, Location e)
        {
            // Remove the old line.
            _locationHistoryLineOverlay.Graphics.Clear();

            // Add any previous position to the history.
            if (_lastPosition != null)
            {
                _polylineBuilder.AddPoint(_lastPosition);
                _locationHistoryOverlay.Graphics.Add(new Graphic(_lastPosition));
            }

            // Store the current position.
            _lastPosition = e.Position;

            // Add the updated line.
            _locationHistoryLineOverlay.Graphics.Add(new Graphic(_polylineBuilder.ToGeometry()));
        }

        private void LocationTrackingButton_OnClick(object sender, RoutedEventArgs e) => ToggleLocationTracking();

        private void ShowMessage(string title, string detail) => MessageBox.Show(detail, title);

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}