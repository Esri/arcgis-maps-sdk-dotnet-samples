// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.AddFeaturesWithContingentValues
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add features with contingent values",
        category: "Data",
        description: "Create and add features whose attribute values satisfy a predefined set of contingencies.",
        instructions: "Tap on the map to add a feature symbolizing a bird's nest. Then choose values describing the nest's status, protection, and buffer size. Notice how different values are available depending on the values of preceding fields. Once the contingent values are validated, tap \"Done\" to add the feature to the map.",
        tags: new[] { "coded values", "contingent values", "feature table", "geodatabase" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e12b54ea799f4606a2712157cf9f6e41", "b5106355f1634b8996e634c04b6a930a")]
    public partial class AddFeaturesWithContingentValues
    {
        private GeodatabaseFeatureTable _geodatabaseFeatureTable;
        private GraphicsOverlay _graphicsOverlay;
        private ArcGISFeature _newFeature;

        // The coded value domains in this sample are hardcoded for simplicity, but can be retrieved from the GeodatabaseFeatureTable's Field's Domains.
        private readonly Dictionary<string, string> _statusValues = new Dictionary<string, string>() { { "Occupied", "OCCUPIED" }, { "Unoccupied", "UNOCCUPIED" } };
        private readonly Dictionary<string, string> _protectionValues = new Dictionary<string, string>() { { "Endangered", "ENDANGERED" }, { "Not endangered", "NOT_ENDANGERED" }, { "N/A", "NA" } };

        public AddFeaturesWithContingentValues()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Get the full path for the vector tile package.
            string vectorTilePackagePath = DataManager.GetDataFolder("b5106355f1634b8996e634c04b6a930a", "FillmoreTopographicMap.vtpk");

            // Open the vector tile package.
            ArcGISVectorTiledLayer fillmoreVectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(vectorTilePackagePath));

            // Load the basemap from the ArcGISVectorTiledLayer.
            Basemap fillmoreBasemap = new Basemap(fillmoreVectorTiledLayer);

            // Add the base map to the map view.
            MyMapView.Map = new Map(fillmoreBasemap);

            // Get the full path for the mobile geodatabase.
            string geodatabasePath = DataManager.GetDataFolder("e12b54ea799f4606a2712157cf9f6e41", "ContingentValuesBirdNests.geodatabase");

            // Load the geodatabase.
            Geodatabase geodatabase = await Geodatabase.OpenAsync(geodatabasePath);

            // Set the initial viewpoint.
            //MyMapView.SetViewpoint(new Viewpoint(-13236000, 4081200, 8822));

            // Create the graphics overlay with which to display the nest buffer exclusion areas.
            _graphicsOverlay = new GraphicsOverlay();
            Symbol bufferSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, System.Drawing.Color.Red, new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 2));
            _graphicsOverlay.Renderer = new SimpleRenderer(bufferSymbol);
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Load the Geodatabase, GeodatabaseFeatureTable and the ContingentValuesDefinition.
            // Get the 'Trailheads' geodatabase feature table from the mobile geodatabase.
            _geodatabaseFeatureTable = geodatabase.GetGeodatabaseFeatureTable("BirdNests");

            // Asynchronously load the 'Trailheads' geodatabase feature table.
            await _geodatabaseFeatureTable.LoadAsync();
            await _geodatabaseFeatureTable.ContingentValuesDefinition.LoadAsync();

            // Create a FeatureLayer based on the geodatabase feature table.
            FeatureLayer nestLayer = new FeatureLayer(_geodatabaseFeatureTable);

            // Add the FeatureLayer to the OperationalLayers.
            MyMapView.Map.OperationalLayers.Add(nestLayer);

            // Zoom the map to the extent of the FeatureLayer.
            await MyMapView.SetViewpointGeometryAsync(nestLayer.FullExtent, 50);

            await QueryAndBufferFeatures();

            StatusCombo.ItemsSource = _statusValues.Keys;
            FeatureAttributesPanel.Visibility = Visibility.Hidden;
            SaveButton.IsEnabled = false;
        }

        private async Task QueryAndBufferFeatures()
        {
            if (_geodatabaseFeatureTable == null || _geodatabaseFeatureTable.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded) return;

            _graphicsOverlay.Graphics.Clear();

            QueryParameters parameters = new QueryParameters();
            parameters.WhereClause = "BufferSize > 0";
            FeatureQueryResult results = await _geodatabaseFeatureTable.QueryFeaturesAsync(parameters);
            BufferFeaturesFromQueryResult(results);
        }

        private void BufferFeaturesFromQueryResult(FeatureQueryResult results)
        {
            foreach (Feature feature in results.ToList())
            {
                double bufferDistance = Convert.ToDouble(feature.GetAttributeValue("BufferSize"));
                Geometry buffer = GeometryEngine.Buffer(feature.Geometry, bufferDistance);
                MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(buffer));
            }
        }

        private void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                _ = CreateNewEmptyFeature(e);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task CreateNewEmptyFeature(GeoViewInputEventArgs e)
        {
            // Create a new empty feature to define attributes for.
            _newFeature = (ArcGISFeature)_geodatabaseFeatureTable.CreateFeature();

            // Get the normalized geometry for the tapped location and use it as the feature's geometry.
            MapPoint tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);
            _newFeature.Geometry = tappedPoint;

            // Add the feature to the table.
            await _geodatabaseFeatureTable.AddFeatureAsync(_newFeature);

            // Update the feature to get the updated objectid - a temporary ID is used before the feature is added.
            _newFeature.Refresh();

            if (!FeatureAttributesPanel.IsVisible)
            {
                FeatureAttributesPanel.Visibility = Visibility.Visible;
            }
        }

        private List<string> GetContingentValues(string field, string fieldGroupName)
        {
            // Create an empty list with which to return the valid contingent values.
            List<string> contingentValuesNamesList = new List<string>();

            if (_geodatabaseFeatureTable.ContingentValuesDefinition.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded) return contingentValuesNamesList;

            // Instantiate a dictionary containing all possible values for a field in the context of the contingent field groups it participates in.
            var contingentValuesResult = _geodatabaseFeatureTable.GetContingentValues(_newFeature, field);

            // Loop through the contingent values.
            foreach (ContingentValue contingentValue in contingentValuesResult.ContingentValuesByFieldGroup[fieldGroupName])
            {
                // Contingent coded values are contingent values defined from a coded value domain.
                // There are often multiple results returned by the ContingentValuesResult.
                if (contingentValue is ContingentCodedValue contingentCodedValue)
                {
                    contingentValuesNamesList.Add(contingentCodedValue.CodedValue.Name);
                }
                else if (contingentValue is ContingentRangeValue contingentRangeValue)
                {
                    contingentValuesNamesList.Add(contingentRangeValue.MinValue.ToString());
                    contingentValuesNamesList.Add(contingentRangeValue.MaxValue.ToString());
                }
            }

            return contingentValuesNamesList;
        }

        private bool ValidateContingentValues()
        {
            if (_geodatabaseFeatureTable.ContingentValuesDefinition.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded || _newFeature == null) return false;

            var issues = _geodatabaseFeatureTable.ValidateContingencyConstraints(_newFeature);
            // If the list of contingency constraints is empty there are no violations and the attribute map satisfies all contingencies.
            if (issues.Count == 0)
            {
                return true;
            }

            return false;
        }

        private void CreateNewNest()
        {
            // Once the attribute map is filled and validated, save the feature to the geodatabase feature table.
            _ = _geodatabaseFeatureTable.UpdateFeatureAsync(_newFeature);

            _ = QueryAndBufferFeatures();
        }

        private async Task DiscardFeature()
        {
            await _geodatabaseFeatureTable.DeleteFeatureAsync(_newFeature);
        }

        private void UpdateField(string field, object value)
        {
            try
            {
                switch (field)
                {
                    case "Status":
                        _newFeature.SetAttributeValue(field, _statusValues[value.ToString()]);
                        break;

                    case "Protection":
                        _newFeature.SetAttributeValue(field, _protectionValues[value.ToString()]);
                        break;

                    case "BufferSize":
                        _newFeature.SetAttributeValue(field, Convert.ToInt32(value));
                        break;

                    default:
                        MessageBox.Show($"{field} not found in any of the data dictionaries.");
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        private void StatusCombo_Selected(object sender, RoutedEventArgs e)
        {
            ProtectionCombo.IsEnabled = StatusCombo.SelectedIndex != -1;

            UpdateField("Status", StatusCombo.SelectedItem);

            if (GetContingentValues("Protection", "ProtectionFieldGroup").Any())
            {
                List<string> newList = new List<string>();
                newList.AddRange(GetContingentValues("Protection", "ProtectionFieldGroup"));

                ProtectionCombo.SelectionChanged -= ProtectionCombo_Selected;
                ProtectionCombo.ItemsSource = newList;
                ProtectionCombo.SelectionChanged += ProtectionCombo_Selected;
            }
        }

        private void ProtectionCombo_Selected(object sender, RoutedEventArgs e)
        {
            BufferSizeSlider.IsEnabled = ProtectionCombo.SelectedIndex != -1;

            // Update the feature's attribute map with the selection.
            UpdateField("Protection", ProtectionCombo.SelectedItem);

            // Get the valid contingent range values for the subsequent buffer size slider.
            var minMax = GetContingentValues("BufferSize", "BufferSizeFieldGroup");

            if (minMax[0] != "")
            {
                BufferSizeSlider.Minimum = int.Parse(minMax[0]);
                BufferSizeSlider.Maximum = int.Parse(minMax[1]);

                // If the max value in the range is 0, set the buffer size to 0.
                if (minMax[1] == "0")
                {
                    UpdateField("BufferSize", 0);
                    SaveButton.IsEnabled = ValidateContingentValues();
                }
            }
        }

        private void BufferSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SaveButton.IsEnabled = ValidateContingentValues();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            _ = DiscardFeature();

            FeatureAttributesPanel.Visibility = Visibility.Hidden;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateContingentValues())
            {
                CreateNewNest();
                FeatureAttributesPanel.Visibility = Visibility.Hidden;
            }
        }

        private void FeatureAttributesPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!FeatureAttributesPanel.IsVisible)
            {
                StatusCombo.SelectionChanged -= StatusCombo_Selected;
                ProtectionCombo.SelectionChanged -= ProtectionCombo_Selected;
                BufferSizeSlider.ValueChanged -= BufferSizeSlider_ValueChanged;

                return;
            }
            // Reset attribute panel values when the panel opens.
            StatusCombo.SelectedIndex = -1;
            ProtectionCombo.SelectedIndex = -1;
            BufferSizeSlider.Value = BufferSizeSlider.Minimum;

            StatusCombo.SelectionChanged += StatusCombo_Selected;
            ProtectionCombo.SelectionChanged += ProtectionCombo_Selected;
            BufferSizeSlider.ValueChanged += BufferSizeSlider_ValueChanged;
        }
    }
}