// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.MapRotation
{
    [Register("MapRotation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Map rotation",
        "MapView",
        "Rotate a map.",
        "Use the slider to rotate the map.",
        "rotate", "rotation", "viewpoint")]
    public class MapRotation : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UILabel _rotationLabel;
        private UISlider _rotationSlider;

        public MapRotation()
        {
            Title = "Map rotation";
        }

        private void Initialize()
        {
            // Show a streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreets());
        }

        private void RotationSlider_Changed(object sender, EventArgs e)
        {
            _myMapView.SetViewpointRotationAsync(_rotationSlider.Value);
            _rotationLabel.Text = $"{_rotationSlider.Value:0}°";
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

            _rotationSlider = new UISlider
            {
                MinValue = 0,
                MaxValue = 360,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _rotationLabel = new UILabel
            {
                Text = "0°",
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(_rotationLabel),
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) {Width = 0},
                new UIBarButtonItem(_rotationSlider)
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _rotationLabel.WidthAnchor.ConstraintEqualTo(64),

                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _rotationSlider.ValueChanged += RotationSlider_Changed;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _rotationSlider.ValueChanged -= RotationSlider_Changed;
        }
    }
}