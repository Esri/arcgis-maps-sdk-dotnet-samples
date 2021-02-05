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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.PerformValveIsolationTrace
{
    [Register("PerformValveIsolationTrace")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Perform valve isolation trace",
        category: "Utility network",
        description: "Run a filtered trace to locate operable features that will isolate an area from the flow of network resources.",
        instructions: "Tap on one or more features to use as filter barriers or create and set the configuration's filter barriers by selecting a category. Check or uncheck 'Include Isolated Features'. Tap 'Trace' to run a subnetwork-based isolation trace. Tap 'Reset' to clear filter barriers.",
        tags: new[] { "category comparison", "condition barriers", "filter barriers", "isolated features", "network analysis", "subnetwork trace", "trace configuration", "trace filter", "utility network" })]
    public class PerformValveIsolationTrace : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _categoryButton;
        private UISwitch _featuresSwitch;
        private UIBarButtonItem _traceButton;
        private UIBarButtonItem _resetButton;
        private UIActivityIndicatorView _loadingView;
        private UIToolbar _buttonToolbar;

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

        private UtilityCategory _selectedCategory;
        private UtilityTraceParameters _parameters;
        private TaskCompletionSource<string> _terminalCompletionSource = null;
        private SimpleMarkerSymbol _barrierPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.OrangeRed, 25d);

        public PerformValveIsolationTrace()
        {
            Title = "Perform valve isolation trace";
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
                _loadingView.StartAnimating();

                // Disable the UI.
                _categoryButton.Enabled = _traceButton.Enabled = false;

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Create a map with layers in this utility network.
                _myMapView.Map = new Map(BasemapStyle.ArcGISStreetsNight);
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
                _selectedCategory = _utilityNetwork.Definition.Categories.First();
                _categoryButton.Title = _selectedCategory.Name;

                // Enable the UI.
                _categoryButton.Enabled = _traceButton.Enabled = true;
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                _loadingView.StopAnimating();
            }
        }

        private void CategoryClicked(object sender, EventArgs e)
        {
            UIAlertController prompt = UIAlertController.Create("Choose category for filter barrier", null, UIAlertControllerStyle.Alert);

            // Build the choice list for categories populated with the `Name` property of each `UtilityCategory` in the `UtilityNetworkDefinition`.
            foreach (var category in _utilityNetwork.Definition.Categories)
            {
                prompt.AddAction(UIAlertAction.Create(category.Name, UIAlertActionStyle.Default, (a) =>
                {
                    _selectedCategory = category;
                    _categoryButton.Title = category.Name;
                }));
            }
            prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            PresentViewController(prompt, true, null);
        }

        private async void OnTrace(object sender, EventArgs e)
        {
            try
            {
                _loadingView.StartAnimating();

                // Clear previous selection from the layers.
                _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                // Check if any filter barriers have been placed.
                if (_parameters.FilterBarriers.Count == 0)
                {
                    if (_selectedCategory is UtilityCategory category)
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
                    _parameters.TraceConfiguration.IncludeIsolatedFeatures = _featuresSwitch.On;
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
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                _loadingView.StopAnimating();
            }
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                _loadingView.StartAnimating();

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
                _categoryButton.Enabled = _featuresSwitch.Enabled = false;
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                _loadingView.StopAnimating();
            }
        }

        #region TerminalSelection

        private async Task<UtilityTerminal> WaitForTerminal(IEnumerable<UtilityTerminal> terminals)
        {
            try
            {
                // Prevent user from tapping map while selecting terminal.
                _myMapView.GeoViewTapped -= OnGeoViewTapped;

                // Start the UI for the user choosing the junction.
                UIAlertController prompt = UIAlertController.Create(null, "Choose terminal", UIAlertControllerStyle.ActionSheet);

                foreach (UtilityTerminal terminal in terminals)
                {
                    prompt.AddAction(UIAlertAction.Create(terminal.Name, UIAlertActionStyle.Default, Choose_Click));
                }

                // Needed to prevent crash on iPad.
                UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
                if (ppc != null)
                {
                    ppc.SourceView = _buttonToolbar;
                    ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                    ppc.DidDismiss += (s, e) => _terminalCompletionSource.TrySetCanceled();
                }

                PresentViewController(prompt, true, null);

                // Wait for the user to select a terminal.
                _terminalCompletionSource = new TaskCompletionSource<string>();
                string selectedName = await _terminalCompletionSource.Task;
                return terminals.Where(x => x.Name.Equals(selectedName)).FirstOrDefault();
            }
            finally
            {
                // Enable the main UI again.
                _myMapView.GeoViewTapped += OnGeoViewTapped;
            }
        }

        private void Choose_Click(UIAlertAction action)
        {
            _terminalCompletionSource.TrySetResult(action.Title);
        }

        #endregion TerminalSelection

        private void OnReset(object sender, EventArgs e)
        {
            _parameters.FilterBarriers.Clear();
            _myMapView.GraphicsOverlays["FilterBarriers"]?.Graphics.Clear();

            // Disable UI not used for filter barriers.
            _categoryButton.Enabled = _featuresSwitch.Enabled = true;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView() { TranslatesAutoresizingMaskIntoConstraints = false };
            var switchToolbar = new UIToolbar() { TranslatesAutoresizingMaskIntoConstraints = false };
            _buttonToolbar = new UIToolbar() { TranslatesAutoresizingMaskIntoConstraints = false };

            var switchLabel = new UILabel() { TranslatesAutoresizingMaskIntoConstraints = false };
            switchLabel.Text = "Include isolated features";
            _featuresSwitch = new UISwitch() { TranslatesAutoresizingMaskIntoConstraints = false };
            switchToolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem(){ CustomView = switchLabel},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(){ CustomView = _featuresSwitch}
            };

            _categoryButton = new UIBarButtonItem() { Title = "Filter Barrier Category" };
            _traceButton = new UIBarButtonItem() { Title = "Trace" };
            _resetButton = new UIBarButtonItem() { Title = "Reset" };

            _buttonToolbar.Items = new UIBarButtonItem[] {
                _categoryButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _traceButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _resetButton,
            };

            _loadingView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .5f),
                HidesWhenStopped = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, switchToolbar, _buttonToolbar, _loadingView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(switchToolbar.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),

                switchToolbar.TopAnchor.ConstraintEqualTo(_myMapView.BottomAnchor),
                switchToolbar.BottomAnchor.ConstraintEqualTo(_buttonToolbar.TopAnchor),
                switchToolbar.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                switchToolbar.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),

                _buttonToolbar.TopAnchor.ConstraintEqualTo(switchToolbar.BottomAnchor),
                _buttonToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _buttonToolbar.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _buttonToolbar.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),

                _loadingView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _loadingView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _loadingView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _loadingView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _traceButton.Clicked += OnTrace;
            _categoryButton.Clicked += CategoryClicked;
            _resetButton.Clicked += OnReset;
            _myMapView.GeoViewTapped += OnGeoViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _traceButton.Clicked -= OnTrace;
            _categoryButton.Clicked -= CategoryClicked;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}