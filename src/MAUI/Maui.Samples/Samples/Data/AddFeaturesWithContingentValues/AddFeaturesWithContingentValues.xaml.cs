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

namespace ArcGISRuntime.Samples.AddFeaturesWithContingentValues
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add features with contingent values",
        category: "Data",
        description: "Create and add features whose attribute values satisfy a predefined set of contingencies.",
        instructions: "Tap on the map to add a feature symbolizing a bird's nest. Then choose values describing the nest's status, protection, and buffer size. Notice how different values are available depending on the values of preceding fields. Once the contingent values are validated, tap \"Done\" to add the feature to the map.",
        tags: new[] { "coded values", "contingent values", "feature table", "geodatabase" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e12b54ea799f4606a2712157cf9f6e41", "b5106355f1634b8996e634c04b6a930a")]
    public partial class AddFeaturesWithContingentValues : ContentPage
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

            StatusPicker.ItemsSource = _statusValues.Keys.ToList();
            FeatureAttributesPopup.IsVisible = false;
            FeatureAttributesPopup.PropertyChanged += FeatureAttributesPopup_PropertyChanged;

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
                    Geometry buffer = GeometryEngine.Buffer(feature.Geometry, bufferDistance);
                    MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(buffer));
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.Message, "OK");
            }
        }

        private async Task CreateNewEmptyFeature(Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
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

                if (!FeatureAttributesPopup.IsVisible)
                {
                    FeatureAttributesPopup.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task<List<string>> GetContingentValues(string field, string fieldGroupName)
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
                await Application.Current.MainPage.DisplayAlert("Error", e.Message, "OK");
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
                await Application.Current.MainPage.DisplayAlert("Error", e.Message, "OK");
            }
        }

        private async Task UpdateField(string field, object value)
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
                        await Application.Current.MainPage.DisplayAlert("Error", $"{field} not found in any of the data dictionaries.", "OK");
                        break;
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.Message, "OK");
            }
        }

        private async Task SetProtectionComboItems()
        {
            List<string> protectionItems = await GetContingentValues("Protection", "ProtectionFieldGroup");

            // Get the appropriate contingent values for populating the protection combo component.
            if (protectionItems.Any())
            {
                ProtectionPicker.SelectedIndexChanged -= ProtectionCombo_SelectedIndexChanged;
                ProtectionPicker.ItemsSource = protectionItems;
                ProtectionPicker.SelectedIndexChanged += ProtectionCombo_SelectedIndexChanged;
            }
        }

        private async Task SetBufferSizeSliderRange()
        {
            // Get the valid contingent range values for the subsequent buffer size slider.
            List<string> minMax = await GetContingentValues("BufferSize", "BufferSizeFieldGroup");

            if (minMax[0] != "")
            {
                // Set the minimum and maximum slider values based on the valid contingent value buffer size range.
                if (minMax[1] == "0")
                {
                    // If the max value in the range is 0, set the buffer size to 0.
                    BufferSizeSlider.Minimum = 0;
                    BufferSizeSlider.Maximum = 1;
                    BufferSizeSlider.IsEnabled = false;

                    // Update the feature's buffer size with the selected value.
                    await UpdateField("BufferSize", 0);
                }
                else if (BufferSizeSlider.Maximum <= double.Parse(minMax[0]))
                {
                    BufferSizeSlider.Maximum = double.Parse(minMax[1]);
                    BufferSizeSlider.Minimum = double.Parse(minMax[0]);
                }
                else
                {
                    BufferSizeSlider.Minimum = double.Parse(minMax[0]);
                    BufferSizeSlider.Maximum = double.Parse(minMax[1]);
                }

                BufferSizeSlider.Value = BufferSizeSlider.Minimum;
            }
        }

        private async Task SaveFeature()
        {
            List<string> fieldGroupNames = new List<string>();
            int numberOfViolations = 0;

            // If the contingent values are valid, save the data and hide the attribute panel.
            if (ValidateContingentValues(out fieldGroupNames, out numberOfViolations))
            {
                _ = CreateNewNest();

                FeatureAttributesPopup.IsVisible = false;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error saving feature. {numberOfViolations} violation(s) in field group(s) {string.Join(", ", fieldGroupNames)}.", "OK");
            }
        }

        private void ClearAndDisableBufferSizeSlider()
        {
            BufferSizeSlider.Minimum = 0;
            BufferSizeSlider.Maximum = 1;
            BufferSizeSlider.Value = BufferSizeSlider.Minimum;
            BufferSizeLabel.Text = BufferSizeSlider.Minimum.ToString();
            BufferSizeSlider.IsEnabled = false;
        }

        #endregion AddFeature

        #region EventHandlers

        private void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // If a new feature is currently being created do not create another one.
            if (_newFeature == null)
            {
                _ = CreateNewEmptyFeature(e);
            }
        }

        private void StatusCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the feature's attribute map with the selection.
            _ = UpdateField("Status", StatusPicker.SelectedItem);
            _ = SetProtectionComboItems();

            if (BufferSizeSlider.IsEnabled)
            {
                ClearAndDisableBufferSizeSlider();
            }
        }

        private void ProtectionCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the feature's attribute map with the selection.
            _ = UpdateField("Protection", ProtectionPicker.SelectedItem);

            BufferSizeSlider.IsEnabled = ProtectionPicker.SelectedIndex != -1;

            if (ProtectionPicker.SelectedIndex == -1)
            {
                BufferSizeSlider.Minimum = 0;
                BufferSizeSlider.Maximum = 1;
                BufferSizeSlider.Value = BufferSizeSlider.Minimum;
            }
            else
            {
                _ = SetBufferSizeSliderRange();
            }
        }

        private void BufferSizeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            BufferSizeSlider.Value = (double)Convert.ToInt32(BufferSizeSlider.Value);

            // Update the feature's buffer size with the selected value.
            _ = UpdateField("BufferSize", BufferSizeSlider.Value);

            BufferSizeLabel.Text = BufferSizeSlider.Value.ToString();
        }

        private void DiscardButton_Clicked(object sender, EventArgs e)
        {
            _ = DiscardFeature();

            FeatureAttributesPopup.IsVisible = false;
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            _ = SaveFeature();
        }

        private void FeatureAttributesPopup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (FeatureAttributesPopup == null || e.PropertyName != "IsVisible")
            {
                return;
            }

            // Disable event handlers when the attribute panel is hidden.
            if (!FeatureAttributesPopup.IsVisible)
            {
                StatusPicker.SelectedIndexChanged -= StatusCombo_SelectedIndexChanged;
                ProtectionPicker.SelectedIndexChanged -= ProtectionCombo_SelectedIndexChanged;
                BufferSizeSlider.ValueChanged -= BufferSizeSlider_ValueChanged;

                return;
            }

            // Reset attribute panel values when the panel opens.
            StatusPicker.SelectedIndex = -1;
            ProtectionPicker.SelectedIndex = -1;
            ClearAndDisableBufferSizeSlider();

            // Add the event handlers to the attribute panel.
            StatusPicker.SelectedIndexChanged += StatusCombo_SelectedIndexChanged;
            ProtectionPicker.SelectedIndexChanged += ProtectionCombo_SelectedIndexChanged;
            BufferSizeSlider.ValueChanged += BufferSizeSlider_ValueChanged;
        }

        #endregion EventHandlers
    }
}