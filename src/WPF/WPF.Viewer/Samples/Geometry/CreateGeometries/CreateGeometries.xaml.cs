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

namespace ArcGIS.WPF.Samples.CreateGeometries
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Create geometries",
        category: "Geometry",
        description: "Create simple geometry types.",
        instructions: "Pan and zoom freely to see the different types of geometries placed onto the map.",
        tags: new[] { "area", "boundary", "line", "marker", "path", "shape" })]
    public partial class CreateGeometries
    {
        public CreateGeometries()
        {
            InitializeComponent();

            // Create the map, set the initial extent, and add the original point graphic.
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap.
            Map theMap = new Map(BasemapStyle.ArcGISTopographic);

            // Assign the map to the MapView.
            MyMapView.Map = theMap;

            // Create a simple fill symbol - used to render a polygon covering Colorado.
            SimpleFillSymbol theSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Cross, System.Drawing.Color.Blue, null);

            // Create a simple line symbol - used to render a polyline border between California and Nevada.
            SimpleLineSymbol theSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 3);

            // Create a simple marker symbol - used to render multi-points for various state capital locations in the Western United States.
            SimpleMarkerSymbol theSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Blue, 14);

            // Create a simple marker symbol - used to render a map point where the Esri headquarters is located.
            SimpleMarkerSymbol theSimpleMarkerSymbol2 = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Red, 18);

            // Create a graphics overlay to hold the various graphics.
            GraphicsOverlay theGraphicsOverlays = new GraphicsOverlay();

            // Get the graphic collection from the graphics overlay.
            GraphicCollection theGraphicCollection = theGraphicsOverlays.Graphics;

            // Add a graphic to the graphic collection - polygon with a simple fill symbol.
            theGraphicCollection.Add(new Graphic(CreatePolygon(), theSimpleFillSymbol));

            // Add a graphic to the graphic collection - polyline with a simple line symbol.
            theGraphicCollection.Add(new Graphic(CreatePolyline(), theSimpleLineSymbol));

            // Add a graphic to the graphic collection - map point with a simple marker symbol.
            theGraphicCollection.Add(new Graphic(CreateMapPoint(), theSimpleMarkerSymbol2));

            // Add a graphic to the graphic collection - multi-point with a simple marker symbol.
            theGraphicCollection.Add(new Graphic(CreateMultipoint(), theSimpleMarkerSymbol));

            // Set the map views graphics overlay to the created graphics overlay.
            MyMapView.GraphicsOverlays.Add(theGraphicsOverlays);

            // Zoom to the extent of an envelope with some padding (10 pixels).
            MyMapView.SetViewpointGeometryAsync(CreateEnvelope(), 10);
        }

        private Envelope CreateEnvelope()
        {
            // Create an envelope that covers the extent of the western United States.
            Envelope theEnvelope = new Envelope(-123.0, 33.5, -101.0, 48.0, SpatialReferences.Wgs84);

            // Return the geometry.
            return theEnvelope;
        }

        private Polygon CreatePolygon()
        {
            // Create a point collection with coordinates that approximates the boundary of Colorado.
            PointCollection thePointCollection = new PointCollection(SpatialReferences.Wgs84);
            thePointCollection.Add(-109.048, 40.998);
            thePointCollection.Add(-102.047, 40.998);
            thePointCollection.Add(-102.037, 36.989);
            thePointCollection.Add(-109.048, 36.998);

            // Create a polygon from the point collection.
            Polygon thePolygon = new Polygon(thePointCollection);

            // Return the geometry.
            return thePolygon;
        }

        private Polyline CreatePolyline()
        {
            // Create a point collection with coordinates that approximates the border between California and Nevada.
            PointCollection thePointCollection = new PointCollection(SpatialReferences.Wgs84);
            thePointCollection.Add(-119.992, 41.989);
            thePointCollection.Add(-119.994, 38.994);
            thePointCollection.Add(-114.620, 35.0);

            // Create a polyline from the point collection.
            Polyline thePolyline = new Polyline(thePointCollection);

            // Return the geometry.
            return thePolyline;
        }

        private MapPoint CreateMapPoint()
        {
            // Create a map point where the Esri headquarters is located.
            MapPoint theMapPoint = new MapPoint(-117.195800, 34.056295, SpatialReferences.Wgs84);

            // Return the geometry.
            return theMapPoint;
        }

        private Multipoint CreateMultipoint()
        {
            // Create a point collection with coordinates representing various state capital locations in the Western United States.
            PointCollection thePointCollection = new PointCollection(SpatialReferences.Wgs84);
            // - Sacramento, CA
            thePointCollection.Add(-121.491014, 38.579065);
            // - Olympia, WA
            thePointCollection.Add(-122.891366, 47.039231);
            // - Salem, OR
            thePointCollection.Add(-123.043814, 44.93326);
            // - Carson City, NV
            thePointCollection.Add(-119.766999, 39.164885);

            // Create a multi-point from the point collection.
            Multipoint theMultipoint = new Multipoint(thePointCollection);

            // Return the geometry.
            return theMultipoint;
        }
    }
}