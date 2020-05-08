// Copyright 2018 Esri.
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

namespace ArcGISRuntime.Samples.ClosestFacility
{
    [Register("ClosestFacility")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Find closest facility to an incident (interactive)",
        category: "Network analysis",
        description: "Find a route to the closest facility from a location.",
        instructions: "Tap near any of the hospitals and a route will be displayed from that clicked location to the nearest hospital.",
        tags: new[] { "incident", "network analysis", "route", "search" })]
    public class ClosestFacility : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // Holds locations of hospitals around San Diego.
        private List<Facility> _facilities;

        // Graphics overlays for facilities and incidents.
        private GraphicsOverlay _facilityGraphicsOverlay;

        // Symbol for facilities.
        private PictureMarkerSymbol _facilitySymbol;

        // Overlay for the incident.
        private GraphicsOverlay _incidentGraphicsOverlay;

        // Black cross where user clicked.
        private MapPoint _incidentPoint;

        // Symbol for the incident.
        private SimpleMarkerSymbol _incidentSymbol;

        // Used to display route between incident and facility to mapview.
        private SimpleLineSymbol _routeSymbol;

        // Solves task to find closest route between an incident and a facility.
        private ClosestFacilityTask _task;

        public ClosestFacility()
        {
            Title = "Closest facility";
        }

        private async void Initialize()
        {
            // Construct the map and set the MapView.Map property.
            Map map = new Map(Basemap.CreateLightGrayCanvasVector());
            _myMapView.Map = map;

            try
            {
                // Create a ClosestFacilityTask using the San Diego Uri.
                _task = await ClosestFacilityTask.CreateAsync(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility"));

                // List of facilities to be placed around San Diego area.
                _facilities = new List<Facility>
                {
                    new Facility(new MapPoint(-1.3042129900625112E7, 3860127.9479775648, SpatialReferences.WebMercator)),
                    new Facility(new MapPoint(-1.3042193400557665E7, 3862448.873041752, SpatialReferences.WebMercator)),
                    new Facility(new MapPoint(-1.3046882875518233E7, 3862704.9896770366, SpatialReferences.WebMercator)),
                    new Facility(new MapPoint(-1.3040539754780494E7, 3862924.5938606677, SpatialReferences.WebMercator)),
                    new Facility(new MapPoint(-1.3042571225655518E7, 3858981.773018156, SpatialReferences.WebMercator)),
                    new Facility(new MapPoint(-1.3039784633928463E7, 3856692.5980474586, SpatialReferences.WebMercator)),
                    new Facility(new MapPoint(-1.3049023883956768E7, 3861993.789732541, SpatialReferences.WebMercator))
                };

                // Center the map on the San Diego facilities.
                Envelope fullExtent = GeometryEngine.CombineExtents(_facilities.Select(facility => facility.Geometry));
                await _myMapView.SetViewpointGeometryAsync(fullExtent, 50);

                // Create a symbol for displaying facilities.
                _facilitySymbol = new PictureMarkerSymbol(new Uri("https://static.arcgis.com/images/Symbols/SafetyHealth/Hospital.png"))
                {
                    Height = 30,
                    Width = 30
                };

                // Incident symbol.
                _incidentSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.FromArgb(255, 0, 0, 0), 30);

                // Route to hospital symbol.
                _routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(255, 0, 0, 255), 5.0f);

                // Create Graphics Overlays for incidents and facilities.
                _incidentGraphicsOverlay = new GraphicsOverlay();
                _facilityGraphicsOverlay = new GraphicsOverlay();

                // Create a graphic and add to graphics overlay for each facility.
                foreach (Facility facility in _facilities)
                {
                    _facilityGraphicsOverlay.Graphics.Add(new Graphic(facility.Geometry, _facilitySymbol));
                }

                // Add each graphics overlay to MyMapView.
                _myMapView.GraphicsOverlays.Add(_incidentGraphicsOverlay);
                _myMapView.GraphicsOverlays.Add(_facilityGraphicsOverlay);
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any prior incident and routes from the graphics.
            _incidentGraphicsOverlay.Graphics.Clear();

            // Get the tapped point.
            _incidentPoint = e.Location;

            // Populate the facility parameters than solve using the task.
            PopulateParametersAndSolveRouteAsync();
        }

        private async void PopulateParametersAndSolveRouteAsync()
        {
            try
            {
                // Set facilities and incident in parameters.
                ClosestFacilityParameters closestFacilityParameters = await _task.CreateDefaultParametersAsync();
                closestFacilityParameters.SetFacilities(_facilities);
                closestFacilityParameters.SetIncidents(new List<Incident> {new Incident(_incidentPoint)});

                // Use the task to solve for the closest facility.
                ClosestFacilityResult result = await _task.SolveClosestFacilityAsync(closestFacilityParameters);

                // Get the index of the closest facility to incident. (0) is the index of the incident, [0] is the index of the closest facility.
                int closestFacility = result.GetRankedFacilityIndexes(0)[0];

                // Get route from closest facility to the incident and display to mapview.
                ClosestFacilityRoute route = result.GetRoute(closestFacility, 0);

                // Add graphics for the incident and route.
                _incidentGraphicsOverlay.Graphics.Add(new Graphic(_incidentPoint, _incidentSymbol));
                _incidentGraphicsOverlay.Graphics.Add(new Graphic(route.RouteGeometry, _routeSymbol));
            }
            catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
            {
                if (exception.Message.Equals("Unable to complete operation."))
                {
                    CreateErrorDialog("Incident not within San Diego area!");
                }
                else
                {
                    CreateErrorDialog("An ArcGIS web exception occurred. \n" + exception.Message);
                }
            }
        }

        private void CreateErrorDialog(string message)
        {
            // Create Alert.
            UIAlertController okAlertController = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);

            // Add Action.
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Present Alert.
            PresentViewController(okAlertController, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel helpLabel = new UILabel
            {
                Text = "Tap to show the route to the nearest facility.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
        }
    }
}