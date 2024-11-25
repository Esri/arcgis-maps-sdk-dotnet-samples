// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;

namespace ArcGIS.WPF.Samples.ValidateUtilityNetworkTopology
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Validate utility network topology",
        category: "Utility network",
        description: "Demonstrates the workflow of getting the network state and validating the topology of a utility network.",
        instructions: "Select features to make edits and then use 'Update and Apply Edit' to send edits to the server.",
        tags: new[] { "dirty areas", "edit", "network topology", "online", "state", "trace", "utility network", "validate" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class ValidateUtilityNetworkTopology
    {
        private static readonly string WebmapItemUrl = "https://sampleserver7.arcgisonline.com/portal/home/item.html?id=6e3fc6db3d0b4e6589eb4097eb3e5b9b";

        private readonly Viewpoint InitialViewpoint = new Viewpoint(
            new Envelope(
                -9815489.0660101417,
                5128463.4221229386,
                -9814625.2768726498,
                5128968.4911854975,
                SpatialReferences.WebMercator
            )
        );

        // For editing
        private const string LineTableName = "Electric Distribution Line";
        private const string DeviceTableName = "Electric Distribution Device";
        private ArcGISFeature _featureToEdit;

        // To impact trace
        private const string DeviceStatusField = "devicestatus";

        private readonly LabelDefinition DeviceLabelDefinition = new LabelDefinition(
            new SimpleLabelExpression($"[{DeviceStatusField}]"),
            new TextSymbol
            {
                Color = Color.Blue,
                HaloColor = Color.White,
                HaloWidth = 2,
                Size = 12
            }
        )
        {
            UseCodedValues = true
        };

        // To better visualize dirty area
        private const string NominalVoltageField = "nominalvoltage";

        private readonly LabelDefinition LineLabelDefinition = new LabelDefinition(
            new SimpleLabelExpression($"[{NominalVoltageField}]"),
            new TextSymbol
            {
                Color = Color.Red,
                HaloColor = Color.White,
                HaloWidth = 2,
                Size = 12
            }
        )
        {
            UseCodedValues = true
        };

        // For tracing
        private const string AssetGroupName = "Circuit Breaker";
        private const string AssetTypeName = "Three Phase";
        private const string GlobalId = "{1CAF7740-0BF4-4113-8DB2-654E18800028}";
        private const string DomainNetworkName = "ElectricDistribution";
        private const string TierName = "Medium Voltage Radial";
        private UtilityTraceParameters _traceParameters;

        public ValidateUtilityNetworkTopology()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Loading a webmap...";

                // Add credential for this webmap.
                // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample
                string sampleServerPortalUrl =
                    "https://sampleserver7.arcgisonline.com/portal/sharing/rest";
                string sampleServer7User = "editor01";
                string sampleServer7Pass = "S7#i2LWmYH75";
                var credential = await AccessTokenCredential.CreateAsync(
                        new Uri(sampleServerPortalUrl),
                        sampleServer7User,
                        sampleServer7Pass
                    );
                AuthenticationManager.Current.AddCredential(credential);

                // Load map to access utility network.
                var map = new Map(new Uri(WebmapItemUrl)) { InitialViewpoint = InitialViewpoint };

                // Load in persistent session mode (workaround for server caching issue).
                // https://support.esri.com/en-us/bug/asynchronous-validate-request-for-utility-network-servi-bug-000160443
                map.LoadSettings = new LoadSettings() { FeatureServiceSessionType = FeatureServiceSessionType.Persistent };

                MyMapView.Map = map;
                await map.LoadAsync();

                // Load and switch utility network version.
                Status.Text = "Loading the utility network...";
                var utilityNetwork =
                    map.UtilityNetworks.FirstOrDefault()
                    ?? throw new InvalidOperationException("Expected a utility network");
                await utilityNetwork.LoadAsync();

                var sgdb =
                    utilityNetwork.ServiceGeodatabase
                    ?? throw new InvalidOperationException("Expected a service geodatabase");

                // Restrict editing and tracing on a random branch.
                var parameters = new ServiceVersionParameters();
                parameters.Name = $"ValidateNetworkTopology_{Guid.NewGuid()}";
                parameters.Access = VersionAccess.Private;
                parameters.Description = "Validate network topology with ArcGIS Runtime";

                var info = await sgdb.CreateVersionAsync(parameters);
                await sgdb.SwitchVersionAsync(info.Name);

                // Visualize attribute editing using labels.
                foreach (var layer in map.OperationalLayers.OfType<FeatureLayer>())
                {
                    if (layer.Name == DeviceTableName)
                    {
                        layer.LabelDefinitions.Add(DeviceLabelDefinition);
                        layer.LabelsEnabled = true;
                    }
                    else if (layer.Name == LineTableName)
                    {
                        layer.LabelDefinitions.Add(LineLabelDefinition);
                        layer.LabelsEnabled = true;
                    }
                }

                // Visualize dirty area by adding to the map.
                var dirtyAreaTable =
                    utilityNetwork.DirtyAreaTable
                    ?? throw new InvalidOperationException("Expected a dirty area table");
                MyMapView.Map.OperationalLayers.Insert(0, new FeatureLayer(utilityNetwork.DirtyAreaTable));

                // Trace with a subnetwork controller as default starting location.
                var networkSource = utilityNetwork.Definition.GetNetworkSource(DeviceTableName);
                var assetGroup = networkSource.GetAssetGroup(AssetGroupName);
                var assetType = assetGroup.GetAssetType(AssetTypeName);
                var globalId = Guid.Parse(GlobalId);
                var startingLocation = utilityNetwork.CreateElement(assetType, globalId);
                startingLocation.Terminal = startingLocation
                    .AssetType
                    .TerminalConfiguration
                    ?.Terminals
                    .FirstOrDefault(term => term.Name == "Load");

                // Display starting location as graphic.
                var features = await utilityNetwork.GetFeaturesForElementsAsync(
                    new[] { startingLocation }
                );
                var feature = features.FirstOrDefault();
                if (feature != null)
                {
                    await feature.LoadAsync();
                    var graphic = new Graphic(feature.Geometry)
                    {
                        Symbol = new SimpleMarkerSymbol(
                            SimpleMarkerSymbolStyle.Cross,
                            Color.Green,
                            25d
                        )
                    };
                    var overlay = new GraphicsOverlay();
                    overlay.Graphics.Add(graphic);
                    MyMapView.GraphicsOverlays.Add(overlay);
                }

                // Trace with a configuration that stops traversability on an open device.
                var domainNetwork = utilityNetwork.Definition.GetDomainNetwork(DomainNetworkName);
                var sourceTier = domainNetwork.GetTier(TierName);

                _traceParameters = new UtilityTraceParameters(
                    UtilityTraceType.Downstream,
                    new[] { startingLocation }
                )
                {
                    TraceConfiguration = sourceTier.GetDefaultTraceConfiguration()
                };

                // Enable buttons with UtilityNetworkCapabilities.
                ValidateBtn.IsEnabled = utilityNetwork
                    .Definition
                    .Capabilities
                    .SupportsValidateNetworkTopology;
                TraceBtn.IsEnabled = utilityNetwork.Definition.Capabilities.SupportsTrace;
                StateBtn.IsEnabled = utilityNetwork.Definition.Capabilities.SupportsNetworkState;
                ClearBtn.IsEnabled = false;

                // Set the instruction text.
                Status.Text = "Utility Network Loaded\n" +
                    "Tap on a feature to edit.\n" +
                    "Click 'Get State' to check if validating is necessary " +
                    "or if tracing is available.\n" +
                    "Click 'Trace' to run a trace.";
            }
            catch (Exception ex)
            {
                Status.Text = "Initialization failed.";
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK);
            }
            finally
            {
                IsBusy.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnGetState(object sender, RoutedEventArgs e)
        {
            try
            {
                var utilityNetwork =
                    MyMapView?.Map?.UtilityNetworks.FirstOrDefault()
                    ?? throw new InvalidOperationException("Expected a utility network");

                if (utilityNetwork.Definition.Capabilities.SupportsNetworkState)
                {
                    IsBusy.Visibility = Visibility.Visible;
                    Status.Text = "Getting utility network state...";

                    var state = await utilityNetwork.GetStateAsync();

                    // Validate if dirty areas or errors exist.
                    ValidateBtn.IsEnabled = state.HasDirtyAreas;

                    // Trace if network topology is enabled.
                    TraceBtn.IsEnabled = state.IsNetworkTopologyEnabled;

                    var sb = new StringBuilder(
                        "Utility Network State:\n"
                            + $"\tHas Dirty Areas: {state.HasDirtyAreas}\n"
                            + $"\tHas Errors: {state.HasErrors}\n"
                            + $"\tIs Network Topology Enabled: {state.IsNetworkTopologyEnabled}\n"
                    );

                    if (state.HasDirtyAreas || state.HasErrors)
                    {
                        sb.AppendLine("Click 'Validate' before trace or expect a trace error.");
                    }
                    else
                    {
                        sb.AppendLine("Tap on a feature to edit or click 'Trace' to run a trace.");
                    }

                    Status.Text = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK);
            }
            finally
            {
                IsBusy.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnValidate(object sender, RoutedEventArgs e)
        {
            try
            {
                var utilityNetwork =
                    MyMapView?.Map?.UtilityNetworks.FirstOrDefault()
                    ?? throw new InvalidOperationException("Expected a utility network");

                // Validate using the current extent.
                var extent = MyMapView
                    ?.GetCurrentViewpoint(ViewpointType.BoundingGeometry)
                    ?.TargetGeometry
                    ?.Extent
                    ?? throw new InvalidOperationException("Expected current extent");

                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Validating utility network topology...";

                // Get the validation result.
                var job = utilityNetwork.ValidateNetworkTopology(extent);
                var result = await job.GetResultAsync();

                Status.Text =
                    "Utility Validation Result:\n"
                    + $"\tHas Dirty Areas: {result.HasDirtyAreas}\n"
                    + $"\tHas Errors: {result.HasErrors}\n" +
                    "Click 'Get State' to check the updated network state.";

                ValidateBtn.IsEnabled = result.HasDirtyAreas;
            }
            catch (Exception ex)
            {
                Status.Text = "Validate network topology failed.";
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK);
            }
            finally
            {
                IsBusy.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Identifying feature to edit...";

                // Perform an identify to determine if a user tapped on a feature.
                var layerResults = await MyMapView.IdentifyLayersAsync(e.Position, 5, false);
                var feature =
                    layerResults
                        .FirstOrDefault(
                            l =>
                                (
                                    l.LayerContent.Name == DeviceTableName
                                    || l.LayerContent.Name == LineTableName
                                )
                        )
                        ?.GeoElements
                        .FirstOrDefault() as ArcGISFeature;
                if (feature is null)
                {
                    Status.Text = "No feature identified. Tap on a feature to edit.";
                    return;
                }

                var updateFieldName =
                    feature.FeatureTable.TableName == DeviceTableName
                        ? DeviceStatusField
                        : NominalVoltageField;
                var field = feature.FeatureTable.GetField(updateFieldName);
                var codedValues = (field?.Domain as CodedValueDomain)?.CodedValues;
                if (field is null || codedValues is null || codedValues.Count == 0)
                {
                    return;
                }

                if (feature.LoadStatus != LoadStatus.Loaded)
                {
                    await feature.LoadAsync();
                }

                _featureToEdit = feature;

                // Clear previous selection.
                MyMapView
                    .Map
                    .OperationalLayers
                    .OfType<FeatureLayer>()
                    .ToList()
                    .ForEach(layer => layer.ClearSelection());

                // Select the feature.
                if (_featureToEdit.FeatureTable.Layer is FeatureLayer featureLayer)
                    featureLayer.SelectFeature(_featureToEdit);

                Choices.ItemsSource = codedValues;
                var actualValue = Convert.ToInt32(_featureToEdit.Attributes[field.Name]);
                Choices.SelectedItem = codedValues.Single(
                    c => Convert.ToInt32(c.Code).Equals(actualValue)
                );

                FieldName.Text = field.Name;
                Status.Text = $"Select a new '{field.Alias ?? field.Name}'";

                // Update the UI for the selection.
                AttributePicker.Visibility = Visibility.Visible;
                ClearBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Status.Text = "Identifying feature to edit failed.";
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK);
            }
            finally
            {
                IsBusy.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnApplyEdits(object sender, RoutedEventArgs e)
        {
            try
            {
                var serviceGeodatabase = (
                    _featureToEdit?.FeatureTable as ServiceFeatureTable
                )?.ServiceGeodatabase;

                var fieldName = FieldName?.Text?.Trim();
                if (
                    _featureToEdit is null
                    || serviceGeodatabase is null
                    || string.IsNullOrWhiteSpace(fieldName)
                    || !_featureToEdit.Attributes.ContainsKey(fieldName)
                    || !(Choices.SelectedItem is CodedValue codedValue)
                )
                {
                    return;
                }

                IsBusy.Visibility = Visibility.Visible;

                Status.Text = "Updating feature...";
                _featureToEdit.Attributes[fieldName] = codedValue.Code;
                await _featureToEdit.FeatureTable.UpdateFeatureAsync(_featureToEdit);

                Status.Text = "Applying edits...";
                var applyEditsResult = await serviceGeodatabase.ApplyEditsAsync();

                if (
                    applyEditsResult.Any(
                        r => r.EditResults.Any(er => er.CompletedWithErrors || er.Error != null)
                    )
                )
                {
                    Status.Text = "Apply edits completed with error.";
                }
                else
                {
                    Status.Text = "Apply edits completed successfully.\n" +
                        "Click 'Get State' to check the updated network state.";
                }
            }
            catch (Exception ex)
            {
                Status.Text = "Apply edits failed.";
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK);
            }
            finally
            {
                // Clear selection.
                MyMapView
                   .Map
                   .OperationalLayers
                   .OfType<FeatureLayer>()
                   .ToList()
                   .ForEach(layer => layer.ClearSelection());

                IsBusy.Visibility = Visibility.Collapsed;
                AttributePicker.Visibility = Visibility.Collapsed;
                ClearBtn.IsEnabled = false;
                ValidateBtn.IsEnabled = true;
            }
        }

        private async void OnTrace(object sender, RoutedEventArgs e)
        {
            var utilityNetwork =
                MyMapView?.Map?.UtilityNetworks.FirstOrDefault()
                ?? throw new InvalidOperationException("Expected a utility network");

            try
            {
                // Update the UI.
                IsBusy.Visibility = Visibility.Visible;
                Status.Text = $"Running a downstream trace...";

                // Clear previous selection from the layers.
                MyMapView
                   .Map
                   .OperationalLayers
                   .OfType<FeatureLayer>()
                   .ToList()
                   .ForEach(layer => layer.ClearSelection());

                //  Get the trace result from the utility network.
                var traceResult = await utilityNetwork.TraceAsync(_traceParameters);
                var elementTraceResult =
                    traceResult.FirstOrDefault(r => r is UtilityElementTraceResult)
                    as UtilityElementTraceResult;

                // Check if there are any elements in the result.
                var elementsFound = elementTraceResult?.Elements.Count ?? 0;
                Status.Text = $"Trace completed: {elementsFound} elements found";
                foreach (var layer in MyMapView.Map.OperationalLayers.OfType<FeatureLayer>())
                {
                    var elements = elementTraceResult
                        .Elements
                        .Where(element => element.NetworkSource.FeatureTable == layer.FeatureTable);
                    if (elements.Any())
                    {
                        var features = await utilityNetwork.GetFeaturesForElementsAsync(elements);
                        layer.SelectFeatures(features);
                    }
                }

                ClearBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Status.Text = "Trace failed.\n" +
                    "Click 'Get State' to check the updated network state.";
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK);
            }
            finally
            {
                IsBusy.Visibility = Visibility.Collapsed;
            }
        }

        private void OnClear(object sender, RoutedEventArgs e)
        {
            AttributePicker.Visibility = Visibility.Collapsed;

            // Clear the selection.
            MyMapView
               .Map
               .OperationalLayers
               .OfType<FeatureLayer>()
               .ToList()
               .ForEach(layer => layer.ClearSelection());

            _featureToEdit = null;

            Status.Text = "Selection cleared.";

            ClearBtn.IsEnabled = false;
        }
    }
}