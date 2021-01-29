// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FindRoute
{
    [Register("FindRoute")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Find route",
        category: "Network analysis",
        description: "Display directions for a route between two points.",
        instructions: "For simplicity, the sample comes loaded with a start and end stop. You can tap on the Find Route button to display a route between these stops. Once the route is generated, turn-by-turn directions are shown in a list.",
        tags: new[] { "directions", "driving", "navigation", "network", "network analysis", "route", "routing", "shortest path", "turn-by-turn" })]
    public class FindRoute : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _solveRouteButton;
        private UIBarButtonItem _resetButton;
        private UIBarButtonItem _directionsButton;

        // List of stops on the route ('from' and 'to').
        private List<Stop> _routeStops;

        // List of direction maneuvers for completing the route (once solved).
        private IReadOnlyList<DirectionManeuver> _directionsList;

        // Graphics overlay to display stops and the route result.
        private GraphicsOverlay _routeGraphicsOverlay;

        // URI for the San Diego route service.
        private readonly Uri _sanDiegoRouteServiceUri =
            new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route");

        // URIs for picture marker images.
        private readonly Uri _checkedFlagIconUri =
            new Uri("https://static.arcgis.com/images/Symbols/Transportation/CheckeredFlag.png");

        private readonly Uri _carIconUri =
            new Uri("https://static.arcgis.com/images/Symbols/Transportation/CarRedFront.png");

        // San Diego viewpoint.
        Viewpoint _sanDiegoViewpoint;

        public FindRoute()
        {
            Title = "Find route";
        }

        private void Initialize()
        {
            // Define the route stop locations (points).
            MapPoint fromPoint = new MapPoint(-117.15494348793044, 32.706506537686927, SpatialReferences.Wgs84);
            MapPoint toPoint = new MapPoint(-117.14905088669816, 32.735308180609138, SpatialReferences.Wgs84);

            // Create Stop objects with the points and add them to a list of stops.
            Stop stop1 = new Stop(fromPoint);
            Stop stop2 = new Stop(toPoint);
            _routeStops = new List<Stop> {stop1, stop2};

            // Picture marker symbols: from = car, to = checkered flag.
            PictureMarkerSymbol carSymbol = new PictureMarkerSymbol(_carIconUri);
            PictureMarkerSymbol flagSymbol = new PictureMarkerSymbol(_checkedFlagIconUri);

            // Add a slight offset (pixels) to the picture symbols.
            carSymbol.OffsetX = -20;
            flagSymbol.OffsetY = -10;

            // Create graphics for the stops.
            Graphic fromGraphic = new Graphic(fromPoint, carSymbol) { ZIndex = 1 };
            Graphic toGraphic = new Graphic(toPoint, flagSymbol) { ZIndex = 1 };

            // Create the graphics overlay and add the stop graphics.
            _routeGraphicsOverlay = new GraphicsOverlay();
            _routeGraphicsOverlay.Graphics.Add(fromGraphic);
            _routeGraphicsOverlay.Graphics.Add(toGraphic);

            // Get an Envelope that covers the area of the stops (and a little more).
            Envelope routeStopsExtent = new Envelope(fromPoint, toPoint);
            EnvelopeBuilder envBuilder = new EnvelopeBuilder(routeStopsExtent);
            envBuilder.Expand(1.5);

            // Create a new viewpoint apply it to the map view when the spatial reference changes.
            _sanDiegoViewpoint = new Viewpoint(envBuilder.ToGeometry());
            _myMapView.SpatialReferenceChanged += MapView_SpatialReferenceChanged;

            // Add a new Map and the graphics overlay to the map view.
            _myMapView.Map = new Map(BasemapStyle.ArcGISStreets);
            _myMapView.GraphicsOverlays.Add(_routeGraphicsOverlay);
        }

        private void MapView_SpatialReferenceChanged(object sender, EventArgs e)
        {
            // Unsubscribe from the event.
            _myMapView.SpatialReferenceChanged -= MapView_SpatialReferenceChanged;

            // Set the viewpoint.
            _myMapView.SetViewpoint(_sanDiegoViewpoint);
        }

        private async void SolveRouteButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a new route task using the San Diego route service URI.
                RouteTask solveRouteTask = await RouteTask.CreateAsync(_sanDiegoRouteServiceUri);

                // Get the default parameters from the route task (defined with the service).
                RouteParameters routeParams = await solveRouteTask.CreateDefaultParametersAsync();

                // Make some changes to the default parameters.
                routeParams.ReturnStops = true;
                routeParams.ReturnDirections = true;

                // Set the list of route stops that were defined at startup.
                routeParams.SetStops(_routeStops);

                // Solve for the best route between the stops and store the result.
                RouteResult solveRouteResult = await solveRouteTask.SolveRouteAsync(routeParams);

                // Get the first (should be only) route from the result.
                Route firstRoute = solveRouteResult.Routes.First();

                // Get the route geometry (polyline).
                Polyline routePolyline = firstRoute.RouteGeometry;

                // Create a thick purple line symbol for the route.
                SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Purple, 8.0);

                // Create a new graphic for the route geometry and add it to the graphics overlay.
                Graphic routeGraphic = new Graphic(routePolyline, routeSymbol) { ZIndex = 0 };
                _routeGraphicsOverlay.Graphics.Add(routeGraphic);

                // Get a list of directions for the route and display it in the list box.
                _directionsList = firstRoute.DirectionManeuvers;

                // Enable the directions button.
                _directionsButton.Enabled = true;
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            // Clear the list of directions.
            _directionsList = null;
            _directionsButton.Enabled = false;

            // Remove the route graphic from the graphics overlay (only line graphic in the collection).
            int graphicsCount = _routeGraphicsOverlay.Graphics.Count;
            for (int i = graphicsCount; i > 0; i--)
            {
                // Get this graphic and see if it has line geometry.
                Graphic graphic = _routeGraphicsOverlay.Graphics[i - 1];
                if (graphic.Geometry.GeometryType == GeometryType.Polyline)
                {
                    // Remove the graphic from the overlay.
                    _routeGraphicsOverlay.Graphics.Remove(graphic);
                }
            }
        }

        private void ShowDirections(object sender, EventArgs e)
        {
            UITableViewController directionsTableController = new UITableViewController
            {
                TableView = {Source = new DirectionsTableSource(_directionsList)}
            };
            NavigationController.PushViewController(directionsTableController, true);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _solveRouteButton = new UIBarButtonItem();
            _solveRouteButton.Title = "Solve route";

            _resetButton = new UIBarButtonItem();
            _resetButton.Title = "Reset";

            _directionsButton = new UIBarButtonItem();
            _directionsButton.Title = "Directions";
            _directionsButton.Enabled = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _solveRouteButton,
                _resetButton,
                _directionsButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _solveRouteButton.Clicked += SolveRouteButton_Click;
            _directionsButton.Clicked += ShowDirections;
            _resetButton.Clicked += ResetButton_Click;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _solveRouteButton.Clicked -= SolveRouteButton_Click;
            _directionsButton.Clicked -= ShowDirections;
            _resetButton.Clicked -= ResetButton_Click;
        }
    }

    public class DirectionsTableSource : UITableViewSource
    {
        private readonly IReadOnlyList<DirectionManeuver> _directionsList;
        private const string CellId = "TableCell";

        public DirectionsTableSource(IReadOnlyList<DirectionManeuver> directions)
        {
            _directionsList = directions;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellId);
            string directionText = _directionsList[indexPath.Row].DirectionText;

            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellId);
                cell.TextLabel.Lines = 2;
                cell.TextLabel.AdjustsFontSizeToFitWidth = true;
            }

            cell.TextLabel.Text = directionText;
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _directionsList?.Count ?? 0;
        }
    }
}