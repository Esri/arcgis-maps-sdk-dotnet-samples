// Copyright 2018 Esri.
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
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.ConvexHullList
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Convex hull list",
        category: "Geometry",
        description: "Generate convex hull polygon(s) from multiple input geometries.",
        instructions: "Tap the 'Create Convex Hull' button to create convex hull(s) from the polygon graphics. If the 'Union' checkbox is checked, the resulting output will be one polygon being the convex hull for the two input polygons. If the 'Union' checkbox is un-checked, the resulting output will have two convex hull polygons - one for each of the two input polygons. Tap the 'Reset' button to start over.",
        tags: new[] { "analysis", "geometry", "outline", "perimeter", "union" })]
    public class ConvexHullList : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        // Graphics overlay to display the graphics.
        private GraphicsOverlay _graphicsOverlay;

        // Graphic that represents polygon1.
        private Graphic _polygonGraphic1;

        // Graphic that represents polygon2.
        private Graphic _polygonGraphic2;

        // Create a Switch to choose whether to union the resulting convex hull(s).
        private Switch _convexHullListSwitch;

        // Create a Button to create convex hull(s).
        private Button _convexHullListButton;

        // Create a Button to create convex hull(s).
        private Button _resetButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Convex hull list";

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap.
            Map newMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView.
            _myMapView.Map = newMap;

            // Create a graphics overlay to hold the various graphics.
            _graphicsOverlay = new GraphicsOverlay();

            // Add the created graphics overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Create a simple line symbol for the outline for the two input polygon graphics.
            SimpleLineSymbol polygonsSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid,
                System.Drawing.Color.Blue, 4);

            // Create the color that will be used for the fill of the two input polygon graphics. It will be a
            // semi -transparent, blue color.
            System.Drawing.Color polygonsFillColor = System.Drawing.Color.FromArgb(34, 0, 0, 255);

            // Create the simple fill symbol for the two input polygon graphics - comprised of a fill style, fill
            // color and outline.
            SimpleFillSymbol polygonsSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, polygonsFillColor,
                polygonsSimpleLineSymbol);

            // Create the graphic for polygon1 - comprised of a polygon shape and fill symbol.
            _polygonGraphic1 = new Graphic(CreatePolygon1(), polygonsSimpleFillSymbol)
            {

                // Set the Z index for the polygon1 graphic so that it appears above the convex hull graphic(s) added later.
                ZIndex = 1
            };

            // Add the polygon1 graphic to the graphics overlay collection.
            _graphicsOverlay.Graphics.Add(_polygonGraphic1);

            // Create the graphic for polygon2 - comprised of a polygon shape and fill symbol.
            _polygonGraphic2 = new Graphic(CreatePolygon2(), polygonsSimpleFillSymbol)
            {

                // Set the Z index for the polygon2 graphic so that it appears above the convex hull graphic(s) added later.
                ZIndex = 1
            };

            // Add the polygon2 graphic to the graphics overlay collection.
            _graphicsOverlay.Graphics.Add(_polygonGraphic2);
        }

        private Polygon CreatePolygon1()
        {
            // Create a point collection that represents polygon1. Use the same spatial reference as the underlying base map.
            PointCollection pointCollection1 = new PointCollection(SpatialReferences.WebMercator)
            {
                // Add all of the polygon1 boundary map points to the point collection.
                new MapPoint(-4983189.15470412, 8679428.55774286),
                new MapPoint(-5222621.66664186, 5147799.00666126),
                new MapPoint(-13483043.3284937, 4728792.11077023),
                new MapPoint(-13273539.8805482, 2244679.79941622),
                new MapPoint(-5372266.98660294, 2035176.3514707),
                new MapPoint(-5432125.11458738, -4100281.76693377),
                new MapPoint(-2469147.7793579, -4160139.89491821),
                new MapPoint(-1900495.56350578, 2035176.3514707),
                new MapPoint(2768438.41928007, 1975318.22348627),
                new MapPoint(2409289.65137346, 5477018.71057565),
                new MapPoint(-2409289.65137346, 5387231.518599),
                new MapPoint(-2469147.7793579, 8709357.62173508)
           };
            // Create a polyline geometry from the point collection.
            Polygon polygon1 = new Polygon(pointCollection1);

            // Return the polygon.
            return polygon1;
        }

        private Polygon CreatePolygon2()
        {
            // Create a point collection that represents polygon2. Use the same spatial reference as the underlying base map.
            PointCollection pointCollection2 = new PointCollection(SpatialReferences.WebMercator)
            {
                // Add all of the polygon2 boundary map points to the point collection.
                new MapPoint(5993520.19456882, -1063938.49607736),
                new MapPoint(3085421.63862418, -1383120.04490055),
                new MapPoint(3794713.96934239, -2979027.78901651),
                new MapPoint(6880135.60796657, -4078430.90162972),
                new MapPoint(7092923.30718203, -2837169.32287287),
                new MapPoint(8617901.81822617, -2092412.37561875),
                new MapPoint(6986529.4575743, 354646.16535905),
                new MapPoint(5319692.48038653, 1205796.96222089)
           };
            // Create a polyline geometry from the point collection.
            Polygon polygon2 = new Polygon(pointCollection2);

            // Return the polygon.
            return polygon2;
        }

        private void OnMakeUnionBufferClicked(object sender, EventArgs e)
        {
            try
            {
                // Get the boolean value whether to create a single convex hull (true) or independent convex hulls (false).
                bool unionBool = (bool)_convexHullListSwitch.Checked;

                // Add the geometries of the two polygon graphics to a list of geometries. It will be used as the 1st
                // input parameter of the GeometryEngine.ConvexHull function.
                List<Geometry> inputListOfGeomtries = new List<Geometry>
                {
                    _polygonGraphic1.Geometry,
                    _polygonGraphic2.Geometry
                };

                // Get the returned result from the convex hull operation. When unionBool = true there will be one returned
                // polygon, when unionBool = false there will be one convex hull returned per input geometry.
                IEnumerable<Geometry> convexHullGeometries = GeometryEngine.ConvexHull(inputListOfGeomtries, unionBool);

                // Loop through the returned geometries.
                foreach (Geometry oneGeometry in convexHullGeometries)
                {
                    // Create a simple line symbol for the outline of the convex hull graphic(s).
                    SimpleLineSymbol convexHullSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid,
                        System.Drawing.Color.Red, 10);

                    // Create the simple fill symbol for the convex hull graphic(s) - comprised of a fill style, fill
                    // color and outline. It will be a hollow (i.e.. see-through) polygon graphic with a thick red outline.
                    SimpleFillSymbol convexHullSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Null,
                        System.Drawing.Color.Red, convexHullSimpleLineSymbol);

                    // Create the graphic for the convex hull(s) - comprised of a polygon shape and fill symbol.
                    Graphic convexHullGraphic = new Graphic(oneGeometry, convexHullSimpleFillSymbol)
                    {

                        // Set the Z index for the convex hull graphic(s) so that they appear below the initial input graphics
                        // added earlier (polygon1 and polygon2).
                        ZIndex = 0
                    };

                    // Add the convex hull graphic to the graphics overlay collection.
                    _graphicsOverlay.Graphics.Add(convexHullGraphic);
                }

                // Disable the button after has been used.
                _convexHullListButton.Enabled = false;
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating convex hull operation.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("There was a problem generating the convex hull.");
                alertBuilder.SetMessage(ex.ToString());
                alertBuilder.Show();
            }
        }

        private void OnResetClicked(object sender, EventArgs e)
        {
            // Clear all existing graphics.
            _graphicsOverlay.Graphics.Clear();

            // Re-enable the button.
            _convexHullListButton.Enabled = true;

            // Add the polygons.
            _graphicsOverlay.Graphics.Add(_polygonGraphic1);
            _graphicsOverlay.Graphics.Add(_polygonGraphic2);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a TextView for the sample instructions.
            TextView sampleInstructionsTextView = new TextView(this)
            {
                Text = "Click the 'Convex Hull' button to create convex hull(s) from the polygon graphics."
            };
            layout.AddView(sampleInstructionsTextView);

            // Create a horizontal sub layout for the text view and switch controls.
            LinearLayout subLayout2 = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create a TextView for the union label.
            TextView unionInstructionsTextView = new TextView(this)
            {
                Text = "Union:"
            };
            subLayout2.AddView(unionInstructionsTextView);

            // Create a Switch for the union option.
            _convexHullListSwitch = new Switch(this)
            {
                Checked = true
            };
            subLayout2.AddView(_convexHullListSwitch);

            layout.AddView(subLayout2);

            // Create button to create the convex hull(s).
            _convexHullListButton = new Button(this)
            {
                Text = "Convex Hull"
            };
            _convexHullListButton.Click += OnMakeUnionBufferClicked;
            layout.AddView(_convexHullListButton);

            // Create button to reset the convex hull(s).
            _resetButton = new Button(this)
            {
                Text = "Reset"
            };
            _resetButton.Click += OnResetClicked;
            layout.AddView(_resetButton);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}