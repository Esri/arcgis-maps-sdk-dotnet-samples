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
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntime.Samples.FindServiceArea
{
    [Register("FindServiceArea")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find service area (interactive)",
        "Network Analysis",
        "Demonstrates how to find services areas around a point using the ServiceAreaTask. A service area shows locations that can be reached from a facility based off a certain impedance [travel time in this case]. Service areas for a two and five minute travel time are used. Barriers can also be added which can effect the service area by not letting traffic through and adding to the time to get to locations.",
        "-To add a facility, click the facility button, then click anywhere on the MapView.\n-To add a barrier, click the barrier button, and click multiple locations on MapView.\n-Double tap on the MapView to finish drawing the barrier.\n-To show service areas around facilities that were added, click the show service areas button.\n-Click the reset button to clear all graphics and features.",
        "ArcGISMap, GraphicsOverlay, MapView, PolylineBarrier, ServiceAreaFacility, ServiceAreaParameters, ServiceAreaPolygon, ServiceAreaResult, ServiceAreaTask, SketchEditor")]
    public class FindServiceArea : UIViewController
    {
        private MapView _myMapView;

        private UIToolbar _toolbar = new UIToolbar();

        // Create buttons for the UI.
        private UIButton _addFacilitiesButton = new UIButton(UIButtonType.Plain);
        private UIButton _addBarrierButton = new UIButton(UIButtonType.Plain);
        private UIButton _showServiceAreasButton = new UIButton(UIButtonType.Plain);
        private UIButton _resetButton = new UIButton(UIButtonType.Plain);

        // Uri for the service area around San Diego.
        private Uri _serviceAreaUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ServiceArea");

        public FindServiceArea()
        {
            Title = "Find service area";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();

            // Add all of the buttons and link their click methods.
            _addFacilitiesButton.SetTitle("Facilities", UIControlState.Normal);
            _addFacilitiesButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _addFacilitiesButton.TouchUpInside += PlaceFacilites_Click;

            _addBarrierButton.SetTitle("Barrier", UIControlState.Normal);
            _addBarrierButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _addBarrierButton.TouchUpInside += DrawBarrier_Click;

            _showServiceAreasButton.SetTitle("Service areas", UIControlState.Normal);
            _showServiceAreasButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _showServiceAreasButton.TouchUpInside += ShowServiceAreas_Click;

            _resetButton.SetTitle("Reset", UIControlState.Normal);
            _resetButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _resetButton.TouchUpInside += Reset_Click;

            // Add the bar button items to an array of UIBarButtonItems
            View.AddSubviews(_myMapView, _toolbar, _addFacilitiesButton, _addBarrierButton, _showServiceAreasButton, _resetButton);

            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            int yPageOffset = 60;

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for the Toolbar
            _toolbar.Frame = new CoreGraphics.CGRect(0, yPageOffset, View.Bounds.Width, 80);

            _addFacilitiesButton.Frame = new CoreGraphics.CGRect(0, yPageOffset, View.Bounds.Width / 2, 40);
            _addBarrierButton.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2, yPageOffset, View.Bounds.Width / 2, 40);
            _showServiceAreasButton.Frame = new CoreGraphics.CGRect(0, yPageOffset + 40, View.Bounds.Width / 2, 40);
            _resetButton.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2, yPageOffset + 40, View.Bounds.Width / 2, 40);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Center the map on San Diego.
            Map streetsMap = new Map(Basemap.CreateLightGrayCanvasVector())
            {
                InitialViewpoint = new Viewpoint(32.73, -117.14, 30000)
            };
            _myMapView.Map = streetsMap;

            // Create graphics overlays for all of the elements of the map.
            _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

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

        private async void PlaceFacilites_Click(object sender, EventArgs e)
        {
            // Disable the button so the user recognizes that they are placing a facility.
            _addFacilitiesButton.Enabled = false;
            _addFacilitiesButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);

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

            // Enable the button when the user is done.
            _addFacilitiesButton.Enabled = true;
            _addFacilitiesButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
        }

        private async void DrawBarrier_Click(object sender, EventArgs e)
        {
            // Disable the button so the user recognizes that they are drawing a barrier.
            _addBarrierButton.Enabled = false;
            _addBarrierButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            try
            {
                // Let the user draw on the map view using the polyline sketch mode.
                SketchCreationMode creationMode = SketchCreationMode.Polyline;
                Geometry geometry = await _myMapView.SketchEditor.StartAsync(creationMode, false);

                // Symbol for the barriers.
                SimpleLineSymbol barrierSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 5.0f);

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

            // Enable the button when the user is done.
            _addBarrierButton.Enabled = true;
            _addBarrierButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
        }

        private async void ShowServiceAreas_Click(object sender, EventArgs e)
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
                    SimpleLineSymbol serviceOutline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkGray, 3.0f);

                    // Create a list of fill symbols for the polygons.
                    List<SimpleFillSymbol> fillSymbols = new List<SimpleFillSymbol>
                    {
                        new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 0, 0), serviceOutline), 
                        new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 165, 0), serviceOutline)
                    };

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

        private void Reset_Click(object sender, EventArgs e)
        {
            // Clear all of the graphics.
            _myMapView.GraphicsOverlays[0].Graphics.Clear();
        }

        private void CreateErrorDialog(string message)
        {
            // Create Alert.
            var okAlertController = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);

            // Add Action.
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Present Alert.
            PresentViewController(okAlertController, true, null);
        }
    }
}