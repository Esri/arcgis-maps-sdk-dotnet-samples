// Copyright 2018 Esri.
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

namespace ArcGISRuntime.Samples.ClosestFacility
{
    [Register("ClosestFacility")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Closest facility (interactive)",
        "Network Analysis",
        "Demonstrates how to solve a Closest Facility Task to find the closest route between a facility (hospital) and a incident (black cross). Tap to find the route to the nearest hospital.",
        "")]
    public partial class ClosestFacility : UIViewController
    {
        // Map view that will be used
        private MapView _myMapView;

        // Toolbar for UI element
        private UIToolbar _toolbar = new UIToolbar();

        // Holds locations of hospitals around San Diego
        private List<Facility> _facilities;

        // Graphics overlays for facilities and incidents
        private GraphicsOverlay _facilityGraphicsOverlay;

        // Symbol for facilities
        private PictureMarkerSymbol _facilitySymbol;

        private GraphicsOverlay _incidentGraphicsOverlay;

        // Black cross where user clicked
        private MapPoint _incidentPoint;

        // Symbol for the incident
        private SimpleMarkerSymbol _incidentSymbol;

        // Used to display route between incident and facility to mapview
        private SimpleLineSymbol _routeSymbol;

        // Solves task to find closest route between an incident and a facility
        private ClosestFacilityTask _task;

        public ClosestFacility()
        {
            Title = "Closest Facility";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();

            // Add the bar button items to an array of UIBarButtonItems
            View.AddSubviews(_myMapView, _toolbar);

            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for the Toolbar
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 80, View.Bounds.Width, 40);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Hook up the DrawStatusChanged event.
            _myMapView.DrawStatusChanged += OnDrawStatusChanged;

            // Load the basemap.
            Map map = new Map(Basemap.CreateLightGrayCanvasVector());
            _myMapView.Map = map;

            // Create a ClosestFacilityTask using the San Diego Uri.
            _task = ClosestFacilityTask.CreateAsync(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility")).Result;

            // Spatial reference to be used
            SpatialReference _spatialReference = SpatialReferences.WebMercator;

            // List of facilities to be placed around San Diego area.
            _facilities = new List<Facility> {
                new Facility(new MapPoint(-1.3042129900625112E7, 3860127.9479775648, _spatialReference)),
                new Facility(new MapPoint(-1.3042193400557665E7, 3862448.873041752, _spatialReference)),
                new Facility(new MapPoint(-1.3046882875518233E7, 3862704.9896770366, _spatialReference)),
                new Facility(new MapPoint(-1.3040539754780494E7, 3862924.5938606677, _spatialReference)),
                new Facility(new MapPoint(-1.3042571225655518E7, 3858981.773018156, _spatialReference)),
                new Facility(new MapPoint(-1.3039784633928463E7, 3856692.5980474586, _spatialReference)),
                new Facility(new MapPoint(-1.3049023883956768E7, 3861993.789732541, _spatialReference))
                };

            // Center the map on the San Diego facilities.
            Envelope fullExtent = GeometryEngine.CombineExtents(_facilities.Select(facility => facility.Geometry));
            _myMapView.SetViewpointGeometryAsync(fullExtent, 50);

            // Create a symbol for displaying facilities.
            _facilitySymbol = new PictureMarkerSymbol(new Uri("http://static.arcgis.com/images/Symbols/SafetyHealth/Hospital.png"));
            _facilitySymbol.Height = 30;
            _facilitySymbol.Width = 30;

            // Incident symbol
            _incidentSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.FromArgb(255, 0, 0, 0), 30);

            // Route to hospital symbol
            _routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.FromArgb(255, 0, 0, 255), 5.0f);

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

        private void OnDrawStatusChanged(object sender, DrawStatusChangedEventArgs e)
        {
            if (e.Status == DrawStatus.Completed)
            {
                // Link the action of tapping on the map with the MyMapView_GeoViewTapped method.
                _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;

                // Remove this method from DrawStatusChanged events.
                _myMapView.DrawStatusChanged -= OnDrawStatusChanged;
            }
        }

        private void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Clear any prior incident and routes from the graphics.
            _incidentGraphicsOverlay.Graphics.Clear();

            // Create a MapPoint where the user clicked.
            _incidentPoint = _myMapView.ScreenToLocation(e.Position);

            // Populate the facility parameters than solve using the task.
            populateParametersAndSolveRouteAsync();
        }

        private async void populateParametersAndSolveRouteAsync()
        {
            // Set facilities and incident in parameters.
            ClosestFacilityParameters _closestFacilityParameters = await _task.CreateDefaultParametersAsync();
            _closestFacilityParameters.SetFacilities(_facilities);
            _closestFacilityParameters.SetIncidents(new List<Incident> { new Incident(_incidentPoint) });

            try
            {
                // Use the task to solve for the closest facility.
                ClosestFacilityResult result = await _task.SolveClosestFacilityAsync(_closestFacilityParameters);

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
                if (exception.Message.ToString().Equals("Unable to complete operation."))
                {
                    CreateErrorDialog("Incident not within San Diego area!");
                }
                else
                {
                    CreateErrorDialog("An ArcGIS web exception occurred. \n" + exception.Message.ToString());
                }
            }
        }

        private void CreateErrorDialog(String message)
        {
            //Create Alert
            var okAlertController = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);

            //Add Action
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Present Alert
            PresentViewController(okAlertController, true, null);
        }
    }
}

