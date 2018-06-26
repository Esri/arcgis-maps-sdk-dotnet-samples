// Copyright 2017 Esri.
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
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.FindServiceArea
{
    [Activity(Label = "FindServiceArea")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find service area (interactive)",
        "Network Analysis",
        "Demonstrates how to find services areas around a point using the ServiceAreaTask. A service area shows locations that can be reached from a facility based off a certain impedance [such as travel time]. Barriers can also be added which can effect the impedance by not letting traffic through or adding the time is takes to pass that barrier.",
        "To add a facility, click the facility button, then click anywhere on the MapView.\nTo add a barrier, click the barrier button, and click multiple locations on MapView.\nDouble tap on the MapView to finish drawing the barrier.\nTo show service areas around facilities that were added, click the show service areas button.\nClick the reset button to clear all graphics and features.",
        "ArcGISMap, GraphicsOverlay, MapView, PolylineBarrier, ServiceAreaFacility, ServiceAreaParameters, ServiceAreaPolygon, ServiceAreaResult, ServiceAreaTask, SketchEditor")]
    public class FindServiceArea : Activity
    {
        private MapView _myMapView = new MapView();

        // Uri for the service area around San Diego.
        private Uri _serviceAreaUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ServiceArea");

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Find service area (interactive)";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new layout for the entire page
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a new layout for the toolbar (buttons)
            GridLayout toolbar = new GridLayout(this) { RowCount = 2, ColumnCount = 2, AlignmentMode = GridAlign.Margins};
            // Create a button to reset the route display, add it to the toolbar
            Button facilityButton = new Button(this) { Text = "Place Facility" };
            facilityButton.Click += PlaceFacilityButtonClick;
            
            toolbar.AddView(facilityButton);

            // Create a button to reset the route display, add it to the toolbar
            Button barrierButton = new Button(this) { Text = "Draw Barrier" };
            barrierButton.Click += DrawBarrierButtonClick;
            toolbar.AddView(barrierButton);

            // Create a button to reset the route display, add it to the toolbar
            Button serviceAreasButton = new Button(this) { Text = "Show Service Areas" };
            serviceAreasButton.Click += ShowServiceAreasButtonClick;
            toolbar.AddView(serviceAreasButton);

            // Create a button to reset the route display, add it to the toolbar
            Button resetButton = new Button(this) { Text = "Reset" };
            resetButton.Click += ResetClick;
            toolbar.AddView(resetButton);

            // Add the toolbar to the layout
            layout.AddView(toolbar);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void Initialize()
        {
            // Center the map on San Diego.
            Map streetsMap = new Map(Basemap.CreateLightGrayCanvasVector());
            streetsMap.InitialViewpoint = new Viewpoint(32.73, -117.14, 30000);
            _myMapView.Map = streetsMap;

            // Create graphics overlays for all of the elements of the map.
            _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Set the sketch editor configuration to allow vertex editing, resizing, and moving.
            SketchEditConfiguration config = _myMapView.SketchEditor.EditConfiguration;

            // Add a new behavior for double taps on the MapView.
            _myMapView.GeoViewDoubleTapped += (s, e) =>
            {
                // If the sketch editor complete command is enabled, a sketch is in progress.
                if (_myMapView.SketchEditor.CompleteCommand.CanExecute(null))
                {
                    // Set the event as handled.
                    e.Handled = true;
                }
            };
        }

        private async void PlaceFacilityButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Let the user tap on the map view using the point sketch mode.
                SketchCreationMode creationMode = SketchCreationMode.Point;
                Geometry geometry = await _myMapView.SketchEditor.StartAsync(creationMode, false);

                // Symbology for a facility.
                PictureMarkerSymbol facilitySymbol = new PictureMarkerSymbol(new Uri("http://static.arcgis.com/images/Symbols/SafetyHealth/Hospital.png"))
                {
                    Height = 30,
                    Width = 30
                };

                // Create a graphic for the facility.
                Graphic facilityGraphic = new Graphic(geometry, new Dictionary<string, object>() { { "Type", "Facility" } }, facilitySymbol)
                {
                    ZIndex = 2
                };

                // Add the graphic to the graphics overlay.
                _myMapView.GraphicsOverlays[0].Graphics.Add(facilityGraphic);
            }
            catch (TaskCanceledException)
            {
                // Ignore this exception.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                CreateErrorDialog("Error drawing facility:\n" + ex.Message);
            }
        }

        private async void DrawBarrierButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Let the user draw on the map view using the polyline sketch mode.
                SketchCreationMode creationMode = SketchCreationMode.Polyline;
                Geometry geometry = await _myMapView.SketchEditor.StartAsync(creationMode, false);

                // Symbol for the barriers.
                SimpleLineSymbol barrierSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkSlateGray, 5.0f);

                // Create the graphic to be used for barriers.
                Graphic barrierGraphic = new Graphic(geometry, new Dictionary<string, object>() { { "Type", "Barrier" } }, barrierSymbol)
                {
                    ZIndex = 1
                };

                // Add a graphic from the polyline the user drew.
                _myMapView.GraphicsOverlays[0].Graphics.Add(barrierGraphic);
            }
            catch (TaskCanceledException)
            {
                // Ignore this exception.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                CreateErrorDialog("Error drawing barrier:\n" + ex.Message);
            }
        }

        private async void ShowServiceAreasButtonClick(object sender, EventArgs e)
        {
            // Use a local variable for the graphics overlay.
            GraphicCollection allGraphics = _myMapView.GraphicsOverlays[0].Graphics;

            // Get a list of the facilities from the graphics overlay.
            List<ServiceAreaFacility> serviceAreaFacilities = (from g in allGraphics
                                                               where (string)g.Attributes["Type"] == "Facility"
                                                               select new ServiceAreaFacility((MapPoint)g.Geometry)).ToList();

            // Check that there is at least 1 facility to find a service area for.
            if (!serviceAreaFacilities.Any())
            {
                CreateErrorDialog("Must have at least one Facility!");
                return;
            }

            // Create the service area task and parameters based on the Uri.
            ServiceAreaTask serviceAreaTask = await ServiceAreaTask.CreateAsync(_serviceAreaUri);

            // An object that defines parameters for solving a service area task.
            ServiceAreaParameters serviceAreaParameters;

            // Store the default parameters for the service area in an object.
            serviceAreaParameters = await serviceAreaTask.CreateDefaultParametersAsync();

            // Add impedance cutoffs for facilities (drive time minutes).
            serviceAreaParameters.DefaultImpedanceCutoffs.Add(2.0);
            serviceAreaParameters.DefaultImpedanceCutoffs.Add(5.0);

            // Set the level of detail for the polygons.
            serviceAreaParameters.PolygonDetail = ServiceAreaPolygonDetail.High;

            // Get a list of the barriers from the graphics overlay.
            List<PolylineBarrier> polylineBarriers = (from g in allGraphics
                                                      where (string)g.Attributes["Type"] == "Barrier"
                                                      select new PolylineBarrier((Polyline)g.Geometry)).ToList();

            // Add the barriers to the service area parameters.
            serviceAreaParameters.SetPolylineBarriers(polylineBarriers);

            // Update the parameters to include all of the placed facilities.
            serviceAreaParameters.SetFacilities(serviceAreaFacilities);

            // Clear existing graphics for service areas.
            foreach (Graphic g in allGraphics.ToList())
            {
                // Check if the graphic g is a service area.
                if ((string)g.Attributes["Type"] == "ServiceArea")
                {
                    allGraphics.Remove(g);
                }
            }

            try
            {
                // Solve for the service area of the facilities.
                ServiceAreaResult result = await serviceAreaTask.SolveServiceAreaAsync(serviceAreaParameters);

                // Loop over each facility.
                for (int i = 0; i < serviceAreaFacilities.Count; i++)
                {
                    // Create list of polygons from a service facility.
                    List<ServiceAreaPolygon> polygons = result.GetResultPolygons(i).ToList();

                    // Symbol for the outline of the service areas.
                    SimpleLineSymbol serviceOutline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 3.0f);

                    // Create a list of fill symbols for the polygons.
                    List<SimpleFillSymbol> fillSymbols = new List<SimpleFillSymbol>();
                    fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 0, 0), serviceOutline));
                    fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 165, 0), serviceOutline));

                    // Loop over every polygon in every facilities result.
                    for (int j = 0; j < polygons.Count; j++)
                    {
                        // Create the graphic for the service areas, alternating between fill symbols.
                        Graphic serviceGraphic = new Graphic(polygons[j].Geometry, new Dictionary<string, object>() { { "Type", "ServiceArea" } }, fillSymbols[j % 2])
                        {
                            ZIndex = 0
                        };

                        // Add graphic for service area. Alternate the color of each polygon.
                        allGraphics.Add(serviceGraphic);
                    }
                }
            }
            catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
            {
                if (exception.Message.ToString().Equals("Unable to complete operation."))
                {
                    CreateErrorDialog("Facility not within San Diego area!");
                }
                else
                {
                    CreateErrorDialog("An ArcGIS web exception occurred. \n" + exception.Message);
                }
            }
        }

        private void ResetClick(object sender, EventArgs e)
        {
            // Clear all of the graphics.
            _myMapView.GraphicsOverlays[0].Graphics.Clear();
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