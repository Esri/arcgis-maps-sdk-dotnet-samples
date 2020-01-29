// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
using Android.OS;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.ConfigureSubnetworkTrace
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Configure subnetwork trace",
        "Network analysis",
        "Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class ConfigureSubnetworkTrace : Activity
    {
        // Hold references to the UI controls.
        private Switch _barrierSwitch;
        private Switch _containerSwitch;
        private Button _attributeButton;
        private Button _comparisonButton;
        private Button _valueButton;
        private Button _addButton;
        private Button _traceButton;
        private Button _resetButton;
        private TextView _expressionLabel;
        private EditText _valueEntry;
        private View _mainView;

        // Feature service for an electric utility network in Naperville, Illinois.
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

        // The source tier of the utility network.
        private UtilityTier _sourceTier;

        // The currently selected values for the barrier expression.
        private UtilityNetworkAttribute _selectedAttribute;
        private UtilityAttributeComparisonOperator _selectedComparison;
        private object _selectedValue;

        // Attributes in the network
        private IEnumerable<UtilityNetworkAttribute> _attributes;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Configure subnetwork trace";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Disable interaction until the data is loaded.
                _mainView.Visibility = ViewStates.Gone;

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Getthe attributes in the utility network.
                _attributes = _utilityNetwork.Definition.NetworkAttributes.Where((i => i.IsSystemDefined == false));

                // Create a default starting location.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(DeviceTableName);
                UtilityAssetGroup assetGroup = networkSource.GetAssetGroup(AssetGroupName);
                UtilityAssetType assetType = assetGroup.GetAssetType(AssetTypeName);
                Guid globalId = Guid.Parse(GlobalId);
                _startingLocation = _utilityNetwork.CreateElement(assetType, globalId);

                // Set the terminal for this location. (For our case, we use the 'Load' terminal.)
                _startingLocation.Terminal = _startingLocation.AssetType.TerminalConfiguration?.Terminals.FirstOrDefault(t => t.Name == "Load");

                // Get a default trace configuration from a tier to update the UI.
                UtilityDomainNetwork domainNetwork = _utilityNetwork.Definition.GetDomainNetwork(DomainNetworkName);
                _sourceTier = domainNetwork.GetTier(TierName);

                // Set the trace configuration.
                _configuration = _sourceTier.TraceConfiguration;

                // Set the default expression (if provided).
                if (_sourceTier.TraceConfiguration.Traversability.Barriers is UtilityTraceConditionalExpression expression)
                {
                    _initialExpression = expression;
                    _expressionLabel.Text = ExpressionToString(_initialExpression);
                }

                // Set the traversability scope.
                _sourceTier.TraceConfiguration.Traversability.Scope = UtilityTraversabilityScope.Junctions;
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
            finally
            {
                _mainView.Visibility = ViewStates.Visible;
            }
        }

        private void AttributeClicked(object sender, EventArgs e)
        {
            // Get the names of every attribute.
            string[] options = _attributes.Select(x => x.Name).ToArray();

            // Create UI for attribute selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Select an attribute");
            builder.SetItems(options, AttributeSelected);
            builder.Show();
        }

        private void AttributeSelected(object sender, DialogClickEventArgs e)
        {
            _selectedAttribute = _attributes.ElementAt(e.Which);
            _attributeButton.Text = _selectedAttribute.Name;

            // Reset the value, this prevents invalid values being used to create expressions.
            _valueButton.Text = "Value";
            _selectedValue = null;
        }

        private void ComparisonClicked(object sender, EventArgs e)
        {
            // Get the names of every comparison operator.
            string[] options = Enum.GetNames(typeof(UtilityAttributeComparisonOperator));

            // Create UI for attribute selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Select comparison");
            builder.SetItems(options, ComparisonSelected);
            builder.Show();
        }

        private void ComparisonSelected(object sender, DialogClickEventArgs e)
        {
            _selectedComparison = (UtilityAttributeComparisonOperator)Enum.GetValues(typeof(UtilityAttributeComparisonOperator)).GetValue(e.Which);
            _comparisonButton.Text = _selectedComparison.ToString();
        }

        private void ValueClicked(object sender, EventArgs e)
        {
            // Verify that an attribute has been selected.
            if (_selectedAttribute != null)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                if (_selectedAttribute.Domain is CodedValueDomain domain)
                {
                    // Create a dialog for selecting from the coded values.
                    builder.SetTitle("Select a value");
                    string[] options = domain.CodedValues.Select(x => x.Name).ToArray();
                    builder.SetItems(options, ValueSelected);
                }
                else
                {
                    // Create a dialog for entering a value.
                    _valueEntry = new EditText(this) { InputType = Android.Text.InputTypes.ClassNumber };
                    builder.SetTitle("Enter a value");
                    builder.SetView(_valueEntry);
                    builder.SetPositiveButton("OK", ValueEntered);
                }
                builder.Show();
            }
        }

        private void ValueSelected(object sender, DialogClickEventArgs e)
        {
            _selectedValue = ((CodedValueDomain)_selectedAttribute.Domain).CodedValues[e.Which];
            _valueButton.Text = ((CodedValue)_selectedValue).Name;
        }

        private void ValueEntered(object sender, DialogClickEventArgs e)
        {
            _selectedValue = _valueEntry.Text;
            _valueButton.Text = _selectedValue.ToString();
        }

        private void AddClicked(object sender, EventArgs e)
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
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
        }

        private async void TraceClicked(object sender, EventArgs e)
        {
            if (_utilityNetwork == null || _startingLocation == null)
            {
                return;
            }
            try
            {
                UtilityTraceParameters parameters = new UtilityTraceParameters(UtilityTraceType.Subnetwork, new[] { _startingLocation });
                if (_configuration is UtilityTraceConfiguration traceConfiguration)
                {
                    parameters.TraceConfiguration = traceConfiguration;
                }
                IEnumerable<UtilityTraceResult> results = await _utilityNetwork.TraceAsync(parameters);
                UtilityElementTraceResult elementResult = results?.FirstOrDefault() as UtilityElementTraceResult;

                // Display the number of elements found by the trace.
                new AlertDialog.Builder(this).SetMessage($"`{elementResult?.Elements?.Count ?? 0}` elements found.").SetTitle("Trace Result").Show();
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
        }

        private void ResetClicked(object sender, EventArgs e)
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
                    CodedValue codedValue = domain.CodedValues.FirstOrDefault(cv => ConvertToDataType(cv.Code, dataType).Equals(attributeValue));
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

        private void CreateLayout()
        {
            // Load the layout from the axml resource. (This sample has the same interface as the navigation sample without rerouting)
            SetContentView(Resource.Layout.ConfigureSubnetworkTrace);

            _barrierSwitch = FindViewById<Switch>(Resource.Id.barrierSwitch);
            _containerSwitch = FindViewById<Switch>(Resource.Id.containerSwitch);
            _attributeButton = FindViewById<Button>(Resource.Id.attributeButton);
            _comparisonButton = FindViewById<Button>(Resource.Id.comparisonButton);
            _valueButton = FindViewById<Button>(Resource.Id.valueButton);
            _addButton = FindViewById<Button>(Resource.Id.addButton);
            _traceButton = FindViewById<Button>(Resource.Id.traceButton);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);
            _expressionLabel = FindViewById<TextView>(Resource.Id.barrierText);
            _mainView = FindViewById<View>(Resource.Id.ConfigureSubnetworkTraceView);

            // Add event handlers for all of the controls.
            _barrierSwitch.CheckedChange += (s, e) => _sourceTier.TraceConfiguration.IncludeBarriers = e.IsChecked;
            _containerSwitch.CheckedChange += (s, e) => _sourceTier.TraceConfiguration.IncludeContainers = e.IsChecked;
            _attributeButton.Click += AttributeClicked;
            _comparisonButton.Click += ComparisonClicked;
            _valueButton.Click += ValueClicked;
            _addButton.Click += AddClicked;
            _traceButton.Click += TraceClicked;
            _resetButton.Click += ResetClicked;

            // Make the label for barrier expression scrollable.
            _expressionLabel.MovementMethod = new ScrollingMovementMethod();
        }
    }
}