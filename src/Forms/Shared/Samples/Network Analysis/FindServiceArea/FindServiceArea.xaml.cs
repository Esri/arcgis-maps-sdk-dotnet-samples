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
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.FindServiceArea
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find Service Area",
        "Network Analysis",
        "Demonstrates how to find services areas around a point using the ServiceAreaTask. A service area shows locations that can be reached from a facility based off a certain impedance [such as travel time]. Barriers can also be added which can effect the impedance by not letting traffic through or adding the time is takes to pass that barrier.",
        "")]
    public partial class FindServiceArea : ContentPage
    {

        // All location where service areas will be found
        private List<ServiceAreaFacility> _serviceAreaFacilities;
        // Task to find service area around a facility
        private ServiceAreaTask _serviceAreaTask;
        // Used for solving task above
        private ServiceAreaParameters _serviceAreaParameters;
        // For displaying service area facilities to the mapview
        private GraphicsOverlay _facilityOverlay;
        // For displaying service areas to the mapview
        private GraphicsOverlay _serviceAreasOverlay;
        // For displaying barriers to mapview
        private GraphicsOverlay _barrierOverlay;
        // Used to make barriers
        private PolylineBuilder _barrierBuilder;
        // Fills service areas with a color when displayed to mapview
        private List<SimpleFillSymbol> _fillSymbols;
        // Used for placing geometry on mapview
        private SpatialReference _spatialReference = SpatialReferences.WebMercator;

        // Symbol for facilities
        private PictureMarkerSymbol _facilitySymbol;

        // Used for service area outlines
        private SimpleLineSymbol _outline;

        // Used to see whether the user is placing barriers or facilities
        private Boolean _barrierMode = false;
        private Boolean _facilityMode = false;

        // Uri for service area
        private Uri _sanDiegoServiceAreaUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ServiceArea");

        // Picture for facility symbol
        private Uri _pictureMarkerUri = new Uri("http://static.arcgis.com/images/Symbols/SafetyHealth/Hospital.png");

        public FindServiceArea()
        {
            InitializeComponent();

            Title = "Find Service Area";

            // Create the map, graphics overlay, and the 'from' and 'to' locations for the route
            Initialize();
        }

        private void Initialize()
        {
            // Create a new service area
            CreateServiceArea();

            // Center the map on San Diego
            Map map = new Map(Basemap.CreateStreets());
            MyMapView.Map = map;
            MyMapView.SetViewpointCenterAsync(32.73, -117.14, 30000);

            // Create graphics overlays for all of the elements of the map
            _serviceAreasOverlay = new GraphicsOverlay();
            _facilityOverlay = new GraphicsOverlay();
            _barrierOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_serviceAreasOverlay);
            MyMapView.GraphicsOverlays.Add(_facilityOverlay);
            MyMapView.GraphicsOverlays.Add(_barrierOverlay);

            // Link the action of tapping on the map with the MyMapView_GeoViewTapped method
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Create a polyline builder for user constructed barriers
            _barrierBuilder = new PolylineBuilder(_spatialReference);

            // Create a list of serviceAreaFacilities
            _serviceAreaFacilities = new List<ServiceAreaFacility>();

            // Symbology for a service area
            _outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3.0f);

            // Create a list of Fill Symbols for the polygons.
            _fillSymbols = new List<SimpleFillSymbol>();
            _fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 0, 0), _outline));
            _fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(70, 255, 165, 0), _outline));

            // Symbology for a facility
            _facilitySymbol = new PictureMarkerSymbol(_pictureMarkerUri);
            _facilitySymbol.Height = 30;
            _facilitySymbol.Width = 30;

        }
        private async void CreateServiceArea()
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

        private void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            // Create a MapPoint where the user clicked.
            MapPoint userTappedMapPoint = MyMapView.ScreenToLocation(e.Position);

            // Check to see if the user is in facility mode.
            if (_facilityMode)
            {
                // Add the new facility location.
                _serviceAreaFacilities.Add(new ServiceAreaFacility(userTappedMapPoint));

                // Update the graphic.
                _facilityOverlay.Graphics.Add(new Graphic(userTappedMapPoint, _facilitySymbol));
            }
            else if (_barrierMode)
            {
                // Add the point to the polyline builder.
                _barrierBuilder.AddPoint(userTappedMapPoint);
                // Update the barrier graphics.
                _barrierOverlay.Graphics.Add(new Graphic(_barrierBuilder.ToGeometry(), _outline));
            }
        }
        private void AddFacilities_Click(object sender, EventArgs e)
        {
            // Disable the facilities button.
            AddFacilities.IsEnabled = false;

            // Enable the barrier button.
            AddBarrier.IsEnabled = true;

            // Turn on facility mode.
            _facilityMode = true;
            _barrierMode = false;

        }
        private void AddBarrier_Click(object sender, EventArgs e)
        {
            // Disable the barrier button.
            AddBarrier.IsEnabled = false;

            // Enable the facilities button.
            AddFacilities.IsEnabled = true;

            // Turn on barrier mode.
            _facilityMode = false;
            _barrierMode = true;

        }
        private async void ShowServiceAreas_Click(object sender, EventArgs e)
        {
            // Enable both facility and barrier buttons.
            AddBarrier.IsEnabled = true;
            AddFacilities.IsEnabled = true;

            // Turn off any drawing mode.
            _facilityMode = false;
            _barrierMode = false;

            // Check that there is at least 1 facility to find a service area for.
            if (_serviceAreaFacilities.Count > 0)
            {
                List<PolylineBarrier> polylineBarriers = new List<PolylineBarrier>();
                foreach (Graphic barrier in _barrierOverlay.Graphics.ToList())
                {
                    polylineBarriers.Add(new PolylineBarrier((Polyline)barrier.Geometry));
                }

                // Add the barriers to the service area parameters.
                _serviceAreaParameters.SetPolylineBarriers(polylineBarriers);

                // Clear existing graphics for service areas.
                _serviceAreasOverlay.Graphics.Clear();

                // Update the parameters to include all of the placed facilities.
                _serviceAreaParameters.SetFacilities(_serviceAreaFacilities);

                try
                {
                    ServiceAreaResult result = await _serviceAreaTask.SolveServiceAreaAsync(_serviceAreaParameters);

                    // Loop over each facility.
                    for (int i = 0; i < _serviceAreaFacilities.Count; i++)
                    {
                        // Create list of polygons from a service facility.
                        List<ServiceAreaPolygon> polygons = result.GetResultPolygons(i).ToList();

                        // Loop over every polygon in every facilities result.
                        for (int j = 0; j < polygons.Count; j++)
                        {
                            // Add graphic for service area. Alternate the color of each polygon.
                            _serviceAreasOverlay.Graphics.Add(new Graphic(polygons[j].Geometry, _fillSymbols[j % 2]));
                        }
                    }
                }
                catch (Esri.ArcGISRuntime.Http.ArcGISWebException exception)
                {
                    if (exception.Message.ToString().Equals("Unable to complete operation."))
                    {
                        await DisplayAlert("Error", "Facility not within San Diego area!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "An ArcGIS web exception occurred. \n" + exception.Message.ToString(), "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Error", "Must have at least 1 Facility!", "OK");
            }
        }
        private void Reset_Click(object sender, EventArgs e)
        {
            // Re-enable both buttons for adding features
            AddBarrier.IsEnabled = true;
            AddFacilities.IsEnabled = true;

            // Disable the drawing mode for both features
            _facilityMode = false;
            _barrierMode = false;

            // Clear all of the graphics
            _facilityOverlay.Graphics.Clear();
            _serviceAreasOverlay.Graphics.Clear();
            _barrierOverlay.Graphics.Clear();

            // Create a new service area
            CreateServiceArea();

            // Clear the current list of facilities
            _serviceAreaFacilities.Clear();

            // Clear the existing barriers
            _barrierBuilder = new PolylineBuilder(_spatialReference);

        }
    }
}
