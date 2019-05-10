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
using Esri.ArcGISRuntime.Data;
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

namespace ArcGISRuntime.Samples.ClosestFacilityStatic
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Closest facility (static)",
        "Network Analysis",
        "Demonstrates how to solve a Closest Facility Task to find the closest route between facilities and incidents.",
        "Click the solve button to find the closest facility to every incident.")]
    public class ClosestFacilityStatic : Activity
    {
        // Create a MapView.
        private MapView _myMapView = new MapView();

        // Add buttons for the UI.
        private Button _solveRoutesButton;
        private Button _resetButton;

        // Used to display route between incident and facility to mapview.
        private List<SimpleLineSymbol> _routeSymbols;

        // Solves task to find closest route between an incident and a facility.
        private ClosestFacilityTask _task;

        // Table of all facilities.
        private ServiceFeatureTable _facilityTable;

        // Table of all incidents.
        private ServiceFeatureTable _incidentTable;

        // Feature layer for facilities.
        private FeatureLayer _facilityLayer;

        // Feature layer for incidents.
        private FeatureLayer _incidentLayer;

        // Uri for facilities feature service.
        private Uri _facilityUri = new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/ArcGIS/rest/services/San_Diego_Facilities/FeatureServer/0");

        // Uri for incident feature service.
        private Uri _incidentUri = new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/ArcGIS/rest/services/San_Diego_Incidents/FeatureServer/0");

        // Uri for the closest facility service.
        private Uri _closestFacilityUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility");

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Closest facility (static)";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new layout for the entire page.
            LinearLayout layout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Create a new layout for the toolbar (buttons).
            LinearLayout toolbar = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };

            // Create a button to solve the routes and add it to the toolbar.
            _solveRoutesButton = new Button(this) { Text = "Solve Routes" };
            _solveRoutesButton.Click += SolveRoutesClick;
            _solveRoutesButton.Enabled = false;
            toolbar.AddView(_solveRoutesButton);

            // Create a button to reset the route display, add it to the toolbar.
            _resetButton = new Button(this) { Text = "Reset" };
            _resetButton.Click += ResetClick;
            _resetButton.Enabled = false;
            toolbar.AddView(_resetButton);

            // Add the toolbar to the layout.
            layout.AddView(toolbar);

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private async void Initialize()
        {
            try
            {
                // Construct the map and set the MapView.Map property.
                _myMapView.Map = new Map(Basemap.CreateLightGrayCanvasVector());

                // Add a graphics overlay to MyMapView. (Will be used later to display routes)
                _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

                // Create a ClosestFacilityTask using the San Diego Uri.
                _task = await ClosestFacilityTask.CreateAsync(_closestFacilityUri);

                // Create a symbol for displaying facilities.
                PictureMarkerSymbol facilitySymbol = new PictureMarkerSymbol(new Uri("https://static.arcgis.com/images/Symbols/SafetyHealth/FireStation.png"))
                {
                    Height = 30,
                    Width = 30
                };

                // Incident symbol.
                PictureMarkerSymbol incidentSymbol = new PictureMarkerSymbol(new Uri("https://static.arcgis.com/images/Symbols/SafetyHealth/esriCrimeMarker_56_Gradient.png"))
                {
                    Height = 30,
                    Width = 30
                };

                // Create a list of line symbols to show unique routes. Different colors help make different routes visually distinguishable.
                _routeSymbols = new List<SimpleLineSymbol>()
                {
                    new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(125, 25, 45, 85), 5.0f),
                    new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(125, 35, 65, 120), 5.0f),
                    new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(125, 55, 100, 190), 5.0f),
                    new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(125, 75, 140, 255), 5.0f)
                };

                // Create a table for facilities using the FeatureServer.
                _facilityTable = new ServiceFeatureTable(_facilityUri);

                // Create a feature layer from the table.
                _facilityLayer = new FeatureLayer(_facilityTable)
                {
                    Renderer = new SimpleRenderer(facilitySymbol)
                };

                // Create a table for facilities using the FeatureServer.
                _incidentTable = new ServiceFeatureTable(_incidentUri);

                // Create a feature layer from the table.
                _incidentLayer = new FeatureLayer(_incidentTable)
                {
                    Renderer = new SimpleRenderer(incidentSymbol)
                };

                // Add the layers to the map.
                _myMapView.Map.OperationalLayers.Add(_facilityLayer);
                _myMapView.Map.OperationalLayers.Add(_incidentLayer);

                // Wait for both layers to load.
                await _facilityLayer.LoadAsync();
                await _incidentLayer.LoadAsync();

                // Zoom to the combined extent of both layers.
                Envelope fullExtent = GeometryEngine.CombineExtents(_facilityLayer.FullExtent, _incidentLayer.FullExtent);
                await _myMapView.SetViewpointGeometryAsync(fullExtent, 50);

                // Enable the solve button.
                _solveRoutesButton.Enabled = true;
            }
            catch (Exception exception)
            {
                CreateErrorDialog("An exception has occurred.\n" + exception.Message);
            }
        }

        private async void SolveRoutesClick(object sender, EventArgs e)
        {
            // Holds locations of hospitals around San Diego.
            List<Facility> facilities = new List<Facility>();

            // Holds locations of hospitals around San Diego.
            List<Incident> incidents = new List<Incident>();

            // Create query parameters to select all features.
            QueryParameters queryParams = new QueryParameters()
            {
                WhereClause = "1=1"
            };

            // Query all features in the facility table.
            FeatureQueryResult facilityResult = await _facilityTable.QueryFeaturesAsync(queryParams);

            // Add all of the query results to facilities as new Facility objects.
            facilities.AddRange(facilityResult.ToList().Select(feature => new Facility((MapPoint)feature.Geometry)));

            // Query all features in the incident table.
            FeatureQueryResult incidentResult = await _incidentTable.QueryFeaturesAsync(queryParams);

            // Add all of the query results to facilities as new Incident objects.
            incidents.AddRange(incidentResult.ToList().Select(feature => new Incident((MapPoint)feature.Geometry)));

            // Set facilities and incident in parameters.
            ClosestFacilityParameters closestFacilityParameters = await _task.CreateDefaultParametersAsync();
            closestFacilityParameters.SetFacilities(facilities);
            closestFacilityParameters.SetIncidents(incidents);

            try
            {
                // Use the task to solve for the closest facility.
                ClosestFacilityResult result = await _task.SolveClosestFacilityAsync(closestFacilityParameters);

                for (int i = 0; i < incidents.Count; i++)
                {
                    // Get the index of the closest facility to incident. (i) is the index of the incident, [0] is the index of the closest facility.
                    int closestFacility = result.GetRankedFacilityIndexes(i)[0];

                    // Get the route to the closest facility.
                    ClosestFacilityRoute route = result.GetRoute(closestFacility, i);

                    // Display the route on the graphics overlay.
                    _myMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(route.RouteGeometry, _routeSymbols[i % _routeSymbols.Count]));
                }

                // Disable the solve button.
                _solveRoutesButton.Enabled = false;

                // Enable the reset button.
                _resetButton.Enabled = true;
            }
            catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
            {
                CreateErrorDialog("An ArcGIS web exception occurred.\n" + exception.Message);
            }
        }

        private void ResetClick(object sender, EventArgs e)
        {
            // Clear the route graphics.
            _myMapView.GraphicsOverlays[0].Graphics.Clear();

            // Reset the buttons.
            _solveRoutesButton.Enabled = true;
            _resetButton.Enabled = false;
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
