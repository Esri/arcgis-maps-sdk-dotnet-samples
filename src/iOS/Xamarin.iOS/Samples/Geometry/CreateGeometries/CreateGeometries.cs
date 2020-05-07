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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.CreateGeometries
{
    [Register("CreateGeometries")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create geometries",
        "Geometry",
        "Create simple geometry types.",
        "Pan and zoom freely to see the different types of geometries placed onto the map.",
        "area", "boundary", "line", "marker", "path", "shape")]
    public class CreateGeometries : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public CreateGeometries()
        {
            Title = "Create geometries";
        }

        private void Initialize()
        {
            // Create and show a map with a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

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
            _myMapView.GraphicsOverlays.Add(theGraphicsOverlays);

            // Zoom to the extent of an envelope with some padding (10 pixels).
            _myMapView.SetViewpointGeometryAsync(CreateEnvelope(), 10);
        }

        private static Envelope CreateEnvelope() => new Envelope(-123.0, 33.5, -101.0, 48.0, SpatialReferences.Wgs84);

        private Polygon CreatePolygon()
        {
            // Create a point collection with coordinates that approximates the boundary of Colorado.
            PointCollection thePointCollection = new PointCollection(SpatialReferences.Wgs84)
            {
                {-109.048, 40.998},
                {-102.047, 40.998},
                {-102.037, 36.989},
                {-109.048, 36.998}
            };

            // Create and return a polygon from the point collection.
            return new Polygon(thePointCollection);
        }

        private Polyline CreatePolyline()
        {
            // Create a point collection with coordinates that approximates the border between California and Nevada.
            PointCollection thePointCollection = new PointCollection(SpatialReferences.Wgs84)
            {
                {-119.992, 41.989},
                {-119.994, 38.994},
                {-114.620, 35.0}
            };

            // Create and return a polyline from the point collection.
            return new Polyline(thePointCollection);
        }

        // Return a map point where the Esri headquarters is located.
        private MapPoint CreateMapPoint() => new MapPoint(-117.195800, 34.056295, SpatialReferences.Wgs84);

        private Multipoint CreateMultipoint()
        {
            // Create a point collection with coordinates representing various state capital locations in the Western United States.
            PointCollection thePointCollection = new PointCollection(SpatialReferences.Wgs84)
            {
                {-121.491014, 38.579065}, // - Sacramento, CA
                {-122.891366, 47.039231}, // - Olympia, WA
                {-123.043814, 44.93326}, // - Salem, OR
                {-119.766999, 39.164885} // - Carson City, NV
            };

            // Create and return a multi-point from the point collection.
            return new Multipoint(thePointCollection);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}