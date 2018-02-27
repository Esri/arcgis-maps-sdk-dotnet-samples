// Copyright 2017 Esri.
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
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace ArcGISRuntime.Samples.FindRoute
{
    [Register("FindRoute")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find route",
        "Network Analysis",
        "This sample illustrates how to solve a simple route between two locations.",
        "")]
    public class FindRoute : UIViewController
    {
        private MapView _myMapView;
        private UIToolbar _toolbar = new UIToolbar();

        UIButton solveRouteButton = new UIButton(UIButtonType.Plain);
        UIButton resetButton = new UIButton(UIButtonType.Plain);
        UIButton showDirectionsButton = new UIButton(UIButtonType.Plain);

        // List of stops on the route ('from' and 'to')
        private List<Stop> _routeStops;

        // List of direction maneuvers for completing the route (once solved)
        IReadOnlyList<DirectionManeuver> _directionsList;

        // Graphics overlay to display stops and the route result
        private GraphicsOverlay _routeGraphicsOverlay;

        // URI for the San Diego route service
        private Uri _sanDiegoRouteServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route");

        // URIs for picture marker images
        private Uri _checkedFlagIconUri = new Uri("https://static.arcgis.com/images/Symbols/Transportation/CheckeredFlag.png");
        private Uri _carIconUri = new Uri("https://static.arcgis.com/images/Symbols/Transportation/CarRedFront.png");

        public FindRoute()
        {
            Title = "Find route";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();

            solveRouteButton.SetTitle("Solve Route", UIControlState.Normal);
            solveRouteButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            solveRouteButton.TouchUpInside += SolveRouteButtonClick;

            resetButton.SetTitle("Reset", UIControlState.Normal);
            resetButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            resetButton.TouchUpInside += ResetButtonClick;

            showDirectionsButton.SetTitle("Directions", UIControlState.Normal);
            showDirectionsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            showDirectionsButton.TouchUpInside += ShowDirections;

            // Add the bar button items to an array of UIBarButtonItems
            View.AddSubviews(_myMapView, _toolbar, solveRouteButton, resetButton, showDirectionsButton);

            Initialize();
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
            flagSymbol.OffsetY = -10;

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
            _myMapView.Map = new Map(Basemap.CreateStreetsVector());
            _myMapView.GraphicsOverlays.Add(_routeGraphicsOverlay);
        }

        private async void SolveRouteButtonClick(object sender, EventArgs e)
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
            SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Purple, 8.0);

            // Create a new graphic for the route geometry and add it to the graphics overlay
            Graphic routeGraphic = new Graphic(routePolyline, routeSymbol);
            _routeGraphicsOverlay.Graphics.Add(routeGraphic);

            // Get a list of directions for the route and display it in the list box
            _directionsList = firstRoute.DirectionManeuvers;          
        }

        private void ResetButtonClick(object sender, EventArgs e)
        {
            // Clear the list of directions
            _directionsList = null;

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
        }

        private void ShowDirections(object sender, EventArgs e)
        {
            UITableViewController directionsTableController = new UITableViewController();
            DirectionsTableSource directionsSource = new DirectionsTableSource(_directionsList);
            directionsTableController.TableView.Source = directionsSource;
            NavigationController.PushViewController(directionsTableController, true);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for the Toolbar
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);

            // Update toolbar item layouts
            solveRouteButton.Frame = new CoreGraphics.CGRect(10, _toolbar.Frame.Top + 10, 100, 20);
            resetButton.Frame = new CoreGraphics.CGRect(120, _toolbar.Frame.Top + 10, 50, 20);
            showDirectionsButton.Frame = new CoreGraphics.CGRect(180, _toolbar.Frame.Top + 10, 100, 20);


            base.ViewDidLayoutSubviews();
        }
    }

    public class DirectionsTableSource : UITableViewSource
    {
        private IReadOnlyList<DirectionManeuver> _directionsList;
        private string _cellID = "TableCell";

        public DirectionsTableSource(IReadOnlyList<DirectionManeuver> directions)
        {
            _directionsList = directions;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(_cellID);
            string directionText = _directionsList[indexPath.Row].DirectionText;

            if(cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, _cellID);
            }

            cell.TextLabel.Text = directionText;
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _directionsList != null ? _directionsList.Count : 0;
        }
    }
}