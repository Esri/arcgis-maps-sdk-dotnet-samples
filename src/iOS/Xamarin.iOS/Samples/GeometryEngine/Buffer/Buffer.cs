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

namespace ArcGISRuntime.Samples.Buffer
{
    [Register("Buffer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine.Buffer to generate a polygon from an input geometry with a buffer distance.",
        "Tap on the map to specify a map point location. A buffer will created and displayed based upon the buffer value (in miles) specified in the textbox. Repeat the procedure to add additional map point and buffers. The generated buffers can overlap and are independent of each other.",
        "")]
    public class Buffer : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _helpToolbar = new UIToolbar();
        private UILabel _bufferHelpLabel;
        private UITextField _bufferDistanceEntry;
        private UILabel _helpLabel;

        // Graphics overlay to display buffer-related graphics.
        private GraphicsOverlay _graphicsOverlay;

        public Buffer()
        {
            Title = "Buffer";
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
                nfloat topStart = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat colSplit = View.Bounds.Width / 3;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _helpToolbar.Frame = new CGRect(0, topStart, View.Bounds.Width, 2 * controlHeight + 3 * margin);
                _helpLabel.Frame = new CGRect(margin, topStart + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _bufferHelpLabel.Frame = new CGRect(margin, topStart + controlHeight + 2 * margin, colSplit - 2 * margin, controlHeight);
                _bufferDistanceEntry.Frame = new CGRect(colSplit + margin, topStart + controlHeight + 2 * margin, View.Bounds.Width - colSplit - 2 * margin, controlHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(_helpToolbar.Frame.Bottom, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Create an envelope that covers the Dallas/Fort Worth area.
            Geometry startingEnvelope = new Envelope(-10863035.97, 3838021.34, -10744801.344, 3887145.299, SpatialReferences.WebMercator);

            // Create and show a map with a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic())
            {
                InitialViewpoint = new Viewpoint(startingEnvelope)
            };

            // Create a graphics overlay to show the buffered related graphics.
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
                // Create a map point (in the WebMercator projected coordinate system) from the GUI screen coordinate.
                MapPoint userTappedMapPoint = _myMapView.ScreenToLocation(e.Position);

                // Get the buffer size from the UITextField.
                double bufferInMiles = Convert.ToDouble(_bufferDistanceEntry.Text);

                // Create a variable to be the buffer size in meters. There are 1609.34 meters in one mile.
                double bufferInMeters = bufferInMiles * 1609.34;

                // Get a buffered polygon from the GeometryEngine Buffer operation centered on the map point. 
                // Note: The input distance to the Buffer operation is in meters. This matches the backdrop 
                // basemap units which is also meters.
                Geometry bufferGeometry = GeometryEngine.Buffer(userTappedMapPoint, bufferInMeters);

                // Create the outline (a simple line symbol) for the buffered polygon. It will be a solid, thick, green line.
                SimpleLineSymbol bufferSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 5);

                // Create the color that will be used for the fill of the buffered polygon. It will be a semi-transparent, green color.
                System.Drawing.Color bufferFillColor = System.Drawing.Color.FromArgb(125, 0, 255, 0);

                // Create simple fill symbol for the buffered polygon. It will be solid, semi-transparent, green fill with a solid, 
                // thick, green outline.
                SimpleFillSymbol bufferSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferFillColor, bufferSimpleLineSymbol);

                // Create a new graphic for the buffered polygon using the defined simple fill symbol.
                Graphic bufferGraphic = new Graphic(bufferGeometry, bufferSimpleFillSymbol);

                // Add the buffered polygon graphic to the graphic overlay.
                _graphicsOverlay.Graphics.Add(bufferGraphic);

                // Create a simple marker symbol to display where the user tapped/clicked on the map. The marker symbol will be a 
                // solid, red circle.
                SimpleMarkerSymbol userTappedSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 5);

                // Create a new graphic for the spot where the user clicked on the map using the simple marker symbol. 
                Graphic userTappedGraphic = new Graphic(userTappedMapPoint, userTappedSimpleMarkerSymbol);

                // Add the user tapped/clicked map point graphic to the graphic overlay.
                _graphicsOverlay.Graphics.Add(userTappedGraphic);
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                UIAlertController alertController = UIAlertController.Create("Geometry Engine Failed!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void CreateLayout()
        {
            // Create the UILabel for instructions.
            _bufferHelpLabel = new UILabel
            {
                Text = "Buffer (miles):",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Right
            };

            _helpLabel = new UILabel
            {
                Text = "Tap to create a buffer with specified size.",
                AdjustsFontSizeToFitWidth = true
            };

            // Create UITextFiled for the buffer value.
            _bufferDistanceEntry = new UITextField
            {
                Text = "10",
                AdjustsFontSizeToFitWidth = true,
                TextColor = View.TintColor,
                BackgroundColor = UIColor.FromWhiteAlpha(1, .8f),
                BorderStyle = UITextBorderStyle.RoundedRect
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _bufferDistanceEntry.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Add the MapView and other controls to the page.
            View.AddSubviews(_myMapView, _helpToolbar, _helpLabel, _bufferHelpLabel, _bufferDistanceEntry);
        }
    }
}