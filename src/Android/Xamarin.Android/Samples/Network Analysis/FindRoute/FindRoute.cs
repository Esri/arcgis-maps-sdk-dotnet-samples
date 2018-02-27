// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ArcGISRuntime.Samples.FindRoute
{
    [Activity(Label = "FindRoute")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find a route",
        "Network Analysis",
        "This sample demonstrates how to solve for the best route between two locations on the map and display driving directions between them.",
        "")]
    public class FindRoute : Activity
    {
        private MapView _myMapView = new MapView();

        // List of stops on the route ('from' and 'to')
        private List<Stop> _routeStops;

        // Graphics overlay to display stops and the route result
        private GraphicsOverlay _routeGraphicsOverlay;

        // URI for the San Diego route service
        private Uri _sanDiegoRouteServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route");

        // URIs for picture marker images
        private Uri _checkedFlagIconUri = new Uri("https://static.arcgis.com/images/Symbols/Transportation/CheckeredFlag.png");
        private Uri _carIconUri = new Uri("https://static.arcgis.com/images/Symbols/Transportation/CarRedFront.png");

        // UI control to show/hide directions dialog (private scope so it can be enabled/disabled as needed)
        private Button _showHideDirectionsButton;

        // Dialog for showing driving directions
        private AlertDialog _directionsDialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Find a route";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new layout for the entire page
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a new layout for the toolbar (buttons)
            var toolbar = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create a button to solve the route and add it to the toolbar
            var solveRouteButton = new Button(this) { Text = "Solve Route" };
            solveRouteButton.Click += SolveRouteClick;
            toolbar.AddView(solveRouteButton);

            // Create a button to reset the route display, add it to the toolbar
            var resetButton = new Button(this) { Text = "Reset" };
            resetButton.Click += ResetClick;
            toolbar.AddView(resetButton);

            // Create a button to show or hide the route directions, add it to the toolbar
            _showHideDirectionsButton = new Button(this) { Text = "Directions" };
            _showHideDirectionsButton.Click += ShowDirectionsClick;
            _showHideDirectionsButton.Enabled = false;
            toolbar.AddView(_showHideDirectionsButton);

            // Add the toolbar to the layout
            layout.AddView(toolbar);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void Initialize()
        {
            // Define the route stop locations (points)
            MapPoint fromPoint = new MapPoint(-117.15494348793044, 32.706506537686927, SpatialReferences.Wgs84);
            MapPoint toPoint = new MapPoint(-117.14905088669816, 32.735308180609138, SpatialReferences.Wgs84);

            // Create Stop objects with the points and add them to a list of stops
            Stop stop1 = new Stop(fromPoint);
            Stop stop2 = new Stop(toPoint);
            _routeStops = new List<Stop> { stop1, stop2 };

            // Picture marker symbols: from = car, to = checkered flag
            PictureMarkerSymbol carSymbol = new PictureMarkerSymbol(_carIconUri);
            PictureMarkerSymbol flagSymbol = new PictureMarkerSymbol(_checkedFlagIconUri);

            // Add a slight offset (pixels) to the picture symbols
            carSymbol.OffsetX = -20;
            flagSymbol.OffsetY = -5;

            // Create graphics for the stops
            Graphic fromGraphic = new Graphic(fromPoint, carSymbol);
            Graphic toGraphic = new Graphic(toPoint, flagSymbol);

            // Create the graphics overlay and add the stop graphics
            _routeGraphicsOverlay = new GraphicsOverlay();
            _routeGraphicsOverlay.Graphics.Add(fromGraphic);
            _routeGraphicsOverlay.Graphics.Add(toGraphic);

            // Get an Envelope that covers the area of the stops (and a little more)
            Envelope routeStopsExtent = new Envelope(fromPoint, toPoint);
            EnvelopeBuilder envBuilder = new EnvelopeBuilder(routeStopsExtent);
            envBuilder.Expand(1.5);

            // Create a new viewpoint apply it to the map view when the spatial reference changes
            Viewpoint sanDiegoViewpoint = new Viewpoint(envBuilder.ToGeometry());
            _myMapView.SpatialReferenceChanged += (s, e) => _myMapView.SetViewpoint(sanDiegoViewpoint);

            // Add a new Map and the graphics overlay to the map view
            _myMapView.Map = new Map(Basemap.CreateStreets());
            _myMapView.GraphicsOverlays.Add(_routeGraphicsOverlay);
        }

        private async void SolveRouteClick(object sender, EventArgs e)
        {
            // Create a new route task using the San Diego route service URI
            RouteTask solveRouteTask = await RouteTask.CreateAsync(_sanDiegoRouteServiceUri);

            // Get the default parameters from the route task (defined with the service)
            RouteParameters routeParams = await solveRouteTask.CreateDefaultParametersAsync();

            // Make some changes to the default parameters
            routeParams.ReturnStops = true;
            routeParams.ReturnDirections = true;

            // Set the list of route stops that were defined at startup
            routeParams.SetStops(_routeStops);

            // Solve for the best route between the stops and store the result
            RouteResult solveRouteResult = await solveRouteTask.SolveRouteAsync(routeParams);

            // Get the first (should be only) route from the result
            Route firstRoute = solveRouteResult.Routes.FirstOrDefault();

            // Get the route geometry (polyline)
            Polyline routePolyline = firstRoute.RouteGeometry;

            // Create a thick purple line symbol for the route
            SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Purple, 8.0);

            // Create a new graphic for the route geometry and add it to the graphics overlay
            Graphic routeGraphic = new Graphic(routePolyline, routeSymbol);
            _routeGraphicsOverlay.Graphics.Add(routeGraphic);

            // Get a list of directions for the route and display it in the list box
            var directions = from d in firstRoute.DirectionManeuvers select d.DirectionText;
            CreateDirectionsDialog(directions);
            _showHideDirectionsButton.Enabled = true;
        }

        private void ResetClick(object sender, EventArgs e)
        {
            // Remove the route graphic from the graphics overlay (only line graphic in the collection)
            int graphicsCount = _routeGraphicsOverlay.Graphics.Count;
            for (var i = graphicsCount; i > 0; i--)
            {
                // Get this graphic and see if it has line geometry
                Graphic g = _routeGraphicsOverlay.Graphics[i - 1];
                if (g.Geometry.GeometryType == GeometryType.Polyline)
                {
                    // Remove the graphic from the overlay
                    _routeGraphicsOverlay.Graphics.Remove(g);
                }
            }

            // Disable the button to show the directions dialog
            _showHideDirectionsButton.Enabled = false;
        }

        private void ShowDirectionsClick(object sender, EventArgs e)
        {
            // Show the directions dialog
            if (_directionsDialog != null)
            {                
                _directionsDialog.Show();
            }
        }

        private void CreateDirectionsDialog(IEnumerable<string> directions)
        {
            // Create a dialog to show route directions
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);

            // Create the layout
            LinearLayout dialogLayout = new LinearLayout(this);
            dialogLayout.Orientation = Orientation.Vertical;            

            // Create a list box for showing the route directions
            var directionsList = new ListView(this);
            var directionsAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, directions.ToArray());
            directionsList.Adapter = directionsAdapter;
            dialogLayout.AddView(directionsList);

            // Add the controls to the dialog
            dialogBuilder.SetView(dialogLayout);
            dialogBuilder.SetTitle("Route Directions");

            // Create the dialog (don't show it)
            _directionsDialog = dialogBuilder.Create();
        }

    }
}