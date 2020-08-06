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
using System.Linq;
using UIKit;

namespace ArcGISRuntime.Samples.ConvexHull
{
    [Register("ConvexHull")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Convex hull",
        category: "Geometry",
        description: "Create a convex hull for a given set of points. The convex hull is a polygon with shortest perimeter that encloses a set of points. As a visual analogy, consider a set of points as nails in a board. The convex hull of the points would be like a rubber band stretched around the outermost nails.",
        instructions: "Tap on the map to add points. Tap the \"Create Convex Hull\" button to generate the convex hull of those points. Tap the \"Reset\" button to start over.",
        tags: new[] { "convex hull", "geometry", "spatial analysis" })]
    public class ConvexHull : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _createHullButton;
        private UIBarButtonItem _helpButton;
        private UIBarButtonItem _resetButton;

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
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Normalize the tapped point.
                var centralizedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);

                // Add the map point to the list that will be used by the GeometryEngine.ConvexHull operation.
                _inputPointCollection.Add(centralizedPoint);

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
                Graphic userTappedGraphic = new Graphic(e.Location, new Dictionary<string, object> { { "Type", "Point" } }, userTappedSimpleMarkerSymbol)
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
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _createHullButton = new UIBarButtonItem();
            _createHullButton.Title = "Create convex hull";
            _createHullButton.Enabled = false;

            _helpButton = new UIBarButtonItem();
            _helpButton.Title = "Help";

            _resetButton = new UIBarButtonItem();
            _resetButton.Title = "Reset";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _helpButton,
                _resetButton,
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            _helpButton.Clicked += HelpButton_Click;
            _resetButton.Clicked += ResetButton_Click;
            _createHullButton.Clicked += ConvexHullButton_Click;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
            _helpButton.Clicked -= HelpButton_Click;
            _resetButton.Clicked -= ResetButton_Click;
            _createHullButton.Clicked -= ConvexHullButton_Click;
        }
    }
}