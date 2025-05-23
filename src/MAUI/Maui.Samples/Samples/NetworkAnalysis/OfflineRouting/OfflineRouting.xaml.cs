﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System.Reflection;

using Color = System.Drawing.Color;

namespace ArcGIS.Samples.OfflineRouting
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Offline routing",
        category: "Network analysis",
        description: "Solve a route on-the-fly using offline data.",
        instructions: "Tap near a road to start adding a stop to the route, tap again to place it on the map. A number graphic will show its order in the route. After adding at least 2 stops, a route will display. Choose \"Fastest\" or \"Shortest\" to control how the route is optimized. The route will update on-the-fly while moving stops. The green box marks the boundary of the routable area provided by the offline data. This sample limits routes to 5 stops for performance reasons.",
        tags: new[] { "connectivity", "disconnected", "fastest", "locator", "navigation", "network analysis", "offline", "routing", "shortest", "turn-by-turn" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("567e14f3420d40c5a206e5c0284cf8fc")]
    public partial class OfflineRouting : ContentPage
    {
        // Graphics overlays for holding graphics.
        private GraphicsOverlay _stopsOverlay;
        private GraphicsOverlay _routeOverlay;

        // Route task and parameters.
        private RouteTask _offlineRouteTask;
        private RouteParameters _offlineRouteParameters;

        // List of travel modes, like 'Fastest' and 'Shortest'.
        private List<TravelMode> _availableTravelModes;

        // The area covered by the geodatabase used for offline routing.
        private readonly Envelope _routableArea = new Envelope(new MapPoint(-13045352.223196, 3864910.900750, 0, SpatialReferences.WebMercator),
            new MapPoint(-13024588.857198, 3838880.505604, 0, SpatialReferences.WebMercator));

        public OfflineRouting()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Get the paths to resources used by the sample.
                string basemapTilePath = DataManager.GetDataFolder("567e14f3420d40c5a206e5c0284cf8fc", "streetmap_SD.tpkx");
                string networkGeodatabasePath = DataManager.GetDataFolder("567e14f3420d40c5a206e5c0284cf8fc", "sandiego.geodatabase");

                // Create the tile cache representing the offline basemap.
                TileCache tiledBasemapCache = new TileCache(basemapTilePath);

                // Create a tiled layer to display the offline tiles.
                ArcGISTiledLayer offlineTiledLayer = new ArcGISTiledLayer(tiledBasemapCache);

                // Create a basemap based on the tile layer.
                Basemap offlineBasemap = new Basemap(offlineTiledLayer);

                // Create a new map with the offline basemap.
                Map theMap = new Map(offlineBasemap);

                // Set the initial viewpoint to show the routable area.
                theMap.InitialViewpoint = new Viewpoint(_routableArea);

                // Show the map in the map view.
                MyMapView.Map = theMap;

                // Create overlays for displaying the stops and the calculated route.
                _stopsOverlay = new GraphicsOverlay();
                _routeOverlay = new GraphicsOverlay();

                // Create a symbol and renderer for symbolizing the calculated route.
                SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 2);
                _routeOverlay.Renderer = new SimpleRenderer(routeSymbol);

                // Add the stops and route overlays to the map.
                MyMapView.GraphicsOverlays.Add(_stopsOverlay);
                MyMapView.GraphicsOverlays.Add(_routeOverlay);

                // Create the route task, referring to the offline geodatabase with the street network.
                _offlineRouteTask = await RouteTask.CreateAsync(networkGeodatabasePath, "Streets_ND");

                // Get the list of available travel modes.
                _availableTravelModes = _offlineRouteTask.RouteTaskInfo.TravelModes.ToList();

                // Update the UI with the travel modes list.
                TravelModesCollection.ItemsSource = _availableTravelModes.ToList();
                TravelModesCollection.SelectedItem = _availableTravelModes.First();

                // Create the default parameters.
                _offlineRouteParameters = await _offlineRouteTask.CreateDefaultParametersAsync();

                // Display the extent of the road network on the map.
                DisplayBoundaryMarker();

                // Now that the sample is ready, hook up the tap event.
                MyMapView.GeoViewTapped += MapView_Tapped;
                TravelModesCollection.SelectionChanged += TravelModesCollection_SelectionChanged;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                ShowMessage("Couldn't start sample", "There was a problem starting the sample. See debug output for details.");
            }
        }

        private void DisplayBoundaryMarker()
        {
            // Displaying the boundary marker helps avoid choosing a non-routable destination.
            SimpleLineSymbol boundarySymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.LawnGreen, 5);
            Graphic boundary = new Graphic(_routableArea, boundarySymbol);
            GraphicsOverlay boundaryOverlay = new GraphicsOverlay();
            boundaryOverlay.Graphics.Add(boundary);
            MyMapView.GraphicsOverlays.Add(boundaryOverlay);
        }

        private void ResetRoute()
        {
            _routeOverlay.Graphics.Clear();
            _stopsOverlay.Graphics.Clear();
        }

        private async Task UpdateRoute(TravelMode selectedTravelMode)
        {
            try
            {
                // Create a list of stops.
                List<Stop> stops = new List<Stop>();

                // Add a stop to the list for each graphic in the stops overlay.
                foreach (Graphic stopGraphic in _stopsOverlay.Graphics)
                {
                    Stop stop = new Stop((MapPoint)stopGraphic.Geometry);
                    stops.Add(stop);
                }

                if (stops.Count < 2)
                {
                    // Don't route, there's no where to go (and the route task would throw an exception).
                    return;
                }

                // Configure the route parameters with the list of stops.
                _offlineRouteParameters.SetStops(stops);

                // Configure the travel mode.
                _offlineRouteParameters.TravelMode = selectedTravelMode;

                // Solve the route.
                RouteResult offlineRouteResult = await _offlineRouteTask.SolveRouteAsync(_offlineRouteParameters);

                // Clear the old route result.
                _routeOverlay.Graphics.Clear();

                // Get the geometry from the route result.
                Polyline routeGeometry = offlineRouteResult.Routes.First().RouteGeometry;

                // Note: symbology left out here because the symbology was set once on the graphics overlay.
                Graphic routeGraphic = new Graphic(routeGeometry);

                // Display the route.
                _routeOverlay.Graphics.Add(routeGraphic);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                ShowMessage("Couldn't update route", "There was an error updating the route. See debug output for details.");
            }
        }

        private async Task AddStop(Point tappedPosition)
        {
            try
            {
                // Get the location on the map.
                MapPoint tappedLocation = MyMapView.ScreenToLocation(tappedPosition);

                // Name the stop by its number.
                string stopName = $"{_stopsOverlay.Graphics.Count + 1}";

                // Create a pushpin marker for the stop.
                PictureMarkerSymbol pushpinMarker = await GetPictureMarker();

                // Create the text symbol for labeling the stop.
                TextSymbol stopSymbol = new TextSymbol(stopName, Color.White, 15,
                    Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center, Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);
                stopSymbol.OffsetY = 15;

                // Create a combined symbol with the pushpin and the label.
                CompositeSymbol combinedSymbol = new CompositeSymbol(new MarkerSymbol[] { pushpinMarker, stopSymbol });

                // Create the graphic from the geometry and the symbology.
                Graphic newStopGraphic = new Graphic(tappedLocation, combinedSymbol);

                // Add the stop to the overlay.
                _stopsOverlay.Graphics.Add(newStopGraphic);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                ShowMessage("Couldn't add stop", "Couldn't add stop. See debug output for details.");
            }
        }

        private async Task<PictureMarkerSymbol> GetPictureMarker()
        {
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource and DoNotCopy.
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGIS.Resources.PictureMarkerSymbols.pin_blue.png");

            // Create new symbol using asynchronous factory method from stream.
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 50;
            pinSymbol.Height = 50;
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;

            return pinSymbol;
        }

        private void ResetButton_Click(object sender, EventArgs e) => ResetRoute();

        private void MapView_Tapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Make sure the user isn't adding too many stops.
            if (_stopsOverlay.Graphics.Count >= 5)
            {
                ShowMessage("Can't add stop.", "Sample limits to 5 stops per route.");
                return;
            }

            // Make sure the stop is valid before proceeding.
            if (!_routableArea.Contains(e.Location))
            {
                ShowMessage("Can't add stop.", "That location is outside of the area where offline routing data is available.");
                return;
            }

            // Add the stop for the tapped position.
            _ = AddStop(e.Position);

            // Update the route with the final list of stops.
            _ = UpdateRoute((TravelMode)TravelModesCollection.SelectedItem);
        }

        private void ShowMessage(string title, string detail)
        {
            DisplayAlert(title, detail, "OK");
        }

        private void TravelModesCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Enforce selection to prevent errors.
                if (TravelModesCollection.SelectedItem == null)
                {
                    TravelModesCollection.SelectedItem = _availableTravelModes.First();
                }

                // Update the route.
                _ = UpdateRoute((TravelMode)TravelModesCollection.SelectedItem);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowMessage("Couldn't change travel mode", "Couldn't change travel mode. See debug output for details.");
            }
        }
    }
}