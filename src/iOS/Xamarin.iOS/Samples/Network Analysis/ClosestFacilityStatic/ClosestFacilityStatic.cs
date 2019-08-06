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
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _solveRoutesButton;
        private UIBarButtonItem _resetButton;

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
        private readonly Uri _facilityUri = new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/ArcGIS/rest/services/San_Diego_Facilities/FeatureServer/0");

        // Uri for incident feature service.
        private readonly Uri _incidentUri = new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/ArcGIS/rest/services/San_Diego_Incidents/FeatureServer/0");

        // Uri for the closest facility service.
        private readonly Uri _closestFacilityUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility");

        public ClosestFacilityStatic()
        {
            Title = "Closest facility (static)";
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
                _routeSymbols = new List<SimpleLineSymbol>
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

        private async void SolveRoutesButton_Click(object sender, EventArgs e)
        {
            // Holds locations of hospitals around San Diego.
            List<Facility> facilities = new List<Facility>();

            // Holds locations of hospitals around San Diego.
            List<Incident> incidents = new List<Incident>();

            // Create query parameters to select all features.
            QueryParameters queryParams = new QueryParameters
            {
                WhereClause = "1=1"
            };

            // Query all features in the facility table.
            FeatureQueryResult facilityResult = await _facilityTable.QueryFeaturesAsync(queryParams);

            // Add all of the query results to facilities as new Facility objects.
            facilities.AddRange(facilityResult.ToList().Select(feature => new Facility((MapPoint) feature.Geometry)));

            // Query all features in the incident table.
            FeatureQueryResult incidentResult = await _incidentTable.QueryFeaturesAsync(queryParams);

            // Add all of the query results to facilities as new Incident objects.
            incidents.AddRange(incidentResult.ToList().Select(feature => new Incident((MapPoint) feature.Geometry)));

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

        private void ResetButton_Click(object sender, EventArgs e)
        {
            // Clear the route graphics.
            _myMapView.GraphicsOverlays[0].Graphics.Clear();

            // Reset the buttons.
            _solveRoutesButton.Enabled = true;
            _resetButton.Enabled = false;
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
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _solveRoutesButton = new UIBarButtonItem();
            _solveRoutesButton.Title = "Solve routes";
            _solveRoutesButton.Enabled = false;

            _resetButton = new UIBarButtonItem();
            _resetButton.Title = "Reset";
            _resetButton.Enabled = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _solveRoutesButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _resetButton
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
            _solveRoutesButton.Clicked += SolveRoutesButton_Click;
            _resetButton.Clicked += ResetButton_Click;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _solveRoutesButton.Clicked -= SolveRoutesButton_Click;
            _resetButton.Clicked -= ResetButton_Click;
        }
    }
}