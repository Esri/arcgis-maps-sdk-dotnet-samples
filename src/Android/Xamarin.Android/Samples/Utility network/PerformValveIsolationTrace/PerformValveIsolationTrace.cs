// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.PerformValveIsolationTrace
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Perform valve isolation trace",
        "Utility network",
        "Run a filtered trace to locate operable features that will isolate an area from the flow of network resources.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("PerformValveIsolationTrace.axml")]
    public class PerformValveIsolationTrace : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Button _traceButton;
        private CheckBox _isolatedCheckBox;
        private Spinner _categorySpinner;

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

        private Dictionary<string, UtilityCategory> _categoryDictionary = new Dictionary<string, UtilityCategory>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Perform valve isolation trace";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Disable the UI.
                _traceButton.Enabled = false;

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
                _utilityNetwork.Definition.Categories.ToList().ForEach(cat => _categoryDictionary.Add(cat.Name, cat));
                _categorySpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, _categoryDictionary.Keys.ToList());

                // Enable the UI.
                _traceButton.Enabled = true;
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
        }

        private async void OnTrace(object sender, EventArgs e)
        {
            try
            {
                // Clear previous selection from the layers.
                _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                if (_categoryDictionary[(string)_categorySpinner.SelectedItem] is UtilityCategory category)
                {
                    // NOTE: UtilityNetworkAttributeComparison or UtilityCategoryComparison with Operator.DoesNotExists
                    // can also be used. These conditions can be joined with either UtilityTraceOrCondition or UtilityTraceAndCondition.
                    UtilityCategoryComparison categoryComparison = new UtilityCategoryComparison(category, UtilityCategoryComparisonOperator.Exists);

                    // Add the filter barrier.
                    _configuration.Filter.Barriers = categoryComparison;
                }

                // Set the include isolated features property.
                _configuration.IncludeIsolatedFeatures = _isolatedCheckBox.Checked;

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
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
        }

        private void CreateLayout()
        {
            // Load the layout from the axml resource.
            SetContentView(Resource.Layout.PerformValveIsolationTrace);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _traceButton = FindViewById<Button>(Resource.Id.traceButton);
            _isolatedCheckBox = FindViewById<CheckBox>(Resource.Id.isolatedCheckBox);
            _categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);

            _traceButton.Click += OnTrace;
        }
    }
}