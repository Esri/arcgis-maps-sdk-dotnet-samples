// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace ArcGISRuntime.Samples.FormatCoordinates
{
    [Register("FormatCoordinates")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Format coordinates",
        "Geometry",
        "This sample demonstrates how to convert between `MapPoint` and string representations of a point using various coordinate systems.",
        "Tap on the map to see the point in several coordinate systems. Update one of the coordinates and select 'recalculate' to see the point converted into other coordinate systems. ")]
    public class FormatCoordinates : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create the text fields
        private UITextField _utmUITextField = new UITextField() { Placeholder = "UTM" };
        private UITextField _dmsUITextField = new UITextField() { Placeholder = "Degrees, Minutes, Seconds" };
        private UITextField _decimalDegreesUITextField = new UITextField() { Placeholder = "Decimal Degrees" };
        private UITextField _usngUITextField = new UITextField() { Placeholder = "USNG" };

        // Create the labels
        private UILabel _utmLabel = new UILabel() { Text = "UTM:", TextColor = UIColor.Blue };
        private UILabel _dmsLabel = new UILabel() { Text = "Degrees, Minutes, Seconds: ", TextColor = UIColor.Blue };
        private UILabel _decimalDegreeslabel = new UILabel() { Text = "Decimal Degrees: ", TextColor = UIColor.Blue };
        private UILabel _usngLabel = new UILabel() { Text = "USNG: ", TextColor = UIColor.Blue };

        // Create the recalculate button
        private UIButton _recalculateButton = new UIButton();

        // Track the most recently edited field
        private UITextField _selectedField;

        public FormatCoordinates()
        {
            Title = "Format coordinates";
        }

        private void Initialize()
        {
            // Update the initial field selection
            _selectedField = _decimalDegreesUITextField;

            // Create the map
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Add the graphics overlay to the map
            _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Create the starting point
            MapPoint startingPoint = new MapPoint(0, 0, SpatialReferences.WebMercator);

            // Update the UI with the initial point
            UpdateUIFromMapPoint(startingPoint);

            // Subscribe to map tap events to enable tapping on map to update coordinates
            _myMapView.GeoViewTapped += (sender, args) => { UpdateUIFromMapPoint(args.Location); };
        }

        private void InputValueChanged(object sender, EventArgs e)
        {
            // Keep track of the last edited field
            _selectedField = (UITextField)sender;
        }

        private void RecalculateFields(object sender, EventArgs e)
        {
            // Hold the entered point
            MapPoint enteredPoint = null;

            // Update the point based on which text sent the event
            try
            {
                switch (_selectedField.Placeholder)
                {
                    case "Decimal Degrees":
                    case "Degrees, Minutes, Seconds":
                        enteredPoint =
                            CoordinateFormatter.FromLatitudeLongitude(_selectedField.Text, _myMapView.SpatialReference);
                        break;

                    case "UTM":
                        enteredPoint =
                            CoordinateFormatter.FromUtm(_selectedField.Text, _myMapView.SpatialReference, UtmConversionMode.NorthSouthIndicators);
                        break;

                    case "USNG":
                        enteredPoint =
                            CoordinateFormatter.FromUsng(_selectedField.Text, _myMapView.SpatialReference);
                        break;
                }
            }
            catch (Exception ex)
            {
                // The coordinate is malformed, warn and return
                UIAlertController alertController = UIAlertController.Create("Invalid Format", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
                return;
            }

            // Update the UI from the MapPoint
            UpdateUIFromMapPoint(enteredPoint);
        }

        private void UpdateUIFromMapPoint(MapPoint startingPoint)
        {
            if (startingPoint == null) { return; }
            // Clear event subscriptions - prevents an infinite loop
            _utmUITextField.EditingDidBegin -= InputValueChanged;
            _dmsUITextField.EditingDidBegin -= InputValueChanged;
            _decimalDegreesUITextField.EditingDidBegin -= InputValueChanged;
            _usngUITextField.EditingDidBegin -= InputValueChanged;

            // Update the decimal degrees text
            _decimalDegreesUITextField.Text =
                CoordinateFormatter.ToLatitudeLongitude(startingPoint, LatitudeLongitudeFormat.DecimalDegrees, 4);

            // Update the degrees, minutes, seconds text
            _dmsUITextField.Text = CoordinateFormatter.ToLatitudeLongitude(startingPoint,
                LatitudeLongitudeFormat.DegreesMinutesSeconds, 1);

            // Update the UTM text
            _utmUITextField.Text = CoordinateFormatter.ToUtm(startingPoint, UtmConversionMode.NorthSouthIndicators, true);

            // Update the USNG text
            _usngUITextField.Text = CoordinateFormatter.ToUsng(startingPoint, 4, true);

            // Clear existing graphics overlays
            _myMapView.GraphicsOverlays[0].Graphics.Clear();

            // Create a symbol to symbolize the point
            SimpleMarkerSymbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Color.Yellow, 20);

            // Create the graphic
            Graphic symbolGraphic = new Graphic(startingPoint, symbol);

            // Add the graphic to the graphics overlay
            _myMapView.GraphicsOverlays[0].Graphics.Add(symbolGraphic);

            // Restore event subscriptions
            _utmUITextField.EditingDidBegin += InputValueChanged;
            _dmsUITextField.EditingDidBegin += InputValueChanged;
            _decimalDegreesUITextField.EditingDidBegin += InputValueChanged;
            _usngUITextField.EditingDidBegin += InputValueChanged;
        }

        private void CreateLayout()
        {
            View.BackgroundColor = UIColor.White;

            // Create the UI button
            _recalculateButton.SetTitle("Recalculate", UIControlState.Normal);
            _recalculateButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _recalculateButton.TouchUpInside += RecalculateFields;

            // Add views to the page
            View.AddSubviews(_myMapView, _recalculateButton, _decimalDegreesUITextField, _decimalDegreeslabel, _dmsLabel, _dmsUITextField,
                _usngLabel, _usngUITextField, _utmLabel, _utmUITextField);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            var topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height + 10;
            var controlHeight = 20;
            var controlWidth = View.Bounds.Width - 20;

            // Decimal degrees
            _decimalDegreeslabel.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            _decimalDegreesUITextField.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            // DMS
            _dmsLabel.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            _dmsUITextField.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            // UTM
            _utmLabel.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            _utmUITextField.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            // USNG
            _usngLabel.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            _usngUITextField.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            // Button
            _recalculateButton.Frame = new CGRect(10, topMargin, controlWidth, controlHeight);
            topMargin += controlHeight;
            // MapView
            _myMapView.Frame = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height - topMargin);

            base.ViewDidLayoutSubviews();
        }
    }
}