// Copyright 2021 Esri.
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
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;

namespace ArcGISRuntime.Samples.AddGraphicsRenderer
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add graphics with renderer",
        category: "GraphicsOverlay",
        description: "A renderer allows you to change the style of all graphics in a graphics overlay by referencing a single symbol style. A renderer will only affect graphics that do not specify their own symbol style.",
        instructions: "Pan and zoom on the map to view graphics for points, lines, and polygons (including polygons with curve segments) which are stylized using renderers.",
        tags: new[] { "arc", "bezier", "curve", "display", "graphics", "marker", "overlay", "renderer", "segment", "symbol", "true curve", "Featured" })]
    public class AddGraphicsRenderer : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Add graphics wih renderer";

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map for the map view.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            // Add graphics overlays to the map view.
            _myMapView.GraphicsOverlays.AddRange(new[]
            {
                MakePointGraphicsOverlay(),
                MakeLineGraphicsOverlay(),
                MakeSquareGraphicsOverlay(),
                MakeCurvedGraphicsOverlay(),
            });
        }

        private GraphicsOverlay MakePointGraphicsOverlay()
        {
            // Create a simple marker symbol.
            SimpleMarkerSymbol pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.Green, 10);

            // Create a graphics overlay for the points.
            GraphicsOverlay pointGraphicsOverlay = new GraphicsOverlay();

            // Create and assign a simple renderer to the graphics overlay.
            pointGraphicsOverlay.Renderer = new SimpleRenderer(pointSymbol);

            // Create a graphic with the map point geometry.
            MapPoint pointGeometry = new MapPoint(x: 40e5, y: 40e5, SpatialReferences.WebMercator);
            Graphic pointGraphic = new Graphic(pointGeometry);

            // Add the graphic to the overlay.
            pointGraphicsOverlay.Graphics.Add(pointGraphic);
            return pointGraphicsOverlay;
        }

        private GraphicsOverlay MakeLineGraphicsOverlay()
        {
            // Create a simple line symbol.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 5);

            // Create a graphics overlay for the polylines.
            GraphicsOverlay lineGraphicsOverlay = new GraphicsOverlay();

            // Create and assign a simple renderer to the graphics overlay.
            lineGraphicsOverlay.Renderer = new SimpleRenderer(lineSymbol);

            // Create a line graphic with new Polyline geometry.
            PolylineBuilder lineBuilder = new PolylineBuilder(SpatialReferences.WebMercator);
            lineBuilder.AddPoint(x: -10e5, y: 40e5);
            lineBuilder.AddPoint(x: 20e5, y: 50e5);
            Graphic lineGraphic = new Graphic(lineBuilder.ToGeometry());

            // Add the graphic to the overlay.
            lineGraphicsOverlay.Graphics.Add(lineGraphic);
            return lineGraphicsOverlay;
        }

        private GraphicsOverlay MakeSquareGraphicsOverlay()
        {
            // Create a simple fill symbol.
            SimpleFillSymbol squareSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Yellow, null);

            // Create a graphics overlay for the square polygons.
            GraphicsOverlay squareGraphicsOverlay = new GraphicsOverlay();

            // Create and assign a simple renderer to the graphics overlay.
            squareGraphicsOverlay.Renderer = new SimpleRenderer(squareSymbol);

            // Create a polygon graphic with `new Polygon` geometry.
            PolygonBuilder polygonBuilder = new PolygonBuilder(SpatialReferences.WebMercator);
            polygonBuilder.AddPoint(x: -20e5, y: 20e5);
            polygonBuilder.AddPoint(x: 20e5, y: 20e5);
            polygonBuilder.AddPoint(x: 20e5, y: -20e5);
            polygonBuilder.AddPoint(x: -20e5, y: -20e5);
            Graphic polygonGraphic = new Graphic(polygonBuilder.ToGeometry());

            // Add the graphic to the overlay.
            squareGraphicsOverlay.Graphics.Add(polygonGraphic);
            return squareGraphicsOverlay;
        }

        private GraphicsOverlay MakeCurvedGraphicsOverlay()
        {
            // Create a simple fill symbol with outline.
            SimpleLineSymbol curvedLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);
            SimpleFillSymbol curvedFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Red, curvedLineSymbol);

            // Create a graphics overlay for the polygons with curve segments.
            GraphicsOverlay curvedGraphicsOverlay = new GraphicsOverlay();

            // Create and assign a simple renderer to the graphics overlay.
            curvedGraphicsOverlay.Renderer = new SimpleRenderer(curvedFillSymbol);

            // Create a heart-shaped graphic.
            MapPoint origin = new MapPoint(x: 40e5, y: 5e5, SpatialReferences.WebMercator);
            Geometry heartGeometry = MakeHeartGeometry(origin, 10e5);
            Graphic heartGraphic = new Graphic(heartGeometry);
            curvedGraphicsOverlay.Graphics.Add(heartGraphic);
            return curvedGraphicsOverlay;
        }

        private Geometry MakeHeartGeometry(MapPoint center, double sideLength)
        {
            if (sideLength <= 0) return null;

            SpatialReference spatialReference = center.SpatialReference;

            // The x and y coordinates to simplify the calculation.
            double minX = center.X - 0.5 * sideLength;
            double minY = center.Y - 0.5 * sideLength;

            // The radius of the arcs.
            double arcRadius = sideLength * 0.25;

            // Bottom left curve.
            MapPoint leftCurveStart = new MapPoint(center.X, minY, spatialReference);
            MapPoint leftCurveEnd = new MapPoint(minX, minY + 0.75 * sideLength, spatialReference);
            MapPoint leftControlMapPoint1 = new MapPoint(center.X, minY + 0.25 * sideLength, spatialReference);
            MapPoint leftControlMapPoint2 = new MapPoint(minX, center.Y, spatialReference: spatialReference);
            CubicBezierSegment leftCurve = new CubicBezierSegment(leftCurveStart, leftControlMapPoint1, leftControlMapPoint2, leftCurveEnd, spatialReference);

            // Top left arc.
            MapPoint leftArcCenter = new MapPoint(minX + 0.25 * sideLength, minY + 0.75 * sideLength, spatialReference);
            EllipticArcSegment leftArc = EllipticArcSegment.CreateCircularEllipticArc(leftArcCenter, arcRadius, Math.PI, centralAngle: -Math.PI, spatialReference);

            // Top right arc.
            MapPoint rightArcCenter = new MapPoint(minX + 0.75 * sideLength, minY + 0.75 * sideLength, spatialReference);
            EllipticArcSegment rightArc = EllipticArcSegment.CreateCircularEllipticArc(rightArcCenter, arcRadius, Math.PI, centralAngle: -Math.PI, spatialReference);

            // Bottom right curve.
            MapPoint rightCurveStart = new MapPoint(minX + sideLength, minY + 0.75 * sideLength, spatialReference);
            MapPoint rightCurveEnd = leftCurveStart;
            MapPoint rightControlMapPoint1 = new MapPoint(minX + sideLength, center.Y, spatialReference);
            MapPoint rightControlMapPoint2 = leftControlMapPoint1;
            CubicBezierSegment rightCurve = new CubicBezierSegment(rightCurveStart, rightControlMapPoint1, rightControlMapPoint2, rightCurveEnd, spatialReference);

            // Create the heart polygon.
            Part newPart = new Part(new Segment[]
            {
                leftCurve,
                leftArc,
                rightArc,
                rightCurve
            }, spatialReference);
            PolygonBuilder builder = new PolygonBuilder(spatialReference);
            builder.AddPart(newPart);
            return builder.ToGeometry();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}