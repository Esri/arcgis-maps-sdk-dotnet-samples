// Copyright 2023 Esri.
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.CreateLoadReport
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Create load report",
        category: "Utility network",
        description: "Demonstrates the creation of a simple electric distribution report. It traces downstream from a given point and adds up the count of customers and total load per phase.",
        instructions: "Choose phases to be included in the report. Click 'Run Report' to initiate a downstream trace on the network and create a load report. Click 'Reset' to clear the phases and start over.",
        tags: new[] { "condition barriers", "downstream trace", "network analysis", "subnetwork trace", "trace configuration", "traversability", "upstream trace", "utility network", "validate consistency" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class CreateLoadReport
    {
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        // Default starting location.
        private const string NetworkSourceName = "Electric Distribution Device";
        private const string AssetGroupName = "Circuit Breaker";
        private const string AssetTypeName = "Three Phase";
        private const string TerminalName = "Load";
        private const string GlobalId = "{1CAF7740-0BF4-4113-8DB2-654E18800028}";

        // Default trace configuration.
        private const string DomainNetworkName = "ElectricDistribution";
        private const string TierName = "Medium Voltage Radial";
        private UtilityTraceConditionalExpression _baseCondition = null;

        // Compute total customers.
        private const string ServiceCategoryName = "ServicePoint";

        // Compute total loads.
        private const string LoadNetworkAttributeName = "Service Load";

        // Varying attribute.
        private const string PhasesNetworkAttributeName = "Phases Current";
        private UtilityNetworkAttribute _phasesNetworkAttribute = null;
        private List<string> _phases = new List<string>(new[] { "A", "B", "C" });
        private UtilityNetwork _utilityNetwork = null;
        private UtilityTraceParameters _traceParameters = null;
        private ObservableCollection<PhaseSummary> _phaseSummaries = new ObservableCollection<PhaseSummary>();

        public class PhaseSummary
        {
            public PhaseSummary(string phase)
            {
                Phase = phase;
            }

            public string Phase { get; private set; }
            public double TotalCustomers { get; set; }
            public double TotalLoad { get; set; }
        }

        public CreateLoadReport()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Handle the login to the feature service.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleServer7User = "editor01";
                    string sampleServer7Pass = "S7#i2LWmYH75";
                    return await AccessTokenCredential.CreateAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            });

            try
            {
                ReportView.ItemsSource = _phaseSummaries;
                Phases.Text = $"Phases: {string.Join(", ", _phases)}";
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Create default starting location.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(NetworkSourceName);
                UtilityAssetGroup assetGroup = networkSource?.GetAssetGroup(AssetGroupName);
                UtilityAssetType assetType = assetGroup?.GetAssetType(AssetTypeName);
                UtilityTerminal terminal = assetType?.TerminalConfiguration?.Terminals?.FirstOrDefault(t => t.Name == TerminalName);
                Guid globalId = Guid.Parse(GlobalId);

                if (assetType != null && terminal != null)
                {
                    var startingLocation = _utilityNetwork.CreateElement(assetType, globalId, terminal);

                    // Get base condition and trace configuration from a default tier.
                    UtilityDomainNetwork domainNetwork = _utilityNetwork.Definition.GetDomainNetwork(DomainNetworkName);
                    UtilityTier tier = domainNetwork?.GetTier(TierName);
                    _baseCondition = tier?.GetDefaultTraceConfiguration()?.Traversability?.Barriers as UtilityTraceConditionalExpression;

                    // Create downstream trace with function outputs.
                    _traceParameters = new UtilityTraceParameters(UtilityTraceType.Downstream, new[] { startingLocation });
                    _traceParameters.ResultTypes.Add(UtilityTraceResultType.FunctionOutputs);
                    _traceParameters.TraceConfiguration = tier?.GetDefaultTraceConfiguration();

                    // Create function input and output condition.
                    UtilityCategory serviceCategory = _utilityNetwork.Definition.Categories.FirstOrDefault(c => c.Name == ServiceCategoryName);
                    UtilityNetworkAttribute loadAttribute = _utilityNetwork.Definition.GetNetworkAttribute(LoadNetworkAttributeName);
                    _phasesNetworkAttribute = _utilityNetwork.Definition.GetNetworkAttribute(PhasesNetworkAttributeName);
                    if (serviceCategory != null && loadAttribute != null && _phasesNetworkAttribute != null)
                    {
                        UtilityCategoryComparison serviceCategoryComparison = new UtilityCategoryComparison(serviceCategory, UtilityCategoryComparisonOperator.Exists);
                        UtilityTraceFunction addLoadAttributeFunction = new UtilityTraceFunction(UtilityTraceFunctionType.Add, loadAttribute, serviceCategoryComparison);
                        _traceParameters.TraceConfiguration.Functions.Clear();
                        _traceParameters.TraceConfiguration.Functions.Add(addLoadAttributeFunction);
                        _traceParameters.TraceConfiguration.OutputCondition = serviceCategoryComparison;

                        if (_phasesNetworkAttribute.Domain is CodedValueDomain codedValueDomain)
                        {
                            PhasesList.ItemsSource = codedValueDomain.CodedValues;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAddPhase(object sender, RoutedEventArgs e)
        {
            if (PhasesList.SelectedItem is CodedValue codedValue && !_phases.Contains(codedValue.Name))
            {
                _phases.Add(codedValue.Name);
                Phases.Text = $"Phases: {string.Join(",", _phases)}";
            }
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            _phases.Clear();
            Phases.Text = $"Phases:";
            _phaseSummaries.Clear();
        }

        private async void RunReportButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_phasesNetworkAttribute?.Domain is CodedValueDomain codedValueDomain)
                {
                    _phaseSummaries.Clear();
                    foreach (CodedValue codedValue in codedValueDomain.CodedValues)
                    {
                        if (!_phases.Contains(codedValue.Name))
                            continue;
                        if (_baseCondition != null)
                        {
                            UtilityNetworkAttributeComparison phasesAttributeComparison = new UtilityNetworkAttributeComparison(_phasesNetworkAttribute, UtilityAttributeComparisonOperator.DoesNotIncludeAny, codedValue.Code);
                            _traceParameters.TraceConfiguration.Traversability.Barriers = new UtilityTraceOrCondition(_baseCondition, phasesAttributeComparison);

                            IEnumerable<UtilityTraceResult> results = await _utilityNetwork.TraceAsync(_traceParameters);
                            PhaseSummary phaseSummary = new PhaseSummary(codedValue.Name);
                            foreach (UtilityTraceResult result in results)
                            {
                                if (result.Warnings.Count > 0)
                                {
                                    MessageBox.Show(string.Join("\n", result.Warnings), "Trace Result Warnings", MessageBoxButton.OK);
                                }
                                if (result is UtilityElementTraceResult elementResult)
                                {
                                    phaseSummary.TotalCustomers = (from element in elementResult.Elements select element.ObjectId).Distinct().Count();
                                }
                                else if (result is UtilityFunctionTraceResult functionResult)
                                {
                                    phaseSummary.TotalLoad = Convert.ToDouble(functionResult.FunctionOutputs.FirstOrDefault()?.Result);
                                }
                            }
                            _phaseSummaries.Insert(0, phaseSummary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}