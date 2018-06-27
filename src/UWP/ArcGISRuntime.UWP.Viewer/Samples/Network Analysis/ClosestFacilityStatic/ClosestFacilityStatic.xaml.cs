// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.ClosestFacilityStatic
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Closest facility (static)",
        "Network Analysis",
        "Demonstrates how to solve a Closest Facility Task to find the closest route between facilities and incidents.",
        "Click the solve button to find the closest facility to every incident.")]
    public partial class ClosestFacilityStatic
    {
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
        private Uri _closestFacilityUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility");

        public ClosestFacilityStatic()
        {
            InitializeComponent();

            // Create the map and graphics overlays.
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Construct the map and set the MapView.Map property.
                Map map = new Map(Basemap.CreateLightGrayCanvasVector());
                MyMapView.Map = map;

                // Add a graphics overlay to MyMapView. (Will be used later to display routes)
                MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

                // Create a ClosestFacilityTask using the San Diego Uri.
                _task = ClosestFacilityTask.CreateAsync(_closestFacilityUri).Result;

                // Create a symbol for displaying facilities.
                PictureMarkerSymbol facilitySymbol = new PictureMarkerSymbol(new Uri("http://static.arcgis.com/images/Symbols/SafetyHealth/FireStation.png"))
                {
                    Height = 30,
                    Width = 30
                };

                // Incident symbol.
                PictureMarkerSymbol incidentSymbol = new PictureMarkerSymbol(new Uri("http://static.arcgis.com/images/Symbols/SafetyHealth/esriCrimeMarker_56_Gradient.png"))
                {
                    Height = 30,
                    Width = 30
                };

                // Create a list of line symbols to show unique routes. Different colors help make different routes visually distinguishable.
                _routeSymbols = new List<SimpleLineSymbol>();
                _routeSymbols.Add(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(125, 25, 45, 85), 5.0f));
                _routeSymbols.Add(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(125, 35, 65, 120), 5.0f));
                _routeSymbols.Add(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(125, 55, 100, 190), 5.0f));
                _routeSymbols.Add(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(125, 75, 140, 255), 5.0f));

                // Create a table for facilities using the FeatureServer.
                _facilityTable = new ServiceFeatureTable(_facilityUri);

                // Create a feature layer from the table.
                _facilityLayer = new FeatureLayer(_facilityTable);

                // Add a renderer that uses the facility symbol.
                _facilityLayer.Renderer = new SimpleRenderer(facilitySymbol);

                // Create a table for facilities using the FeatureServer.
                _incidentTable = new ServiceFeatureTable(_incidentUri);

                // Create a feature layer from the table.
                _incidentLayer = new FeatureLayer(_incidentTable);

                // Add a renderer that uses the incident symbol.
                _incidentLayer.Renderer = new SimpleRenderer(incidentSymbol);

                // Add the layers to the map.
                MyMapView.Map.OperationalLayers.Add(_facilityLayer);
                MyMapView.Map.OperationalLayers.Add(_incidentLayer);

                // Wait for both layers to load.
                await _facilityLayer.LoadAsync();
                await _incidentLayer.LoadAsync();

                // Zoom to the combined extent of both layers.
                Envelope fullExtent = GeometryEngine.CombineExtents(_facilityLayer.FullExtent, _incidentLayer.FullExtent);
                await MyMapView.SetViewpointGeometryAsync(fullExtent, 50);

                // Enable the solve button.
                SolveRoutesButton.IsEnabled = true;
            }
            catch
            {
            }
        }

        private async void SolveRoutesClick(object sender, RoutedEventArgs e)
        {
            // Holds locations of hospitals around San Diego.
            List<Facility> _facilities = new List<Facility>();

            // Holds locations of hospitals around San Diego.
            List<Incident> _incidents = new List<Incident>();

            // Create query parameters to select all features.
            QueryParameters queryParams = new QueryParameters() { WhereClause = "1=1" };

            // Query all features in the facility table.
            FeatureQueryResult facilityResult = await _facilityTable.QueryFeaturesAsync(queryParams);

            // Add all of the query results to facilities as new Facility objects.
            _facilities.AddRange(facilityResult.ToList().Select(feature => new Facility((MapPoint)feature.Geometry)));

            // Query all features in the incident table.
            FeatureQueryResult incidentResult = await _incidentTable.QueryFeaturesAsync(queryParams);

            // Add all of the query results to facilities as new Incident objects.
            _incidents.AddRange(incidentResult.ToList().Select(feature => new Incident((MapPoint)feature.Geometry)));

            // Set facilities and incident in parameters.
            ClosestFacilityParameters closestFacilityParameters = await _task.CreateDefaultParametersAsync();
            closestFacilityParameters.SetFacilities(_facilities);
            closestFacilityParameters.SetIncidents(_incidents);

            try
            {
                // Use the task to solve for the closest facility.
                ClosestFacilityResult result = await _task.SolveClosestFacilityAsync(closestFacilityParameters);

                // Create a list of routes between incidents and facilities.
                List<ClosestFacilityRoute> routes = new List<ClosestFacilityRoute>();

                for (int i = 0; i < _incidents.Count; i++)
                {
                    // Get the index of the closest facility to incident. (i) is the index of the incident, [0] is the index of the closest facility.
                    int closestFacility = result.GetRankedFacilityIndexes(i)[0];

                    // Add the closest route to the routes list.
                    routes.Add(result.GetRoute(closestFacility, i));
                }

                for (int i = 0; i < routes.Count; i++)
                {
                    // Add the graphic for each route.
                    MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(routes[i].RouteGeometry, _routeSymbols[i % _routeSymbols.Count]));
                }

                // Disable the solve button.
                SolveRoutesButton.IsEnabled = false;

                // Enable the reset button.
                ResetButton.IsEnabled = true;
            }
            catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
            {
                await new MessageDialog("An ArcGIS web exception occurred.\n" + exception.Message.ToString(), "Sample error").ShowAsync();
            }
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            // Clear the route graphics.
            MyMapView.GraphicsOverlays[0].Graphics.Clear();

            // Reset the buttons.
            SolveRoutesButton.IsEnabled = true;
            ResetButton.IsEnabled = false;
        }
    }
}