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
        "Buffer",
        "Geometry",
        "This sample demonstrates how to use `GeometryEngine.Buffer` to create polygons from a map location and linear distance (radius). For each input location, the sample creates two buffer polygons (using the same distance) and displays them on the map using different symbols. One polygon is calculated using the `planar` (flat) coordinate space of the map's spatial reference. The other is created using a `geodesic` technique that considers the curved shape of the Earth's surface (which is generally a more accurate representation). Distortion in the map increases as you move away from the standard parallels of the spatial reference's projection. This map is in Web Mercator so areas near the equator are the most accurate. As you move the buffer location north or south from that line, you'll see a greater difference in the polygon size and shape. Planar operations are generally faster, but performance improvement may only be noticeable for large operations (buffering a great number or complex geometry).\nCreating buffers is a core concept in GIS proximity analysis, allowing you to visualize and locate geographic features contained within a polygon. For example, suppose you wanted to visualize areas of your city where alcohol sales are prohibited because they are within 500 meters of a school. The first step in this proximity analysis would be to generate 500 meter buffer polygons around all schools in the city. Any such businesses you find inside one of the resulting polygons are violating the law. If you are using planar buffers, make sure that the input locations and distance are suited to the spatial reference you're using. Remember that you can also create your buffers using geodesic and then project them to the spatial reference you need for display or analysis. For more information about using buffer analysis, see [How buffer analysis works](https://pro.arcgis.com/en/pro-app/tool-reference/analysis/how-buffer-analysis-works.htm) in the ArcGIS Pro documentation.",
        "1. Tap on the map.\n2. A planar and a geodesic buffer will be created at the tap location using the distance (miles) specified in the text box.\n3. Continue tapping to create additional buffers. Notice that buffers closer to the equator are similar in size. As you move north or south from the equator, however, the geodesic polygons appear larger. Geodesic polygons are in fact a better representation of the true shape and size of the buffer.\n 4. Click `Clear` to remove all buffers and start again.",
        "Buffer, Geodesic, Planar")]
    public class Buffer : UIViewController
    {
        // Create and hold references to the UI controls.
        private MapView _myMapView;
        private UILabel _helpLabel;
        private UIToolbar _formArea;
        private UILabel _bufferInputLabel;
        private UITextField _bufferDistanceMilesTextField;
        private UILabel _geodesicSwatchLabel;
        private UILabel _planarSwatchLabel;

        // A button to clear buffer graphics from the display.
        private UIButton _clearBuffersButton;

        public Buffer()
        {
            Title = "Buffer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap and add it to the map view.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Handle the MapView's GeoViewTapped event to create buffers.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;

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
            _planarSwatchLabel.BackgroundColor = planarLabelColor;
            _geodesicSwatchLabel.BackgroundColor = geodesicLabelColor;
        }

        private void ClearBuffersButton_TouchUpInside(object sender, EventArgs e)
        {
            // Clear the buffer and point graphics.
            foreach (GraphicsOverlay ov in _myMapView.GraphicsOverlays)
            {
                ov.Graphics.Clear();
            }
        }

        public override void LoadView()
        {
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            
            // Create the help label.
            _helpLabel = new UILabel
            {
                Text = "Tap the map to create planar and geodesic buffers.",
                TextAlignment = UITextAlignment.Center,
                AdjustsFontSizeToFitWidth = true,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Create the buffer input area (toolbar).
            _formArea = new UIToolbar();
            _formArea.TranslatesAutoresizingMaskIntoConstraints = false;

            // Create the buffer input label.
            _bufferInputLabel = new UILabel
            {
                Text = "Distance (miles):",
                TextAlignment = UITextAlignment.Right,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Create the buffer input control.
            _bufferDistanceMilesTextField = new UITextField
            {
                BackgroundColor = UIColor.White,
                KeyboardType = UIKeyboardType.NumberPad,
                Text = "1000",
                TextAlignment = UITextAlignment.Right,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _bufferDistanceMilesTextField.Layer.CornerRadius = 5;

            // Add padding within the field.
            _bufferDistanceMilesTextField.RightView = new UIView(new CGRect(0, 0, 5, 20)); // 5 is amount of left padding
            _bufferDistanceMilesTextField.RightViewMode = UITextFieldViewMode.Always;

            // Allow pressing 'return' to dismiss the keyboard.
            _bufferDistanceMilesTextField.ShouldReturn += (textField) =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Create the label to show the planar buffer color.
            _planarSwatchLabel = new UILabel
            {
                AdjustsFontSizeToFitWidth = true,
                TextColor = UIColor.White,
                Text = "Planar buffers",
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Layer = { CornerRadius = 5 }
            };

            // Create the label to show the geodesic buffer color.
            _geodesicSwatchLabel = new UILabel
            {
                AdjustsFontSizeToFitWidth = true,
                TextColor = UIColor.White,
                Text = "Geodesic buffers",
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Layer = { CornerRadius = 5 }
            };

            // Create the clear buffers button.
            _clearBuffersButton = new UIButton
            {
                ClipsToBounds = true,
                BackgroundColor = View.TintColor
            };
            _clearBuffersButton.SetTitle("Clear", UIControlState.Normal);
            _clearBuffersButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _clearBuffersButton.Layer.CornerRadius = 5;
            _clearBuffersButton.TranslatesAutoresizingMaskIntoConstraints = false;

            // Handle the clear buffers button press.
            _clearBuffersButton.TouchUpInside += ClearBuffersButton_TouchUpInside;

            // Add views to the page.
            View.AddSubviews(_myMapView,
                _formArea,
                _helpLabel,
                _bufferInputLabel,
                _bufferDistanceMilesTextField,
                _planarSwatchLabel,
                _geodesicSwatchLabel,
                _clearBuffersButton);

            nfloat margin = 8;
            nfloat controlHeight = 4 * margin;
            _formArea.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _formArea.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _formArea.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _formArea.HeightAnchor.ConstraintEqualTo((4 * margin) + (3 * controlHeight)).Active = true;

            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.TopAnchor.ConstraintEqualTo(_formArea.BottomAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            _helpLabel.TopAnchor.ConstraintEqualTo(_formArea.TopAnchor, margin).Active = true;
            _helpLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor).Active = true;
            _helpLabel.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor).Active = true;
            _helpLabel.HeightAnchor.ConstraintEqualTo(controlHeight).Active = true;

            _bufferInputLabel.TopAnchor.ConstraintEqualTo(_helpLabel.BottomAnchor, margin).Active = true;
            _bufferInputLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, margin).Active = true;
            _bufferInputLabel.TrailingAnchor.ConstraintEqualTo(_formArea.CenterXAnchor, -margin).Active = true;
            _bufferInputLabel.HeightAnchor.ConstraintEqualTo(controlHeight).Active = true;

            _bufferDistanceMilesTextField.TopAnchor.ConstraintEqualTo(_bufferInputLabel.TopAnchor).Active = true;
            _bufferDistanceMilesTextField.LeadingAnchor.ConstraintEqualTo(_formArea.CenterXAnchor, margin).Active = true;
            _bufferDistanceMilesTextField.TrailingAnchor.ConstraintEqualTo(_geodesicSwatchLabel.CenterXAnchor, -margin).Active = true;
            _bufferDistanceMilesTextField.HeightAnchor.ConstraintEqualTo(controlHeight).Active = true;

            _clearBuffersButton.TopAnchor.ConstraintEqualTo(_bufferDistanceMilesTextField.TopAnchor).Active = true;
            _clearBuffersButton.LeadingAnchor.ConstraintEqualTo(_geodesicSwatchLabel.CenterXAnchor, margin).Active = true;
            _clearBuffersButton.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -margin).Active = true;
            _clearBuffersButton.HeightAnchor.ConstraintEqualTo(controlHeight).Active = true;
            
            _planarSwatchLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, margin).Active = true;
            _planarSwatchLabel.TrailingAnchor.ConstraintEqualTo(View.CenterXAnchor, -margin).Active = true;
            _planarSwatchLabel.TopAnchor.ConstraintEqualTo(_bufferInputLabel.BottomAnchor, margin).Active = true;
            _planarSwatchLabel.HeightAnchor.ConstraintEqualTo(controlHeight).Active = true;

            _geodesicSwatchLabel.LeadingAnchor.ConstraintEqualTo(View.CenterXAnchor, margin).Active = true;
            _geodesicSwatchLabel.TopAnchor.ConstraintEqualTo(_planarSwatchLabel.TopAnchor).Active = true;
            _geodesicSwatchLabel.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -margin).Active = true;
            _geodesicSwatchLabel.HeightAnchor.ConstraintEqualTo(controlHeight).Active = true;
        }
    }
}