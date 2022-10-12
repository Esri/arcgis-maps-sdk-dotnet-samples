// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UtilityNetworks;
using System.Diagnostics;

namespace ArcGISRuntime.Samples.ConfigureSubnetworkTrace
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Configure subnetwork trace",
        category: "Utility network",
        description: "Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.",
        instructions: "The sample loads with a server-defined trace configuration from a tier. Check or uncheck which options to include in the trace - such as containers or barriers. Use the selection boxes to define a new condition network attribute comparison, and then use 'Add' to add the it to the trace configuration. Tap 'Trace' to run a subnetwork trace with this modified configuration from a default starting location.",
        tags: new[] { "category comparison", "condition barriers", "network analysis", "network attribute comparison", "subnetwork trace", "trace configuration", "traversability", "utility network", "validate consistency" })]
    public partial class ConfigureSubnetworkTrace : ContentPage
    {
        // Feature service for an electric utility network in Naperville, Illinois.
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";
        private UtilityNetwork _utilityNetwork;
        private UtilityTier _sourceTier;

        // For creating the default starting location.
        private const string DeviceTableName = "Electric Distribution Device";
        private const string AssetGroupName = "Service Point";
        private const string AssetTypeName = "Three Phase Low Voltage Meter";
        private const string GlobalId = "{3AEC2649-D867-4EA7-965F-DBFE1F64B090}";
        private UtilityElement _startingLocation;

        // For creating the default trace configuration.
        private const string DomainNetworkName = "ElectricDistribution";
        private const string TierName = "Medium Voltage Radial";

        // Holding the initial conditional expression.
        private UtilityTraceConditionalExpression _initialExpression;

        public ConfigureSubnetworkTrace()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
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
                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Build the choice lists for network attribute comparison.
                Attributes.ItemsSource = _utilityNetwork.Definition.NetworkAttributes.Where(i => i.IsSystemDefined == false).ToList();
                Operators.ItemsSource = Enum.GetValues(typeof(UtilityAttributeComparisonOperator));

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

                if (_sourceTier.GetDefaultTraceConfiguration().Traversability.Barriers is UtilityTraceConditionalExpression expression)
                {
                    ConditionBarrierExpression.Text = ExpressionToString(expression);
                    _initialExpression = expression;
                }

                // Set the traversability scope.
                _sourceTier.GetDefaultTraceConfiguration().Traversability.Scope = UtilityTraversabilityScope.Junctions;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
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

        private void OnAttributeChanged(object sender, System.EventArgs e)
        {
            // Update the UI to show the correct value entry for the attribute.
            ValueEntry.Text = string.Empty;

            if (Attributes.SelectedItem is UtilityNetworkAttribute attribute)
            {
                if (attribute.Domain is CodedValueDomain domain)
                {
                    ValueSelection.ItemsSource = domain.CodedValues.ToList();
                    ValueSelection.IsVisible = true;
                    ValueEntry.IsVisible = false;
                }
                else
                {
                    ValueSelection.IsVisible = false;
                    ValueEntry.IsVisible = true;
                }
            }
        }

        private async void OnTrace(object sender, System.EventArgs e)
        {
            if (_utilityNetwork == null || _startingLocation == null)
            {
                return;
            }
            try
            {
                // Create utility trace parameters for the starting location.
                UtilityTraceParameters parameters = new UtilityTraceParameters(UtilityTraceType.Subnetwork, new[] { _startingLocation });
                parameters.TraceConfiguration = _sourceTier.GetDefaultTraceConfiguration();

                // Trace the utility network.
                IEnumerable<UtilityTraceResult> results = await _utilityNetwork.TraceAsync(parameters);

                // Get the first result.
                UtilityElementTraceResult elementResult = results?.FirstOrDefault() as UtilityElementTraceResult;

                // Display the number of elements found by the trace.
                await Application.Current.MainPage.DisplayAlert("Trace Result", $"`{elementResult?.Elements?.Count ?? 0}` elements found.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"{ex.Message}\nFor a working barrier condition, try \"Transformer Load\" Equal \"15\".", "OK");
            }
        }

        private void OnReset(object sender, System.EventArgs e)
        {
            // Reset the barrier condition to the initial value.
            UtilityTraceConfiguration traceConfiguration = _sourceTier.GetDefaultTraceConfiguration();
            traceConfiguration.Traversability.Barriers = _initialExpression;
            ConditionBarrierExpression.Text = ExpressionToString(_initialExpression);
        }

        private async void OnAddCondition(object sender, EventArgs e)
        {
            try
            {
                UtilityTraceConfiguration traceConfiguration = _sourceTier.GetDefaultTraceConfiguration();
                if (traceConfiguration == null)
                {
                    traceConfiguration = new UtilityTraceConfiguration();
                }
                if (traceConfiguration.Traversability == null)
                {
                    traceConfiguration.Traversability = new UtilityTraversability();
                }

                // NOTE: You may also create a UtilityCategoryComparison with UtilityNetworkDefinition.Categories and UtilityCategoryComparisonOperator.
                if (Attributes.SelectedItem is UtilityNetworkAttribute attribute
                    && Operators.SelectedItem is UtilityAttributeComparisonOperator attributeOperator)
                {
                    object otherValue;
                    // If the value is a coded value.
                    if (attribute.Domain is CodedValueDomain && ValueSelection.SelectedItem is CodedValue codedValue)
                    {
                        otherValue = ConvertToDataType(codedValue.Code, attribute.DataType);
                    }
                    // If the value is free entry.
                    else
                    {
                        otherValue = ConvertToDataType(ValueEntry.Text.Trim(), attribute.DataType);
                    }
                    // NOTE: You may also create a UtilityNetworkAttributeComparison with another NetworkAttribute.
                    UtilityTraceConditionalExpression expression = new UtilityNetworkAttributeComparison(attribute, attributeOperator, otherValue);
                    if (traceConfiguration.Traversability.Barriers is UtilityTraceConditionalExpression otherExpression)
                    {
                        // NOTE: You may also combine expressions with UtilityTraceAndCondition
                        expression = new UtilityTraceOrCondition(otherExpression, expression);
                    }
                    traceConfiguration.Traversability.Barriers = expression;
                    ConditionBarrierExpression.Text = ExpressionToString(expression);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void IncludeBarriersChanged(object sender, ToggledEventArgs e)
        {
            _sourceTier.GetDefaultTraceConfiguration().IncludeBarriers = e.Value;
        }

        private void IncludeContainersChanged(object sender, ToggledEventArgs e)
        {
            _sourceTier.GetDefaultTraceConfiguration().IncludeContainers = e.Value;
        }
    }
}