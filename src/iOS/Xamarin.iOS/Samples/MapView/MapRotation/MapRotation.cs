// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

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
        "This sample illustrates how to rotate a map.",
        "")]
    public class MapRotation : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;
        private UILabel _rotationLabel;
        private UISlider _rotationSlider;

        public MapRotation()
        {
            Title = "Map rotation";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Show a streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Configure the slider.
            _rotationSlider.ValueChanged += (s, e) =>
            {
                _myMapView.SetViewpointRotationAsync(_rotationSlider.Value);
                _rotationLabel.Text = $"{_rotationSlider.Value:0}°";
            };
        }

        public override void LoadView()
        {
            View = new UIView();
            View.BackgroundColor = UIColor.White;

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(_myMapView);

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

            _toolbar = new UIToolbar
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Items = new[] {
                    new UIBarButtonItem(_rotationLabel),
                    new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) {Width = 0},
                    new UIBarButtonItem(_rotationSlider)
                }
            };
            View.AddSubview(_toolbar);

            _rotationLabel.WidthAnchor.ConstraintEqualTo(64).Active = true;

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor).Active = true;

            _toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
        }
    }
}