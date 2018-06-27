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
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ClosestFacilityStatic
{
    [Register("ClosestFacilityStatic")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Closest facility (static)",
        "Network Analysis",
        "Demonstrates how to solve a Closest Facility Task to find the closest route between facilities and incidents.",
        "Click the solve button to find the closest facility to every incident.")]
    public class ClosestFacilityStatic : UIViewController
    {
        // Create and hold references to the views.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UIButton _solveRoutesButton = new UIButton(UIButtonType.Plain);
        private readonly UIButton _resetButton = new UIButton(UIButtonType.Plain);

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
            Title = "Closest facility (static)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            // Configure the UI controls.
            _solveRoutesButton.SetTitle("Solve routes", UIControlState.Normal);
            _solveRoutesButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _solveRoutesButton.TouchUpInside += SolveRoutesButton_Click;

            _resetButton.SetTitle("Reset", UIControlState.Normal);
            _resetButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _resetButton.TouchUpInside += ResetButton_Click;

            // Add the controls to the view.
            View.AddSubviews(_myMapView, _toolbar, _solveRoutesButton, _resetButton);
        }

        private async void Initialize()
        {
            try
            {
                // Construct the map and set the MapView.Map property.
                Map map = new Map(Basemap.CreateLightGrayCanvasVector());
                _myMapView.Map = map;

                // Add a graphics overlay to MyMapView. (Will be used later to display routes)
                _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

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
            catch
            {
            }
        }

        private async void SolveRoutesButton_Click(object sender, EventArgs e)
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
                    _myMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(routes[i].RouteGeometry, _routeSymbols[i % _routeSymbols.Count]));
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

        private void ResetButton_Click(object sender, EventArgs e)
        {
            // Clear the route graphics.
            _myMapView.GraphicsOverlays[0].Graphics.Clear();

            // Reset the buttons.
            _solveRoutesButton.Enabled = true;
            _resetButton.Enabled = false;
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat toolbarHeight = 40;

                // Reposition the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);
                _solveRoutesButton.Frame = new CGRect(10, _toolbar.Frame.Top + 10, View.Bounds.Width/2, 20);
                _resetButton.Frame = new CGRect((View.Bounds.Width-20)/2, _toolbar.Frame.Top + 10, View.Bounds.Width/2, 20);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
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
    }
}