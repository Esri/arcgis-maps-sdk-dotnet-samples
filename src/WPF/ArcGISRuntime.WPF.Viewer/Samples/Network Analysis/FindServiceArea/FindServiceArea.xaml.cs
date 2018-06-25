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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.FindServiceArea
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find service area (interactive)",
        "Network Analysis",
        "Demonstrates how to find services areas around a point using the ServiceAreaTask. A service area shows locations that can be reached from a facility based off a certain impedance [such as travel time]. Barriers can also be added which can effect the impedance by not letting traffic through or adding the time is takes to pass that barrier.",
        "")]
    public partial class FindServiceArea
    {
        // An object that defines the task to find service area around a facility.
        private ServiceAreaTask _serviceAreaTask;

        // A symbol for facilities.
        private PictureMarkerSymbol _facilitySymbol;

        // A symbol for service area outlines.
        private SimpleLineSymbol _outline;

        private Uri _serviceAreaUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ServiceArea");

        public FindServiceArea()
        {
            InitializeComponent();

            // Create the map, graphics overlay, and sketch editor.
            Initialize();
        }

        private void Initialize()
        {
            // Center the map on San Diego.
            Map streetsMap = new Map(Basemap.CreateLightGrayCanvasVector());
            streetsMap.InitialViewpoint = new Viewpoint(32.73, -117.14, 30000);
            MyMapView.Map = streetsMap;

            // Create graphics overlays for all of the elements of the map.
            MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Symbology for a facility.
            _facilitySymbol = new PictureMarkerSymbol(new Uri("http://static.arcgis.com/images/Symbols/SafetyHealth/Hospital.png"))
            {
                Height = 30,
                Width = 30
            };

            // Symbology for a service area.
            _outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 3.0f);

            // Set the sketch editor configuration to allow vertex editing, resizing, and moving.
            SketchEditConfiguration config = MyMapView.SketchEditor.EditConfiguration;

            MyMapView.GeoViewDoubleTapped += (s, e) =>
            {
                // If the sketch editor complete command is enabled, a sketch is in progress.
                if (MyMapView.SketchEditor.CompleteCommand.CanExecute(null))
                {
                    // Set the event as handled.
                    e.Handled = true;
                }
            };
        }

        private async void PlaceFacilityButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Let the user tap on the map view using the point sketch mode.
                SketchCreationMode creationMode = SketchCreationMode.Point;
                Geometry geometry = await MyMapView.SketchEditor.StartAsync(creationMode, false);

                // Create and add a graphic for the facility.
                MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(geometry, new Dictionary<string, object>() { { "Type", "Facility" } }, _facilitySymbol)
                {
                    ZIndex = 2
                });
            }
            catch (TaskCanceledException)
            {
                // Ignore this exception.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                MessageBox.Show("Error drawing facility:\n" + ex.Message);
            }
        }

        private async void DrawBarrierButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Let the user draw on the map view using the polyline sketch mode.
                SketchCreationMode creationMode = SketchCreationMode.Polyline;
                Geometry geometry = await MyMapView.SketchEditor.StartAsync(creationMode, false);

                // Create and add a graphic from the polyline the user drew.
                MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(geometry, new Dictionary<string, object>() { { "Type", "Barrier" } }, _outline)
                {
                    ZIndex = 1
                });
            }
            catch (TaskCanceledException)
            {
                // Ignore this exception.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                MessageBox.Show("Error drawing barrier:\n" + ex.Message);
            }
        }

        private async void ShowServiceAreasButton_Click(object sender, RoutedEventArgs e)
        {
            // Get a list of the facilities from the graphics overlay.
            List<ServiceAreaFacility> serviceAreaFacilities = (from g in MyMapView.GraphicsOverlays[0].Graphics
                                                               where (string)g.Attributes["Type"] == "Facility"
                                                               select new ServiceAreaFacility((MapPoint)g.Geometry)).ToList();

            // Check that there is at least 1 facility to find a service area for.
            if (!serviceAreaFacilities.Any())
            {
                MessageBox.Show("Must have at least one Facility!", "Sample error");
                return;
            }

            // Create the service area task and parameters based on the Uri.
            _serviceAreaTask = await ServiceAreaTask.CreateAsync(_serviceAreaUri);

            // An object that defines parameters for solving a service area task.
            ServiceAreaParameters serviceAreaParameters;

            // Store the default parameters for the service area in an object.
            serviceAreaParameters = await _serviceAreaTask.CreateDefaultParametersAsync();

            // Set the parameters to return polygons.
            serviceAreaParameters.ReturnPolygons = true;
            serviceAreaParameters.ReturnPolylines = false;

            // Add impedance cutoffs for facilities.
            serviceAreaParameters.DefaultImpedanceCutoffs.Add(2.0);
            serviceAreaParameters.DefaultImpedanceCutoffs.Add(5.0);

            // Set the level of detail for the polygons.
            serviceAreaParameters.PolygonDetail = ServiceAreaPolygonDetail.High;

            // Get a list of the barriers from the graphics overlay.
            List<PolylineBarrier> polylineBarriers = (from g in MyMapView.GraphicsOverlays[0].Graphics
                                                      where (string)g.Attributes["Type"] == "Barrier"
                                                      select new PolylineBarrier((Polyline)g.Geometry)).ToList();

            // Add the barriers to the service area parameters.
            serviceAreaParameters.SetPolylineBarriers(polylineBarriers);

            // Update the parameters to include all of the placed facilities.
            serviceAreaParameters.SetFacilities(serviceAreaFacilities);

            // Clear existing graphics for service areas.
            foreach (Graphic g in MyMapView.GraphicsOverlays[0].Graphics.ToList())
            {
                if ((string)g.Attributes["Type"] == "ServiceArea")
                {
                    MyMapView.GraphicsOverlays[0].Graphics.Remove(g);
                }
            }

            try
            {
                // Solve for the service area of the facilities.
                ServiceAreaResult result = await _serviceAreaTask.SolveServiceAreaAsync(serviceAreaParameters);

                // Loop over each facility.
                for (int i = 0; i < serviceAreaFacilities.Count; i++)
                {
                    // Create list of polygons from a service facility.
                    List<ServiceAreaPolygon> polygons = result.GetResultPolygons(i).ToList();

                    // Create a list of fill symbols for the polygons.
                    List<SimpleFillSymbol> fillSymbols = new List<SimpleFillSymbol>();
                    fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 0, 0), _outline));
                    fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 165, 0), _outline));

                    // Loop over every polygon in every facilities result.
                    for (int j = 0; j < polygons.Count; j++)
                    {
                        // Add graphic for service area. Alternate the color of each polygon.
                        MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(polygons[j].Geometry, new Dictionary<string, object>() { { "Type", "ServiceArea" } }, fillSymbols[j % 2]) { ZIndex = 0 });
                    }
                }
            }
            catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
            {
                if (exception.Message.ToString().Equals("Unable to complete operation."))
                {
                    MessageBox.Show("Facility not within San Diego area!", "Sample error");
                }
                else
                {
                    MessageBox.Show("An ArcGIS web exception occurred. \n" + exception.Message.ToString(), "Sample error");
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // Clear all of the graphics.
            MyMapView.GraphicsOverlays[0].Graphics.Clear();
        }
    }
}