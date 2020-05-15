// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.AnimateImageOverlay
{
    [Register("AnimateImageOverlay")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Animate images with image overlay",
        "MapView",
        "Animate a series of images with an image overlay.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9465e8c02b294c69bdb42de056a23ab1")]
    public class AnimateImageOverlay : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UIBarButtonItem _categoryButton;
        private UIBarButtonItem _fpsButton;
        private UISlider _opacitySlider;

        public AnimateImageOverlay()
        {
            Title = "Animate images with image overlay";
        }

        private void Initialize()
        {
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _mySceneView = new SceneView() { TranslatesAutoresizingMaskIntoConstraints = false };
            var opacityToolbar = new UIToolbar() { TranslatesAutoresizingMaskIntoConstraints = false };
            var buttonToolbar = new UIToolbar() { TranslatesAutoresizingMaskIntoConstraints = false };

            var opacityLabel = new UILabel() { TranslatesAutoresizingMaskIntoConstraints = false };
            opacityLabel.Text = "Opacity";
            _opacitySlider = new UISlider() { TranslatesAutoresizingMaskIntoConstraints = false, MinValue = 0, MaxValue = 1, Value = 1 };
            opacityToolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem(){ CustomView = opacityLabel},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(){ CustomView = _opacitySlider}
            };

            _categoryButton = new UIBarButtonItem() { Title = "Stop" };
            _fpsButton = new UIBarButtonItem() { Title = "FPS: 15" };

            buttonToolbar.Items = new UIBarButtonItem[] {
                _categoryButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _fpsButton
            };

            // Add the views.
            View.AddSubviews(_mySceneView, opacityToolbar, buttonToolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(opacityToolbar.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                opacityToolbar.TopAnchor.ConstraintEqualTo(_mySceneView.BottomAnchor),
                opacityToolbar.BottomAnchor.ConstraintEqualTo(buttonToolbar.TopAnchor),
                opacityToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                opacityToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                buttonToolbar.TopAnchor.ConstraintEqualTo(opacityToolbar.BottomAnchor),
                buttonToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                buttonToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                buttonToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}