// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.PerformValveIsolationTrace
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Perform valve isolation trace",
        category: "Utility network",
        description: "Run a filtered trace to locate operable features that will isolate an area from the flow of network resources.",
        instructions: "Create and set the configuration's filter barriers by selecting a category. Check or uncheck 'Include Isolated Features'. Tap 'Trace' to run a subnetwork-based isolation trace.",
        tags: new[] { "category comparison", "condition barriers", "isolated features", "network analysis", "subnetwork trace", "trace configuration", "trace filter", "utility network" })]
    public partial class PerformValveIsolationTrace : ContentPage
    {
        // Feature service for an electric utility network in Naperville, Illinois.
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleGas/FeatureServer";
        private const int LineLayerId = 3;
        private const int DeviceLayerId = 0;
        private UtilityNetwork _utilityNetwork;

        // For creating the default trace configuration.
        private const string DomainNetworkName = "Pipeline";
        private const string TierName = "Pipe Distribution System";

        // For creating the default starting location.
        private const string NetworkSourceName = "Gas Device";
        private const string AssetGroupName = "Meter";
        private const string AssetTypeName = "Customer";
        private const string GlobalId = "{98A06E95-70BE-43E7-91B7-E34C9D3CB9FF}";

        private UtilityTraceParameters _parameters;
        private TaskCompletionSource<string> _terminalCompletionSource;
        private SimpleMarkerSymbol _barrierPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.OrangeRed, 25d);

        public PerformValveIsolationTrace()
        {
            InitializeComponent();
            Initialize();
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
                // Disable the UI.
                FilterOptions.IsVisible = false;

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Create a map with layers in this utility network.
                MyMapView.Map = new Map(new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=1970c1995b8f44749f4b9b6e81b5ba45")));
                MyMapView.Map.OperationalLayers.Add(new FeatureLayer(new Uri($"{FeatureServiceUrl}/{LineLayerId}")));
                MyMapView.Map.OperationalLayers.Add(new FeatureLayer(new Uri($"{FeatureServiceUrl}/{DeviceLayerId}")));

                // Get a trace configuration from a tier.
                UtilityDomainNetwork domainNetwork = _utilityNetwork.Definition.GetDomainNetwork(DomainNetworkName) ?? throw new ArgumentException(DomainNetworkName);
                UtilityTier tier = domainNetwork.GetTier(TierName) ?? throw new ArgumentException(TierName);
                UtilityTraceConfiguration configuration = tier.TraceConfiguration;

                // Create a trace filter.
                configuration.Filter = new UtilityTraceFilter();

                // Get a default starting location.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(NetworkSourceName) ?? throw new ArgumentException(NetworkSourceName);
                UtilityAssetGroup assetGroup = networkSource.GetAssetGroup(AssetGroupName) ?? throw new ArgumentException(AssetGroupName);
                UtilityAssetType assetType = assetGroup.GetAssetType(AssetTypeName) ?? throw new ArgumentException(AssetTypeName);
                Guid globalId = Guid.Parse(GlobalId);
                UtilityElement startingLocation = _utilityNetwork.CreateElement(assetType, globalId);

                // Build parameters for isolation trace.
                _parameters = new UtilityTraceParameters(UtilityTraceType.Isolation, new[] { startingLocation });
                _parameters.TraceConfiguration = tier.TraceConfiguration;

                // Create a graphics overlay.
                GraphicsOverlay overlay = new GraphicsOverlay() { Id = "StartingLocations" };
                MyMapView.GraphicsOverlays.Add(overlay);
                MyMapView.GraphicsOverlays.Add(new GraphicsOverlay() { Id = "FilterBarriers" });

                // Display starting location.
                IEnumerable<ArcGISFeature> elementFeatures = await _utilityNetwork.GetFeaturesForElementsAsync(new List<UtilityElement> { startingLocation });
                MapPoint startingLocationGeometry = elementFeatures.FirstOrDefault().Geometry as MapPoint;
                Symbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.LimeGreen, 25d);
                Graphic graphic = new Graphic(startingLocationGeometry, symbol);
                overlay.Graphics.Add(graphic);

                // Set the starting viewpoint.
                await MyMapView.SetViewpointAsync(new Viewpoint(startingLocationGeometry, 3000));

                // Build the choice list for categories populated with the `Name` property of each `UtilityCategory` in the `UtilityNetworkDefinition`.
                CategoryPicker.ItemsSource = _utilityNetwork.Definition.Categories.ToList();
                CategoryPicker.SelectedItem = _utilityNetwork.Definition.Categories.First();

                // Enable the UI.
                FilterOptions.IsVisible = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnTrace(object sender, EventArgs e)
        {
            try
            {
                LoadingIndicator.IsVisible = true;

                // Clear previous selection from the layers.
                MyMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                // Check if any filter barriers have been placed.
                if (_parameters.FilterBarriers.Count == 0)
                {
                    if (CategoryPicker.SelectedItem is UtilityCategory category)
                    {
                        // NOTE: UtilityNetworkAttributeComparison or UtilityCategoryComparison with Operator.DoesNotExists
                        // can also be used. These conditions can be joined with either UtilityTraceOrCondition or UtilityTraceAndCondition.
                        UtilityCategoryComparison categoryComparison = new UtilityCategoryComparison(category, UtilityCategoryComparisonOperator.Exists);

                        // Add the filter barrier.
                        _parameters.TraceConfiguration.Filter = new UtilityTraceFilter()
                        {
                            Barriers = categoryComparison
                        };
                    }

                    // Set the include isolated features property.
                    _parameters.TraceConfiguration.IncludeIsolatedFeatures = IncludeIsolatedFeatures.IsToggled == true;
                }
                else
                {
                    // Reset the trace configuration filter.
                    _parameters.TraceConfiguration.Filter = new UtilityTraceFilter();
                    _parameters.TraceConfiguration.IncludeIsolatedFeatures = false;
                }

                // Get the trace result from trace.
                IEnumerable<UtilityTraceResult> traceResult = await _utilityNetwork.TraceAsync(_parameters);
                UtilityElementTraceResult elementTraceResult = traceResult?.FirstOrDefault() as UtilityElementTraceResult;

                // Select all the features from the result.
                if (elementTraceResult?.Elements?.Count > 0)
                {
                    foreach (FeatureLayer layer in MyMapView.Map.OperationalLayers.OfType<FeatureLayer>())
                    {
                        IEnumerable<UtilityElement> elements = elementTraceResult.Elements.Where(element => element.NetworkSource.Name == layer.FeatureTable.TableName);
                        IEnumerable<Feature> features = await _utilityNetwork.GetFeaturesForElementsAsync(elements);
                        layer.SelectFeatures(features);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnGeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            try
            {
                LoadingIndicator.IsVisible = true;

                // Identify the feature to be used.
                IEnumerable<IdentifyLayerResult> identifyResult = await MyMapView.IdentifyLayersAsync(e.Position, 10.0, false);
                ArcGISFeature feature = identifyResult?.FirstOrDefault()?.GeoElements?.FirstOrDefault() as ArcGISFeature;
                if (feature == null) { return; }

                // Create element from the identified feature.
                UtilityElement element = _utilityNetwork.CreateElement(feature);

                if (element.NetworkSource.SourceType == UtilityNetworkSourceType.Junction)
                {
                    // Select terminal for junction feature.
                    IEnumerable<UtilityTerminal> terminals = element.AssetType.TerminalConfiguration?.Terminals;
                    if (terminals?.Count() > 1)
                    {
                        element.Terminal = await GetTerminalAsync(terminals);
                    }
                }
                else if (element.NetworkSource.SourceType == UtilityNetworkSourceType.Edge)
                {
                    // Compute how far tapped location is along the edge feature.
                    if (feature.Geometry is Polyline line)
                    {
                        line = GeometryEngine.RemoveZ(line) as Polyline;
                        double fraction = GeometryEngine.FractionAlong(line, e.Location, -1);
                        if (double.IsNaN(fraction)) { return; }
                        element.FractionAlongEdge = fraction;
                    }
                }

                _parameters.FilterBarriers.Add(element);

                // Add a graphic for the new utility element.
                Graphic traceLocationGraphic = new Graphic(feature.Geometry as MapPoint ?? e.Location, _barrierPointSymbol);
                MyMapView.GraphicsOverlays["FilterBarriers"]?.Graphics.Add(traceLocationGraphic);

                // Disable UI not used for filter barriers.
                CategoryPicker.IsEnabled = false;
                IncludeIsolatedFeatures.IsEnabled = false;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }

        private async Task<UtilityTerminal> GetTerminalAsync(IEnumerable<UtilityTerminal> terminals)
        {
            try
            {
                // Switch the UI for the user choosing the junction.
                ResponsiveFormContainer.IsVisible = false;
                MyMapView.IsVisible = false;
                PickerUI.IsVisible = true;
                MyMapView.GeoViewTapped -= OnGeoViewTapped;

                // Load the terminals into the UI.
                TerminalPicker.ItemsSource = terminals.Select(x => x.Name).ToList();
                TerminalPicker.SelectedItem = null;

                // Waits for user to select a terminal.
                _terminalCompletionSource = new TaskCompletionSource<string>();
                string selectedName = await _terminalCompletionSource.Task;
                return terminals.Where(x => x.Name.Equals(selectedName)).FirstOrDefault();
            }
            finally
            {
                // Make the main UI visible again.
                ResponsiveFormContainer.IsVisible = true;
                MyMapView.IsVisible = true;
                PickerUI.IsVisible = false;
                MyMapView.GeoViewTapped += OnGeoViewTapped;
            }
        }

        private void TerminalPicker_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            _terminalCompletionSource.TrySetResult(TerminalPicker.SelectedItem as string);
        }

        private void OnReset(object sender, EventArgs e)
        {
            _parameters.FilterBarriers.Clear();
            MyMapView.GraphicsOverlays["FilterBarriers"]?.Graphics.Clear();

            // Re-enable the UI.
            CategoryPicker.IsEnabled = true;
            IncludeIsolatedFeatures.IsEnabled = true;
        }
    }
}