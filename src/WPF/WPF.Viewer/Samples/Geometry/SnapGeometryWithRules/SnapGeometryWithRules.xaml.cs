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
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleGasV6/FeatureServer";

        // Store the local geodatabase to edit.
        private Geodatabase _localGeodatabase;

        // Task to be used for generating the geodatabase.
        private GeodatabaseSyncTask _gdbSyncTask;

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
                AssetTypePickerLabel.Visibility = Visibility.Collapsed;
                SelectedAssetTypeGrid.Visibility = Visibility.Visible;
                SelectedFeatureLabel.Visibility = Visibility.Visible;
            }
            else
            {
                AssetTypeComboBox.Visibility = Visibility.Visible;
                AssetTypePickerLabel.Visibility = Visibility.Visible;
                SelectedAssetTypeGrid.Visibility = Visibility.Collapsed;
                SelectedFeatureLabel.Visibility = Visibility.Collapsed;
            }
        }

        public SnapGeometryWithRules()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
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

                //var gdbPath = $"{Path.GetTempFileName()}.geodatabase";

                //// Create a task for generating a geodatabase (GeodatabaseSyncTask).
                //_gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(FeatureServiceUrl));

                //// Get the default parameters for the generate geodatabase task.
                ////GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(new Envelope(-9812623.60602845, 5130750.79502501, -9809486.70602845, 5132833.59502501, SpatialReference.Create(3857)));
                //GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(new Envelope(-9813333.7476951, 5129691.40335834, -9807059.94769511, 5133857.00335834, SpatialReference.Create(3857)));
                //generateParams.UtilityNetworkSyncMode = UtilityNetworkSyncMode.SyncSystemTables;
                ////Extent = { Envelope[XMin = -9813333.7476951, YMin = 5129691.40335834, XMax = -9807059.94769511, YMax = 5133857.00335834, Wkid = 3857]}

                //// Create a generate geodatabase job.
                //GenerateGeodatabaseJob generateGdbJob = _gdbSyncTask.GenerateGeodatabase(generateParams, gdbPath);

                //generateGdbJob.Start();

                ////Get the result.
                //var result = await generateGdbJob.GetResultAsync();

                // Create and load a service geodatabase that matches utility network.
                //ServiceGeodatabase serviceGeodatabase = await ServiceGeodatabase.CreateAsync(new Uri(FeatureServiceUrl));
                //_pipeLayer = new SubtypeFeatureLayer(serviceGeodatabase.GetTable(LineLayerId));
                //MyMapView.Map.OperationalLayers.Add(_pipeLayer);

                //_deviceTable = _localGeodatabase.GeodatabaseFeatureTables[DeviceLayerId];
                //SubtypeFeatureLayer devicelayer = new SubtypeFeatureLayer(_deviceTable);
                //MyMapView.Map.OperationalLayers.Add(devicelayer);

                //_junctionTable = _localGeodatabase.GeodatabaseFeatureTables[JunctionLayerId];
                //SubtypeFeatureLayer junctionlayer = new SubtypeFeatureLayer(_junctionTable);
                //MyMapView.Map.OperationalLayers.Add(junctionlayer);

                //// Create and load the utility network.
                //_utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl), MyMapView.Map);

                // Create a graphics overlay and add it to the map view.
                _graphicsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

                var geometry = Geometry.FromJson(graphicJson);
                var graphic = new Graphic(geometry, new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Gray, 3));
                _graphicsOverlay.Graphics.Add(graphic);

                // Create and add a geometry editor to the map view.
                _geometryEditor = new GeometryEditor();
                MyMapView.GeometryEditor = _geometryEditor;

                // Show the UI.
                SnappingControls.Visibility = Visibility.Visible;

                /* 
                 Rough ideas for data...

                Can connect to only service pipe
                From: Excess Flow Valve + Automatic Reset 
                To: Service Pipe + all...

                Can connect to service pipe and distribution pipe
                From: Tee + Plastic 3-Way
                To: Service Pipe, Distribution Pipe etc...

                Can't connect to anything as it is containment only
                From: Meter Setting + Meter Set
                To: Service Pipe etc...
                 
                 
                 
                 */

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

            // Create and load the utility network.
            _utilityNetwork = _localGeodatabase.UtilityNetworks[0];
            MyMapView.Map.UtilityNetworks.Add(_utilityNetwork);
            await _utilityNetwork.LoadAsync();

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

        private void SetSnapSettings()
        {
            // Synchronize the snap source collection with the map's operational layers. 
            // Note that layers that have not been loaded will not synchronize.
            _geometryEditor.SnapSettings.SyncSourceSettings();

            // Enable snapping on the geometry layer.
            _geometryEditor.SnapSettings.IsEnabled = true;
            _geometryEditor.SnapSettings.IsFeatureSnappingEnabled = true;

            var pipeSubtypeFeatureLayer = _geometryEditor.SnapSettings.SourceSettings.FirstOrDefault(sourceSetting => sourceSetting.Source is SubtypeFeatureLayer subtypeFeatureLayer && subtypeFeatureLayer.Name == "PipelineLine");
            pipeSubtypeFeatureLayer.IsEnabled = true;

            // Create a list of snap source settings with snapping disabled.
            List<SnapSourceSettingsVM> snapSourceSettingsVMs = _geometryEditor.SnapSettings.SourceSettings.Where(sourceSetting => (sourceSetting.Source is SubtypeSublayer subtypeSublayer && (subtypeSublayer.Name == "Service Pipe" || subtypeSublayer.Name == "Distribution Pipe")) || sourceSetting.Source is GraphicsOverlay).Select(sourceSettings => new SnapSourceSettingsVM(sourceSettings) { IsEnabled = true }).ToList();

            SnapSourcesList.ItemsSource = snapSourceSettingsVMs;
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

        private void AssetTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            if (_distributionPipeSubtypeSublayer == null || _servicePipeSubtypeSublayer == null)
            {
                _distributionPipeSubtypeSublayer = _pipeLayer.SubtypeSublayers.FirstOrDefault(sl => sl.Name == "Distribution Pipe");
                _servicePipeSubtypeSublayer = _pipeLayer.SubtypeSublayers.FirstOrDefault(sl => sl.Name == "Service Pipe");
            }

            // Set the snap source settings.
            SetSnapSettings();

            switch (AssetTypeComboBox.SelectedIndex)
            {
                case 0:
                    _distributionPipeSubtypeSublayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 4));
                    _servicePipeSubtypeSublayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Orange, 3));
                    _graphicsOverlay.Graphics[0].Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Green, 3);

                    foreach (var item in _geometryEditor.SnapSettings.SourceSettings)
                    {
                        if (item.Source is SubtypeSublayer subtypeSublayer)
                        {
                            if (subtypeSublayer == _distributionPipeSubtypeSublayer)
                            {
                                item.IsEnabled = false;
                            }
                            else if (subtypeSublayer == _servicePipeSubtypeSublayer)
                            {
                                item.IsEnabled = true;
                            }
                        }
                    }
                    break;
                case 1:
                    _distributionPipeSubtypeSublayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Orange, 4));
                    _servicePipeSubtypeSublayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Orange, 3));
                    _graphicsOverlay.Graphics[0].Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Green, 3);

                    foreach (var item in _geometryEditor.SnapSettings.SourceSettings)
                    {
                        if (item.Source is SubtypeSublayer subtypeSublayer)
                        {
                            if (subtypeSublayer == _distributionPipeSubtypeSublayer)
                            {
                                item.IsEnabled = true;
                            }
                            else if (subtypeSublayer == _servicePipeSubtypeSublayer)
                            {
                                item.IsEnabled = true;
                            }
                        }
                    }
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
            else if (feature != _selectedFeature)
            {
                ResetSelections();
            }

            var element = _utilityNetwork?.CreateElement(feature);
            SelectedAssetTypeLabel.Text = element?.AssetType.Name;
            _selectedFeature = feature;
            _lastLayer = identifyResult?.First().LayerContent as SubtypeFeatureLayer;
            _lastLayer?.SelectFeature(feature);
            IsFeatureSelected = true;
            SetSnapSettings();

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
                Name = subTypeFeatureLayer.Name;
            }
            else if (snapSourceSettings.Source is SubtypeSublayer subtypeSublayer)
            {
                Name = subtypeSublayer.Name;
            }
            else if (snapSourceSettings.Source is GraphicsOverlay)
            {
                Name = "Graphics overlay";
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
            set
            {
                _name = value;
                OnPropertyChanged();
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
