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

namespace ArcGISRuntime.WPF.Samples.FindServiceArea
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find Service Area (Interactive)",
        "Network Analysis",
        "Demonstrates how to find services areas around a point using the ServiceAreaTask. A service area shows locations that can be reached from a facility based off a certain impedance [such as travel time]. Barriers can also be added which can effect the impedance by not letting traffic through or adding the time is takes to pass that barrier.",
        "")]
    public partial class FindServiceArea
    {
        // Task to find service area around a facility.
        private ServiceAreaTask _serviceAreaTask;

        // Used for solving task above.
        private ServiceAreaParameters _serviceAreaParameters;

        // For displaying service areas to the mapview.
        private GraphicsOverlay _graphicsOverlay;

        // Used to make barriers.
        private PolylineBuilder _barrierBuilder;

        // Fills service areas with a color when displayed to mapview.

        // Used for placing geometry on mapview.
        private SpatialReference _spatialReference = SpatialReferences.WebMercator;

        // Symbol for facilities.
        private PictureMarkerSymbol _facilitySymbol;

        // Used for service area outlines.
        private SimpleLineSymbol _outline;

        // Used to see whether the user is placing barriers or facilities.
        private Boolean _barrierMode = false;

        // Used to see whether the user is placing barriers or facilities.
        private Boolean _facilityMode = false;

        // Uri for service area.
        private Uri _sanDiegoServiceAreaUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ServiceArea");

        // Picture for facility symbol.
        private Uri _pictureMarkerUri = new Uri("http://static.arcgis.com/images/Symbols/SafetyHealth/Hospital.png");

        public FindServiceArea()
        {
            InitializeComponent();

            // Create the map, graphics overlay, and the 'from' and 'to' locations for the route.
            Initialize();
        }

        private void Initialize()
        {
            // Center the map on San Diego.
            Map map = new Map(Basemap.CreateLightGrayCanvasVector());
            MyMapView.Map = map;
            MyMapView.SetViewpointCenterAsync(32.73, -117.14, 30000);

            // Create graphics overlays for all of the elements of the map.
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Link the action of tapping on the map with the MyMapView_GeoViewTapped method.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Create a polyline builder for user constructed barriers.
            _barrierBuilder = new PolylineBuilder(_spatialReference);

            // Symbology for a service area.
            _outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3.0f);

            // Symbology for a facility
            _facilitySymbol = new PictureMarkerSymbol(_pictureMarkerUri) { Height = 30, Width = 30 };
        }

        private async Task CreateServiceArea()
        {
            // Create the service area task and paramaters based on the Uri.
            _serviceAreaTask = await ServiceAreaTask.CreateAsync(_sanDiegoServiceAreaUri);
            _serviceAreaParameters = await _serviceAreaTask.CreateDefaultParametersAsync();

            // Set the parameters to return polygons.
            _serviceAreaParameters.ReturnPolygons = true;
            _serviceAreaParameters.ReturnPolylines = false;

            // Add impedance cutoffs for facilities.
            _serviceAreaParameters.DefaultImpedanceCutoffs.Add(2.0);
            _serviceAreaParameters.DefaultImpedanceCutoffs.Add(5.0);

            // Set the level of detail for the polygons.
            _serviceAreaParameters.PolygonDetail = ServiceAreaPolygonDetail.High;
        }

        private void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Create a MapPoint where the user clicked.
            MapPoint userTappedMapPoint = MyMapView.ScreenToLocation(e.Position);

            // Check to see if the user is in facility mode.
            if (_facilityMode)
            {
                // Update the graphic.
                _graphicsOverlay.Graphics.Add(new Graphic(userTappedMapPoint, new Dictionary<string, object>() { { "Type", "Facility" } }, _facilitySymbol) { ZIndex = 2 });
            }
            else if (_barrierMode)
            {
                // Add the point to the polyline builder.
                _barrierBuilder.AddPoint(userTappedMapPoint);
                // Update the barrier graphics.
                _graphicsOverlay.Graphics.Add(new Graphic(_barrierBuilder.ToGeometry(), new Dictionary<string, object>() { { "Type", "Barrier" } }, _outline) { ZIndex = 1 });
            }
        }

        private void AddFacilitiesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Disable the facilities button.
            AddFacilitiesButton.IsEnabled = false;

            // Enable the barrier button.
            AddBarrierButton.IsEnabled = true;

            // Turn on facility mode.
            _facilityMode = true;
            _barrierMode = false;
        }

        private void AddBarrierButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Disable the barrier button.
            AddBarrierButton.IsEnabled = false;

            // Enable the facilities button.
            AddFacilitiesButton.IsEnabled = true;

            // Turn on barrier mode.
            _facilityMode = false;
            _barrierMode = true;
        }

        private async void ShowServiceAreasButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Enable both facility and barrier buttons.
            AddBarrierButton.IsEnabled = true;
            AddFacilitiesButton.IsEnabled = true;

            // Turn off any drawing mode.
            _facilityMode = false;
            _barrierMode = false;

            await CreateServiceArea();

            List<ServiceAreaFacility> serviceAreaFacilities = (from g in _graphicsOverlay.Graphics
                                                               where (string)g.Attributes["Type"] == "Facility"
                                                               select new ServiceAreaFacility((MapPoint)g.Geometry)).ToList();

            // Check that there is at least 1 facility to find a service area for.
            if (serviceAreaFacilities.Count > 0)
            {
                List<PolylineBarrier> polylineBarriers = (from g in _graphicsOverlay.Graphics
                                                          where (string)g.Attributes["Type"] == "Barrier"
                                                          select new PolylineBarrier((Polyline)g.Geometry)).ToList();

                // Add the barriers to the service area parameters.
                _serviceAreaParameters.SetPolylineBarriers(polylineBarriers);

                // Clear existing graphics for service areas.
                foreach (Graphic g in _graphicsOverlay.Graphics.ToList())
                {
                    if ((string)g.Attributes["Type"] == "ServiceArea")
                    {
                        _graphicsOverlay.Graphics.Remove(g);
                    }
                }

                // Update the parameters to include all of the placed facilities.
                _serviceAreaParameters.SetFacilities(serviceAreaFacilities);

                try
                {
                    ServiceAreaResult result = await _serviceAreaTask.SolveServiceAreaAsync(_serviceAreaParameters);

                    // Loop over each facility.
                    for (int i = 0; i < serviceAreaFacilities.Count; i++)
                    {
                        // Create list of polygons from a service facility.
                        List<ServiceAreaPolygon> polygons = result.GetResultPolygons(i).ToList();

                        // Create a list of Fill Symbols for the polygons.
                        List<SimpleFillSymbol> fillSymbols = new List<SimpleFillSymbol>();
                        fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 0, 0), _outline));
                        fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 165, 0), _outline));

                        // Loop over every polygon in every facilities result.
                        for (int j = 0; j < polygons.Count; j++)
                        {
                            // Add graphic for service area. Alternate the color of each polygon.
                            _graphicsOverlay.Graphics.Add(new Graphic(polygons[j].Geometry, new Dictionary<string, object>() { { "Type", "ServiceArea" } }, fillSymbols[j % 2]) { ZIndex = 0 });
                        }
                    }
                }
                catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
                {
                    if (exception.Message.ToString().Equals("Unable to complete operation."))
                    {
                        System.Windows.MessageBox.Show("Facility not within San Diego area!", "Sample error");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("An ArcGIS web exception occurred. \n" + exception.Message.ToString(), "Sample error");
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Must have at least 1 Facility!", "Sample error");
            }
        }

        private void Reset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Re-enable both buttons for adding features.
            AddBarrierButton.IsEnabled = true;
            AddFacilitiesButton.IsEnabled = true;

            // Disable the drawing mode for both features.
            _facilityMode = false;
            _barrierMode = false;

            // Clear all of the graphics.
            _graphicsOverlay.Graphics.Clear();

            // Clear the existing barriers.
            _barrierBuilder = new PolylineBuilder(_spatialReference);
        }
    }
}