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
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIButton _recalculateButton = new UIButton();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UILabel _helpLabel = new UILabel();
        private UITextField _selectedField;

        private readonly UITextField _utmEntry = new UITextField
        {
            Placeholder = "UTM"
        };

        private readonly UITextField _dmsEntry = new UITextField
        {
            Placeholder = "Degrees, Minutes, Seconds"
        };

        private readonly UITextField _ddEntry = new UITextField
        {
            Placeholder = "Decimal Degrees"
        };

        private readonly UITextField _usngEntry = new UITextField
        {
            Placeholder = "USNG"
        };

        private readonly UILabel _utmLabel = new UILabel
        {
            Text = "UTM:"
        };

        private readonly UILabel _dmsLabel = new UILabel
        {
            Text = "Degrees, Minutes, Seconds: "
        };

        private readonly UILabel _decimalDegreeslabel = new UILabel
        {
            Text = "Decimal Degrees: "
        };

        private readonly UILabel _usngLabel = new UILabel
        {
            Text = "USNG: "
        };

        public FormatCoordinates()
        {
            Title = "Format coordinates";
        }

        private void Initialize()
        {
            // Update the initial field selection.
            _selectedField = _ddEntry;

            // Create the map.
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Add the graphics overlay to the map.
            _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Create the starting point.
            MapPoint startingPoint = new MapPoint(0, 0, SpatialReferences.WebMercator);

            // Update the UI with the initial point.
            UpdateUiFromMapPoint(startingPoint);

            // Subscribe to map tap events to enable tapping on map to update coordinates.
            _myMapView.GeoViewTapped += (sender, args) => UpdateUiFromMapPoint(args.Location);
        }

        private void InputValueChanged(object sender, EventArgs e)
        {
            // Keep track of the last edited field.
            _selectedField = (UITextField) sender;
        }

        private void RecalculateFields(object sender, EventArgs e)
        {
            // Hold the entered point.
            MapPoint enteredPoint = null;

            // Update the point based on which text sent the event.
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
                // The coordinate is malformed, warn and return.
                UIAlertController alertController = UIAlertController.Create("Invalid Format", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
                return;
            }

            // Update the UI from the MapPoint.
            UpdateUiFromMapPoint(enteredPoint);
        }

        private void UpdateUiFromMapPoint(MapPoint selectedPoint)
        {
            if (selectedPoint == null)
            {
                return;
            }

            // Clear event subscriptions - prevents an infinite loop.
            _utmEntry.EditingDidBegin -= InputValueChanged;
            _dmsEntry.EditingDidBegin -= InputValueChanged;
            _ddEntry.EditingDidBegin -= InputValueChanged;
            _usngEntry.EditingDidBegin -= InputValueChanged;

            try
            {
                // Update the decimal degrees text.
                _ddEntry.Text =
                    CoordinateFormatter.ToLatitudeLongitude(selectedPoint, LatitudeLongitudeFormat.DecimalDegrees, 4);

                // Update the degrees, minutes, seconds text.
                _dmsEntry.Text = CoordinateFormatter.ToLatitudeLongitude(selectedPoint,
                    LatitudeLongitudeFormat.DegreesMinutesSeconds, 1);

                // Update the UTM text.
                _utmEntry.Text = CoordinateFormatter.ToUtm(selectedPoint, UtmConversionMode.NorthSouthIndicators, true);

                // Update the USNG text.
                _usngEntry.Text = CoordinateFormatter.ToUsng(selectedPoint, 4, true);

                // Clear existing graphics overlays.
                _myMapView.GraphicsOverlays[0].Graphics.Clear();

                // Create a symbol to symbolize the point.
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Color.Yellow, 20);

                // Create the graphic.
                Graphic symbolGraphic = new Graphic(selectedPoint, symbol);

                // Add the graphic to the graphics overlay.
                _myMapView.GraphicsOverlays[0].Graphics.Add(symbolGraphic);
            }
            catch (Exception ex)
            {
                // The coordinate is malformed, warn and return.
                UIAlertController alertController = UIAlertController.Create("There was a problem formatting coordinates.", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }

            // Restore event subscriptions.
            _utmEntry.EditingDidBegin += InputValueChanged;
            _dmsEntry.EditingDidBegin += InputValueChanged;
            _ddEntry.EditingDidBegin += InputValueChanged;
            _usngEntry.EditingDidBegin += InputValueChanged;
        }

        private void CreateLayout()
        {
            // Update the colors.
            _dmsEntry.TextColor = View.TintColor;
            _utmEntry.TextColor = View.TintColor;
            _usngEntry.TextColor = View.TintColor;
            _ddEntry.TextColor = View.TintColor;
            _ddEntry.BorderStyle = UITextBorderStyle.RoundedRect;
            _utmEntry.BorderStyle = UITextBorderStyle.RoundedRect;
            _usngEntry.BorderStyle = UITextBorderStyle.RoundedRect;
            _dmsEntry.BorderStyle = UITextBorderStyle.RoundedRect;

            // Enable text fields to close keyboard.
            _dmsEntry.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            _ddEntry.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            _utmEntry.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            _usngEntry.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Set up the help label.
            _helpLabel.Text = "Tap the map to see coordinates in each format. Update any value and tap 'Recalculate' to see updated coordinates.";
            _helpLabel.Lines = 2;
            _helpLabel.AdjustsFontSizeToFitWidth = true;

            // Create the UI button.
            _recalculateButton.SetTitle("Recalculate", UIControlState.Normal);
            _recalculateButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _recalculateButton.TouchUpInside += RecalculateFields;

            // Add views to the page.
            View.AddSubviews(_myMapView, _toolbar, _helpLabel, _recalculateButton, _ddEntry,
                _decimalDegreeslabel, _dmsLabel, _dmsEntry, _usngLabel, _usngEntry, _utmLabel, _utmEntry);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                var topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                int controlHeight = 20;
                int margin = 5;
                var controlWidth = View.Bounds.Width - 2 * margin;

                // Reposition the controls.
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, controlHeight * 11 + margin * 5);

                // Help label.
                topMargin += margin;
                _helpLabel.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight * 2);
                topMargin += controlHeight * 2 + margin * 2;

                // Decimal degrees.
                _decimalDegreeslabel.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight;
                _ddEntry.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight;

                // DMS.
                _dmsLabel.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight;
                _dmsEntry.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight;

                // UTM.
                _utmLabel.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight;
                _utmEntry.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight;

                // USNG.
                _usngLabel.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight;
                _usngEntry.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight + margin;

                // Button.
                _recalculateButton.Frame = new CGRect(margin, topMargin, controlWidth, controlHeight);
                topMargin += controlHeight + margin;

                // MapView.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}