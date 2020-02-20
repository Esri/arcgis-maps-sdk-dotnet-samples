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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.PerformValveIsolationTrace
{
    [Register("PerformValveIsolationTrace")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Perform valve isolation trace",
        "Utility network",
        "Run a filtered trace to locate operable features that will isolate an area from the flow of network resources.",
        "")]
    public class PerformValveIsolationTrace : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _categoryButton;
        private UISwitch _featuresSwitch;
        private UIBarButtonItem _traceButton;

        // Feature service for an electric utility network in Naperville, Illinois.
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleGas/FeatureServer";
        private const int LineLayerId = 3;
        private const int DeviceLayerId = 0;
        private UtilityNetwork _utilityNetwork;

        // For creating the default trace configuration.
        private const string DomainNetworkName = "Pipeline";
        private const string TierName = "Pipe Distribution System";
        private UtilityTraceConfiguration _configuration;

        // For creating the default starting location.
        private const string NetworkSourceName = "Gas Device";
        private const string AssetGroupName = "Meter";
        private const string AssetTypeName = "Customer";
        private const string GlobalId = "{98A06E95-70BE-43E7-91B7-E34C9D3CB9FF}";
        private UtilityElement _startingLocation;

        private UtilityCategory _selectedCategory;

        public PerformValveIsolationTrace()
        {
            Title = "Perform valve isolation trace";
        }

        private async void Initialize()
        {
            try
            {
                // Disable the UI.
                _categoryButton.Enabled = _traceButton.Enabled = false;

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl));

                // Create a map with layers in this utility network.
                _myMapView.Map = new Map(Basemap.CreateStreetsNightVector());
                _myMapView.Map.OperationalLayers.Add(new FeatureLayer(new Uri($"{FeatureServiceUrl}/{LineLayerId}")));
                _myMapView.Map.OperationalLayers.Add(new FeatureLayer(new Uri($"{FeatureServiceUrl}/{DeviceLayerId}")));

                // Get a trace configuration from a tier.
                UtilityDomainNetwork domainNetwork = _utilityNetwork.Definition.GetDomainNetwork(DomainNetworkName) ?? throw new ArgumentException(DomainNetworkName);
                UtilityTier tier = domainNetwork.GetTier(TierName) ?? throw new ArgumentException(TierName);
                _configuration = tier.TraceConfiguration;

                // Create a trace filter.
                _configuration.Filter = new UtilityTraceFilter();

                // Get a default starting location.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(NetworkSourceName) ?? throw new ArgumentException(NetworkSourceName);
                UtilityAssetGroup assetGroup = networkSource.GetAssetGroup(AssetGroupName) ?? throw new ArgumentException(AssetGroupName);
                UtilityAssetType assetType = assetGroup.GetAssetType(AssetTypeName) ?? throw new ArgumentException(AssetTypeName);
                Guid globalId = Guid.Parse(GlobalId);
                _startingLocation = _utilityNetwork.CreateElement(assetType, globalId);

                // Create a graphics overlay.
                GraphicsOverlay overlay = new GraphicsOverlay();
                _myMapView.GraphicsOverlays.Add(overlay);
                Symbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.LimeGreen, 25d);

                // Display starting location.
                IEnumerable<ArcGISFeature> elementFeatures = await _utilityNetwork.GetFeaturesForElementsAsync(new List<UtilityElement> { _startingLocation });
                MapPoint startingLocationGeometry = elementFeatures.FirstOrDefault().Geometry as MapPoint;
                Graphic graphic = new Graphic(startingLocationGeometry, symbol);
                overlay.Graphics.Add(graphic);

                // Set the starting viewpoint.
                await _myMapView.SetViewpointAsync(new Viewpoint(startingLocationGeometry, 3000));

                // Build the choice list for categories populated with the `Name` property of each `UtilityCategory` in the `UtilityNetworkDefinition`.
                //CategoryPicker.ItemsSource = _utilityNetwork.Definition.Categories.ToList();

                // Enable the UI.
                _categoryButton.Enabled = _traceButton.Enabled = true;
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
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
                // Clear previous selection from the layers.
                _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                if (_selectedCategory != null)
                {
                    // NOTE: UtilityNetworkAttributeComparison or UtilityCategoryComparison with Operator.DoesNotExists
                    // can also be used. These conditions can be joined with either UtilityTraceOrCondition or UtilityTraceAndCondition.
                    UtilityCategoryComparison categoryComparison = new UtilityCategoryComparison(_selectedCategory, UtilityCategoryComparisonOperator.Exists);

                    // Add the filter barrier.
                    _configuration.Filter.Barriers = categoryComparison;
                }

                // Set the include isolated features property.
                _configuration.IncludeIsolatedFeatures = _featuresSwitch.On;

                // Build parameters for isolation trace.
                UtilityTraceParameters parameters = new UtilityTraceParameters(UtilityTraceType.Isolation, new[] { _startingLocation });
                parameters.TraceConfiguration = _configuration;

                // Get the trace result from trace.
                IEnumerable<UtilityTraceResult> traceResult = await _utilityNetwork.TraceAsync(parameters);
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
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView() { TranslatesAutoresizingMaskIntoConstraints = false };
            var switchToolbar = new UIToolbar() { TranslatesAutoresizingMaskIntoConstraints = false };
            var buttonToolbar = new UIToolbar() { TranslatesAutoresizingMaskIntoConstraints = false };

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

            buttonToolbar.Items = new UIBarButtonItem[] {
                _categoryButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _traceButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, switchToolbar, buttonToolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(switchToolbar.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                switchToolbar.TopAnchor.ConstraintEqualTo(_myMapView.BottomAnchor),
                switchToolbar.BottomAnchor.ConstraintEqualTo(buttonToolbar.TopAnchor),
                switchToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                switchToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                buttonToolbar.TopAnchor.ConstraintEqualTo(switchToolbar.BottomAnchor),
                buttonToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                buttonToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                buttonToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _traceButton.Clicked += OnTrace;
            _categoryButton.Clicked += CategoryClicked;
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