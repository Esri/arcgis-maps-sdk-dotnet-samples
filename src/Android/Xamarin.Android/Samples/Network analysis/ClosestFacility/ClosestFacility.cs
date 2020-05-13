// Copyright 2018 Esri.
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
using System.Linq;

namespace ArcGISRuntime.Samples.ClosestFacility
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Find closest facility to an incident (interactive)",
        category: "Network analysis",
        description: "Find a route to the closest facility from a location.",
        instructions: "Tap near any of the hospitals and a route will be displayed from that clicked location to the nearest hospital.",
        tags: new[] { "incident", "network analysis", "route", "search" })]
    public class ClosestFacility : Activity
    {
        // Hold a reference to the map view.
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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Closest facility";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();            
        }

        private void CreateLayout()
        {
            // Create a new layout for the entire page
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a TextView for instructions.
            TextView sampleInstructionsTextView = new TextView(this)
            {
                Text = "Tap to find the route to the nearest hospital to your incident."
            };
            layout.AddView(sampleInstructionsTextView);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private async void Initialize()
        {
            // Hook up the DrawStatusChanged event.
            _myMapView.DrawStatusChanged += OnDrawStatusChanged;

            // Construct the map and set the MapView.Map property.
            Map map = new Map(Basemap.CreateLightGrayCanvasVector());
            _myMapView.Map = map;

            // Create a ClosestFacilityTask using the San Diego Uri.
            _task = await ClosestFacilityTask.CreateAsync(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility"));

            // List of facilities to be placed around San Diego area.
            _facilities = new List<Facility> {
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
            _incidentSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.FromArgb(255, 0, 0, 0), 30);

            // Route to hospital symbol.
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
                closestFacilityParameters.SetIncidents(new List<Incident> { new Incident(_incidentPoint) });

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
            // Create a dialog to show message to user.
            AlertDialog alert = new AlertDialog.Builder(this).Create();
            alert.SetMessage(message);
            alert.Show();
        }
    }
}