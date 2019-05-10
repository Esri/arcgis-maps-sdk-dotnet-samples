// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ConvexHull
{
    [Register("ConvexHull")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Convex hull",
        "Geometry",
        "This sample demonstrates how to use the GeometryEngine.ConvexHull operation to generate a polygon that encloses a series of user-tapped map points.",
        "Tap on the map in several places, then click the 'Convex Hull' button.",
        "Analysis", "ConvexHull", "GeometryEngine")]
    public class ConvexHull : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _createHullButton;

        // Graphics overlay to display the graphics.
        private GraphicsOverlay _graphicsOverlay;

        // List of geometry values (MapPoints in this case) that will be used by the GeometryEngine.ConvexHull operation.
        private readonly PointCollection _inputPointCollection = new PointCollection(SpatialReferences.WebMercator);

        public ConvexHull()
        {
            Title = "Convex hull";
        }

        private void Initialize()
        {
            // Create and show a map with a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Create an overlay to hold the lines of the hull.
            _graphicsOverlay = new GraphicsOverlay();

            // Add the created graphics overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Wire up the MapView's GeoViewTapped event handler.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Add the map point to the list that will be used by the GeometryEngine.ConvexHull operation.
                _inputPointCollection.Add(e.Location);

                // Check if there are at least three points.
                if (_inputPointCollection.Count > 2)
                {
                    // Enable the button for creating hulls.
                    _createHullButton.Enabled = true;
                }

                // Create a simple marker symbol to display where the user tapped/clicked on the map. The marker symbol
                // will be a solid, red circle.
                SimpleMarkerSymbol userTappedSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);

                // Create a new graphic for the spot where the user clicked on the map using the simple marker symbol. 
                Graphic userTappedGraphic = new Graphic(e.Location, new Dictionary<string, object>{{ "Type", "Point" }}, userTappedSimpleMarkerSymbol)
                {
                    // Set the Z index for the user tapped graphic so that it appears above the convex hull graphic(s) added later.
                    ZIndex = 1
                };

                // Add the user tapped/clicked map point graphic to the graphic overlay.
                _graphicsOverlay.Graphics.Add(userTappedGraphic);
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem adding user tapped graphics.
                UIAlertController alertController = UIAlertController.Create("Can't add user-tapped graphic.", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void ConvexHullButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a multi-point geometry from the user tapped input map points.
                Multipoint inputMultipoint = new Multipoint(_inputPointCollection);

                // Get the returned result from the convex hull operation.
                Geometry convexHullGeometry = GeometryEngine.ConvexHull(inputMultipoint);

                // Create a simple line symbol for the outline of the convex hull graphic(s).
                SimpleLineSymbol convexHullSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 4);

                // Create the simple fill symbol for the convex hull graphic(s) - comprised of a fill style, fill
                // color and outline. It will be a hollow (i.e.. see-through) polygon graphic with a thick red outline.
                SimpleFillSymbol convexHullSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, System.Drawing.Color.Red, convexHullSimpleLineSymbol);

                // Create the graphic for the convex hull - comprised of a polygon shape and fill symbol.
                Graphic convexHullGraphic = new Graphic(convexHullGeometry, new Dictionary<string, object>() { { "Type", "Hull" } }, convexHullSimpleFillSymbol)
                {
                    // Set the Z index for the convex hull graphic so that it appears below the initial input user tapped map point graphics added earlier.
                    ZIndex = 0
                };

                // Remove any existing convex hull graphics from the overlay.
                foreach (Graphic g in _graphicsOverlay.Graphics.ToList())
                    if ((string)g.Attributes["Type"] == "Hull")
                        _graphicsOverlay.Graphics.Remove(g);

                // Add the convex hull graphic to the graphics overlay collection.
                _graphicsOverlay.Graphics.Add(convexHullGraphic);

                // Disable the button after has been used.
                _createHullButton.Enabled = false;
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating convex hull operation.
                UIAlertController alertController = UIAlertController.Create("Geometry Engine Failed!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            // Clear the existing points and graphics.
            _inputPointCollection.Clear();
            _graphicsOverlay.Graphics.Clear();

            // Disable the convex hull button.
            _createHullButton.Enabled = false;
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            UIAlertController unionAlert = UIAlertController.Create("Create a convex hull", "Tap the map in several places, then use the buttons to create or reset the convex hull.", UIAlertControllerStyle.Alert);
            unionAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(unionAlert, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _createHullButton =
                new UIBarButtonItem("Create convex hull", UIBarButtonItemStyle.Plain, ConvexHullButton_Click) {Enabled = false};

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem("Help", UIBarButtonItemStyle.Plain, HelpButton_Click),
                new UIBarButtonItem("Reset", UIBarButtonItemStyle.Plain, ResetButton_Click),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _createHullButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }
    }
}