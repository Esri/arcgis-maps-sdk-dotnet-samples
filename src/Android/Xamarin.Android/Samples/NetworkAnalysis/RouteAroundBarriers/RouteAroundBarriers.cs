// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.Views;

namespace ArcGISRuntimeXamarin.Samples.RouteAroundBarriers
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Route around barriers",
        category: "Network analysis",
        description: "Find a route that reaches all stops without crossing any barriers.",
        instructions: "Tap 'Add stop' to add stops to the route. Tap 'Add barrier' to add areas that can't be crossed by the route. Tap 'Route' to find the route and display it. Select 'Allow stops to be re-ordered' to find the best sequence. Select 'Preserve first stop' if there is a known start point, and 'Preserve last stop' if there is a known final destination.",
        tags: new[] { "barriers", "best sequence", "directions", "maneuver", "network analysis", "routing", "sequence", "stop order", "stops" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("RouteAroundBarriers.axml")]
    public class RouteAroundBarriers : Activity
    {
        // Hold references to the UI controls.
        private MapView _mapView;
        private Button _addStopButton;
        private Button _addBarrierButton;
        private Button _resetButton;
        private Button _routeButton;
        private Button _directionsButton;
        private TextView _statusLabel;
        private CheckBox _reorderStopsCheckbox;
        private CheckBox _preserveFirstStopCheckbox;
        private CheckBox _preserveLastStopCheckbox;
        private AlertDialog _busyIndicator;

        // Track the current state of the sample.
        private SampleState _currentSampleState;

        // Graphics overlays to maintain the stops, barriers, and route result.
        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _stopsOverlay;
        private GraphicsOverlay _barriersOverlay;

        // The route task manages routing work.
        private RouteTask _routeTask;

        // The route parameters defines how the route will be calculated.
        private RouteParameters _routeParameters;

        // Symbols for displaying the barriers and the route line.
        private Symbol _routeSymbol;
        private Symbol _barrierSymbol;

        // URL to the network analysis service.
        private const string RouteServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route";

        // Hold a list of directions.
        private List<DirectionManeuver> _directions = new List<DirectionManeuver>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Route around barriers";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Update interface state.
                UpdateInterfaceState(SampleState.NotReady);

                // Create the map with a basemap.
                Map sampleMap = new Map(BasemapStyle.ArcGISTopographic);
                sampleMap.InitialViewpoint = new Viewpoint(32.7157, -117.1611, 1e5);
                _mapView.Map = sampleMap;

                // Create the graphics overlays. These will manage rendering of route, direction, stop, and barrier graphics.
                _routeOverlay = new GraphicsOverlay();
                _stopsOverlay = new GraphicsOverlay();
                _barriersOverlay = new GraphicsOverlay();

                // Add graphics overlays to the map view.
                _mapView.GraphicsOverlays.Add(_routeOverlay);
                _mapView.GraphicsOverlays.Add(_stopsOverlay);
                _mapView.GraphicsOverlays.Add(_barriersOverlay);

                // Create and initialize the route task.
                _routeTask = await RouteTask.CreateAsync(new Uri(RouteServiceUrl));

                // Get the route parameters from the route task.
                _routeParameters = await _routeTask.CreateDefaultParametersAsync();

                // Prepare symbols.
                _routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 2);
                _barrierSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Cross, Color.Red, null);

                // Enable the UI.
                UpdateInterfaceState(SampleState.Ready);
            }
            catch (Exception e)
            {
                UpdateInterfaceState(SampleState.NotReady);
                System.Diagnostics.Debug.WriteLine(e);
                ShowMessage("Couldn't load sample", "Couldn't start the sample. See the debug output for detail.");
            }
        }

        private void ShowDirections_Click(object sender, EventArgs e)
        {
            if (!_directions.Any())
            {
                ShowMessage("No directions", "Add stops and barriers, then click 'Route' before displaying directions.");
                return;
            }

            // Create a dialog to show route directions.
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);

            // Create the layout.
            LinearLayout dialogLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Convert the directions list to a suitable format for display.
            string[] directionText = _directions.Select(directionObject => directionObject.DirectionText).ToArray();

            // Create a list box for showing the route directions.
            ListView directionsList = new ListView(this);
            ArrayAdapter<string> directionsAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, directionText);
            directionsList.Adapter = directionsAdapter;
            dialogLayout.AddView(directionsList);

            // Add the controls to the dialog.
            dialogBuilder.SetView(dialogLayout);
            dialogBuilder.SetTitle("Directions");

            // Show the dialog.
            dialogBuilder.Create().Show();
        }

        private async void HandleMapTap(MapPoint mapLocation)
        {
            // Normalize geometry - important for geometries that will be sent to a server for processing.
            mapLocation = (MapPoint) GeometryEngine.NormalizeCentralMeridian(mapLocation);

            switch (_currentSampleState)
            {
                case SampleState.AddingBarriers:
                    // Buffer the tapped point to create a larger barrier.
                    Geometry bufferedGeometry = GeometryEngine.BufferGeodetic(mapLocation, 500, LinearUnits.Meters);

                    // Create the graphic to show the barrier.
                    Graphic barrierGraphic = new Graphic(bufferedGeometry, _barrierSymbol);

                    // Add the graphic to the overlay - this will cause it to appear on the map.
                    _barriersOverlay.Graphics.Add(barrierGraphic);
                    break;
                case SampleState.AddingStops:
                    // Get the name of this stop.
                    string stopName = $"{_stopsOverlay.Graphics.Count + 1}";

                    // Create the marker to show underneath the stop number.
                    PictureMarkerSymbol pushpinMarker = await GetPictureMarker();

                    // Create the text symbol for showing the stop.
                    TextSymbol stopSymbol = new TextSymbol(stopName, Color.White, 15, HorizontalAlignment.Center, VerticalAlignment.Middle);
                    stopSymbol.OffsetY = 15;

                    CompositeSymbol combinedSymbol = new CompositeSymbol(new MarkerSymbol[] {pushpinMarker, stopSymbol});

                    // Create the graphic to show the stop.
                    Graphic stopGraphic = new Graphic(mapLocation, combinedSymbol);

                    // Add the graphic to the overlay - this will cause it to appear on the map.
                    _stopsOverlay.Graphics.Add(stopGraphic);
                    break;
            }
        }

        private void ConfigureThenRoute()
        {
            // Guard against error conditions.
            if (_routeParameters == null)
            {
                ShowMessage("Not ready yet", "Sample isn't ready yet; define route parameters first.");
                return;
            }

            if (_stopsOverlay.Graphics.Count < 2)
            {
                ShowMessage("Not enough stops", "Add at least two stops before solving a route.");
                return;
            }

            // Clear any existing route from the map.
            _routeOverlay.Graphics.Clear();

            // Configure the route result to include directions and stops.
            _routeParameters.ReturnStops = true;
            _routeParameters.ReturnDirections = true;

            // Create a list to hold stops that should be on the route.
            List<Stop> routeStops = new List<Stop>();

            // Create stops from the graphics.
            foreach (Graphic stopGraphic in _stopsOverlay.Graphics)
            {
                // Note: this assumes that only points were added to the stops overlay.
                MapPoint stopPoint = (MapPoint) stopGraphic.Geometry;

                // Create the stop from the graphic's geometry.
                Stop routeStop = new Stop(stopPoint);

                // Set the name of the stop to its position in the list.
                routeStop.Name = $"{_stopsOverlay.Graphics.IndexOf(stopGraphic) + 1}";

                // Add the stop to the list of stops.
                routeStops.Add(routeStop);
            }

            // Configure the route parameters with the stops.
            _routeParameters.ClearStops();
            _routeParameters.SetStops(routeStops);

            // Create a list to hold barriers that should be routed around.
            List<PolygonBarrier> routeBarriers = new List<PolygonBarrier>();

            // Create barriers from the graphics.
            foreach (Graphic barrierGraphic in _barriersOverlay.Graphics)
            {
                // Get the polygon from the graphic.
                Polygon barrierPolygon = (Polygon) barrierGraphic.Geometry;

                // Create a barrier from the polygon.
                PolygonBarrier routeBarrier = new PolygonBarrier(barrierPolygon);

                // Add the barrier to the list of barriers.
                routeBarriers.Add(routeBarrier);
            }

            // Configure the route parameters with the barriers.
            _routeParameters.ClearPolygonBarriers();
            _routeParameters.SetPolygonBarriers(routeBarriers);

            // If the user allows stops to be re-ordered, the service will find the optimal order.
            _routeParameters.FindBestSequence = _reorderStopsCheckbox.Checked;

            // If the user has allowed re-ordering, but has a definite start point, tell the service to preserve the first stop.
            _routeParameters.PreserveFirstStop = _preserveFirstStopCheckbox.Checked;

            // If the user has allowed re-ordering, but has a definite end point, tell the service to preserve the last stop.
            _routeParameters.PreserveLastStop = _preserveLastStopCheckbox.Checked;

            // Calculate and show the route.
            CalculateAndShowRoute();
        }

        private async void CalculateAndShowRoute()
        {
            try
            {
                // Calculate the route.
                RouteResult calculatedRoute = await _routeTask.SolveRouteAsync(_routeParameters);

                // Get the first returned result.
                Route firstResult = calculatedRoute.Routes.First();

                // Get the route geometry - this is the line that shows the route.
                Polyline calculatedRouteGeometry = firstResult.RouteGeometry;

                // Create the route graphic from the geometry and the symbol.
                Graphic routeGraphic = new Graphic(calculatedRouteGeometry, _routeSymbol);

                // Clear any existing routes, then add this one to the map.
                _routeOverlay.Graphics.Clear();
                _routeOverlay.Graphics.Add(routeGraphic);

                // Add the directions to the textbox.
                PrepareDirectionsList(firstResult.DirectionManeuvers);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e);
                ShowMessage("Routing error", $"Couldn't calculate route. See debug output for details. Message: {e.Message}");
            }
        }

        private void PrepareDirectionsList(IReadOnlyList<DirectionManeuver> directions)
        {
            _directions = directions.ToList();
        }

        private void Route_Click(object sender, EventArgs e)
        {
            UpdateInterfaceState(SampleState.Routing);
            ConfigureThenRoute();
            UpdateInterfaceState(SampleState.Ready);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            UpdateInterfaceState(SampleState.NotReady);
            _stopsOverlay.Graphics.Clear();
            _barriersOverlay.Graphics.Clear();
            _routeOverlay.Graphics.Clear();
            _directions.Clear();
            UpdateInterfaceState(SampleState.Ready);
        }

        private void AddBarrier_Click(object sender, EventArgs e) => UpdateInterfaceState(SampleState.AddingBarriers);

        private void AddStop_Click(object sender, EventArgs e) => UpdateInterfaceState(SampleState.AddingStops);

        private void MyMapView_Tapped(object sender, GeoViewInputEventArgs e) => HandleMapTap(e.Location);

        private async Task<PictureMarkerSymbol> GetPictureMarker()
        {
            // Get current assembly that contains the image
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_blue.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 50;
            pinSymbol.Height = 50;
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;

            return pinSymbol;
        }

        private void UpdateInterfaceState(SampleState newState)
        {
            // Manage the UI state for the sample.
            _currentSampleState = newState;
            switch (_currentSampleState)
            {
                case SampleState.NotReady:
                    _addStopButton.Enabled = false;
                    _addBarrierButton.Enabled = false;
                    _resetButton.Enabled = false;
                    _reorderStopsCheckbox.Enabled = false;
                    _preserveLastStopCheckbox.Enabled = false;
                    _preserveFirstStopCheckbox.Enabled = false;
                    _directionsButton.Enabled = false;
                    _routeButton.Enabled = false;
                    _statusLabel.Text = "Preparing sample...";
                    break;
                case SampleState.AddingBarriers:
                    _statusLabel.Text = "Tap the map to add a barrier.";
                    break;
                case SampleState.AddingStops:
                    _statusLabel.Text = "Tap the map to add a stop.";
                    break;
                case SampleState.Ready:
                    _addStopButton.Enabled = true;
                    _addBarrierButton.Enabled = true;
                    _resetButton.Enabled = true;
                    _reorderStopsCheckbox.Enabled = true;
                    _preserveLastStopCheckbox.Enabled = true;
                    _preserveFirstStopCheckbox.Enabled = true;
                    _directionsButton.Enabled = true;
                    _routeButton.Enabled = true;
                    _busyIndicator.Hide();
                    _statusLabel.Text = "Click 'Add stop' or 'Add barrier', then tap on the map to add stops and barriers.";
                    break;
                case SampleState.Routing:
                    _busyIndicator.Show();
                    break;
            }
        }

        // Enum represents various UI states.
        private enum SampleState
        {
            NotReady,
            Ready,
            AddingStops,
            AddingBarriers,
            Routing
        }

        private void ShowMessage(string title, string detail) => new AlertDialog.Builder(this).SetTitle(title).SetMessage(detail).Show();

        private void CreateLayout()
        {
            // Load the layout for the sample from the .axml file.
            SetContentView(Resource.Layout.RouteAroundBarriers);

            // Hook up references to controls.
            _mapView = FindViewById<MapView>(Resource.Id.RouteAroundBarriers_MyMapView);
            _addStopButton = FindViewById<Button>(Resource.Id.RouteAroundBarriers_AddStopButton);
            _addBarrierButton = FindViewById<Button>(Resource.Id.RouteAroundBarriers_AddBarrierButton);
            _resetButton = FindViewById<Button>(Resource.Id.RouteAroundBarriers_ResetButton);
            _routeButton = FindViewById<Button>(Resource.Id.RouteAroundBarriers_RouteButton);
            _directionsButton = FindViewById<Button>(Resource.Id.RouteAroundBarriers_DirectionsButton);
            _statusLabel = FindViewById<TextView>(Resource.Id.RouteAroundBarriers_InstructionLabel);
            _reorderStopsCheckbox = FindViewById<CheckBox>(Resource.Id.RouteAroundBarriers_AllowReorderCheckbox);
            _preserveFirstStopCheckbox = FindViewById<CheckBox>(Resource.Id.RouteAroundBarriers_PreserveFirstStopCheckbox);
            _preserveLastStopCheckbox = FindViewById<CheckBox>(Resource.Id.RouteAroundBarriers_PreserveLastStopCheckbox);

            // Set up the alert dialog component that is shown while the route is being calculated.
            ConfigureBusyOverlayAlert();

            // Configure event handlers.
            _mapView.GeoViewTapped += MyMapView_Tapped;
            _addStopButton.Click += AddStop_Click;
            _addBarrierButton.Click += AddBarrier_Click;
            _resetButton.Click += Reset_Click;
            _routeButton.Click += Route_Click;
            _directionsButton.Click += ShowDirections_Click;
        }

        private void ConfigureBusyOverlayAlert()
        {
            // Custom UI to show an indicator that route work is in progress.
            LinearLayout alertLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );
            param.SetMargins(0, 10, 0, 10);

            TextView processingText = new TextView(this)
            {
                Text = "Calculating route...",
                LayoutParameters = param,
                Gravity = GravityFlags.Center,
            };

            ProgressBar progressBar = new ProgressBar(this)
            {
                Indeterminate = true,
                LayoutParameters = param,
                TextAlignment = TextAlignment.Center
            };

            // Add the text and progress bar to the Linear Layout.
            alertLayout.AddView(processingText);
            alertLayout.AddView(progressBar);

            // Create the alert dialog.
            _busyIndicator = new AlertDialog.Builder(this).Create();
            _busyIndicator.SetCanceledOnTouchOutside(false);
            _busyIndicator.Show();
            _busyIndicator.Cancel();

            // Add the layout to the alert
            _busyIndicator.AddContentView(alertLayout, param);
        }
    }
}