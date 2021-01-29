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
        name: "Format coordinates",
        category: "Geometry",
        description: "Format coordinates in a variety of common notations.",
        instructions: "Tap on the map to see a callout with the clicked location's coordinate formatted in 4 different ways. You can also put a coordinate string in any of these formats in the text field. Hit Enter and the coordinate string will be parsed to a map location which the callout will move to.",
        tags: new[] { "USNG", "UTM", "convert", "coordinate", "decimal degrees", "degree minutes seconds", "format", "latitude", "longitude" })]
    public class FormatCoordinates : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITextField _selectedField;
        private UITextField _utmEntry;
        private UITextField _dmsEntry;
        private UITextField _ddEntry;
        private UITextField _usngEntry;
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
            _myMapView.Map = new Map(BasemapStyle.ArcGISStreets);

            // Add the graphics overlay to the map.
            _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Create the starting point.
            MapPoint startingPoint = new MapPoint(0, 0, SpatialReferences.WebMercator);

            // Update the UI with the initial point.
            UpdateUiFromMapPoint(startingPoint);
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e) => UpdateUiFromMapPoint(e.Location);

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
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel decimalDegreesLabel = new UILabel();
            decimalDegreesLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            decimalDegreesLabel.Text = "Decimal degrees:";

            UILabel dmsLabel = new UILabel();
            dmsLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            dmsLabel.Text = "Degrees, minutes, seconds:";

            UILabel utmLabel = new UILabel();
            utmLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            utmLabel.Text = "UTM:";

            UILabel usngLabel = new UILabel();
            usngLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            usngLabel.Text = "USNG:";

            _ddEntry = new UITextField();
            _dmsEntry = new UITextField();
            _utmEntry = new UITextField();
            _usngEntry = new UITextField();

            // Uniformly configure the entries.
            foreach (UITextField tf in new[] {_ddEntry, _dmsEntry, _utmEntry, _usngEntry})
            {
                tf.TranslatesAutoresizingMaskIntoConstraints = false;
                tf.BorderStyle = UITextBorderStyle.RoundedRect;
            }

            UIStackView formView = new UIStackView(new UIView[]
                {decimalDegreesLabel, _ddEntry, dmsLabel, _dmsEntry, utmLabel, _utmEntry, usngLabel, _usngEntry});
            formView.TranslatesAutoresizingMaskIntoConstraints = false;
            formView.Axis = UILayoutConstraintAxis.Vertical;
            formView.Spacing = 4;
            formView.LayoutMarginsRelativeArrangement = true;
            formView.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formView.SetContentHuggingPriority((float) UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Vertical);

            // Add the views.
            View.AddSubviews(formView, _myMapView);

            // Lay out the views.
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

        private bool HandleTextField(UITextField textField)
        {
            textField.ResignFirstResponder();
            // Recalculate fields.
            RecalculateFields(textField);
            return true;
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            _utmEntry.EditingDidBegin += InputValueChanged;
            _dmsEntry.EditingDidBegin += InputValueChanged;
            _ddEntry.EditingDidBegin += InputValueChanged;
            _usngEntry.EditingDidBegin += InputValueChanged;
            foreach (UITextField tf in new[] {_ddEntry, _dmsEntry, _utmEntry, _usngEntry})
            {
                tf.ShouldReturn += HandleTextField;
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
            _utmEntry.EditingDidBegin -= InputValueChanged;
            _dmsEntry.EditingDidBegin -= InputValueChanged;
            _ddEntry.EditingDidBegin -= InputValueChanged;
            _usngEntry.EditingDidBegin -= InputValueChanged;
            foreach (UITextField tf in new[] {_ddEntry, _dmsEntry, _utmEntry, _usngEntry})
            {
                tf.ShouldReturn -= HandleTextField;
            }
        }
    }
}