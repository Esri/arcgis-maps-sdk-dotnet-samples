﻿// Copyright 2020 Esri.
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
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ConfigureSubnetworkTrace
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Configure subnetwork trace",
        "Network analysis",
        "Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class ConfigureSubnetworkTrace : ContentPage
    {
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";
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
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                ConfigureTable.IsEnabled = false;

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Build the choice lists for network attribute comparison.
                ComparisonSources.ItemsSource = _utilityNetwork.Definition.NetworkAttributes.Where(i => i.IsSystemDefined == false).ToList();
                ComparisonOperators.ItemsSource = Enum.GetValues(typeof(UtilityAttributeComparisonOperator));

                // Create a default starting location.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(DeviceTableName);
                UtilityAssetGroup assetGroup = networkSource.GetAssetGroup(AssetGroupName);
                UtilityAssetType assetType = assetGroup.GetAssetType(AssetTypeName);
                Guid globalId = Guid.Parse(GlobalId);
                _startingLocation = _utilityNetwork.CreateElement(assetType, globalId);

                // Get a default trace configuration from a tier to update the UI.
                UtilityDomainNetwork domainNetwork = _utilityNetwork.Definition.GetDomainNetwork(DomainNetworkName);
                _sourceTier = domainNetwork.GetTier(TierName);

                if (_sourceTier.TraceConfiguration.Traversability.Barriers is UtilityTraceConditionalExpression expression)
                {
                    ConditionBarrierExpression.Text = GetExpression(expression);
                    _initialExpression = expression;
                }

                // Set the traversability scope.
                _sourceTier.TraceConfiguration.Traversability.Scope = UtilityTraversabilityScope.Junctions;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                ConfigureTable.IsEnabled = true;
            }
        }
        private string GetExpression(UtilityTraceConditionalExpression expression)
        {
            if (expression is UtilityCategoryComparison categoryComparison)
                return $"`{categoryComparison.Category.Name}` {categoryComparison.ComparisonOperator}";
            if (expression is UtilityNetworkAttributeComparison attributeComparison)
            {
                if (attributeComparison.NetworkAttribute.Domain is CodedValueDomain cvd)
                {
                    string cvdName = cvd.CodedValues.FirstOrDefault(c => ConvertToDataType(c.Code, attributeComparison.NetworkAttribute.DataType).Equals(ConvertToDataType(attributeComparison.Value, attributeComparison.NetworkAttribute.DataType)))?.Name;
                    return $"`{attributeComparison.NetworkAttribute.Name}` {attributeComparison.ComparisonOperator} `{cvdName}`";
                }
                else
                    return $"`{attributeComparison.NetworkAttribute.Name}` {attributeComparison.ComparisonOperator} `{attributeComparison.OtherNetworkAttribute?.Name ?? attributeComparison.Value}`";
            }
            if (expression is UtilityTraceAndCondition andCondition)
                return $"({GetExpression(andCondition.LeftExpression)}) AND\n ({GetExpression(andCondition.RightExpression)})";
            if (expression is UtilityTraceOrCondition orCondition)
                return $"({GetExpression(orCondition.LeftExpression)}) OR\n ({GetExpression(orCondition.RightExpression)})";
            return null;
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

        private void OnComparisonSourceChanged(object sender, System.EventArgs e)
        {
            ComparisonValue.Text = string.Empty;

            if (ComparisonSources.SelectedItem is UtilityNetworkAttribute attribute)
            {
                if (attribute.Domain is CodedValueDomain domain)
                {
                    ComparisonValueChoices.ItemsSource = domain.CodedValues.ToList();
                    ComparisonValueChoices.IsVisible = true;
                    ComparisonValue.IsVisible = false;
                }
                else
                {
                    ComparisonValueChoices.IsVisible = false;
                    ComparisonValue.IsVisible = true;
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
                UtilityTraceParameters parameters = new UtilityTraceParameters(UtilityTraceType.Subnetwork, new[] { _startingLocation });

                parameters.TraceConfiguration = _sourceTier.TraceConfiguration;

                IEnumerable<UtilityTraceResult> results = await _utilityNetwork.TraceAsync(parameters);
                UtilityElementTraceResult elementResult = results?.FirstOrDefault() as UtilityElementTraceResult;
                await Application.Current.MainPage.DisplayAlert("Trace Result", $"`{elementResult?.Elements?.Count ?? 0}` elements found.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"{ex.Message}\nFor a working barrier condition, try \"Transformer Load\" Equal \"15\".", "OK");
            }
        }

        private void OnReset(object sender, System.EventArgs e)
        {
            UtilityTraceConfiguration traceConfiguration = _sourceTier.TraceConfiguration;
            traceConfiguration.Traversability.Barriers = _initialExpression;
            ConditionBarrierExpression.Text = GetExpression(_initialExpression);
        }

        private async void OnAddCondition(object sender, EventArgs e)
        {
            try
            {
                UtilityTraceConfiguration traceConfiguration = _sourceTier.TraceConfiguration;
                if (traceConfiguration == null)
                {
                    traceConfiguration = new UtilityTraceConfiguration();
                }
                if (traceConfiguration.Traversability == null)
                {
                    traceConfiguration.Traversability = new UtilityTraversability();
                }

                // NOTE: You may also create a UtilityCategoryComparison with UtilityNetworkDefinition.Categories and UtilityCategoryComparisonOperator.
                if (ComparisonSources.SelectedItem is UtilityNetworkAttribute attribute
                    && ComparisonOperators.SelectedItem is UtilityAttributeComparisonOperator attributeOperator)
                {
                    object otherValue;
                    if (attribute.Domain is CodedValueDomain && ComparisonValueChoices.SelectedItem is CodedValue codedValue)
                    {
                        otherValue = ConvertToDataType(codedValue.Code, attribute.DataType);
                    }
                    else
                    {
                        otherValue = ConvertToDataType(ComparisonValue.Text.Trim(), attribute.DataType);
                    }
                    // NOTE: You may also create a UtilityNetworkAttributeComparison with another NetworkAttribute.
                    UtilityTraceConditionalExpression expression = new UtilityNetworkAttributeComparison(attribute, attributeOperator, otherValue);
                    if (traceConfiguration.Traversability.Barriers is UtilityTraceConditionalExpression otherExpression)
                    {
                        // NOTE: You may also combine expressions with UtilityTraceAndCondition
                        expression = new UtilityTraceOrCondition(otherExpression, expression);
                    }
                    traceConfiguration.Traversability.Barriers = expression;
                    ConditionBarrierExpression.Text = GetExpression(expression);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void IncludeBarriersChanged(object sender, ToggledEventArgs e)
        {
            _sourceTier.TraceConfiguration.IncludeBarriers = e.Value;
        }

        private void IncludContainersChanged(object sender, ToggledEventArgs e)
        {
            _sourceTier.TraceConfiguration.IncludeContainers = e.Value;
        }
    }
}