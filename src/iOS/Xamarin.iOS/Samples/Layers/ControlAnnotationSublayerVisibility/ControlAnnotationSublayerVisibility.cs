// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ControlAnnotationSublayerVisibility
{
    [Register("ControlAnnotationSublayerVisibility")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Control annotation sublayer visibility",
        category: "Layers",
        description: "Use annotation sublayers to gain finer control of annotation layer subtypes.",
        instructions: "Start the sample and take note of the visibility of the annotation. Zoom in and out to see the annotation turn on and off based on scale ranges set on the data.",
        tags: new[] { "annotation", "scale", "text", "utilities", "visualization", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("b87307dcfb26411eb2e92e1627cb615b")]
    public class ControlAnnotationSublayerVisibility : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISwitch _openSwitch;
        private UISwitch _closedSwitch;
        private UILabel _openLabel;
        private UILabel _closedLabel;
        private UILabel _scaleLabel;

        // Mobile map package that contains annotation layers.
        private MobileMapPackage _mobileMapPackage;

        // Sub layers of the annotation layer.
        private AnnotationSublayer _openSublayer;
        private AnnotationSublayer _closedSublayer;

        public ControlAnnotationSublayerVisibility()
        {
            Title = "Control annotation sublayer visibility";
        }

        private async void Initialize()
        {
            try
            {
                // Load the mobile map package.
                _mobileMapPackage = new MobileMapPackage(DataManager.GetDataFolder("b87307dcfb26411eb2e92e1627cb615b", "GasDeviceAnno.mmpk"));
                await _mobileMapPackage.LoadAsync();

                // Set the mapview to display the map from the package.
                _myMapView.Map = _mobileMapPackage.Maps.First();

                // Get the annotation layer from the MapView operational layers.
                AnnotationLayer annotationLayer = (AnnotationLayer)_myMapView.Map.OperationalLayers.Where(layer => layer is AnnotationLayer).First();

                // Load the annotation layer.
                await annotationLayer.LoadAsync();

                // Get the annotation sub layers.
                _closedSublayer = (AnnotationSublayer)annotationLayer.SublayerContents[0];
                _openSublayer = (AnnotationSublayer)annotationLayer.SublayerContents[1];

                // Set the label content.
                _openLabel.Text = $"{_openSublayer.Name} (1:{_openSublayer.MaxScale} - 1:{_openSublayer.MinScale})";
                _closedLabel.Text = _closedSublayer.Name;

                // Enable the check boxes.
                _openSwitch.Enabled = true;
                _closedSwitch.Enabled = true;

                // Add event handler for changing the text to indicate whether the "open" sublayer is visible at the current scale.
                _myMapView.ViewpointChanged += (s, e) =>
                {
                    // Check if the sublayer is visible at the current map scale.
                    if (_openSublayer.IsVisibleAtScale(_myMapView.MapScale))
                    {
                        _openLabel.TextColor = UIColor.Black;
                    }
                    else
                    {
                        _openLabel.TextColor = UIColor.Gray;
                    }

                    // Set the current map scale text.
                    _scaleLabel.Text = "Current map scale: 1:" + (int)_myMapView.MapScale;
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void OpenSwitchChanged(object sender, EventArgs e)
        {
            // Set the visibility of the sub layer.
            if (_openSublayer != null) _openSublayer.IsVisible = _openSwitch.On;
        }

        private void ClosedSwitchChanged(object sender, EventArgs e)
        {
            // Set the visibility of the sub layer.
            if (_closedSublayer != null) _closedSublayer.IsVisible = _closedSwitch.On;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _openSwitch = new UISwitch() { Enabled = false };
            _openSwitch.On = true;
            _closedSwitch = new UISwitch() { Enabled = false };
            _closedSwitch.On = true;

            _openLabel = new UILabel();
            _closedLabel = new UILabel();

            UIStackView openRow = new UIStackView(new UIView[] { _openSwitch, _openLabel })
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                LayoutMarginsRelativeArrangement = true,
                LayoutMargins = new UIEdgeInsets(5, 5, 5, 5),
                Spacing = 8,
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.Fill
            };

            UIStackView closedRow = new UIStackView(new UIView[] { _closedSwitch, _closedLabel })
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                LayoutMarginsRelativeArrangement = true,
                LayoutMargins = new UIEdgeInsets(5, 5, 5, 5),
                Spacing = 8,
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.Fill
            };

            _scaleLabel = new UILabel
            {
                Text = "Current map scale: 1:",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, openRow, closedRow, _scaleLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(openRow.TopAnchor),

                openRow.BottomAnchor.ConstraintEqualTo(closedRow.TopAnchor),
                openRow.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                openRow.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                closedRow.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                closedRow.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                closedRow.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _scaleLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _scaleLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _scaleLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _scaleLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Subscribe to events.
            _openSwitch.ValueChanged += OpenSwitchChanged;
            _closedSwitch.ValueChanged += ClosedSwitchChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _openSwitch.ValueChanged -= OpenSwitchChanged;
            _closedSwitch.ValueChanged -= ClosedSwitchChanged;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}