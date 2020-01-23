// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.UtilityNetworks;
using Foundation;
using System;
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

        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";
        private UtilityNetwork _utilityNetwork;

        // For creating the default starting location.
        private const string DeviceTableName = "Electric Distribution Device";
        private const string AssetGroupName = "Circuit Breaker";
        private const string AssetTypeName = "Three Phase";
        private const string GlobalId = "{1CAF7740-0BF4-4113-8DB2-654E18800028}";

        // For creating the default trace configuration.
        private const string DomainNetworkName = "ElectricDistribution";
        private const string TierName = "Medium Voltage Radial";

        // Utility element to start the trace from.
        private UtilityElement _startingLocation;

        // Holding the initial conditional expression.
        private UtilityTraceConditionalExpression _initialExpression;

        // The trace configuration.
        private UtilityTraceConfiguration _configuration;

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
            View = new UIView() { BackgroundColor = UIColor.White };

            UIScrollView scrollView = new UIScrollView();
            scrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(scrollView);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                scrollView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                scrollView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
            });

            UIStackView buttonContainer = new UIStackView();
            buttonContainer.Axis = UILayoutConstraintAxis.Vertical;
            buttonContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonContainer.Distribution = UIStackViewDistribution.Fill;
            buttonContainer.Alignment = UIStackViewAlignment.Top;
            buttonContainer.Spacing = 5;
            buttonContainer.LayoutMarginsRelativeArrangement = true;
            buttonContainer.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 10, 10);

            UILabel barrierLabel = new UILabel() { Text = "Include barriers", TranslatesAutoresizingMaskIntoConstraints = false };
            _barrierSwitch = new UISwitch() { TranslatesAutoresizingMaskIntoConstraints = false };
            buttonContainer.AddArrangedSubview(GetRowStackView(new UIView[] { barrierLabel, _barrierSwitch }));

            UILabel containerLabel = new UILabel() { Text = "Include containers", TranslatesAutoresizingMaskIntoConstraints = false };
            _containerSwitch = new UISwitch() { TranslatesAutoresizingMaskIntoConstraints = false };
            buttonContainer.AddArrangedSubview(GetRowStackView(new UIView[] { containerLabel, _containerSwitch }));

            UILabel helpLabel = new UILabel() { Text = "Example barrier condition for this data: 'Transformer Load' Equal '15'", TranslatesAutoresizingMaskIntoConstraints = false, Lines = 0 };
            buttonContainer.AddArrangedSubview(helpLabel);

            UILabel conditionTitleLabel = new UILabel() { Text = "Barrier Condition:", TranslatesAutoresizingMaskIntoConstraints = false, Lines = 0, MinimumFontSize = (nfloat)(helpLabel.MinimumFontSize * 1.5) };
            buttonContainer.AddArrangedSubview(conditionTitleLabel);

            _attributePicker = new UIPickerView() { TranslatesAutoresizingMaskIntoConstraints = false };
            buttonContainer.AddArrangedSubview(_attributePicker);
            _comparisonPicker = new UIPickerView() { TranslatesAutoresizingMaskIntoConstraints = false };
            buttonContainer.AddArrangedSubview(_comparisonPicker);
            _codedValuePicker = new UIPickerView() { TranslatesAutoresizingMaskIntoConstraints = false };
            buttonContainer.AddArrangedSubview(_codedValuePicker);
            _valueTextEntry = new UITextField() { Enabled = false, KeyboardType = UIKeyboardType.NumbersAndPunctuation, TranslatesAutoresizingMaskIntoConstraints = false };
            buttonContainer.AddArrangedSubview(_valueTextEntry);

            _addButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _addButton.SetTitle("Add barrier condition", UIControlState.Normal);
            _addButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            buttonContainer.AddArrangedSubview(_addButton);

            _expressionLabel = new UILabel() { TranslatesAutoresizingMaskIntoConstraints = false, Text = "Test" };
            buttonContainer.AddArrangedSubview(_expressionLabel);

            _traceButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _traceButton.SetTitle("Trace", UIControlState.Normal);
            _traceButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            _resetButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _resetButton.SetTitle("Reset", UIControlState.Normal);
            _resetButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            buttonContainer.AddArrangedSubview(GetRowStackView(new UIView[] { _traceButton, _resetButton }));

            scrollView.AddSubview(buttonContainer);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                buttonContainer.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                buttonContainer.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor),
                buttonContainer.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor),
                buttonContainer.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor),
                buttonContainer.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor),
            });
        }
        private UIStackView GetRowStackView(UIView[] views)
        {
            UIStackView row = new UIStackView(views);
            row.TranslatesAutoresizingMaskIntoConstraints = false;
            row.Spacing = 8;
            row.Axis = UILayoutConstraintAxis.Horizontal;
            row.Distribution = UIStackViewDistribution.FillProportionally;
            return row;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}