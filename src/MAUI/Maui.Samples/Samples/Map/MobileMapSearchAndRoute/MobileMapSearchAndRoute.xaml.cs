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
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace ArcGIS.Samples.MobileMapSearchAndRoute
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Mobile map (search and route)",
        category: "Map",
        description: "Display maps and use locators to enable search and routing offline using a Mobile Map Package.",
        instructions: "A list of maps from a mobile map package will be displayed. If the map contains transportation networks, the list item will have a navigation icon. Tap on a map in the list to open it. If a locator task is available, tap on the map to reverse geocode the location's address. If transportation networks are available, a route will be calculated between geocode locations.",
        tags: new[] { "disconnected", "field mobility", "geocode", "network", "network analysis", "offline", "routing", "search", "transportation" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("260eb6535c824209964cf281766ebe43")]
    public partial class MobileMapSearchAndRoute : ContentPage
    {
        // Hold references to map resources for easy access.
        public ObservableCollection<Map> Maps { get; } = new ObservableCollection<Map>();

        private LocatorTask _packageLocator;
        private TransportationNetworkDataset _networkDataset;

        // Overlays for use in visualizing routes.
        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _waypointOverlay;

        private Symbol _pointGraphic = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Blue, 15);

        // Track the start and end point for route calculation.
        private MapPoint _startPoint;
        private MapPoint _endPoint;

        public MobileMapSearchAndRoute()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Get the path to the package on disk.
            string filePath = DataManager.GetDataFolder("260eb6535c824209964cf281766ebe43", "SanFrancisco.mmpk");

            // Open the map package.
            MobileMapPackage package = await OpenMobileMapPackage(filePath);

            // Populate the list of maps.
            foreach (Map map in package.Maps)
            {
                await map.LoadAsync();
                Maps.Add(map);
            }

            // Show the first map by default.
            MyMapView.Map = Maps.First();

            // Populate the map list in the UI.
            MapListView.ItemsSource = Maps;

            // Get the locator task from the package.
            _packageLocator = package.LocatorTask;

            // Create and add an overlay for showing a route.
            _routeOverlay = new GraphicsOverlay();
            _routeOverlay.Renderer = new SimpleRenderer
            {
                Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 3)
            };
            MyMapView.GraphicsOverlays.Add(_routeOverlay);

            // Create and add an overlay for showing waypoints/stops.
            _waypointOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_waypointOverlay);

            // Enable tap-to-reverse geocode and tap-to-route.
            MyMapView.GeoViewTapped += MapView_Tapped;
        }

        private async Task<MobileMapPackage> OpenMobileMapPackage(string path)
        {
            // Open the map package.
            MobileMapPackage package = await MobileMapPackage.OpenAsync(path);

            // Load the package.
            await package.LoadAsync();

            // Return the opened package.
            return package;
        }

        private async void MapView_Tapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Handle routing.
            try
            {
                await ProcessRouteRequest(e.Location);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                await Application.Current.MainPage.DisplayAlert("Error", "Couldn't geocode or route.", "OK");
            }
        }

        private async Task ShowGeocodeResult(MapPoint tappedPoint)
        {
            // Reverse geocode to get an address.
            IReadOnlyList<GeocodeResult> results = await _packageLocator.ReverseGeocodeAsync(tappedPoint);

            // Process the address into usable strings.
            string address = results.First().Label;

            // Show the address in a callout.
            MyMapView.ShowCalloutAt(tappedPoint, new CalloutDefinition(address));
        }

        private async Task ProcessRouteRequest(MapPoint tappedPoint)
        {
            // Clear any existing overlays.
            _routeOverlay.Graphics.Clear();
            MyMapView.DismissCallout();

            // Return if there is no network available for routing.
            if (_networkDataset == null)
            {
                await ShowGeocodeResult(tappedPoint);
                return;
            }

            // Set the start point if it hasn't been set.
            if (_startPoint == null)
            {
                _startPoint = tappedPoint;

                await ShowGeocodeResult(tappedPoint);

                // Show the start point.
                _waypointOverlay.Graphics.Add(new Graphic(_startPoint, _pointGraphic));

                return;
            }

            if (_endPoint == null)
            {
                await ShowGeocodeResult(tappedPoint);

                // Show the end point.
                _endPoint = tappedPoint;
                _waypointOverlay.Graphics.Add(new Graphic(_endPoint, _pointGraphic));

                // Create the route task from the local network dataset.
                RouteTask routingTask = await RouteTask.CreateAsync(_networkDataset);

                // Configure route parameters for the route between the two tapped points.
                RouteParameters routingParameters = await routingTask.CreateDefaultParametersAsync();
                List<Stop> stops = new List<Stop> { new Stop(_startPoint), new Stop(_endPoint) };
                routingParameters.SetStops(stops);

                // Get the first route result.
                RouteResult result = await routingTask.SolveRouteAsync(routingParameters);
                Route firstRoute = result.Routes.First();

                // Show the route on the map. Note that symbology for the graphics overlay is defined in Initialize().
                Polyline routeLine = firstRoute.RouteGeometry;
                _routeOverlay.Graphics.Add(new Graphic(routeLine));

                return;
            }

            // Reset graphics and route.
            _routeOverlay.Graphics.Clear();
            _waypointOverlay.Graphics.Clear();
            _startPoint = null;
            _endPoint = null;
        }

        private async void Map_Selected(object sender, EventArgs e)
        {
            // Clear existing overlays.
            MyMapView.DismissCallout();
            _waypointOverlay.Graphics.Clear();
            _routeOverlay.Graphics.Clear();

            try
            {
                ListView sendingList = (ListView)sender;
                // Get the selected map.
                Map selectedMap = sendingList.SelectedItem as Map;

                // Show the map in the view.
                MyMapView.Map = selectedMap;

                // Get the transportation network if there is one. Will be set to null otherwise.
                _networkDataset = selectedMap.TransportationNetworks.FirstOrDefault();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }
    }
}