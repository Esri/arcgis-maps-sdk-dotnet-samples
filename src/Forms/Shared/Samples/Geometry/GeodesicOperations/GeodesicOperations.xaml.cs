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
using Esri.ArcGISRuntime.UI;
using Xamarin.Forms;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.GeodesicOperations
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Geodesic operations",
        category: "Geometry",
        description: "Calculate a geodesic path between two points and measure its distance.",
        instructions: "Tap anywhere on the map. A line graphic will display the geodesic line between the two points. In addition, text that indicates the geodesic distance between the two points will be updated. Tap elsewhere and a new line will be created.",
        tags: new[] { "densify", "distance", "geodesic", "geodetic" })]
    public partial class GeodesicOperations : ContentPage
    {
        // Hold references to the graphics.
        private Graphic _startLocationGraphic;
        private Graphic _endLocationGraphic;
        private Graphic _pathGraphic;

        public GeodesicOperations()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            MyMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);

            // Create the graphics overlay and add it to the map view.
            GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(graphicsOverlay);

            // Add a graphic at JFK to serve as the origin.
            MapPoint start = new MapPoint(-73.7781, 40.6413, SpatialReferences.Wgs84);
            SimpleMarkerSymbol startMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Blue, 10);
            _startLocationGraphic = new Graphic(start, startMarker);

            // Create the graphic for the destination.
            _endLocationGraphic = new Graphic
            {
                Symbol = startMarker
            };

            // Create the graphic for the path.
            _pathGraphic = new Graphic
            {
                Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Blue, 5)
            };

            // Add the graphics to the overlay.
            graphicsOverlay.Graphics.Add(_startLocationGraphic);
            graphicsOverlay.Graphics.Add(_endLocationGraphic);
            graphicsOverlay.Graphics.Add(_pathGraphic);

            // Update end location when the user taps.
            MyMapView.GeoViewTapped += MyMapViewOnGeoViewTapped;
        }

        private void MyMapViewOnGeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs geoViewInputEventArgs)
        {
            // Get the tapped point, projected to WGS84.
            MapPoint destination = (MapPoint) GeometryEngine.Project(geoViewInputEventArgs.Location, SpatialReferences.Wgs84);

            // Update the destination graphic.
            _endLocationGraphic.Geometry = destination;

            // Get the points that define the route polyline.
            PointCollection polylinePoints = new PointCollection(SpatialReferences.Wgs84)
            {
                (MapPoint) _startLocationGraphic.Geometry,
                destination
            };

            // Create the polyline for the two points.
            Polyline routeLine = new Polyline(polylinePoints);

            // Densify the polyline to show the geodesic curve.
            Geometry pathGeometry = GeometryEngine.DensifyGeodetic(routeLine, 1, LinearUnits.Kilometers, GeodeticCurveType.Geodesic);

            // Apply the curved line to the path graphic.
            _pathGraphic.Geometry = pathGeometry;

            // Calculate and show the distance.
            double distance = GeometryEngine.LengthGeodetic(pathGeometry, LinearUnits.Kilometers, GeodeticCurveType.Geodesic);
            ResultsLabel.Text = $"{(int) distance} kilometers";
        }
    }
}