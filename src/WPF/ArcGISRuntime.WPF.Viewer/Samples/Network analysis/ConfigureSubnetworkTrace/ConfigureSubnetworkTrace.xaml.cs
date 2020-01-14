// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.ConfigureSubnetworkTrace
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Configure subnetwork trace",
        "Network analysis",
        "Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class ConfigureSubnetworkTrace
    {
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
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                Configuration.IsEnabled = false;

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Build the choice lists for network attribute comparison.
                ComparisonSources.ItemsSource = _utilityNetwork.Definition.NetworkAttributes.Where((i => i.IsSystemDefined == false));
                ComparisonOperators.ItemsSource = Enum.GetValues(typeof(UtilityAttributeComparisonOperator));

                // Create a default starting location.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(DeviceTableName);
                UtilityAssetGroup assetGroup = networkSource.GetAssetGroup(AssetGroupName);
                UtilityAssetType assetType = assetGroup.GetAssetType(AssetTypeName);
                Guid globalId = Guid.Parse(GlobalId);
                _startingLocation = _utilityNetwork.CreateElement(assetType, globalId);

                // Set the terminal for this location. (For our case, we use the 'Load' terminal.)
                _startingLocation.Terminal = _startingLocation.AssetType.TerminalConfiguration?.Terminals.Where(t => t.Name == "Load").FirstOrDefault();

                // Get a default trace configuration from a tier to update the UI.
                UtilityDomainNetwork domainNetwork = _utilityNetwork.Definition.GetDomainNetwork(DomainNetworkName);
                UtilityTier sourceTier = domainNetwork.GetTier(TierName);

                // Set the trace configuration.
                _configuration = sourceTier.TraceConfiguration;

                // Set the default expression (if provided).
                if (sourceTier.TraceConfiguration.Traversability.Barriers is UtilityTraceConditionalExpression expression)
                {
                    ConditionBarrierExpression.Text = ExpressionToString(expression);
                    _initialExpression = expression;
                }

                // Setting DataContext will resolve the data-binding in XAML.
                Configuration.DataContext = _configuration;

                // Set the traversability scope.
                sourceTier.TraceConfiguration.Traversability.Scope = UtilityTraversabilityScope.Junctions;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Message.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Configuration.IsEnabled = true;
            }
        }

        private void OnAddCondition(object sender, RoutedEventArgs e)
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
                if (ComparisonSources.SelectedItem is UtilityNetworkAttribute attribute
                    && ComparisonOperators.SelectedItem is UtilityAttributeComparisonOperator attributeOperator)
                {
                    object selectedValue;

                    // If the value is a coded value.
                    if (attribute.Domain is CodedValueDomain && ComparisonValueChoices.SelectedItem is CodedValue codedValue)
                    {
                        selectedValue = ConvertToDataType(codedValue.Code, attribute.DataType);
                    }
                    // If the value is free entry.
                    else
                    {
                        selectedValue = ConvertToDataType(ComparisonValue.Text.Trim(), attribute.DataType);
                    }
                    // NOTE: You may also create a UtilityNetworkAttributeComparison with another NetworkAttribute.
                    UtilityTraceConditionalExpression expression = new UtilityNetworkAttributeComparison(attribute, attributeOperator, selectedValue);
                    if (_configuration.Traversability.Barriers is UtilityTraceConditionalExpression otherExpression)
                    {
                        // NOTE: You may also combine expressions with UtilityTraceAndCondition
                        expression = new UtilityTraceOrCondition(otherExpression, expression);
                    }
                    _configuration.Traversability.Barriers = expression;
                    ConditionBarrierExpression.Text = ExpressionToString(expression);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ExpressionToString(UtilityTraceConditionalExpression expression)
        {
            if (expression is UtilityCategoryComparison categoryComparison)
            {
                return $"`{categoryComparison.Category.Name}` {categoryComparison.ComparisonOperator}";
            }
            else if (expression is UtilityNetworkAttributeComparison attributeComparison)
            {
                if (attributeComparison.NetworkAttribute.Domain is CodedValueDomain cvd)
                {
                    string cvdName = cvd.CodedValues.FirstOrDefault(c => ConvertToDataType(c.Code, attributeComparison.NetworkAttribute.DataType).Equals(ConvertToDataType(attributeComparison.Value, attributeComparison.NetworkAttribute.DataType)))?.Name;
                    return $"`{attributeComparison.NetworkAttribute.Name}` {attributeComparison.ComparisonOperator} `{cvdName}`";
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

        private async void OnTrace(object sender, RoutedEventArgs e)
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
                MessageBox.Show($"`{elementResult?.Elements?.Count ?? 0}` elements found.", "Trace Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{ex.Message}\nFor a working barrier condition, try \"Transformer Load\" Equal \"15\".",
                    ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnComparisonSourceChanged(object sender, SelectionChangedEventArgs e)
        {
            ComparisonValue.Text = string.Empty;

            // Update the UI to show the correct value entry for the attribute.
            if (ComparisonSources.SelectedItem is UtilityNetworkAttribute attribute)
            {
                if (attribute.Domain is CodedValueDomain domain)
                {
                    ComparisonValueChoices.ItemsSource = domain.CodedValues;
                    ComparisonValueChoices.Visibility = Visibility.Visible;
                    ComparisonValue.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ComparisonValue.Visibility = Visibility.Visible;
                    ComparisonValueChoices.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            // Reset the barrier condition to the initial value.
            UtilityTraceConfiguration traceConfiguration = _configuration;
            traceConfiguration.Traversability.Barriers = _initialExpression;
            ConditionBarrierExpression.Text = ExpressionToString(_initialExpression);
        }
    }
}