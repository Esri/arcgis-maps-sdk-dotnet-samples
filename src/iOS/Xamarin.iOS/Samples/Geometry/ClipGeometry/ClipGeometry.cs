// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ClipGeometry
{
    [Register("ClipGeometry")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Clip geometry",
        category: "Geometry",
        description: "Clip a geometry with another geometry.",
        instructions: "Tap the \"Clip\" button to clip the blue graphic with the red dashed envelopes.",
        tags: new[] { "analysis", "clip", "geometry" })]
    public class ClipGeometry : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _clipButton;

        // Graphics overlay to display input geometries for the clip operation.
        private GraphicsOverlay _inputGeometriesGraphicsOverlay;

        // Graphic that represents the 1st parameter of the GeometryEngine.Clip operation - it follows the boundary of Colorado.
        private Graphic _coloradoGraphic;

        // One of the graphics that represents the 2nd parameter of the GeometryEngine.Clip operation - it will be an envelope 
        // that falls completely outside the boundary of Colorado.
        private Graphic _outsideGraphic;

        // One of the graphics that represents the 2nd parameter of the GeometryEngine.Clip operation - it will be an envelope 
        // that is completely contained within the boundary of Colorado.
        private Graphic _containedGraphic;

        // One of the graphics that represents the 2nd parameter of the GeometryEngine.Clip operation - it will be an envelope 
        // that is intersects the boundary of Colorado.
        private Graphic _intersectingGraphic;

        // Graphics overlay to display the resulting geometries from the three GeometryEngine.Clip operations.
        private GraphicsOverlay _clipAreasGraphicsOverlay;

        public ClipGeometry()
        {
            Title = "Clip geometry";
        }

        private void Initialize()
        {
            // Create and show a new map using the WebMercator spatial reference.
            Map newMap = new Map(SpatialReferences.WebMercator)
            {
                // Set the basemap of the map to be a topographic layer.
                Basemap = Basemap.CreateTopographic()
            };

            // Create a graphics overlay to hold the input geometries for the clip operation.
            _inputGeometriesGraphicsOverlay = new GraphicsOverlay();

            // Add the input geometries graphics overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_inputGeometriesGraphicsOverlay);

            // Create a graphics overlay to hold the resulting geometries from the three GeometryEngine.Clip operations.
            _clipAreasGraphicsOverlay = new GraphicsOverlay();

            // Add the resulting geometries graphics overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_clipAreasGraphicsOverlay);

            // Create a simple line symbol for the 1st parameter of the GeometryEngine.Clip operation - it follows the 
            // boundary of Colorado.
            SimpleLineSymbol coloradoSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 4);

            // Create the color that will be used as the fill for the Colorado graphic. It will be a semi-transparent, blue color.
            System.Drawing.Color coloradoFillColor = System.Drawing.Color.FromArgb(34, 0, 0, 255);

            // Create the simple fill symbol for the Colorado graphic - comprised of a fill style, fill color and outline.
            SimpleFillSymbol coloradoSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, coloradoFillColor, coloradoSimpleLineSymbol);

            // Create the geometry of the Colorado graphic.
            Envelope colorado = new Envelope(
                new MapPoint(-11362327.128340, 5012861.290274, SpatialReferences.WebMercator),
                new MapPoint(-12138232.018408, 4441198.773776, SpatialReferences.WebMercator));

            // Create the graphic for Colorado - comprised of a polygon shape and fill symbol.
            _coloradoGraphic = new Graphic(colorado, coloradoSimpleFillSymbol);

            // Add the Colorado graphic to the input geometries graphics overlay collection.
            _inputGeometriesGraphicsOverlay.Graphics.Add(_coloradoGraphic);

            // Create a simple line symbol for the three different clip geometries.
            SimpleLineSymbol clipGeometriesLineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Dot, System.Drawing.Color.Red, 5);

            // Create an envelope outside Colorado.
            Envelope outsideEnvelope = new Envelope(
                new MapPoint(-11858344.321294, 5147942.225174, SpatialReferences.WebMercator),
                new MapPoint(-12201990.219681, 5297071.577304, SpatialReferences.WebMercator)
            );

            // Create the graphic for an envelope outside Colorado - comprised of a polyline shape and line symbol.
            _outsideGraphic = new Graphic(outsideEnvelope, clipGeometriesLineSymbol);

            // Add the envelope outside Colorado graphic to the graphics overlay collection.
            _inputGeometriesGraphicsOverlay.Graphics.Add(_outsideGraphic);

            // Create an envelope intersecting Colorado.
            Envelope intersectingEnvelope = new Envelope(
                new MapPoint(-11962086.479298, 4566553.881363, SpatialReferences.WebMercator),
                new MapPoint(-12260345.183558, 4332053.378376, SpatialReferences.WebMercator)
            );

            // Create the graphic for an envelope intersecting Colorado - comprised of a polyline shape and line symbol.
            _intersectingGraphic = new Graphic(intersectingEnvelope, clipGeometriesLineSymbol);

            // Add the envelope intersecting Colorado graphic to the graphics overlay collection.
            _inputGeometriesGraphicsOverlay.Graphics.Add(_intersectingGraphic);

            // Create a envelope inside Colorado.
            Envelope containedEnvelope = new Envelope(
                new MapPoint(-11655182.595204, 4741618.772994, SpatialReferences.WebMercator),
                new MapPoint(-11431488.567009, 4593570.068343, SpatialReferences.WebMercator)
            );

            // Create the graphic for an envelope inside Colorado - comprised of a polyline shape and line symbol.
            _containedGraphic = new Graphic(containedEnvelope, clipGeometriesLineSymbol);

            // Add the envelope inside Colorado graphic to the graphics overlay collection.
            _inputGeometriesGraphicsOverlay.Graphics.Add(_containedGraphic);

            // Get the extent of all of the graphics in the graphics overlay with a little padding to used as the initial zoom extent of the map.
            Geometry visibleExtent = GetExtentOfGraphicsOverlay(_inputGeometriesGraphicsOverlay, 1.3, SpatialReferences.WebMercator);

            // Set the initial visual extent of the map view to the extent of the graphics overlay.
            newMap.InitialViewpoint = new Viewpoint(visibleExtent);

            // Assign the map to the MapView.
            _myMapView.Map = newMap;
        }

        private Geometry GetExtentOfGraphicsOverlay(GraphicsOverlay inputGraphicsOverlay, double expansionFactor, SpatialReference spatialReferenceType)
        {
            // Get all of the graphics contained in the graphics overlay.
            GraphicCollection inputGraphicCollection = inputGraphicsOverlay.Graphics;

            // Create a new envelope builder using the same spatial reference as the graphics.
            EnvelopeBuilder unionEnvelopeBuilder = new EnvelopeBuilder(spatialReferenceType);

            // Loop through each graphic in the graphic collection.
            foreach (Graphic oneGraphic in inputGraphicCollection)
            {
                // Union the extent of each graphic in the envelope builder.
                unionEnvelopeBuilder.UnionOf(oneGraphic.Geometry.Extent);
            }

            // Expand the envelope builder by the expansion factor.
            unionEnvelopeBuilder.Expand(expansionFactor);

            // Return the unioned extent plus the expansion factor.
            return unionEnvelopeBuilder.Extent;
        }

        private void ClipButton_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                // Remove the Colorado graphic from the input geometries graphics overlay collection. That way it will be easier 
                // to see the clip versions of the GeometryEngine.Clip operation. 
                _inputGeometriesGraphicsOverlay.Graphics.Remove(_coloradoGraphic);

                // Loop through each graphic in the input geometries for the clip operation.
                foreach (Graphic oneGraphic in _inputGeometriesGraphicsOverlay.Graphics)
                {
                    // Perform the clip operation. The first parameter of the clip operation will always be the Colorado graphic. 
                    // The second parameter of the clip operation will be one of the 3 different clip geometries (_outsideGraphic, 
                    // _containedGraphic, or _intersectingGraphic).
                    Geometry myGeometry = GeometryEngine.Clip(_coloradoGraphic.Geometry, (Envelope) oneGraphic.Geometry);

                    // Only work on returned geometries that are not null.
                    if (myGeometry != null)
                    {
                        // Create the graphic as a result of the clip operation using the same symbology that was defined for 
                        // the _coloradoGraphic defined in the Initialize() method previously. 
                        Graphic clippedGraphic = new Graphic(myGeometry, _coloradoGraphic.Symbol);

                        // Add the clipped graphic to the clip areas graphics overlay collection.
                        _clipAreasGraphicsOverlay.Graphics.Add(clippedGraphic);
                    }
                }

                // Disable the button after has been used.
                (sender as UIBarButtonItem).Enabled = false;
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating clip operation.
                UIAlertController alertController = UIAlertController.Create("Geometry Engine Failed!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
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

            _clipButton = new UIBarButtonItem();
            _clipButton.Title = "Clip geometries";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _clipButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
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
            _clipButton.Clicked += ClipButton_TouchUpInside;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _clipButton.Clicked -= ClipButton_TouchUpInside;
        }
    }
}