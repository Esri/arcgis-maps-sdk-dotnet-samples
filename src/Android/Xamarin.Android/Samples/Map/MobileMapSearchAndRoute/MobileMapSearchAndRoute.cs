// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.MobileMapSearchAndRoute
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Mobile map (search and route)",
        "Map",
        "Display maps and use locators to enable search and routing offline using a Mobile Map Package.",
        "A list of maps from a mobile map package will be displayed. If the map contains transportation networks, the list item will have a navigation icon. Tap on a map in the list to open it. If a locator task is available, tap on the map to reverse geocode the location's address. If transportation networks are available, a route will be calculated between geocode locations.",
        "disconnected", "field mobility", "geocode", "network", "network analysis", "offline", "routing", "search", "transportation")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("260eb6535c824209964cf281766ebe43")]
    public class MobileMapSearchAndRoute : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private LinearLayout _mapListView;

        // Hold references to map resources for easy access.
        private List<Map> _maps = new List<Map>();
        private LocatorTask _packageLocator;
        private TransportationNetworkDataset _networkDataset;

        // Overlays for use in visualizing routes.
        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _waypointOverlay;

        // Track the start and end point for route calculation.
        private MapPoint _startPoint;
        private MapPoint _endPoint;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Mobile map (search and route)";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Get the path to the package on disk.
            string filePath = DataManager.GetDataFolder("260eb6535c824209964cf281766ebe43", "SanFrancisco.mmpk");

            // Open the map package.
            MobileMapPackage package = await OpenMobileMapPackage(filePath);

            // Populate the list of maps.
            foreach (Map map in package.Maps)
            {
                await map.LoadAsync();
                _maps.Add(map);
            }

            // Show the first map by default.
            _myMapView.Map = _maps.First();

            // Get the locator task from the package.
            _packageLocator = package.LocatorTask;

            // Create and add an overlay for showing a route.
            _routeOverlay = new GraphicsOverlay();
            _routeOverlay.Renderer = new SimpleRenderer
            {
                Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 3)
            };
            _myMapView.GraphicsOverlays.Add(_routeOverlay);

            // Create and add an overlay for showing waypoints/stops.
            _waypointOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_waypointOverlay);

            // Enable tap-to-reverse geocode and tap-to-route.
            _myMapView.GeoViewTapped += MapView_Tapped;

            // Show list of maps in the UI.
            configureMapsButtons();
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

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Handle routing.
            try
            {
                await ProcessRouteRequest(e.Location);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                new AlertDialog.Builder(this).SetMessage("Couldn't geocode or route.").SetTitle("Error").Show();
            }
        }

        private async Task ShowGeocodeResult(MapPoint tappedPoint)
        {
            // Reverse geocode to get an address.
            IReadOnlyList<GeocodeResult> results = await _packageLocator.ReverseGeocodeAsync(tappedPoint);

            // Process the address into usable strings.
            string address = results.First().Label;

            // Show the address in a callout.
            _myMapView.ShowCalloutAt(tappedPoint, new CalloutDefinition(address));
        }

        private async Task ProcessRouteRequest(MapPoint tappedPoint)
        {
            // Clear any existing overlays.
            _routeOverlay.Graphics.Clear();
            _myMapView.DismissCallout();

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
                _waypointOverlay.Graphics.Add(await GraphicForPoint(_startPoint));

                return;
            }

            if (_endPoint == null)
            {
                await ShowGeocodeResult(tappedPoint);

                // Show the end point.
                _endPoint = tappedPoint;
                _waypointOverlay.Graphics.Add(await GraphicForPoint(_endPoint));

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

        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource and DoNotCopy.
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream.
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 60;
            pinSymbol.Height = 60;
            // The image is a pin; offset the image so that the pinpoint
            //     is on the point rather than the image's true center.
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;
            return new Graphic(point, pinSymbol);
        }

        private void Map_Selected(Map selectedMap)
        {
            // Clear existing overlays.
            _myMapView.DismissCallout();
            _waypointOverlay.Graphics.Clear();
            _routeOverlay.Graphics.Clear();

            try
            {
                // Show the map in the view.
                _myMapView.Map = selectedMap;

                // Get the transportation network if there is one. Will be set to null otherwise.
                _networkDataset = selectedMap.TransportationNetworks.FirstOrDefault();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                new AlertDialog.Builder(this).SetMessage(exception.ToString()).SetTitle("Couldn't select map").Show();
            }
        }

        private async void configureMapsButtons()
        {
            foreach (Map map in _maps)
            {
                Button mapButton = new Button(this);
                if (map.TransportationNetworks.Any())
                {
                    mapButton.Text = $"{map.Item.Title} (✔)";
                }
                else
                {
                    mapButton.Text = $"{map.Item.Title}";
                }

                mapButton.SetTextColor(Color.Black);
                mapButton.Background = Drawable.CreateFromStream(await map.Item.Thumbnail.GetEncodedBufferAsync(), "");
                mapButton.Click += (o, e) => { Map_Selected(map); };
                _mapListView.AddView(mapButton);

                LinearLayout.LayoutParams lparams = (LinearLayout.LayoutParams)mapButton.LayoutParameters;
                lparams.SetMargins(5, 5, 0, 5);
                mapButton.LayoutParameters = lparams;
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a help label.
            TextView helpLabel = new TextView(this);
            helpLabel.Text = "Tap to show address. If a network is available, you can show a route between tapped points. Maps with networks are denoted with ✔.";
            layout.AddView(helpLabel);

            // Add space for adding options for each map.
            _mapListView = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            layout.AddView(_mapListView);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}