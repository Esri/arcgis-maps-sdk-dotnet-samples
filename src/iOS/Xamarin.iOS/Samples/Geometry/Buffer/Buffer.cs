// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.Buffer
{
    [Register("Buffer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Buffer",
        category: "Geometry",
        description: "Create a buffer around a map point and display the results as a `Graphic`",
        instructions: "1. Tap on the map.",
        tags: new[] { "analysis", "buffer", "euclidean", "geodesic", "geometry", "planar" })]
    public class Buffer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITextField _bufferDistanceMilesTextField;
        private UIView _geodesicSwatchSwatch;
        private UIView _planarSwatchSwatch;
        private UIButton _clearBuffersButton;

        public Buffer()
        {
            Title = "Buffer";
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap and add it to the map view.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Create a fill symbol for geodesic buffer polygons.            
            Colors geodesicBufferColor = Colors.FromArgb(120, 255, 0, 0);
            SimpleLineSymbol geodesicOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, geodesicBufferColor, 2);
            SimpleFillSymbol geodesicBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, geodesicBufferColor, geodesicOutlineSymbol);

            // Create a fill symbol for planar buffer polygons.            
            Colors planarBufferColor = Colors.FromArgb(120, 0, 0, 255);
            SimpleLineSymbol planarOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, planarBufferColor, 2);
            SimpleFillSymbol planarBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, planarBufferColor, planarOutlineSymbol);

            // Create a marker symbol for tap locations.
            SimpleMarkerSymbol tapSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.White, 14);

            // Create a graphics overlay to display geodesic polygons, set its renderer and add it to the map view.
            GraphicsOverlay geodesicPolysOverlay = new GraphicsOverlay
            {
                Id = "GeodesicPolys",
                Renderer = new SimpleRenderer(geodesicBufferFillSymbol)
            };
            _myMapView.GraphicsOverlays.Add(geodesicPolysOverlay);

            // Create a graphics overlay to display planar polygons, set its renderer and add it to the map view.
            GraphicsOverlay planarPolysOverlay = new GraphicsOverlay
            {
                Id = "PlanarPolys",
                Renderer = new SimpleRenderer(planarBufferFillSymbol)
            };
            _myMapView.GraphicsOverlays.Add(planarPolysOverlay);

            // Create a graphics overlay to display tap locations for buffers, set its renderer and add it to the map view.
            GraphicsOverlay tapLocationsOverlay = new GraphicsOverlay
            {
                Id = "TapPoints",
                Renderer = new SimpleRenderer(tapSymbol)
            };
            _myMapView.GraphicsOverlays.Add(tapLocationsOverlay);

            // Show the colors for each type of buffer in the UI.
            ShowBufferSwatches(planarBufferColor, geodesicBufferColor);
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Hide keyboard if present.
            _bufferDistanceMilesTextField.ResignFirstResponder();

            try
            {
                // Get the location tapped by the user (a map point in the WebMercator projected coordinate system).
                MapPoint userTapPoint = e.Location;

                // Get the buffer distance (miles) entered in the text box.
                double bufferInMiles = Convert.ToDouble(_bufferDistanceMilesTextField.Text);

                // Call a helper method to convert the input distance to meters.
                double bufferInMeters = LinearUnits.Miles.ToMeters(bufferInMiles);

                // Create a planar buffer graphic around the input location at the specified distance.
                Geometry bufferGeometryPlanar = GeometryEngine.Buffer(userTapPoint, bufferInMeters);
                Graphic planarBufferGraphic = new Graphic(bufferGeometryPlanar);

                // Create a geodesic buffer graphic using the same location and distance.
                Geometry bufferGeometryGeodesic = GeometryEngine.BufferGeodetic(userTapPoint, bufferInMeters, LinearUnits.Meters, double.NaN, GeodeticCurveType.Geodesic);
                Graphic geodesicBufferGraphic = new Graphic(bufferGeometryGeodesic);

                // Create a graphic for the user tap location.
                Graphic locationGraphic = new Graphic(userTapPoint);

                // Get the graphics overlays.
                GraphicsOverlay planarBufferGraphicsOverlay = _myMapView.GraphicsOverlays["PlanarPolys"];
                GraphicsOverlay geodesicBufferGraphicsOverlay = _myMapView.GraphicsOverlays["GeodesicPolys"];
                GraphicsOverlay tapPointGraphicsOverlay = _myMapView.GraphicsOverlays["TapPoints"];

                // Add the buffer polygons and tap location graphics to the appropriate graphic overlays.
                planarBufferGraphicsOverlay.Graphics.Add(planarBufferGraphic);
                geodesicBufferGraphicsOverlay.Graphics.Add(geodesicBufferGraphic);
                tapPointGraphicsOverlay.Graphics.Add(locationGraphic);
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating the buffers.
                UIAlertController alertController = UIAlertController.Create("Error creating buffers", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void ShowBufferSwatches(Colors planarBufferColor, Colors geodesicBufferColor)
        {
            // Create UIKit.UIColors to represent the System.Drawing.Colors used for the buffers.
            UIColor planarLabelColor = UIColor.FromRGBA(planarBufferColor.R,
                planarBufferColor.G,
                planarBufferColor.B,
                planarBufferColor.A);
            UIColor geodesicLabelColor = UIColor.FromRGBA(geodesicBufferColor.R,
                geodesicBufferColor.G,
                geodesicBufferColor.B,
                geodesicBufferColor.A);

            // Show buffer symbol colors in the UI by setting the appropriate text view fill color.
            _planarSwatchSwatch.BackgroundColor = planarLabelColor;
            _geodesicSwatchSwatch.BackgroundColor = geodesicLabelColor;
        }

        private void ClearBuffersButton_TouchUpInside(object sender, EventArgs e)
        {
            // Hide keyboard if present.
            _bufferDistanceMilesTextField.ResignFirstResponder();

            // Clear the buffer and point graphics.
            foreach (GraphicsOverlay ov in _myMapView.GraphicsOverlays)
            {
                ov.Graphics.Clear();
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

            UILabel helpLabel = new UILabel
            {
                Text = "Tap the map to create planar and geodesic buffers.",
                TextAlignment = UITextAlignment.Center,
                AdjustsFontSizeToFitWidth = true,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            UIView formArea = new UIView {BackgroundColor = UIColor.White};
            formArea.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel bufferInputLabel = new UILabel
            {
                Text = "Distance (miles):",
                TextAlignment = UITextAlignment.Right,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _bufferDistanceMilesTextField = new UITextField
            {
                BackgroundColor = UIColor.LightGray,
                KeyboardType = UIKeyboardType.NumberPad,
                Text = "1000",
                TextAlignment = UITextAlignment.Right,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _bufferDistanceMilesTextField.Layer.CornerRadius = 5;

            // Add padding within the field.
            _bufferDistanceMilesTextField.RightView = new UIView(new CGRect(0, 0, 5, 20)); // 5 is amount of left padding.
            _bufferDistanceMilesTextField.RightViewMode = UITextFieldViewMode.Always;

            UIStackView legendView = new UIStackView();
            legendView.TranslatesAutoresizingMaskIntoConstraints = false;
            legendView.Axis = UILayoutConstraintAxis.Horizontal;
            legendView.Alignment = UIStackViewAlignment.Center;
            legendView.Spacing = 8;

            _geodesicSwatchSwatch = new UIView();
            _geodesicSwatchSwatch.TranslatesAutoresizingMaskIntoConstraints = false;
            _geodesicSwatchSwatch.BackgroundColor = UIColor.Red;
            _geodesicSwatchSwatch.WidthAnchor.ConstraintEqualTo(16).Active = true;
            _geodesicSwatchSwatch.HeightAnchor.ConstraintEqualTo(16).Active = true;
            _geodesicSwatchSwatch.ClipsToBounds = true;
            _geodesicSwatchSwatch.Layer.CornerRadius = 8;
            legendView.AddArrangedSubview(_geodesicSwatchSwatch);

            UILabel geodesicSwatchLabel = new UILabel
            {
                Text = "Geodesic buffers",
                TextColor = UIColor.Red,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            legendView.AddArrangedSubview(geodesicSwatchLabel);

            UIView spacer = new UIView();
            spacer.TranslatesAutoresizingMaskIntoConstraints = false;
            spacer.SetContentCompressionResistancePriority((float) UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Horizontal);
            legendView.AddArrangedSubview(spacer);

            _planarSwatchSwatch = new UIView();
            _planarSwatchSwatch.BackgroundColor = UIColor.Blue;
            _planarSwatchSwatch.TranslatesAutoresizingMaskIntoConstraints = false;
            _planarSwatchSwatch.WidthAnchor.ConstraintEqualTo(16).Active = true;
            _planarSwatchSwatch.HeightAnchor.ConstraintEqualTo(16).Active = true;
            _planarSwatchSwatch.ClipsToBounds = true;
            _planarSwatchSwatch.Layer.CornerRadius = 8;
            legendView.AddArrangedSubview(_planarSwatchSwatch);

            UILabel planarSwatchLabel = new UILabel
            {
                Text = "Planar buffers",
                TextColor = UIColor.Blue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            legendView.AddArrangedSubview(planarSwatchLabel);

            _clearBuffersButton = new UIButton
            {
                ClipsToBounds = true,
                BackgroundColor = View.TintColor
            };
            _clearBuffersButton.SetTitle("Clear", UIControlState.Normal);
            _clearBuffersButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _clearBuffersButton.Layer.CornerRadius = 5;
            _clearBuffersButton.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView,
                formArea,
                helpLabel,
                bufferInputLabel,
                _bufferDistanceMilesTextField,
                legendView,
                _clearBuffersButton);

            // Lay out the views.
            // TODO: consider replacing with UIStackView-based layout.
            nfloat margin = 8;
            nfloat controlHeight = 4 * margin;

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                formArea.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                formArea.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                formArea.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                formArea.HeightAnchor.ConstraintEqualTo((3 * margin) + (2 * controlHeight)),

                legendView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8),
                legendView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),
                legendView.TopAnchor.ConstraintEqualTo(formArea.BottomAnchor),
                legendView.HeightAnchor.ConstraintEqualTo(24),

                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.TopAnchor.ConstraintEqualTo(legendView.BottomAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                helpLabel.TopAnchor.ConstraintEqualTo(formArea.TopAnchor, margin),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(controlHeight),

                bufferInputLabel.TopAnchor.ConstraintEqualTo(helpLabel.BottomAnchor, margin),
                bufferInputLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, margin),
                bufferInputLabel.TrailingAnchor.ConstraintEqualTo(formArea.CenterXAnchor, -margin),
                bufferInputLabel.HeightAnchor.ConstraintEqualTo(controlHeight),

                _bufferDistanceMilesTextField.TopAnchor.ConstraintEqualTo(bufferInputLabel.TopAnchor),
                _bufferDistanceMilesTextField.LeadingAnchor.ConstraintEqualTo(formArea.CenterXAnchor, margin),
                _bufferDistanceMilesTextField.TrailingAnchor.ConstraintEqualTo(_clearBuffersButton.LeadingAnchor, -margin),
                _bufferDistanceMilesTextField.HeightAnchor.ConstraintEqualTo(controlHeight),

                _clearBuffersButton.TopAnchor.ConstraintEqualTo(_bufferDistanceMilesTextField.TopAnchor),
                _clearBuffersButton.WidthAnchor.ConstraintEqualTo(100),
                _clearBuffersButton.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -margin),
                _clearBuffersButton.HeightAnchor.ConstraintEqualTo(controlHeight)
            });
        }

        private bool HandleTextField(UITextField textField)
        {
            // This method allows pressing 'return' to dismiss the software keyboard.
            textField.ResignFirstResponder();
            return true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            _clearBuffersButton.TouchUpInside += ClearBuffersButton_TouchUpInside;
            _bufferDistanceMilesTextField.ShouldReturn += HandleTextField;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
            _bufferDistanceMilesTextField.ShouldReturn -= HandleTextField;
            _clearBuffersButton.TouchUpInside -= ClearBuffersButton_TouchUpInside;
        }
    }
}