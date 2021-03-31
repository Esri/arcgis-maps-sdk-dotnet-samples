// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UtilityNetworks;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ConfigureSubnetworkTrace
{
    [Register("ConfigureSubnetworkTrace")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Configure subnetwork trace",
        category: "Utility network",
        description: "Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.",
        instructions: "The sample loads with a server-defined trace configuration from a tier. Check or uncheck which options to include in the trace - such as containers or barriers. Use the selection boxes to define a new condition network attribute comparison, and then use 'Add' to add the it to the trace configuration. Tap 'Trace' to run a subnetwork trace with this modified configuration from a default starting location.",
        tags: new[] { "category comparison", "condition barriers", "network analysis", "network attribute comparison", "subnetwork trace", "trace configuration", "traversability", "utility network", "validate consistency" })]
    public class ConfigureSubnetworkTrace : UIViewController
    {
        // References to controls and labels in the UI.
        private UISwitch _barrierSwitch;
        private UISwitch _containerSwitch;
        private UILabel _expressionLabel;
        private UIButton _attributeButton;
        private UIButton _comparisonButton;
        private UIButton _valueButton;
        private UITextField _valueTextEntry;
        private UIButton _addButton;
        private UIButton _traceButton;
        private UIButton _resetButton;

        // Feature service for an electric utility network in Naperville, Illinois.
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";
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

        // The source tier of the utility network.
        private UtilityTier _sourceTier;

        // The currently selected values for the barrier expression.
        private UtilityNetworkAttribute _selectedAttribute;
        private UtilityAttributeComparisonOperator _selectedComparison;
        private object _selectedValue;

        public ConfigureSubnetworkTrace()
        {
            Title = "Configure subnetwork trace";
        }

        private async void Initialize()
        {
            // As of ArcGIS Enterprise 10.8.1, using utility network functionality requires a licensed user. The following login for the sample server is licensed to perform utility network operations.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleServer7User = "viewer01";
                    string sampleServer7Pass = "I68VGU^nMurF";
                    return await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            try
            {
                // Disable interaction until the data is loaded.
                View.UserInteractionEnabled = false;

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Create a default starting location.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(DeviceTableName);
                UtilityAssetGroup assetGroup = networkSource.GetAssetGroup(AssetGroupName);
                UtilityAssetType assetType = assetGroup.GetAssetType(AssetTypeName);
                Guid globalId = Guid.Parse(GlobalId);
                _startingLocation = _utilityNetwork.CreateElement(assetType, globalId);

                // Set the terminal for this location. (For our case, we use the 'Load' terminal.)
                _startingLocation.Terminal = _startingLocation.AssetType.TerminalConfiguration?.Terminals.FirstOrDefault(term => term.Name == "Load");

                // Get a default trace configuration from a tier to update the UI.
                UtilityDomainNetwork domainNetwork = _utilityNetwork.Definition.GetDomainNetwork(DomainNetworkName);
                _sourceTier = domainNetwork.GetTier(TierName);

                // Set the trace configuration.
                _configuration = _sourceTier.TraceConfiguration;

                // Set the default expression (if provided).
                if (_sourceTier.TraceConfiguration.Traversability.Barriers is UtilityTraceConditionalExpression expression)
                {
                    _expressionLabel.Text = ExpressionToString(expression);
                    _initialExpression = expression;
                }

                // Set the traversability scope.
                _sourceTier.TraceConfiguration.Traversability.Scope = UtilityTraversabilityScope.Junctions;
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "Error loading network", null).Show();
            }
            finally
            {
                View.UserInteractionEnabled = true;
            }
        }

        private void BarrierChanged(object sender, EventArgs e)
        {
            _sourceTier.TraceConfiguration.IncludeBarriers = _barrierSwitch.On;
        }

        private void ContainerChanged(object sender, EventArgs e)
        {
            _sourceTier.TraceConfiguration.IncludeContainers = _containerSwitch.On;
        }

        private void AttributeClick(object sender, EventArgs e)
        {
            // Create a prompt for selecting the attribute of the barrier expression.
            UIAlertController prompt = UIAlertController.Create(null, "Choose the attribute.", UIAlertControllerStyle.Alert);
            foreach (UtilityNetworkAttribute attribute in _utilityNetwork.Definition.NetworkAttributes.Where(i => i.IsSystemDefined == false))
            {
                UIAlertAction action = UIAlertAction.Create(attribute.Name, UIAlertActionStyle.Default, (s) => AttributeSelected(attribute));
                prompt.AddAction(action);
            }
            prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(prompt, true, null);
        }

        private void AttributeSelected(UtilityNetworkAttribute attribute)
        {
            _attributeButton.SetTitle(attribute.Name, UIControlState.Normal);
            _selectedAttribute = attribute;

            // Reset the value, this prevents invalid values being used to create expressions.
            _valueButton.SetTitle("Value", UIControlState.Normal);
            _selectedValue = null;
        }

        private void ComparisonClick(object sender, EventArgs e)
        {
            // Create a prompt for selecting the comparison operator.
            UIAlertController prompt = UIAlertController.Create(null, "Choose the comparison.", UIAlertControllerStyle.Alert);
            foreach (UtilityAttributeComparisonOperator op in Enum.GetValues(typeof(UtilityAttributeComparisonOperator)))
            {
                UIAlertAction action = UIAlertAction.Create(op.ToString(), UIAlertActionStyle.Default, (s) => ComparisonSelected(op));
                prompt.AddAction(action);
            }
            prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(prompt, true, null);
        }

        private void ComparisonSelected(UtilityAttributeComparisonOperator op)
        {
            _comparisonButton.SetTitle(op.ToString(), UIControlState.Normal);
            _selectedComparison = op;
        }

        private void ValueClick(object sender, EventArgs e)
        {
            // Verify that an attribute has been selected.
            if (_selectedAttribute != null)
            {
                UIAlertController prompt = null;
                if (_selectedAttribute.Domain is CodedValueDomain domain)
                {
                    // Add every coded value as an action on the prompt.
                    prompt = UIAlertController.Create(null, "Choose the value.", UIAlertControllerStyle.Alert);
                    foreach (CodedValue value in domain.CodedValues)
                    {
                        UIAlertAction action = UIAlertAction.Create(value.Name, UIAlertActionStyle.Default, (s) => ValueSelected(value));
                        prompt.AddAction(action);
                    }
                }
                else
                {
                    // Create an alert for the user to enter the value using the keyboard.
                    prompt = UIAlertController.Create(null, "Enter the value", UIAlertControllerStyle.Alert);
                    prompt.AddTextField(obj =>
                    {
                        _valueTextEntry = obj;
                        obj.Text = "";
                        obj.KeyboardType = UIKeyboardType.NumberPad;
                    });
                    prompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, ValueEntered));
                }
                prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                PresentViewController(prompt, true, null);
            }
        }

        private void ValueEntered(UIAlertAction action)
        {
            _valueButton.SetTitle(_valueTextEntry.Text, UIControlState.Normal);
            _selectedValue = _valueTextEntry.Text;
        }

        private void ValueSelected(CodedValue val)
        {
            _valueButton.SetTitle(val.Name, UIControlState.Normal);
            _selectedValue = val;
        }

        private void AddClick(object sender, EventArgs e)
        {
            try
            {
                if (_configuration == null)
                {
                    _configuration = new UtilityTraceConfiguration();
                }
                if (_configuration.Traversability == null)
                {
                    _configuration.Traversability = new UtilityTraversability();
                }

                // NOTE: You may also create a UtilityCategoryComparison with UtilityNetworkDefinition.Categories and UtilityCategoryComparisonOperator.
                if (_selectedAttribute != null)
                {
                    object selectedValue;

                    // If the value is a coded value.
                    if (_selectedAttribute.Domain is CodedValueDomain && _selectedValue is CodedValue codedValue)
                    {
                        selectedValue = ConvertToDataType(codedValue.Code, _selectedAttribute.DataType);
                    }
                    // If the value is free entry.
                    else
                    {
                        selectedValue = ConvertToDataType(_selectedValue.ToString().Trim(), _selectedAttribute.DataType);
                    }
                    // NOTE: You may also create a UtilityNetworkAttributeComparison with another NetworkAttribute.
                    UtilityTraceConditionalExpression expression = new UtilityNetworkAttributeComparison(_selectedAttribute, _selectedComparison, selectedValue);
                    if (_configuration.Traversability.Barriers is UtilityTraceConditionalExpression otherExpression)
                    {
                        // NOTE: You may also combine expressions with UtilityTraceAndCondition
                        expression = new UtilityTraceOrCondition(otherExpression, expression);
                    }
                    _configuration.Traversability.Barriers = expression;
                    _expressionLabel.Text = ExpressionToString(expression);
                }
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "Error adding barrier", null).Show();
            }
        }

        private async void TraceClick(object sender, EventArgs e)
        {
            if (_utilityNetwork == null || _startingLocation == null)
            {
                return;
            }
            try
            {
                // Create utility trace parameters for the starting location.
                UtilityTraceParameters parameters = new UtilityTraceParameters(UtilityTraceType.Subnetwork, new[] { _startingLocation });
                parameters.TraceConfiguration = _configuration;

                // Trace the utility network.
                IEnumerable<UtilityTraceResult> results = await _utilityNetwork.TraceAsync(parameters);

                // Get the first result.
                UtilityElementTraceResult elementResult = results?.FirstOrDefault() as UtilityElementTraceResult;

                // Display the number of elements found by the trace.
                new UIAlertView("Trace Result", $"`{elementResult?.Elements?.Count ?? 0}` elements found.", (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private void ResetClick(object sender, EventArgs e)
        {
            // Reset the barrier condition to the initial value.
            UtilityTraceConfiguration traceConfiguration = _configuration;
            traceConfiguration.Traversability.Barriers = _initialExpression;
            _expressionLabel.Text = ExpressionToString(_initialExpression);
        }

        private object ConvertToDataType(object otherValue, UtilityNetworkAttributeDataType dataType)
        {
            switch (dataType)
            {
                case UtilityNetworkAttributeDataType.Boolean:
                    return Convert.ToBoolean(otherValue);

                case UtilityNetworkAttributeDataType.Double:
                    return Convert.ToDouble(otherValue);

                case UtilityNetworkAttributeDataType.Float:
                    return Convert.ToSingle(otherValue);

                case UtilityNetworkAttributeDataType.Integer:
                    return Convert.ToInt32(otherValue);
            }
            throw new NotSupportedException();
        }

        private string ExpressionToString(UtilityTraceConditionalExpression expression)
        {
            if (expression is UtilityCategoryComparison categoryComparison)
            {
                return $"`{categoryComparison.Category.Name}` {categoryComparison.ComparisonOperator}";
            }
            else if (expression is UtilityNetworkAttributeComparison attributeComparison)
            {
                // Check if attribute domain is a coded value domain.
                if (attributeComparison.NetworkAttribute.Domain is CodedValueDomain domain)
                {
                    // Get the coded value using the the attribute comparison value and attribute data type.
                    UtilityNetworkAttributeDataType dataType = attributeComparison.NetworkAttribute.DataType;
                    object attributeValue = ConvertToDataType(attributeComparison.Value, attributeComparison.NetworkAttribute.DataType);
                    CodedValue codedValue = domain.CodedValues.FirstOrDefault(value => ConvertToDataType(value.Code, dataType).Equals(attributeValue));
                    return $"`{attributeComparison.NetworkAttribute.Name}` {attributeComparison.ComparisonOperator} `{codedValue?.Name}`";
                }
                else
                {
                    return $"`{attributeComparison.NetworkAttribute.Name}` {attributeComparison.ComparisonOperator} `{attributeComparison.OtherNetworkAttribute?.Name ?? attributeComparison.Value}`";
                }
            }
            else if (expression is UtilityTraceAndCondition andCondition)
            {
                return $"({ExpressionToString(andCondition.LeftExpression)}) AND\n ({ExpressionToString(andCondition.RightExpression)})";
            }
            else if (expression is UtilityTraceOrCondition orCondition)
            {
                return $"({ExpressionToString(orCondition.LeftExpression)}) OR\n ({ExpressionToString(orCondition.RightExpression)})";
            }
            else
            {
                return null;
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

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
            buttonContainer.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 10, 0);

            UILabel barrierLabel = new UILabel() { Text = "Include barriers   ", TranslatesAutoresizingMaskIntoConstraints = false };
            _barrierSwitch = new UISwitch() { TranslatesAutoresizingMaskIntoConstraints = false, On = true };
            buttonContainer.AddArrangedSubview(GetRowStackView(new UIView[] { barrierLabel, _barrierSwitch }));

            UILabel containerLabel = new UILabel() { Text = "Include containers", TranslatesAutoresizingMaskIntoConstraints = false };
            _containerSwitch = new UISwitch() { TranslatesAutoresizingMaskIntoConstraints = false, On = true };
            buttonContainer.AddArrangedSubview(GetRowStackView(new UIView[] { containerLabel, _containerSwitch }));

            UILabel helpLabel = new UILabel() { Text = "Example barrier condition for this data: 'Transformer Load' Equal '15'", TranslatesAutoresizingMaskIntoConstraints = false, Lines = 0 };
            buttonContainer.AddArrangedSubview(helpLabel);

            UILabel conditionTitleLabel = new UILabel() { Text = "Barrier Condition:", TranslatesAutoresizingMaskIntoConstraints = false, Lines = 0, MinimumFontSize = (nfloat)(helpLabel.MinimumFontSize * 1.5) };
            buttonContainer.AddArrangedSubview(conditionTitleLabel);

            _attributeButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _attributeButton.SetTitle("Attribute", UIControlState.Normal);
            _attributeButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _comparisonButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _comparisonButton.SetTitle("Comparison", UIControlState.Normal);
            _comparisonButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _valueButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _valueButton.SetTitle("Value", UIControlState.Normal);
            _valueButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            buttonContainer.AddArrangedSubview(GetRowStackView(new UIView[] { _attributeButton, _comparisonButton, _valueButton }));

            _addButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _addButton.SetTitle("Add barrier condition", UIControlState.Normal);
            _addButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            buttonContainer.AddArrangedSubview(GetRowStackView(new UIView[] { _addButton }));

            _expressionLabel = new UILabel() { TranslatesAutoresizingMaskIntoConstraints = false, Text = "", Lines = 0 };
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
            row.Distribution = UIStackViewDistribution.EqualCentering;
            row.WidthAnchor.ConstraintEqualTo(350).Active = true;
            return row;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _attributeButton.TouchUpInside += AttributeClick;
            _comparisonButton.TouchUpInside += ComparisonClick;
            _valueButton.TouchUpInside += ValueClick;
            _addButton.TouchUpInside += AddClick;
            _traceButton.TouchUpInside += TraceClick;
            _resetButton.TouchUpInside += ResetClick;
            _barrierSwitch.ValueChanged += BarrierChanged;
            _containerSwitch.ValueChanged += ContainerChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _attributeButton.TouchUpInside -= AttributeClick;
            _comparisonButton.TouchUpInside -= ComparisonClick;
            _valueButton.TouchUpInside -= ValueClick;
            _addButton.TouchUpInside -= AddClick;
            _traceButton.TouchUpInside -= TraceClick;
            _resetButton.TouchUpInside -= ResetClick;
            _barrierSwitch.ValueChanged -= BarrierChanged;
            _containerSwitch.ValueChanged -= ContainerChanged;
        }
    }
}