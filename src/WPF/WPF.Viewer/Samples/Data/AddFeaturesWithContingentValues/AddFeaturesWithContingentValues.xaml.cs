// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.AddFeaturesWithContingentValues
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Add features with contingent values",
        category: "Data",
        description: "Create and add features whose attribute values satisfy a predefined set of contingencies.",
        instructions: "Tap on the map to add a feature symbolizing a bird's nest. Then choose values describing the nest's status, protection, and buffer size. Notice how different values are available depending on the values of preceding fields. Once the contingent values are validated, tap \"Done\" to add the feature to the map.",
        tags: new[] { "coded values", "contingent values", "feature table", "geodatabase" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("e12b54ea799f4606a2712157cf9f6e41", "b5106355f1634b8996e634c04b6a930a")]
    public partial class AddFeaturesWithContingentValues
    {
        // The coded value domains in this sample are hardcoded for simplicity, but can be retrieved from the GeodatabaseFeatureTable's Field's Domains.
        private readonly Dictionary<string, string> _statusValues = new Dictionary<string, string>() { { "Occupied", "OCCUPIED" }, { "Unoccupied", "UNOCCUPIED" } };
        private readonly Dictionary<string, string> _protectionValues = new Dictionary<string, string>() { { "Endangered", "ENDANGERED" }, { "Not endangered", "NOT_ENDANGERED" }, { "N/A", "NA" } };

        // Hold references for use in event handlers.
        private GeodatabaseFeatureTable _geodatabaseFeatureTable;
        private GraphicsOverlay _graphicsOverlay;
        private ArcGISFeature _newFeature;

        public AddFeaturesWithContingentValues()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            #region Basemap

            // Get the full path for the vector tile package.
            string vectorTilePackagePath = DataManager.GetDataFolder("b5106355f1634b8996e634c04b6a930a", "FillmoreTopographicMap.vtpk");

            // Open the vector tile package.
            ArcGISVectorTiledLayer fillmoreVectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(vectorTilePackagePath));

            // Load the basemap from the ArcGISVectorTiledLayer.
            Basemap fillmoreBasemap = new Basemap(fillmoreVectorTiledLayer);

            // Add the base map to the map view.
            MyMapView.Map = new Map(fillmoreBasemap);

            #endregion Basemap

            #region FeatureLayer

            // Get the full path for the mobile geodatabase.
            string geodatabasePath = DataManager.GetDataFolder("e12b54ea799f4606a2712157cf9f6e41", "ContingentValuesBirdNests.geodatabase");

            // Load the geodatabase.
            Geodatabase geodatabase = await Geodatabase.OpenAsync(geodatabasePath);

            // Load the Geodatabase, GeodatabaseFeatureTable and the ContingentValuesDefinition.
            // Get the 'BirdNests' geodatabase feature table from the mobile geodatabase.
            _geodatabaseFeatureTable = geodatabase.GetGeodatabaseFeatureTable("BirdNests");

            // Asynchronously load the 'BirdNests' geodatabase feature table.
            await _geodatabaseFeatureTable.LoadAsync();

            // Asynchronously load the contingent values definition.
            await _geodatabaseFeatureTable.ContingentValuesDefinition.LoadAsync();

            // Create a FeatureLayer based on the GeoDatabaseFeatureTable.
            FeatureLayer nestLayer = new FeatureLayer(_geodatabaseFeatureTable);

            // Add the FeatureLayer to the OperationalLayers.
            MyMapView.Map.OperationalLayers.Add(nestLayer);

            #endregion FeatureLayer

            #region GraphicsOverlay

            // Create the GraphicsOverlay with which to display the nest buffer exclusion areas.
            _graphicsOverlay = new GraphicsOverlay();
            Symbol bufferSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, System.Drawing.Color.Red, new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 2));
            _graphicsOverlay.Renderer = new SimpleRenderer(bufferSymbol);
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Query all the features that have a buffer size greater than zero and render a graphic that depicts the buffer.
            await QueryAndBufferFeatures();

            #endregion GraphicsOverlay

            #region Initialize UI components

            // Zoom the map to the extent of the FeatureLayer.
            await MyMapView.SetViewpointGeometryAsync(nestLayer.FullExtent, 50);

            StatusCombo.ItemsSource = _statusValues.Keys;
            FeatureAttributesPanel.Visibility = Visibility.Hidden;

            #endregion Initialize UI components
        }

        #region AddFeature

        private async Task QueryAndBufferFeatures()
        {
            if (_geodatabaseFeatureTable == null || _geodatabaseFeatureTable.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded) return;

            try
            {
                // Clear the existing buffer graphics.
                _graphicsOverlay.Graphics.Clear();

                // Get all the features that have buffer size greater than zero.
                QueryParameters parameters = new QueryParameters();
                parameters.WhereClause = "BufferSize > 0";
                FeatureQueryResult results = await _geodatabaseFeatureTable.QueryFeaturesAsync(parameters);

                // Add a buffer graphic for each feature based on the above query.
                foreach (Feature feature in results.ToList())
                {
                    double bufferDistance = Convert.ToDouble(feature.GetAttributeValue("BufferSize"));
                    Geometry buffer = feature.Geometry.Buffer(bufferDistance);
                    MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(buffer));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        private async Task CreateNewEmptyFeature(GeoViewInputEventArgs e)
        {
            try
            {
                // Create a new empty feature to define attributes for.
                _newFeature = (ArcGISFeature)_geodatabaseFeatureTable.CreateFeature();

                // Get the normalized geometry for the tapped location and use it as the feature's geometry.
                MapPoint tappedPoint = (MapPoint)e.Location.NormalizeCentralMeridian();
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private List<string> GetContingentValues(string field, string fieldGroupName)
        {
            // Create an empty list with which to return the valid contingent values.
            List<string> contingentValuesNamesList = new List<string>();

            if (_geodatabaseFeatureTable.ContingentValuesDefinition.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded) return contingentValuesNamesList;

            try
            {
                // Instantiate a dictionary containing all possible values for a field in the context of the contingent field groups it participates in.
                ContingentValuesResult contingentValuesResult = _geodatabaseFeatureTable.GetContingentValues(_newFeature, field);

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
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }

            return contingentValuesNamesList;
        }

        private bool ValidateContingentValues(out List<string> fieldGroupNames, out int numberOfViolations)
        {
            fieldGroupNames = new List<string>();
            numberOfViolations = 0;

            if (_geodatabaseFeatureTable.ContingentValuesDefinition.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded || _newFeature == null) return false;

            IReadOnlyList<ContingencyConstraintViolation> contingencyConstraintViolations = _geodatabaseFeatureTable.ValidateContingencyConstraints(_newFeature);
            numberOfViolations = contingencyConstraintViolations.Count;

            // If the number of contingency constraint violations is zero the attribute map satisfies all contingencies.
            if (numberOfViolations.Equals(0))
            {
                return true;
            }
            else
            {
                foreach (ContingencyConstraintViolation violation in contingencyConstraintViolations)
                {
                    fieldGroupNames.Add(violation.FieldGroup.Name);
                }
            }

            return false;
        }

        private async Task CreateNewNest()
        {
            // Once the attribute map is filled and validated, save the feature to the geodatabase feature table.
            await _geodatabaseFeatureTable.UpdateFeatureAsync(_newFeature);

            _ = QueryAndBufferFeatures();

            _newFeature = null;
        }

        private async Task DiscardFeature()
        {
            try
            {
                // Delete the newly created feature from the geodatabase feature table.
                await _geodatabaseFeatureTable.DeleteFeatureAsync(_newFeature);

                _newFeature = null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        private void UpdateField(string field, object value)
        {
            try
            {
                // Update the feature's appropriate attribute based on a given field name and a value.
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

        #endregion AddFeature

        #region EventHandlers

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // If a new feature is currently being created do not create another one.
            if (_newFeature == null)
            {
                _ = CreateNewEmptyFeature(e);
            }
        }

        private void StatusCombo_Selected(object sender, RoutedEventArgs e)
        {
            // Update the feature's attribute map with the selection.
            UpdateField("Status", StatusCombo.SelectedItem);

            List<string> protectionItems = GetContingentValues("Protection", "ProtectionFieldGroup");

            // Get the appropriate contingent values for populating the protection combo component.
            if (protectionItems.Any())
            {
                ProtectionCombo.SelectionChanged -= ProtectionCombo_Selected;
                ProtectionCombo.ItemsSource = protectionItems;
                ProtectionCombo.SelectionChanged += ProtectionCombo_Selected;
            }
        }

        private void ProtectionCombo_Selected(object sender, RoutedEventArgs e)
        {
            // Update the feature's attribute map with the selection.
            UpdateField("Protection", ProtectionCombo.SelectedItem);

            // Get the valid contingent range values for the subsequent buffer size slider.
            List<string> minMax = GetContingentValues("BufferSize", "BufferSizeFieldGroup");

            if (minMax[0] != "")
            {
                // Set the minimum and maximum slider values based on the valid contingent value buffer size range.
                BufferSizeSlider.Minimum = int.Parse(minMax[0]);
                BufferSizeSlider.Maximum = int.Parse(minMax[1]);

                BufferSizeSlider.Value = BufferSizeSlider.Minimum;

                // If the max value in the range is 0, set the buffer size to 0.
                if (minMax[1] == "0")
                {
                    // Update the feature's buffer size with the selected value.
                    UpdateField("BufferSize", 0);
                }
            }
        }

        private void BufferSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the feature's buffer size with the selected value.
            UpdateField("BufferSize", BufferSizeSlider.Value);
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            _ = DiscardFeature();

            FeatureAttributesPanel.Visibility = Visibility.Hidden;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> fieldGroupNames = new List<string>();
            int numberOfViolations = 0;

            // If the contingent values are valid, save the data and hide the attribute panel.
            if (ValidateContingentValues(out fieldGroupNames, out numberOfViolations))
            {
                _ = CreateNewNest();

                FeatureAttributesPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBox.Show($"Error saving feature. {numberOfViolations} violation(s) in field group(s) {string.Join(", ", fieldGroupNames)}.", "Error");
            }
        }

        private void FeatureAttributesPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Disable event handlers when the attribute panel is hidden.
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

            // Add the event handlers to the attribute panel.
            StatusCombo.SelectionChanged += StatusCombo_Selected;
            ProtectionCombo.SelectionChanged += ProtectionCombo_Selected;
            BufferSizeSlider.ValueChanged += BufferSizeSlider_ValueChanged;
        }

        #endregion EventHandlers
    }
}