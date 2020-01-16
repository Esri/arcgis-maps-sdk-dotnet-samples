// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ConfigureSubnetworkTrace
{
    [Register("ConfigureSubnetworkTrace")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Configure subnetwork trace",
        "Network analysis",
        "Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class ConfigureSubnetworkTrace : UIViewController
    {
        private UISwitch _barrierSwitch;
        private UISwitch _containerSwitch;
        private UILabel _expressionLabel;
        private UIPickerView _attributePicker;
        private UIPickerView _comparisonPicker;
        private UIPickerView _codedValuePicker;
        private UITextField _valueTextEntry;
        private UIButton _addButton;
        private UIButton _traceButton;
        private UIButton _resetButton;

        public ConfigureSubnetworkTrace()
        {
            Title = "Configure subnetwork trace";
        }

        private void Initialize()
        {
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIScrollView() { BackgroundColor = UIColor.White, ScrollEnabled = true, DirectionalLayoutMargins = new NSDirectionalEdgeInsets(5,5,5,5) };

            UILabel barrierLabel = new UILabel() { Text = "Include barriers" };
            _barrierSwitch = new UISwitch();
            UILabel containerLabel = new UILabel() { Text = "Include containers" };
            _containerSwitch = new UISwitch() ;

            UILabel helpLabel = new UILabel() { Text = "Example barrier condition for this data: 'Transformer Load' Equal '15'" };

            _attributePicker = new UIPickerView();
            _comparisonPicker = new UIPickerView();
            _codedValuePicker = new UIPickerView();
            _valueTextEntry = new UITextField() { Enabled = false, KeyboardType = UIKeyboardType.NumbersAndPunctuation };

            _addButton = new UIButton();
            _addButton.SetTitle("Add barrier condition", UIControlState.Normal);

            _expressionLabel = new UILabel();

            _traceButton = new UIButton();
            _traceButton.SetTitle("Trace", UIControlState.Normal);
            _resetButton = new UIButton();
            _resetButton.SetTitle("Reset", UIControlState.Normal);

            View.AddSubviews(barrierLabel, _barrierSwitch, containerLabel, _containerSwitch, helpLabel, _attributePicker, _comparisonPicker, _codedValuePicker, _valueTextEntry, _expressionLabel, _traceButton, _resetButton);

            //Set the autoresizing constraints to false;
            foreach (UIView sub in View.Subviews) { sub.TranslatesAutoresizingMaskIntoConstraints = false; }

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                barrierLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                barrierLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                barrierLabel.TrailingAnchor.ConstraintEqualTo(_barrierSwitch.LeadingAnchor),
                barrierLabel.BottomAnchor.ConstraintEqualTo(containerLabel.TopAnchor),

                _barrierSwitch.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _barrierSwitch.LeadingAnchor.ConstraintEqualTo(barrierLabel.TrailingAnchor),
                _barrierSwitch.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                _barrierSwitch.BottomAnchor.ConstraintEqualTo(_containerSwitch.TopAnchor),

                containerLabel.TopAnchor.ConstraintEqualTo(barrierLabel.BottomAnchor),
                containerLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                containerLabel.TrailingAnchor.ConstraintEqualTo(_containerSwitch.LeadingAnchor),
                containerLabel.BottomAnchor.ConstraintEqualTo(helpLabel.TopAnchor),

                _containerSwitch.TopAnchor.ConstraintEqualTo(_barrierSwitch.BottomAnchor),
                _containerSwitch.LeadingAnchor.ConstraintEqualTo(containerLabel.TrailingAnchor),
                _containerSwitch.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                _containerSwitch.BottomAnchor.ConstraintEqualTo(helpLabel.TopAnchor),

                helpLabel.TopAnchor.ConstraintEqualTo(containerLabel.BottomAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                //helpLabel.BottomAnchor.ConstraintEqualTo(_containerSwitch.TopAnchor),

            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}