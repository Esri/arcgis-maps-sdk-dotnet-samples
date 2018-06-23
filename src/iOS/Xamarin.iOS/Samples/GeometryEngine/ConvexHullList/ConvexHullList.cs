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
using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.ConvexHullList
{
    [Register("ConvexHullList")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Convex hull list",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine.ConvexHull to generate convex hull polygon(s) from multiple input geometries.",
        "Click the 'ConvexHull' button to create convex hull(s) from the polygon graphics. If the 'Union' checkbox is checked, the resulting output will be one polygon being the convex hull for the two input polygons. If the 'Union' checkbox is un-checked, the resulting output will have two convex hull polygons - one for each of the two input polygons.",
        "Analysis", "ConvexHull", "GeometryEngine")]
    public class ConvexHullList : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _helpToolbar = new UIToolbar();
        private readonly UIToolbar _controlsToolbar = new UIToolbar();
        private UIButton _convexHullListButton;
        private UISwitch _unionSwitch;
        private UILabel _switchLabel;
        private UILabel _sampleHelpLabel;

        // Graphics overlay to display the graphics.
        private GraphicsOverlay _graphicsOverlay;

        // Graphic that represents polygon1.
        private Graphic _polygonGraphic1;

        // Graphic that represents polygon2.
        private Graphic _polygonGraphic2;

        public ConvexHullList()
        {
            Title = "Convex hull list";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin + toolbarHeight, 0, toolbarHeight, 0);
                _helpToolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, toolbarHeight);
                _controlsToolbar.Frame = new CGRect(0, View.Bounds.Height - controlHeight - 2 * margin, View.Bounds.Width, toolbarHeight);
                _sampleHelpLabel.Frame = new CGRect(margin, topMargin + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _switchLabel.Frame = new CGRect(margin, View.Bounds.Height - controlHeight - margin, 50 - 2 * margin, controlHeight);
                _unionSwitch.Frame = new CGRect(50 + margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width - 50 - 2 * margin, controlHeight);
                _convexHullListButton.Frame = new CGRect(View.Bounds.Width / 2 + margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Create and show a map with a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

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

            // Return a polyline geometry from the point collection.
            return new Polygon(pointCollection1);
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

            // Return a polyline geometry from the point collection.
            return new Polygon(pointCollection2);
        }

        private void BufferButton_Click(object sender, EventArgs e)
        {
            // Reset the sample state.
            // - Clear all existing graphics.
            _graphicsOverlay.Graphics.Clear();
            // - Add the polygons.
            _graphicsOverlay.Graphics.Add(_polygonGraphic1);
            _graphicsOverlay.Graphics.Add(_polygonGraphic2);

            try
            {
                // Get the boolean value whether to create a single convex hull (true) or independent convex hulls (false).
                bool unionBool = _unionSwitch.On;

                // Add the geometries of the two polygon graphics to a list of geometries. It will be used as the 1st
                // input parameter of the GeometryEngine.ConvexHull function.
                List<Geometry> inputGeometryList = new List<Geometry>
                {
                    _polygonGraphic1.Geometry,
                    _polygonGraphic2.Geometry
                };

                // Get the returned result from the convex hull operation. When unionBool = true there will be one returned
                // polygon, when unionBool = false there will be one convex hull returned per input geometry.
                foreach (Geometry oneGeometry in GeometryEngine.ConvexHull(inputGeometryList, unionBool))
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
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating convex hull operation.
                UIAlertController alertController = UIAlertController.Create("Geometry Engine Failed!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void CreateLayout()
        {
            // Create a UITextView for the overall sample instructions.
            _sampleHelpLabel = new UILabel
            {
                Text = "Tap 'Create convex hull'. Result will be two polygons if 'Union' is off.",
                Lines = 1,
                AdjustsFontSizeToFitWidth = true
            };

            // Create a UILabel for the UISwitch label.
            _switchLabel = new UILabel
            {
                Text = "Union:",
                AdjustsFontSizeToFitWidth = true
            };

            // Create a UISwitch for toggling the union of the convex hull(s).
            _unionSwitch = new UISwitch
            {
                On = true
            };

            // Create a UIButton to create the convex hull(s).
            _convexHullListButton = new UIButton();
            _convexHullListButton.SetTitle("Create convex hull", UIControlState.Normal);
            _convexHullListButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _convexHullListButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
            // - Hook to touch event to do querying
            _convexHullListButton.TouchUpInside += BufferButton_Click;

            // Add the MapView and other controls to the page.
            View.AddSubviews(_myMapView, _helpToolbar, _controlsToolbar, _sampleHelpLabel, _switchLabel, _unionSwitch, _convexHullListButton);
        }
    }
}