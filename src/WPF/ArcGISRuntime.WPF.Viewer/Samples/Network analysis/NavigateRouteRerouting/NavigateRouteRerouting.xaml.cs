// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

#if !NET_CORE_3
using System.Speech.Synthesis;
#endif

namespace ArcGISRuntime.WPF.Samples.NavigateRouteRerouting
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Navigate route with rerouting",
        "Network analysis",
        "Navigate between two points and dynamically recalculate an alternate route when the original route is unavailable.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("567e14f3420d40c5a206e5c0284cf8fc", "91e7e6be79cc4d2f8416eff867674c1e")]
    public partial class NavigateRouteRerouting
    {
        // Variables for tracking the navigation route.
        private RouteTracker _tracker;
        private RouteResult _routeResult;
        private Route _route;
        private RouteTask _routeTask;
        private RouteParameters _routeParams;

        // List of driving directions for the route.
        private IReadOnlyList<DirectionManeuver> _directionsList;

#if !NET_CORE_3
        // Speech synthesizer to play voice guidance audio.
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
#endif

        // Graphics to show progress along the route.
        private Graphic _routeAheadGraphic;
        private Graphic _routeTraveledGraphic;

        // San Diego Convention Center.
        private readonly MapPoint _conventionCenter = new MapPoint(-117.160386727, 32.706608, SpatialReferences.Wgs84);

        // RH Fleet Aerospace Museum.
        private readonly MapPoint _aerospaceMuseum = new MapPoint(-117.146678, 32.730463, SpatialReferences.Wgs84);

        // GPX file that contains simulated location data for driving route.
        private readonly string _gpxPath = DataManager.GetDataFolder("91e7e6be79cc4d2f8416eff867674c1e", "navigate_a_route_detour.gpx");

        // Path to the network geodatabase for San Diego.
        private readonly string _networkGeodatabasePath = DataManager.GetDataFolder("567e14f3420d40c5a206e5c0284cf8fc", "sandiego.geodatabase");

        public NavigateRouteRerouting()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Add event handler for when this sample is unloaded.
                Unloaded += SampleUnloaded;

                // Create the map view.
                MyMapView.Map = new Map(Basemap.CreateNavigationVector());

                // Create the route task, using the routing service.
                _routeTask = await RouteTask.CreateAsync(_networkGeodatabasePath, "Streets_ND");

                // Get the default route parameters.
                _routeParams = await _routeTask.CreateDefaultParametersAsync();

                // Explicitly set values for parameters.
                _routeParams.ReturnDirections = true;
                _routeParams.ReturnStops = true;
                _routeParams.ReturnRoutes = true;
                _routeParams.OutputSpatialReference = SpatialReferences.Wgs84;

                // Create stops for each location.
                Stop stop1 = new Stop(_conventionCenter) { Name = "San Diego Convention Center" };
                Stop stop2 = new Stop(_aerospaceMuseum) { Name = "RH Fleet Aerospace Museum" };

                // Assign the stops to the route parameters.
                List<Stop> stopPoints = new List<Stop> { stop1, stop2 };
                _routeParams.SetStops(stopPoints);

                // Get the route results.
                _routeResult = await _routeTask.SolveRouteAsync(_routeParams);
                _route = _routeResult.Routes[0];

                // Add a graphics overlay for the route graphics.
                MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

                // Add graphics for the stops.
                SimpleMarkerSymbol stopSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.OrangeRed, 20);
                MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_conventionCenter, stopSymbol));
                MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_aerospaceMuseum, stopSymbol));

                // Create a graphic (with a dashed line symbol) to represent the route.
                _routeAheadGraphic = new Graphic(_route.RouteGeometry) { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.BlueViolet, 5) };

                // Create a graphic (solid) to represent the route that's been traveled (initially empty).
                _routeTraveledGraphic = new Graphic { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.LightBlue, 3) };

                // Add the route graphics to the map view.
                MyMapView.GraphicsOverlays[0].Graphics.Add(_routeAheadGraphic);
                MyMapView.GraphicsOverlays[0].Graphics.Add(_routeTraveledGraphic);

                // Set the map viewpoint to show the entire route.
                await MyMapView.SetViewpointGeometryAsync(_route.RouteGeometry, 100);

                // Enable the navigation button.
                StartNavigationButton.IsEnabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        private async void StartNavigation(object sender, RoutedEventArgs e)
        {
            // Disable the start navigation button.
            StartNavigationButton.IsEnabled = false;

            // Get the directions for the route.
            _directionsList = _route.DirectionManeuvers;

            // Create a route tracker.
            _tracker = new RouteTracker(_routeResult, 0);
            _tracker.NewVoiceGuidance += SpeakDirection;

            // Handle route tracking status changes.
            _tracker.TrackingStatusChanged += TrackingStatusUpdated;

            // Check if this route task supports rerouting.
            if (_routeTask.RouteTaskInfo.SupportsRerouting)
            {
                // Enable automatic re-routing.
                await _tracker.EnableReroutingAsync(_routeTask, _routeParams, ReroutingStrategy.ToNextWaypoint, false);

                // Handle re-routing completion to display updated route graphic and report new status.
                _tracker.RerouteStarted += RerouteStarted;
                _tracker.RerouteCompleted += RerouteCompleted;
            }

            // Turn on navigation mode for the map view.
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
            MyMapView.LocationDisplay.AutoPanModeChanged += AutoPanModeChanged;

            // Add a data source for the location display.
            MyMapView.LocationDisplay.DataSource = new RouteTrackerDisplayLocationDataSource(new GpxProvider(_gpxPath), _tracker);
            // Use this instead if you want real location:
            // MyMapView.LocationDisplay.DataSource = new RouteTrackerLocationDataSource(new SystemLocationDataSource(), _tracker);

            // Enable the location display (this will start the location data source).
            MyMapView.LocationDisplay.IsEnabled = true;
        }

        private void RerouteStarted(object sender, EventArgs e)
        {
            // Remove the event listeners for tracking status changes while the route tracker recalculates.
            _tracker.NewVoiceGuidance -= SpeakDirection;
            _tracker.TrackingStatusChanged -= TrackingStatusUpdated;
        }

        private void RerouteCompleted(object sender, RouteTrackerRerouteCompletedEventArgs e)
        {
            // Get the new directions.
            _directionsList = e.TrackingStatus.RouteResult.Routes[0].DirectionManeuvers;

            // Re-add the event listeners for tracking status changes.
            _tracker.NewVoiceGuidance += SpeakDirection;
            _tracker.TrackingStatusChanged += TrackingStatusUpdated;
        }

        private void TrackingStatusUpdated(object sender, RouteTrackerTrackingStatusChangedEventArgs e)
        {
            TrackingStatus status = e.TrackingStatus;

            // Start building a status message for the UI.
            System.Text.StringBuilder statusMessageBuilder = new System.Text.StringBuilder("Route Status:\n");

            // Check if navigation is on route.
            if (status.IsOnRoute && !status.IsRouteCalculating)
            {
                // Check the destination status.
                if (status.DestinationStatus == DestinationStatus.NotReached || status.DestinationStatus == DestinationStatus.Approaching)
                {
                    statusMessageBuilder.AppendLine("Distance remaining: " +
                                                status.RouteProgress.RemainingDistance.DisplayText + " " +
                                                status.RouteProgress.RemainingDistance.DisplayTextUnits.PluralDisplayName);

                    statusMessageBuilder.AppendLine("Time remaining: " +
                                                    status.RouteProgress.RemainingTime.ToString(@"hh\:mm\:ss"));

                    if (status.CurrentManeuverIndex + 1 < _directionsList.Count)
                    {
                        statusMessageBuilder.AppendLine("Next direction: " + _directionsList[status.CurrentManeuverIndex + 1].DirectionText);
                    }

                    // Set geometries for progress and the remaining route.
                    _routeAheadGraphic.Geometry = status.RouteProgress.RemainingGeometry;
                    _routeTraveledGraphic.Geometry = status.RouteProgress.TraversedGeometry;
                }
                else if (status.DestinationStatus == DestinationStatus.Reached)
                {
                    statusMessageBuilder.AppendLine("Destination reached.");

                    // Set the route geometries to reflect the completed route.
                    _routeAheadGraphic.Geometry = null;
                    _routeTraveledGraphic.Geometry = status.RouteResult.Routes[0].RouteGeometry;
                }
            }
            else
            {
                statusMessageBuilder.AppendLine("Off route!");
            }

            Dispatcher.BeginInvoke((Action)delegate ()
            {
                // Show the status information in the UI.
                MessagesTextBlock.Text = statusMessageBuilder.ToString();
            });
        }

        private void SpeakDirection(object sender, RouteTrackerNewVoiceGuidanceEventArgs e)
        {
#if !NET_CORE_3
            // Say the direction using voice synthesis.
            _speechSynthesizer.SpeakAsyncCancelAll();
            _speechSynthesizer.SpeakAsync(e.VoiceGuidance.Text);
#endif
        }

        private void AutoPanModeChanged(object sender, LocationDisplayAutoPanMode e)
        {
            // Turn the recenter button on or off when the location display changes to or from navigation mode.
            RecenterButton.IsEnabled = e != LocationDisplayAutoPanMode.Navigation;
        }

        private void RecenterButton_Click(object sender, RoutedEventArgs e)
        {
            // Change the mapview to use navigation mode.
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
#if !NET_CORE_3
            // Stop the speech synthesizer.
            _speechSynthesizer.SpeakAsyncCancelAll();
            _speechSynthesizer.Dispose();
#endif

            // Stop the tracker.
            if (_tracker != null)
            {
                _tracker.TrackingStatusChanged -= TrackingStatusUpdated;
                _tracker.NewVoiceGuidance -= SpeakDirection;
                _tracker.RerouteStarted -= RerouteStarted;
                _tracker.RerouteCompleted -= RerouteCompleted;
                _tracker = null;
            }

            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }

    // This location data source uses an input data source and a route tracker.
    // The location source that it updates is based on the snapped-to-route location from the route tracker.
    public class RouteTrackerDisplayLocationDataSource : LocationDataSource
    {
        private LocationDataSource _inputDataSource;
        private RouteTracker _routeTracker;

        public RouteTrackerDisplayLocationDataSource(LocationDataSource dataSource, RouteTracker routeTracker)
        {
            // Set the data source
            _inputDataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

            // Set the route tracker.
            _routeTracker = routeTracker ?? throw new ArgumentNullException(nameof(routeTracker));

            // Change the tracker location when the source location changes.
            _inputDataSource.LocationChanged += InputLocationChanged;

            // Update the location output when the tracker location updates.
            _routeTracker.TrackingStatusChanged += TrackingStatusChanged;
        }

        private void InputLocationChanged(object sender, Location e)
        {
            // Update the tracker location with the new location from the source (simulation or GPS).
            _routeTracker.TrackLocationAsync(e);
        }

        private void TrackingStatusChanged(object sender, RouteTrackerTrackingStatusChangedEventArgs e)
        {
            // Check if the tracking status has a location.
            if (e.TrackingStatus.DisplayLocation != null)
            {
                // Call the base method for LocationDataSource to update the location with the tracked (snapped to route) location.
                UpdateLocation(e.TrackingStatus.DisplayLocation);
            }
        }

        protected override Task OnStartAsync() => _inputDataSource.StartAsync();

        protected override Task OnStopAsync() => _inputDataSource.StartAsync();
    }
}