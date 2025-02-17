// Copyright 2025 Esri.
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
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Esri.ArcGISRuntime.Security;
using System.Diagnostics;
using Esri.ArcGISRuntime.UtilityNetworks;
using Esri.ArcGISRuntime.UI.Editing;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;
using System.Xml.Linq;
using ArcGIS.Samples.Managers;

namespace ArcGIS.WPF.Samples.SnapGeometryWithRules
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Snap geometry with rules",
        "Geometry",
        "Perform snap geometry edits with rules",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("0fd3a39660d54c12b05d5f81f207dffd")]
    public partial class SnapGeometryWithRules : INotifyPropertyChanged
    {
        // Store the local geodatabase to edit.
        private Geodatabase _localGeodatabase;

        private const int LineLayerId = 3;
        private const int JunctionLayerId = 2;
        private const int DeviceLayerId = 0;

        private Feature _selectedFeature;
        private SubtypeFeatureLayer _lastLayer;

        // Viewpoint in the utility network area.
        private Viewpoint _startingViewpoint = new Viewpoint(new MapPoint(-9811055.156028448, 5131792.19502501, SpatialReferences.WebMercator), 1e4);

        // Graphic json
        string graphicJson = "{\"paths\":[[[-9811826.6810284462,5132074.7700250093],[-9811786.4643617794,5132440.9533583419],[-9811384.2976951133,5132354.1700250087],[-9810372.5310284477,5132360.5200250093],[-9810353.4810284469,5132066.3033583425]]],\"spatialReference\":{\"wkid\":102100,\"latestWkid\":3857}}";

        // Utility network objects.
        private UtilityNetwork _utilityNetwork;

        private GeometryEditor _geometryEditor;

        private GraphicsOverlay _graphicsOverlay;

        private GeodatabaseFeatureTable _deviceTable;
        private GeodatabaseFeatureTable _junctionTable;
        private SubtypeFeatureLayer _pipeLayer;
        private SubtypeSublayer _distributionPipeSubtypeSublayer;
        private SubtypeSublayer _servicePipeSubtypeSublayer;

        private Renderer _defaultDistributionRenderer;
        private Renderer _defaultServiceRenderer;
        private Symbol _defaultGraphicsSymbol;

        private UtilityAssetType _junctionAssetType;
        private UtilityAssetType _deviceAssetType;

        List<SnapSourceSettingsVM> _snapSourceSettingsVMs;

        private bool _isFeatureSelected = false;
        public bool IsFeatureSelected 
        { 
            get => _isFeatureSelected; 
            set 
            { 
                _isFeatureSelected = value; 
                OnPropertyChanged(nameof(IsFeatureSelected));
                UpdateUI();
            } 
        }

        private void UpdateUI()
        {
            if (IsFeatureSelected)
            {
                AssetTypeComboBox.Visibility = Visibility.Collapsed;
                SelectedAssetTypeGrid.Visibility = Visibility.Visible;
                SelectedFeatureLabel.Visibility = Visibility.Visible;
            }
            else
            {
                AssetTypeComboBox.Visibility = Visibility.Visible;
                SelectedAssetTypeGrid.Visibility = Visibility.Collapsed;
                SelectedFeatureLabel.Visibility = Visibility.Collapsed;
            }
        }

        public SnapGeometryWithRules()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                MyMapView.Loaded += (s, e) =>
                {
                    // Create a map.
                    MyMapView.Map = new Map(BasemapStyle.ArcGISStreetsNight)
                    {
                        InitialViewpoint = _startingViewpoint,
                    };

                    MyMapView.Map.LoadSettings.FeatureTilingMode = FeatureTilingMode.EnabledWithFullResolutionWhenSupported;
                };

                // When the spatial reference changes (the map loads) add the local geodatabase tables as feature layers.
                MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;

                Unloaded += SampleUnloaded;

                // Create a graphics overlay and add it to the map view.
                _graphicsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

                var geometry = Geometry.FromJson(graphicJson);
                var graphic = new Graphic(geometry, new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Gray, 3));
                _graphicsOverlay.Graphics.Add(graphic);

                _defaultGraphicsSymbol = graphic.Symbol;

                // Create and add a geometry editor to the map view.
                _geometryEditor = new GeometryEditor();
                MyMapView.GeometryEditor = _geometryEditor;

                // Show the UI.
                SnappingControls.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            _localGeodatabase.RollbackTransaction();
            _localGeodatabase?.Close();
        }

        private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
        {
            // Call a function (and await it) to get the local geodatabase (or generate it from the feature service).
            await GetLocalGeodatabase();

            // Once the local geodatabase is available, load the tables as layers to the map.
            _ = LoadLocalGeodatabaseTables();

            MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;
        }

        private async Task GetLocalGeodatabase()
        {
            // Get the path to the local geodatabase for this platform (temp directory, for example).
            string localGeodatabasePath = DataManager.GetDataFolder("0fd3a39660d54c12b05d5f81f207dffd", "NapervilleGasUtilities.geodatabase");

            try
            {
                // See if the geodatabase file is already present.
                if (File.Exists(localGeodatabasePath))
                {
                    // If the geodatabase is already available, open it, hide the progress control, and update the message.
                    _localGeodatabase = await Geodatabase.OpenAsync(localGeodatabasePath);

                    if (!_localGeodatabase.IsInTransaction)
                    {
                        _localGeodatabase.BeginTransaction();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task LoadLocalGeodatabaseTables()
        {
            var pipeTable = _localGeodatabase.GeodatabaseFeatureTables[LineLayerId];
            await pipeTable.LoadAsync();

            _pipeLayer = new SubtypeFeatureLayer(pipeTable);
            MyMapView.Map.OperationalLayers.Add(_pipeLayer);

            _deviceTable = _localGeodatabase.GeodatabaseFeatureTables[DeviceLayerId];
            await _deviceTable.LoadAsync();

            SubtypeFeatureLayer devicelayer = new SubtypeFeatureLayer(_deviceTable);
            MyMapView.Map.OperationalLayers.Add(devicelayer);

            _junctionTable = _localGeodatabase.GeodatabaseFeatureTables[JunctionLayerId];
            await _junctionTable.LoadAsync();

            SubtypeFeatureLayer junctionlayer = new SubtypeFeatureLayer(_junctionTable);
            MyMapView.Map.OperationalLayers.Add(junctionlayer);

            var assemblyTable = _localGeodatabase.GeodatabaseFeatureTables[1];
            await assemblyTable.LoadAsync();

            FeatureLayer flayer = new FeatureLayer(assemblyTable);
            MyMapView.Map.OperationalLayers.Add(flayer);

            // Create and load the utility network.
            _utilityNetwork = _localGeodatabase.UtilityNetworks[0];
            MyMapView.Map.UtilityNetworks.Add(_utilityNetwork);
            await _utilityNetwork.LoadAsync();

            var deviceSource = _utilityNetwork.Definition.GetNetworkSource("Pipeline Device");
            var deviceAssetGroup = deviceSource.AssetGroups[1];
            _deviceAssetType = deviceAssetGroup.GetAssetType("Automatic Reset");

            var junctionSource = _utilityNetwork.Definition.GetNetworkSource("Pipeline Junction");
            var junctionAssetGroup = junctionSource.AssetGroups[7];
            _junctionAssetType = junctionAssetGroup.GetAssetType("Plastic 3-Way");

            // Load the map.
            await MyMapView.Map.LoadAsync();

            // Ensure all layers are loaded before setting the snap settings.
            // If this is not awaited there is a risk that operational layers may not have loaded and therefore would not have been included in the snap sources.
            await Task.WhenAll(MyMapView.Map.OperationalLayers.ToList().Select(layer => layer.LoadAsync()).ToList());

            foreach (var sublayer in _pipeLayer.SubtypeSublayers)
            {
                if (sublayer.Name != "Service Pipe" && sublayer.Name != "Distribution Pipe")
                {
                    sublayer.IsVisible = false;
                }
            }

            foreach (var sublayer in devicelayer.SubtypeSublayers)
            {
                if (sublayer.Name != "Excess Flow Valve" && sublayer.Name != "Controllable Tee")
                {
                    sublayer.IsVisible = false;
                }
            }
        }

        private void SetSnapSettings(SnapRules snapRules = null)
        {
            // Synchronize the snap source collection with the map's operational layers. 
            // Note that layers that have not been loaded will not synchronize.
            if (snapRules == null)
            {
                _geometryEditor.SnapSettings.SyncSourceSettings();
            }
            else
            {
                _geometryEditor.SnapSettings.SyncSourceSettings(snapRules);
            }

            // Enable snapping on the geometry layer.
            _geometryEditor.SnapSettings.IsEnabled = true;
            _geometryEditor.SnapSettings.IsFeatureSnappingEnabled = true;

            var pipeSubtypeFeatureLayer = _geometryEditor.SnapSettings.SourceSettings.FirstOrDefault(sourceSetting => sourceSetting.Source is SubtypeFeatureLayer subtypeFeatureLayer && subtypeFeatureLayer.Name == "PipelineLine");
            pipeSubtypeFeatureLayer.IsEnabled = true;

            // Create a list of snap source settings with snapping disabled.
            _snapSourceSettingsVMs = _geometryEditor.SnapSettings.SourceSettings.Where(sourceSetting => (sourceSetting.Source is SubtypeSublayer subtypeSublayer && (subtypeSublayer.Name == "Service Pipe" || subtypeSublayer.Name == "Distribution Pipe")) || sourceSetting.Source is GraphicsOverlay).Select(sourceSettings => new SnapSourceSettingsVM(sourceSettings) { IsEnabled = true }).ToList();

            SnapSourcesList.ItemsSource = _snapSourceSettingsVMs;
        }

        private void GeometryEditorButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsFeatureSelected)
            {
                var symbol = ((GeodatabaseFeatureTable)_selectedFeature.FeatureTable).LayerInfo?.DrawingInfo?.Renderer?.GetSymbol(_selectedFeature);
                _geometryEditor.Tool.Style.VertexSymbol = _geometryEditor.Tool.Style.FeedbackVertexSymbol = _geometryEditor.Tool.Style.SelectedVertexSymbol = symbol;
                _geometryEditor.Tool.Style.VertexTextSymbol = null;

                _lastLayer.SetFeatureVisible(_selectedFeature, false);

                _geometryEditor.Start(_selectedFeature.Geometry);
            }
            else
            {
                if (AssetTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select an asset type before starting the geometry editor");
                    return;
                }

                string featureType = string.Empty;

                switch (AssetTypeComboBox.Text)
                {
                    case "Automatic Reset":
                        featureType = "Excess Flow Valve";
                        break;
                    case "Plastic 3-Way":
                        featureType = "Controllable Tee";
                        break;
                }

                var feature = _deviceTable.CreateFeature(_deviceTable.FeatureTypes.First(f => f.Name == featureType));
                var symbol = _deviceTable.LayerInfo?.DrawingInfo?.Renderer?.GetSymbol(feature);
                _geometryEditor.Tool.Style.VertexSymbol = _geometryEditor.Tool.Style.FeedbackVertexSymbol = _geometryEditor.Tool.Style.SelectedVertexSymbol = symbol;
                _geometryEditor.Tool.Style.VertexTextSymbol = null;

                _geometryEditor.Start(geometryType: GeometryType.Point);
            }
        }

        private async void AssetTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            if (_distributionPipeSubtypeSublayer == null || _servicePipeSubtypeSublayer == null)
            {
                _distributionPipeSubtypeSublayer = _pipeLayer.SubtypeSublayers.FirstOrDefault(sl => sl.Name == "Distribution Pipe");
                _servicePipeSubtypeSublayer = _pipeLayer.SubtypeSublayers.FirstOrDefault(sl => sl.Name == "Service Pipe");
                _defaultDistributionRenderer = _distributionPipeSubtypeSublayer.Renderer;
                _defaultServiceRenderer = _servicePipeSubtypeSublayer.Renderer;
            }

            switch (AssetTypeComboBox.SelectedIndex)
            {
                case 0:
                    var snapRules = await SnapRules.CreateSnapRulesAsync(_utilityNetwork, _deviceAssetType);
                    SetSnapSettings(snapRules);
                    UpdateSymbology();
                    break;
                case 1:
                    snapRules = await SnapRules.CreateSnapRulesAsync(_utilityNetwork, _junctionAssetType);
                    SetSnapSettings(snapRules);
                    UpdateSymbology();
                    break;
            }
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.Stop();
            ResetSelections();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var geometry = _geometryEditor.Stop();

            if (IsFeatureSelected)
            {
                _selectedFeature.Geometry = geometry;
                await ((GeodatabaseFeatureTable)_selectedFeature.FeatureTable).UpdateFeatureAsync(_selectedFeature);
            }
            else
            {
                switch (AssetTypeComboBox.SelectedIndex)
                {
                    case 0:
                        var feature = _deviceTable.CreateFeature(_deviceTable.FeatureTypes.First(f => f.Name == "Excess Flow Valve"));
                        feature.SetAttributeValue("ASSETGROUP", 1);
                        feature.SetAttributeValue("ASSETTYPE", (Int16)1);
                        feature.Geometry = geometry;
                        await _deviceTable.AddFeatureAsync(feature);
                        var element = _utilityNetwork.CreateElement(feature);
                        break;
                    case 1:
                        var feature2 = _junctionTable.CreateFeature(_junctionTable.FeatureTypes.First(f => f.Name == "Tee"));



                        var networkSource = _utilityNetwork.Definition?.GetNetworkSource("Pipeline Junction");

                        var subtypeName = feature2.GetFeatureSubtype()?.Name;
                        var assetGroup = networkSource?.GetAssetGroup(subtypeName);

                        var assetTypeCode = (Int16)feature2.Attributes["ASSETTYPE"];
                        var assetType = assetGroup?.AssetTypes?.FirstOrDefault(t => t.Code == (Int16)1);


                        feature2.SetAttributeValue("ASSETGROUP", 7);
                        feature2.SetAttributeValue("ASSETTYPE", (Int16)243);
                        feature2.Geometry = geometry;
                        await _junctionTable.AddFeatureAsync(feature2);
                    break;
                }
            }

            ResetSelections();
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (_geometryEditor.IsStarted) return;
            
            var identifyResult = await MyMapView.IdentifyLayersAsync(e.Position, 5, false);
            ArcGISFeature feature = identifyResult?.FirstOrDefault()?.GeoElements?.FirstOrDefault() as ArcGISFeature;

            if (feature == null)
            {
                feature = identifyResult?.FirstOrDefault()?.SublayerResults?.FirstOrDefault()?.GeoElements?.FirstOrDefault() as ArcGISFeature;
            }

            if (feature == null || feature.FeatureTable.GeometryType != GeometryType.Point)
            {
                ResetSelections();
                return;
            }
            else if (_selectedFeature != null && feature != _selectedFeature)
            {
                ResetSelections();
            }

            var element = _utilityNetwork?.CreateElement(feature);
            SelectedAssetGroupLabel.Text = element?.AssetGroup.Name;
            SelectedAssetTypeLabel.Text = element?.AssetType.Name;
            _selectedFeature = feature;
            _lastLayer = identifyResult?.First().LayerContent as SubtypeFeatureLayer;
            _lastLayer?.SelectFeature(feature);
            IsFeatureSelected = true;

            SnapRules snapRules = await SnapRules.CreateSnapRulesAsync(_utilityNetwork, element?.AssetType);

            SetSnapSettings(snapRules);
            UpdateSymbology();

        }

        private void UpdateSymbology()
        {
            if (_distributionPipeSubtypeSublayer == null || _servicePipeSubtypeSublayer == null)
            {
                _distributionPipeSubtypeSublayer = _pipeLayer.SubtypeSublayers.FirstOrDefault(sl => sl.Name == "Distribution Pipe");
                _servicePipeSubtypeSublayer = _pipeLayer.SubtypeSublayers.FirstOrDefault(sl => sl.Name == "Service Pipe");
                _defaultDistributionRenderer = _distributionPipeSubtypeSublayer.Renderer;
                _defaultServiceRenderer = _servicePipeSubtypeSublayer.Renderer;
            }

            foreach (var snapSourceSettingsVM in _snapSourceSettingsVMs)
            {
                if (snapSourceSettingsVM.SnapSourceSettings.RulePolicy == SnapRulePolicy.None)
                {
                    if (snapSourceSettingsVM.SnapSourceSettings.Source is GraphicsOverlay graphicsOverlay)
                    {
                        graphicsOverlay.Graphics[0].Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Green, 3);
                    }
                    else if (snapSourceSettingsVM.SnapSourceSettings.Source is SubtypeSublayer subtypeSublayer)
                    {
                        subtypeSublayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Green, 3));
                    }
                }
                else if (snapSourceSettingsVM.SnapSourceSettings.RulePolicy == SnapRulePolicy.Conditional)
                {
                    if (snapSourceSettingsVM.SnapSourceSettings.Source is GraphicsOverlay graphicsOverlay)
                    {
                        graphicsOverlay.Graphics[0].Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Orange, 3);
                    }
                    else if (snapSourceSettingsVM.SnapSourceSettings.Source is SubtypeSublayer subtypeSublayer)
                    {
                        subtypeSublayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Orange, 3));
                    }
                }
                else
                {
                    if (snapSourceSettingsVM.SnapSourceSettings.Source is GraphicsOverlay graphicsOverlay)
                    {
                        graphicsOverlay.Graphics[0].Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3);
                    }
                    else if (snapSourceSettingsVM.SnapSourceSettings.Source is SubtypeSublayer subtypeSublayer)
                    {
                        subtypeSublayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3));
                    }
                }
            }

            return;
        }

        private void ResetSelections()
        {
            foreach (var layer in MyMapView.Map.OperationalLayers)
            {
                if (layer is SubtypeFeatureLayer subtypeLayer)
                {
                    subtypeLayer.ClearSelection();
                    if (_selectedFeature != null && subtypeLayer.FeatureTable == _selectedFeature.FeatureTable)
                    {
                        subtypeLayer.SetFeatureVisible(_selectedFeature, true);
                    }
                }
            }

            _selectedFeature = null;
            IsFeatureSelected = false;
            AssetTypeComboBox.SelectedIndex = -1;
            _distributionPipeSubtypeSublayer.Renderer = _defaultDistributionRenderer;
            _servicePipeSubtypeSublayer.Renderer = _defaultServiceRenderer;
            _graphicsOverlay.Graphics[0].Symbol = _defaultGraphicsSymbol;
            SnapSourcesList.ItemsSource = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class SnapSourceSettingsVM : INotifyPropertyChanged
    {
        public SnapSourceSettings SnapSourceSettings { get; set; }

        // Wrap the snap source settings in a view model to expose them to the UI.
        public SnapSourceSettingsVM(SnapSourceSettings snapSourceSettings)
        {
            SnapSourceSettings = snapSourceSettings;

            if (snapSourceSettings.Source is SubtypeFeatureLayer subTypeFeatureLayer)
            {
                _name = subTypeFeatureLayer.Name;
            }
            else if (snapSourceSettings.Source is SubtypeSublayer subtypeSublayer)
            {
                _name = subtypeSublayer.Name;
            }
            else if (snapSourceSettings.Source is GraphicsOverlay)
            {
                _name = "Graphics overlay";
            }

            switch (snapSourceSettings.RulePolicy)
            {
                case SnapRulePolicy.None:
                    _fillColor = new SolidColorBrush(System.Windows.Media.Colors.Green);
                    break;
                case SnapRulePolicy.Conditional:
                    _fillColor = new SolidColorBrush(System.Windows.Media.Colors.Orange);
                    break;
                case SnapRulePolicy.Forbidden:
                    _fillColor = new SolidColorBrush(System.Windows.Media.Colors.Red);
                    break;
            }

            IsEnabled = snapSourceSettings.IsEnabled;
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                SnapSourceSettings.IsEnabled = value;
                OnPropertyChanged();
            }
        }

        private SolidColorBrush _fillColor;
        public SolidColorBrush FillColor
        {
            get
            {
                return _fillColor;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
