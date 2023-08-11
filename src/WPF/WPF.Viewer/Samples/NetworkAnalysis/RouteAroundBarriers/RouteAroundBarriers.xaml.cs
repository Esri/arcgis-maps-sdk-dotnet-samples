// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

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
using System.Windows;
using System.Xml.Linq;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;

using Symbology = Esri.ArcGISRuntime.Symbology;

namespace ArcGIS.WPF.Samples.RouteAroundBarriers
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Route around barriers",
        category: "Network analysis",
        description: "Find a route that reaches all stops without crossing any barriers.",
        instructions: "Click 'Add stop' to add stops to the route. Click 'Add barrier' to add areas that can't be crossed by the route. Click 'Route' to find the route and display it. Select 'Allow stops to be re-ordered' to find the best sequence. Select 'Preserve first stop' if there is a known start point, and 'Preserve last stop' if there is a known final destination.",
        tags: new[] { "barriers", "best sequence", "directions", "maneuver", "network analysis", "routing", "sequence", "stop order", "stops" })]
    public partial class RouteAroundBarriers
    {
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

        public RouteAroundBarriers()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Update interface state.
                UpdateInterfaceState(SampleState.NotReady);

                // Create the map with a basemap.
                Map sampleMap = new Map(BasemapStyle.ArcGISTopographic);
                sampleMap.InitialViewpoint = new Viewpoint(32.7157, -117.1611, 1e5);
                MyMapView.Map = sampleMap;

                // Create the graphics overlays. These will manage rendering of route, direction, stop, and barrier graphics.
                _routeOverlay = new GraphicsOverlay();
                _stopsOverlay = new GraphicsOverlay();
                _barriersOverlay = new GraphicsOverlay();

                // Add graphics overlays to the map view.
                MyMapView.GraphicsOverlays.Add(_routeOverlay);
                MyMapView.GraphicsOverlays.Add(_stopsOverlay);
                MyMapView.GraphicsOverlays.Add(_barriersOverlay);

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

        private async Task HandleMapTap(MapPoint mapLocation)
        {
            // Normalize geometry - important for geometries that will be sent to a server for processing.
            mapLocation = (MapPoint)mapLocation.NormalizeCentralMeridian();

            switch (_currentSampleState)
            {
                case SampleState.AddingBarriers:
                    // Buffer the tapped point to create a larger barrier.
                    Geometry bufferedGeometry = mapLocation.BufferGeodetic(500, LinearUnits.Meters);

                    // Create the graphic to show the barrier.
                    Graphic barrierGraphic = new Graphic(bufferedGeometry, _barrierSymbol);

                    // Add the graphic to the overlay - this will cause it to appear on the map.
                    _barriersOverlay.Graphics.Add(barrierGraphic);
                    break;

                case SampleState.AddingStops:
                    try
                    {
                        // Get the name of this stop.
                        string stopName = $"{_stopsOverlay.Graphics.Count + 1}";

                        // Create the marker to show underneath the stop number.
                        PictureMarkerSymbol pushpinMarker = await GetPictureMarker();

                        // Create the text symbol for showing the stop.
                        TextSymbol stopSymbol = new TextSymbol(stopName, System.Drawing.Color.White, 15,
                            Symbology.HorizontalAlignment.Center, Symbology.VerticalAlignment.Middle);
                        stopSymbol.OffsetY = 15;

                        CompositeSymbol combinedSymbol = new CompositeSymbol(new MarkerSymbol[] { pushpinMarker, stopSymbol });

                        // Create the graphic to show the stop.
                        Graphic stopGraphic = new Graphic(mapLocation, combinedSymbol);

                        // Add the graphic to the overlay - this will cause it to appear on the map.
                        _stopsOverlay.Graphics.Add(stopGraphic);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
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
                MapPoint stopPoint = (MapPoint)stopGraphic.Geometry;

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
                Polygon barrierPolygon = (Polygon)barrierGraphic.Geometry;

                // Create a barrier from the polygon.
                PolygonBarrier routeBarrier = new PolygonBarrier(barrierPolygon);

                // Add the barrier to the list of barriers.
                routeBarriers.Add(routeBarrier);
            }

            // Configure the route parameters with the barriers.
            _routeParameters.ClearPolygonBarriers();
            _routeParameters.SetPolygonBarriers(routeBarriers);

            // If the user allows stops to be re-ordered, the service will find the optimal order.
            _routeParameters.FindBestSequence = AllowReorderStopsCheckbox.IsChecked == true;

            // If the user has allowed re-ordering, but has a definite start point, tell the service to preserve the first stop.
            _routeParameters.PreserveFirstStop = PreserveFirstStopCheckbox.IsChecked == true;

            // If the user has allowed re-ordering, but has a definite end point, tell the service to preserve the last stop.
            _routeParameters.PreserveLastStop = PreserveLastStopCheckbox.IsChecked == true;

            // Calculate and show the route.
            _ = CalculateAndShowRoute();
        }

        private async Task CalculateAndShowRoute()
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
            // Show the text for each step on the route in the UI.
            DirectionsListBox.ItemsSource = directions;
        }

        private void MyMapView_OnGeoViewTapped(object sender, GeoViewInputEventArgs e) => _ = HandleMapTap(e.Location);

        private void AddStop_Clicked(object sender, RoutedEventArgs e) => UpdateInterfaceState(SampleState.AddingStops);

        private void AddBarrier_Clicked(object sender, RoutedEventArgs e) => UpdateInterfaceState(SampleState.AddingBarriers);

        private void ResetRoute_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateInterfaceState(SampleState.NotReady);
            _stopsOverlay.Graphics.Clear();
            _barriersOverlay.Graphics.Clear();
            _routeOverlay.Graphics.Clear();
            DirectionsListBox.ItemsSource = null;
            UpdateInterfaceState(SampleState.Ready);
        }

        private void RouteButton_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateInterfaceState(SampleState.Routing);
            ConfigureThenRoute();
            UpdateInterfaceState(SampleState.Ready);
        }

        private async Task<PictureMarkerSymbol> GetPictureMarker()
        {
            // Hold a reference to the picture marker symbol
            PictureMarkerSymbol pinSymbol;

            // Get current assembly that contains the image
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get the resource name of the blue pin image
            string resourceStreamName = this.GetType().Assembly.GetManifestResourceNames().Single(str => str.EndsWith("pin_blue.png"));

            // Load the blue pin resource stream
            using (Stream resourceStream = this.GetType().Assembly.
                       GetManifestResourceStream(resourceStreamName))
            {
                // Create new symbol using asynchronous factory method from stream
                pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
                pinSymbol.Width = 50;
                pinSymbol.Height = 50;
                pinSymbol.LeaderOffsetX = 30;
                pinSymbol.OffsetY = 14;
            }

            return pinSymbol;
        }

        private void UpdateInterfaceState(SampleState newState)
        {
            // Manage the UI state for the sample.
            _currentSampleState = newState;
            switch (_currentSampleState)
            {
                case SampleState.NotReady:
                    AddStopButton.IsEnabled = false;
                    AddBarrierButton.IsEnabled = false;
                    ResetRoutingButton.IsEnabled = false;
                    AllowReorderStopsCheckbox.IsEnabled = false;
                    PreserveFirstStopCheckbox.IsEnabled = false;
                    PreserveLastStopCheckbox.IsEnabled = false;
                    DirectionsListBox.IsEnabled = false;
                    CalculateRouteButton.IsEnabled = false;
                    StatusLabel.Text = "Preparing sample...";
                    break;

                case SampleState.AddingBarriers:
                    StatusLabel.Text = "Tap the map to add a barrier.";
                    break;

                case SampleState.AddingStops:
                    StatusLabel.Text = "Tap the map to add a stop.";
                    break;

                case SampleState.Ready:
                    AddStopButton.IsEnabled = true;
                    AddBarrierButton.IsEnabled = true;
                    ResetRoutingButton.IsEnabled = true;
                    AllowReorderStopsCheckbox.IsEnabled = true;
                    PreserveLastStopCheckbox.IsEnabled = true;
                    PreserveFirstStopCheckbox.IsEnabled = true;
                    DirectionsListBox.IsEnabled = true;
                    CalculateRouteButton.IsEnabled = true;
                    StatusLabel.Text = "Click 'Add stop' or 'Add barrier', then tap on the map to add stops and barriers.";
                    BusyOverlay.Visibility = Visibility.Collapsed;
                    break;

                case SampleState.Routing:
                    BusyOverlay.Visibility = Visibility.Visible;
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

        private void ShowMessage(string title, string detail) => MessageBox.Show(detail, title);
    }
}