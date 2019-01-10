// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
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
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UITextField _selectedField;
        private UITextField _utmEntry;
        private UITextField _dmsEntry;
        private UITextField _ddEntry;
        private UITextField _usngEntry;
        private UILabel _utmLabel;
        private UILabel _dmsLabel;
        private UILabel _decimalDegreesLabel;
        private UILabel _usngLabel;
        private NSLayoutConstraint[] _landscapeConstraints;
        private NSLayoutConstraint[] _portraitConstraints;

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

        private void RecalculateFields(object sender)
        {
            // Hold the entered point.
            MapPoint enteredPoint;

            // Update the point based on which text sent the event.
            try
            {
                if (sender == _ddEntry || sender == _dmsEntry)
                {
                    enteredPoint = CoordinateFormatter.FromLatitudeLongitude(_selectedField.Text, _myMapView.SpatialReference);
                }
                else if (sender == _utmEntry)
                {
                    enteredPoint = CoordinateFormatter.FromUtm(_selectedField.Text, _myMapView.SpatialReference, UtmConversionMode.NorthSouthIndicators);
                }
                else
                {
                    enteredPoint = CoordinateFormatter.FromUsng(_selectedField.Text, _myMapView.SpatialReference);
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _decimalDegreesLabel = new UILabel();
            _decimalDegreesLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _decimalDegreesLabel.Text = "Decimal degrees:";

            _dmsLabel = new UILabel();
            _dmsLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _dmsLabel.Text = "Degrees, minutes, seconds:";

            _utmLabel = new UILabel();
            _utmLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _utmLabel.Text = "UTM:";

            _usngLabel = new UILabel();
            _usngLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _usngLabel.Text = "USNG:";

            _ddEntry = new UITextField();
            _dmsEntry = new UITextField();
            _utmEntry = new UITextField();
            _usngEntry = new UITextField();

            // Uniformly configure the entries.
            foreach (UITextField tf in new[] {_ddEntry, _dmsEntry, _utmEntry, _usngEntry})
            {
                tf.TranslatesAutoresizingMaskIntoConstraints = false;
                tf.BorderStyle = UITextBorderStyle.RoundedRect;

                // Allow returning to dismiss the keyboard.
                tf.ShouldReturn += field =>
                {
                    field.ResignFirstResponder();
                    // Recalculate fields.
                    RecalculateFields(field);
                    return true;
                };
            }

            UIStackView formView = new UIStackView(new UIView[]
                {_decimalDegreesLabel, _ddEntry, _dmsLabel, _dmsEntry, _utmLabel, _utmEntry, _usngLabel, _usngEntry});
            formView.TranslatesAutoresizingMaskIntoConstraints = false;
            formView.Axis = UILayoutConstraintAxis.Vertical;
            formView.Spacing = 4;
            formView.LayoutMarginsRelativeArrangement = true;
            formView.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formView.SetContentHuggingPriority((float) UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Vertical);

            View.AddSubviews(formView, _myMapView);

            _portraitConstraints = new[]
            {
                formView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                formView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                formView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                formView.BottomAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            };
            _landscapeConstraints = new[]
            {
                formView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                formView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                formView.TrailingAnchor.ConstraintEqualTo(_myMapView.LeadingAnchor),
                formView.WidthAnchor.ConstraintGreaterThanOrEqualTo(280),
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            };

            ApplyConstraints();
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            // Deactivate old constraints.
            NSLayoutConstraint.DeactivateConstraints(_portraitConstraints);
            NSLayoutConstraint.DeactivateConstraints(_landscapeConstraints);

            // Apply new constraints.
            ApplyConstraints();
        }

        private void ApplyConstraints()
        {
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                // Update layout for landscape.
                NSLayoutConstraint.ActivateConstraints(_landscapeConstraints);
            }
            else
            {
                // Update layout for portrait.
                NSLayoutConstraint.ActivateConstraints(_portraitConstraints);
            }
        }
    }
}