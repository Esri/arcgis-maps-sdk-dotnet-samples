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
using System.Threading.Tasks;

namespace ArcGIS.WPF.Samples.ClosestFacilityStatic
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Find closest facility to multiple incidents (service)",
        category: "Network analysis",
        description: "Find routes from several locations to the respective closest facility.",
        instructions: "Click the button to solve and display the route from each incident (fire) to the nearest facility (fire station).",
        tags: new[] { "incident", "network analysis", "route", "search" })]
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
        private Uri _closestFacilityUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility");

        public ClosestFacilityStatic()
        {
            InitializeComponent();

            // Create the map and graphics overlays.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Construct the map and set the MapView.Map property.
                MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);

                // Add a graphics overlay to MyMapView. (Will be used later to display routes)
                MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

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
                MyMapView.Map.OperationalLayers.Add(_facilityLayer);
                MyMapView.Map.OperationalLayers.Add(_incidentLayer);

                // Wait for both layers to load.
                await _facilityLayer.LoadAsync();
                await _incidentLayer.LoadAsync();

                // Zoom to the combined extent of both layers.
                Envelope fullExtent = _facilityLayer.FullExtent.CombineExtents(_incidentLayer.FullExtent);
                await MyMapView.SetViewpointGeometryAsync(fullExtent, 50);

                // Enable the solve button.
                SolveRoutesButton.IsEnabled = true;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("An exception has occurred.\n" + exception.Message, "Sample error");
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

            try
            {
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

                // Use the task to solve for the closest facility.
                ClosestFacilityResult result = await _task.SolveClosestFacilityAsync(closestFacilityParameters);

                for (int i = 0; i < incidents.Count; i++)
                {
                    // Get the index of the closest facility to incident. (i) is the index of the incident, [0] is the index of the closest facility.
                    int closestFacility = result.GetRankedFacilityIndexes(i)[0];

                    // Get the route to the closest facility.
                    ClosestFacilityRoute route = result.GetRoute(closestFacility, i);

                    // Display the route on the graphics overlay.
                    MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(route.RouteGeometry, _routeSymbols[i % _routeSymbols.Count]));
                }

                // Disable the solve button.
                SolveRoutesButton.IsEnabled = false;

                // Enable the reset button.
                ResetButton.IsEnabled = true;
            }
            catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
            {
                System.Windows.MessageBox.Show("An ArcGIS web exception occurred.\n" + exception.Message, "Sample error");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error");
            }
        }

        private void ResetClick(object sender, EventArgs e)
        {
            // Clear the route graphics.
            MyMapView.GraphicsOverlays[0].Graphics.Clear();

            // Reset the buttons.
            SolveRoutesButton.IsEnabled = true;
            ResetButton.IsEnabled = false;
        }
    }
}