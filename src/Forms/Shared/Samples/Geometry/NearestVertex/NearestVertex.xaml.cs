// Copyright 2020 Esri.
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
using System;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.NearestVertex
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Nearest vertex",
        category: "Geometry",
        description: "Find the closest vertex and coordinate of a geometry to a point.",
        instructions: "Tap anywhere on the map. An orange cross will show at that location. A blue circle will show the polygon's nearest vertex to the point that was clicked. A red diamond will appear at the coordinate on the geometry that is nearest to the point that was clicked. If tapped inside the geometry, the red and orange markers will overlap. The information box showing distance between the tapped point and the nearest vertex/coordinate will be updated with every new location clicked.",
        tags: new[] { "analysis", "coordinate", "geometry", "nearest", "proximity", "vertex" })]
    public partial class NearestVertex
    {
        // Hold references to the graphics overlay and the polygon graphic.
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _polygonGraphic;

        // Hold references to the graphics for the user and results points.
        private Graphic _tappedLocationGraphic;
        private Graphic _nearestVertexGraphic;
        private Graphic _nearestCoordinateGraphic;

        public NearestVertex()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Planar distances are only accurate for geometries that have a defined projected coordinate system.
            // Create a spatial reference using the California zone 5 (ftUS) state plane coordinate system.
            // This coordinate system maintains planar distance accuracy in the region of Southern California.
            SpatialReference californiaZone5SpatialReference = SpatialReference.Create(2229);

            // Create a map that uses the California zone 5 state spatial reference.
            MyMapView.Map = new Map(californiaZone5SpatialReference);

            // Create the feature layer. 
            Uri uriLayerSource = new Uri("https://services.arcgis.com/P3ePLMYs2RVChkJx/arcgis/rest/services/USA_States_Generalized/FeatureServer/0");
            FeatureLayer usaStatesFeatureLayer = new FeatureLayer(uriLayerSource);

            // Add the feature layer to the MapView.
            MyMapView.Map.OperationalLayers.Add(usaStatesFeatureLayer);

            // Create the graphics overlay and set the selection color.
            _graphicsOverlay = new GraphicsOverlay();

            // Add the overlay to the MapView.
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Create the point collection that defines the polygon.
            PointCollection polygonPoints = new PointCollection(californiaZone5SpatialReference)
            {
                new MapPoint(6627416.41469281, 1804532.53233782),
                new MapPoint(6669147.89779046, 2479145.16609522),
                new MapPoint(7265673.02678292, 2484254.50442408),
                new MapPoint(7676192.55880379, 2001458.66365744),
                new MapPoint(7175695.94143837, 1840722.34474458)
            };

            // Create the polygon.
            Polygon polygonGeometry = new Polygon(polygonPoints);

            // Define and apply the symbology.
            SimpleLineSymbol polygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Green, 2);
            SimpleFillSymbol polygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, Colors.Green, polygonOutlineSymbol);

            // Create the graphic and add it to the graphics overlay.
            _polygonGraphic = new Graphic(polygonGeometry, polygonFillSymbol);
            _graphicsOverlay.Graphics.Add(_polygonGraphic);

            // Create the graphics and symbology for the tapped point, the nearest vertex, and the nearest coordinate.
            SimpleMarkerSymbol tappedLocationSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Colors.Black, 15);
            SimpleMarkerSymbol nearestCoordinateSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Colors.Red, 10);
            SimpleMarkerSymbol nearestVertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Blue, 15);
            _nearestCoordinateGraphic = new Graphic { Symbol = nearestCoordinateSymbol };
            _tappedLocationGraphic = new Graphic { Symbol = tappedLocationSymbol };
            _nearestVertexGraphic = new Graphic { Symbol = nearestVertexSymbol };

            _graphicsOverlay.Graphics.Add(_tappedLocationGraphic);
            _graphicsOverlay.Graphics.Add(_nearestVertexGraphic);
            _graphicsOverlay.Graphics.Add(_nearestCoordinateGraphic);

            // Listen for taps; the spatial relationships will be updated in the handler.
            MyMapView.GeoViewTapped += MapViewTapped;

            // Center the map on the polygon
            MyMapView.SetViewpointCenterAsync(polygonGeometry.Extent.GetCenter(), 8000000);
        }

        private void MapViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs geoViewInputEventArgs)
        {
            // Get the tapped location.
            MapPoint tappedLocation = (MapPoint)GeometryEngine.NormalizeCentralMeridian(geoViewInputEventArgs.Location);

            // Show the tapped location.
            _tappedLocationGraphic.Geometry = tappedLocation;

            // Get the nearest vertex in the polygon
            ProximityResult nearestVertexResult = GeometryEngine.NearestVertex(_polygonGraphic.Geometry, tappedLocation);

            // Get the nearest coordinate in the polygon.
            ProximityResult nearestCoordinateResult =
                GeometryEngine.NearestCoordinate(_polygonGraphic.Geometry, tappedLocation);

            // Get the distance to the nearest vertex in the polygon.
            int distanceVertex = (int)(nearestVertexResult.Distance / 1000);

            // Get the distance to the nearest coordinate in the polygon.
            int distanceCoordinate = (int)(nearestCoordinateResult.Distance / 1000);

            // Show the nearest vertex in blue.
            _nearestVertexGraphic.Geometry = nearestVertexResult.Coordinate;

            // Show the nearest coordinate in red.
            _nearestCoordinateGraphic.Geometry = nearestCoordinateResult.Coordinate;

            // Show the distances in the UI.
            ResultsLabel.Text = $"Vertex dist: {distanceVertex} km, Point dist: {distanceCoordinate} km";
        }
    }
}