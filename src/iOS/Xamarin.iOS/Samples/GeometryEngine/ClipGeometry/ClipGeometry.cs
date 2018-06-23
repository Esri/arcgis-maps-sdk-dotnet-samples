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
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.ClipGeometry
{
    [Register("ClipGeometry")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Clip geometry",
        "GeometryEngine",
        "This sample demonstrates how to clip a geometry with an envelope using the GeometryEngine",
        "Click the 'Clip' button to clip the blue graphic with red envelopes.",
        "")]
    public class ClipGeometry : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _helpToolbar = new UIToolbar();
        private readonly UIToolbar _controlsToolbar = new UIToolbar();
        private UIButton _clipButton;
        private UILabel _sampleInstructionsLabel;

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
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin + toolbarHeight, 0, toolbarHeight, 0);
                _helpToolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, toolbarHeight);
                _controlsToolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _sampleInstructionsLabel.Frame = new CGRect(margin, topMargin + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _clipButton.Frame = new CGRect(margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create and show a new map using the WebMercator spatial reference.
            _myMapView.Map = new Map(SpatialReferences.WebMercator)
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
            await _myMapView.SetViewpointGeometryAsync(visibleExtent);
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
                _clipButton.Enabled = false;
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating clip operation.
                UIAlertController alertController = UIAlertController.Create("Geometry Engine Failed!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void CreateLayout()
        {
            // Create a UITextView for the overall sample instructions.
            _sampleInstructionsLabel = new UILabel
            {
                Text = "Tap 'Clip' to clip the blue graphic with red envelopes.",
                Lines = 1,
                AdjustsFontSizeToFitWidth = true
            };

            // Create a UIButton to clip the polygons.
            _clipButton = new UIButton();
            _clipButton.SetTitle("Clip", UIControlState.Normal);
            _clipButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            // - Hook to touch event to clip the polygons.
            _clipButton.TouchUpInside += ClipButton_TouchUpInside;

            // Add the MapView and other controls to the page.
            View.AddSubviews(_myMapView, _helpToolbar, _controlsToolbar, _sampleInstructionsLabel, _clipButton);
        }
    }
}