/*
// Copyright 2018 Esri.
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
using UIKit;
using Foundation;

namespace ArcGISRuntime.WPF.Samples.ClosestFacility
{
    [Register("ClosestFacility")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Closest Facility (Interactive)",
        "Network Analysis",
        "Demonstrates how to solve a Closest Facility Task to find the closest route between a facility (hospital) and a incident (black cross).",
        "")]
    public partial class ClosestFacility : UIViewController
    {
        private MapView _myMapView;
        private UIToolbar _toolbar = new UIToolbar();

        // Black cross where user clicked
        private MapPoint _incidentPoint;

        // Graphics overlays for facilities and incidents
        private GraphicsOverlay _facilityGraphicsOverlay;
        private GraphicsOverlay _incidentGraphicsOverlay;

        // Holds locations of hospitals around San Diego
        private List<Facility> _facilities;

        // Solves task to find closest route between an incident and a facility
        private ClosestFacilityTask _task;
        // Parameters needed to solve for route
        private ClosestFacilityParameters _closestFacilityParameters;
        // Used to display route between incident and facility to mapview
        private SimpleLineSymbol _routeSymbol;
        // Same spatial reference of the map
        private SpatialReference _spatialReference = SpatialReferences.WebMercator;

        // Symbol for facilities
        private PictureMarkerSymbol _facilitySymbol;

        // Symbol for the incident
        private SimpleMarkerSymbol _incidentSymbol;

        // Uri for service area
        private Uri _sanDiegoServiceAreaUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility");

        // Picture for facility symbol
        private Uri _pictureMarkerUri = new Uri("http://static.arcgis.com/images/Symbols/SafetyHealth/Hospital.png");

        public ClosestFacility()
        {
            Title = "Closest Facility";
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();

            // Add the bar button items to an array of UIBarButtonItems
            View.AddSubviews(_myMapView, _toolbar);

            Initialize();
        }
        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for the Toolbar
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 80, View.Bounds.Width, 40);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Center the map on San Diego.
            Map map = new Map(Basemap.CreateStreets());
            _myMapView.Map = map;
            _myMapView.SetViewpointCenterAsync(32.727, -117.1750, 40000);

            // Create a ClosestFacilityTask using the San Diego Uri.
            _task = ClosestFacilityTask.CreateAsync(_sanDiegoServiceAreaUri).Result;

            // List of facilities to be placed around San Diego area.
            _facilities = new List<Facility> {
                new Facility(new MapPoint(-1.3042129900625112E7, 3860127.9479775648, _spatialReference)),
                new Facility(new MapPoint(-1.3042193400557665E7, 3862448.873041752, _spatialReference)),
                new Facility(new MapPoint(-1.3046882875518233E7, 3862704.9896770366, _spatialReference)),
                new Facility(new MapPoint(-1.3040539754780494E7, 3862924.5938606677, _spatialReference)),
                new Facility(new MapPoint(-1.3042571225655518E7, 3858981.773018156, _spatialReference)),
                new Facility(new MapPoint(-1.3039784633928463E7, 3856692.5980474586, _spatialReference)),
                new Facility(new MapPoint(-1.3049023883956768E7, 3861993.789732541, _spatialReference))
                };

            // Create a symbol for displaying facilities.
            _facilitySymbol = new PictureMarkerSymbol(_pictureMarkerUri);
            _facilitySymbol.Height = 30;
            _facilitySymbol.Width = 30;

            // Incident symbol
            _incidentSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.FromArgb(255, 0, 0, 0), 20);

            // Route to hospital symbol
            _routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.FromArgb(255, 0, 0, 255), 2.5f);

            // Create Graphics Overlays for incidents and facilities.
            _incidentGraphicsOverlay = new GraphicsOverlay();
            _facilityGraphicsOverlay = new GraphicsOverlay();

            // Create a graphic and add to graphics overlay for each facility.
            foreach (Facility facility in _facilities)
            {
                _facilityGraphicsOverlay.Graphics.Add(new Graphic(facility.Geometry, _facilitySymbol));
            }

            // Add each graphics overlay to MyMapView.
            _myMapView.GraphicsOverlays.Add(_facilityGraphicsOverlay);
            _myMapView.GraphicsOverlays.Add(_incidentGraphicsOverlay);

            // Link the action of tapping on the map with the MyMapView_GeoViewTapped method.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTappedAsync;
        }

        private void MyMapView_GeoViewTappedAsync(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Clear any prior incident and routes from the graphics.
            _incidentGraphicsOverlay.Graphics.Clear();

            // Create a MapPoint where the user clicked.
            _incidentPoint = _myMapView.ScreenToLocation(e.Position);

            // Populate the facility parameters than solve using the task.
            populateParametersAndSolveRouteAsync();
        }

        private async void populateParametersAndSolveRouteAsync()
        {
            // Set facilities and incident in parameters.
            _closestFacilityParameters = _task.CreateDefaultParametersAsync().Result;
            _closestFacilityParameters.SetFacilities(_facilities);
            _closestFacilityParameters.SetIncidents(new List<Incident> { new Incident(_incidentPoint) });

            try
            {
                // Use the task to solve for the closest facility.
                ClosestFacilityResult result = await _task.SolveClosestFacilityAsync(_closestFacilityParameters);

                // Get the index of the closest facility to incident. (0) is the index of the incident, [0] is the index of the closest facility.
                int closestFacility = result.GetRankedFacilityIndexes(0)[0];

                // Get route from closest facility to the incident and display to mapview.
                ClosestFacilityRoute route = result.GetRoute(closestFacility, 0);

                // Add graphics for the incident and route.
                _incidentGraphicsOverlay.Graphics.Add(new Graphic(route.RouteGeometry, _routeSymbol));
                _incidentGraphicsOverlay.Graphics.Add(new Graphic(_incidentPoint, _incidentSymbol));
            }
            catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
            {
                if (exception.Message.ToString().Equals("Unable to complete operation."))
                {
                    CreateErrorDialog("Incident not within San Diego area!");
                }
                else
                {
                    CreateErrorDialog("An ArcGIS web exception occurred. \n" + exception.Message.ToString());
                }
            }
        }
        private void CreateErrorDialog(String message)
        {
            //Create Alert
            var okAlertController = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);

            //Add Action
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Present Alert
            PresentViewController(okAlertController, true, null);
        }
    }
}
*/