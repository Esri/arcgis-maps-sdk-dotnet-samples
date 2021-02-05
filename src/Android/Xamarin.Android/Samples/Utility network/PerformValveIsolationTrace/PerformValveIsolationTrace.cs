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
using Android.Widget;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.PerformValveIsolationTrace
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Perform valve isolation trace",
        category: "Utility network",
        description: "Run a filtered trace to locate operable features that will isolate an area from the flow of network resources.",
        instructions: "Create and set the configuration's filter barriers by selecting a category. Check or uncheck 'Include Isolated Features'. Tap 'Trace' to run a subnetwork-based isolation trace.",
        tags: new[] { "category comparison", "condition barriers", "isolated features", "network analysis", "subnetwork trace", "trace configuration", "trace filter", "utility network" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("PerformValveIsolationTrace.xml")]
    public class PerformValveIsolationTrace : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Button _traceButton;
        private Button _resetButton;
        private CheckBox _isolatedCheckBox;
        private Spinner _categorySpinner;
        private ProgressBar _loadingBar;

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

        private Dictionary<string, UtilityCategory> _categoryDictionary = new Dictionary<string, UtilityCategory>();

        private UtilityTraceParameters _parameters;
        private TaskCompletionSource<int> _terminalCompletionSource;
        private SimpleMarkerSymbol _barrierPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.OrangeRed, 25d);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Perform valve isolation trace";

            CreateLayout();
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
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            try
            {
                // Disable the UI.
                _traceButton.Enabled = false;

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Create a map with layers in this utility network.
                _myMapView.Map = new Map(new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=1970c1995b8f44749f4b9b6e81b5ba45")));
                _myMapView.Map.OperationalLayers.Add(new FeatureLayer(new Uri($"{FeatureServiceUrl}/{LineLayerId}")));
                _myMapView.Map.OperationalLayers.Add(new FeatureLayer(new Uri($"{FeatureServiceUrl}/{DeviceLayerId}")));

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
                GraphicsOverlay overlay = new GraphicsOverlay();
                _myMapView.GraphicsOverlays.Add(overlay);
                _myMapView.GraphicsOverlays.Add(new GraphicsOverlay() { Id = "FilterBarriers" });

                // Display starting location.
                IEnumerable<ArcGISFeature> elementFeatures = await _utilityNetwork.GetFeaturesForElementsAsync(new List<UtilityElement> { startingLocation });
                MapPoint startingLocationGeometry = elementFeatures.FirstOrDefault().Geometry as MapPoint;
                Symbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.LimeGreen, 25d);
                Graphic graphic = new Graphic(startingLocationGeometry, symbol);
                overlay.Graphics.Add(graphic);

                // Set the starting viewpoint.
                await _myMapView.SetViewpointAsync(new Viewpoint(startingLocationGeometry, 3000));

                // Build the choice list for categories populated with the `Name` property of each `UtilityCategory` in the `UtilityNetworkDefinition`.
                _utilityNetwork.Definition.Categories.ToList().ForEach(cat => _categoryDictionary.Add(cat.Name, cat));
                _categorySpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, _categoryDictionary.Keys.ToList());
                _categorySpinner.SetSelection(0);

                // Enable the UI.
                _traceButton.Enabled = true;
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
            finally
            {
                _loadingBar.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private async void OnTrace(object sender, EventArgs e)
        {
            try
            {
                _loadingBar.Visibility = Android.Views.ViewStates.Visible;

                // Clear previous selection from the layers.
                _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                // Check if any filter barriers have been placed.
                if (_parameters.FilterBarriers.Count == 0)
                {
                    if (_categoryDictionary[(string)_categorySpinner.SelectedItem] is UtilityCategory category)
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
                    _parameters.TraceConfiguration.IncludeIsolatedFeatures = _isolatedCheckBox.Checked;
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
                    foreach (FeatureLayer layer in _myMapView.Map.OperationalLayers.OfType<FeatureLayer>())
                    {
                        IEnumerable<UtilityElement> elements = elementTraceResult.Elements.Where(element => element.NetworkSource.Name == layer.FeatureTable.TableName);
                        IEnumerable<Feature> features = await _utilityNetwork.GetFeaturesForElementsAsync(elements);
                        layer.SelectFeatures(features);
                    }
                }
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
            finally
            {
                _loadingBar.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                _loadingBar.Visibility = Android.Views.ViewStates.Visible;

                // Identify the feature to be used.
                IEnumerable<IdentifyLayerResult> identifyResult = await _myMapView.IdentifyLayersAsync(e.Position, 10.0, false);
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
                        element.Terminal = await WaitForTerminal(terminals);
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
                _myMapView.GraphicsOverlays["FilterBarriers"]?.Graphics.Add(traceLocationGraphic);

                // Disable UI not used for filter barriers.
                _categorySpinner.Enabled = _isolatedCheckBox.Enabled = false;
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
            finally
            {
                _loadingBar.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private async Task<UtilityTerminal> WaitForTerminal(IEnumerable<UtilityTerminal> terminals)
        {
            // Create an array of the terminal names for the UI.
            string[] terminalNames = terminals.ToList().Select(t => t.Name).ToArray();

            // Create UI for terminal selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Select terminal for junction");
            builder.SetItems(terminalNames, Choose_Click);
            builder.SetCancelable(false);
            builder.Show();

            // Return the selected terminal.
            _terminalCompletionSource = new TaskCompletionSource<int>();
            string selectedName = terminalNames[await _terminalCompletionSource.Task];
            return terminals.Where(x => x.Name.Equals(selectedName)).FirstOrDefault();
        }

        private void Choose_Click(object sender, DialogClickEventArgs e)
        {
            _terminalCompletionSource.TrySetResult(e.Which);
        }

        private void OnReset(object sender, EventArgs e)
        {
            _parameters.FilterBarriers.Clear();
            _myMapView.GraphicsOverlays["FilterBarriers"]?.Graphics.Clear();

            // Re-enable the UI.
            _categorySpinner.Enabled = _isolatedCheckBox.Enabled = true;
        }

        private void CreateLayout()
        {
            // Load the layout from the axml resource.
            SetContentView(Resource.Layout.PerformValveIsolationTrace);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _traceButton = FindViewById<Button>(Resource.Id.traceButton);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);
            _isolatedCheckBox = FindViewById<CheckBox>(Resource.Id.isolatedCheckBox);
            _categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
            _loadingBar = FindViewById<ProgressBar>(Resource.Id.loadingBar);

            _traceButton.Click += OnTrace;
            _resetButton.Click += OnReset;
            _myMapView.GeoViewTapped += OnGeoViewTapped;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _traceButton.Click -= OnTrace;
            _resetButton.Click -= OnReset;
            _myMapView.GeoViewTapped -= OnGeoViewTapped;
        }
    }
}