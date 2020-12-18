// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using AVFoundation;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.NavigateRoute
{
    [Register("NavigateRoute")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Navigate route",
        category: "Network analysis",
        description: "Use a routing service to navigate between points.",
        instructions: "Tap 'Navigate' to simulate traveling and to receive directions from a preset starting point to a preset destination. Tap 'Recenter' to refocus on the location display.",
        tags: new[] { "directions", "maneuver", "navigation", "route", "turn-by-turn", "voice", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class NavigateRoute : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;
        private UIBarButtonItem _recenterButton;
        private UIBarButtonItem _navigateButton;
        private UILabel _statusLabel;

        // Variables for tracking the navigation route.
        private RouteTracker _tracker;
        private RouteResult _routeResult;
        private Route _route;

        // List of driving directions for the route.
        private IReadOnlyList<DirectionManeuver> _directionsList;

        // Speech synthesizer to play voice guidance audio.
        private AVSpeechSynthesizer _speechSynthesizer = new AVSpeechSynthesizer();
        private string _statusMessage;

        // Graphics to show progress along the route.
        private Graphic _routeAheadGraphic;
        private Graphic _routeTraveledGraphic;

        // San Diego Convention Center.
        private readonly MapPoint _conventionCenter = new MapPoint(-117.160386727, 32.706608, SpatialReferences.Wgs84);

        // USS San Diego Memorial.
        private readonly MapPoint _memorial = new MapPoint(-117.173034, 32.712327, SpatialReferences.Wgs84);

        // RH Fleet Aerospace Museum.
        private readonly MapPoint _aerospaceMuseum = new MapPoint(-117.147230, 32.730467, SpatialReferences.Wgs84);

        // Feature service for routing in San Diego.
        private readonly Uri _routingUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route");

        public NavigateRoute()
        {
            Title = "Navigate route";
        }

        private async void Initialize()
        {
            try
            {
                // Create the map view.
                _myMapView.Map = new Map(Basemap.CreateNavigationVector());

                // Create the route task, using the online routing service.
                RouteTask routeTask = await RouteTask.CreateAsync(_routingUri);

                // Get the default route parameters.
                RouteParameters routeParams = await routeTask.CreateDefaultParametersAsync();

                // Explicitly set values for parameters.
                routeParams.ReturnDirections = true;
                routeParams.ReturnStops = true;
                routeParams.ReturnRoutes = true;
                routeParams.OutputSpatialReference = SpatialReferences.Wgs84;

                // Create stops for each location.
                Stop stop1 = new Stop(_conventionCenter) { Name = "San Diego Convention Center" };
                Stop stop2 = new Stop(_memorial) { Name = "USS San Diego Memorial" };
                Stop stop3 = new Stop(_aerospaceMuseum) { Name = "RH Fleet Aerospace Museum" };

                // Assign the stops to the route parameters.
                List<Stop> stopPoints = new List<Stop> { stop1, stop2, stop3 };
                routeParams.SetStops(stopPoints);

                // Get the route results.
                _routeResult = await routeTask.SolveRouteAsync(routeParams);
                _route = _routeResult.Routes[0];

                // Add a graphics overlay for the route graphics.
                _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

                // Add graphics for the stops.
                SimpleMarkerSymbol stopSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.OrangeRed, 20);
                _myMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_conventionCenter, stopSymbol));
                _myMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_memorial, stopSymbol));
                _myMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_aerospaceMuseum, stopSymbol));

                // Create a graphic (with a dashed line symbol) to represent the route.
                _routeAheadGraphic = new Graphic(_route.RouteGeometry) { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.BlueViolet, 5) };

                // Create a graphic (solid) to represent the route that's been traveled (initially empty).
                _routeTraveledGraphic = new Graphic { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.LightBlue, 3) };

                // Add the route graphics to the map view.
                _myMapView.GraphicsOverlays[0].Graphics.Add(_routeAheadGraphic);
                _myMapView.GraphicsOverlays[0].Graphics.Add(_routeTraveledGraphic);

                // Set the map viewpoint to show the entire route.
                await _myMapView.SetViewpointGeometryAsync(_route.RouteGeometry, 100);

                // Enable the navigation button.
                _navigateButton.Enabled = true;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private void StartNavigation(object sender, EventArgs e)
        {
            // Disable the start navigation button.
            _navigateButton.Enabled = false;

            // Get the directions for the route.
            _directionsList = _route.DirectionManeuvers;

            // Create a route tracker.
            _tracker = new RouteTracker(_routeResult, 0);
            _tracker.NewVoiceGuidance += SpeakDirection;

            // Handle route tracking status changes.
            _tracker.TrackingStatusChanged += TrackingStatusUpdated;

            // Turn on navigation mode for the map view.
            _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
            _myMapView.LocationDisplay.AutoPanModeChanged += AutoPanModeChanged;

            // Add a data source for the location display.
            var simulationParameters = new SimulationParameters(DateTimeOffset.Now, 40.0);
            var simulatedDataSource = new SimulatedLocationDataSource();
            simulatedDataSource.SetLocationsWithPolyline(_route.RouteGeometry, simulationParameters);
            _myMapView.LocationDisplay.DataSource = new RouteTrackerDisplayLocationDataSource(simulatedDataSource, _tracker);

            // Use this instead if you want real location:
            // _myMapView.LocationDisplay.DataSource = new RouteTrackerLocationDataSource(new SystemLocationDataSource(), _tracker);

            // Enable the location display (this wil start the location data source).
            _myMapView.LocationDisplay.IsEnabled = true;
        }

        private void TrackingStatusUpdated(object sender, RouteTrackerTrackingStatusChangedEventArgs e)
        {
            TrackingStatus status = e.TrackingStatus;

            // Start building a status message for the UI.
            System.Text.StringBuilder statusMessageBuilder = new System.Text.StringBuilder();

            // Check the destination status.
            if (status.DestinationStatus == DestinationStatus.NotReached || status.DestinationStatus == DestinationStatus.Approaching)
            {
                statusMessageBuilder.AppendLine("Distance remaining: " +
                                            status.RouteProgress.RemainingDistance.DisplayText + " " +
                                            status.RouteProgress.RemainingDistance.DisplayTextUnits.PluralDisplayName);

                statusMessageBuilder.AppendLine("Time remaining: " + status.RouteProgress.RemainingTime.ToString(@"hh\:mm\:ss"));

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

                // Navigate to the next stop (if there are stops remaining).
                if (status.RemainingDestinationCount > 1)
                {
                    _tracker.SwitchToNextDestinationAsync();
                }
                else
                {
                    // This is needed because measurement change events occur on a non-UI thread and this code accesses UI object.
                    BeginInvokeOnMainThread(() =>
                    {
                        // Stop the simulated location data source.
                        _myMapView.LocationDisplay.DataSource.StopAsync();
                    });
                }
            }

            // Show the status information in the UI.
            _statusMessage = statusMessageBuilder.ToString().TrimEnd('\n').TrimEnd('\r');
            BeginInvokeOnMainThread(UpdateStatusText);
        }

        private void UpdateStatusText()
        {
            _statusLabel.Text = _statusMessage;
            _statusLabel.SizeToFit();
        }

        private void AutoPanModeChanged(object sender, LocationDisplayAutoPanMode e)
        {
            // Turn the recenter button on or off when the location display changes to or from navigation mode.
            _recenterButton.Enabled = e != LocationDisplayAutoPanMode.Navigation;
        }

        private void SpeakDirection(object sender, RouteTrackerNewVoiceGuidanceEventArgs e)
        {
            // Say the direction using voice synthesis.
            _speechSynthesizer.StopSpeaking(AVSpeechBoundary.Word);
            _speechSynthesizer.SpeakUtterance(new AVSpeechUtterance(e.VoiceGuidance.Text));
        }

        private void Recenter(object sender, EventArgs e)
        {
            // Change the mapview to use navigation mode.
            _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView() { TranslatesAutoresizingMaskIntoConstraints = false };

            _navigateButton = new UIBarButtonItem() { Title = "Navigate", Enabled = false };
            _recenterButton = new UIBarButtonItem() { Title = "Recenter", Enabled = false };

            _toolbar = new UIToolbar() { TranslatesAutoresizingMaskIntoConstraints = false };
            _toolbar.Items = new[]
            {
                _navigateButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _recenterButton
            };

            _statusLabel = new UILabel
            {
                Text = "",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Left,
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TextColor = ApplicationTheme.ForegroundColor,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, _toolbar, _statusLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor),

                _toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _statusLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _statusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _statusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _navigateButton.Clicked += StartNavigation;
            _recenterButton.Clicked += Recenter;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Check if sample is being popped.
            var vControllers = this.NavigationController?.ViewControllers;
            if (vControllers == null || (vControllers.Count() > 1 && vControllers.GetValue(vControllers.Count() - 2) != this))
            {
                // Stop the location data source.
                _myMapView.LocationDisplay?.DataSource?.StopAsync();

                // Stop the speech synthesizer.
                _speechSynthesizer.StopSpeaking(AVSpeechBoundary.Word);
                _speechSynthesizer.Dispose();

                // Stop the tracker.
                if (_tracker != null)
                {
                    _tracker.TrackingStatusChanged -= TrackingStatusUpdated;
                    _tracker.NewVoiceGuidance -= SpeakDirection;
                    _tracker = null;
                }
            }

            // Unsubscribe from events, per best practice.
            _navigateButton.Clicked -= StartNavigation;
            _recenterButton.Clicked -= Recenter;
        }
    }

    /*
     * This location data source uses an input data source and a route tracker.
     * The location source that it updates is based on the snapped-to-route location from the route tracker.
     */

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