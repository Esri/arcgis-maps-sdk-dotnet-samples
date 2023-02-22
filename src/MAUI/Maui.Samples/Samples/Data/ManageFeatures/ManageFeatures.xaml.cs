// Copyright 2023 Esri.
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
using Map = Esri.ArcGISRuntime.Mapping.Map;
using GeoViewInputEventArgs = Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs;

namespace ArcGIS.Samples.ManageFeatures
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Manage features",
        "Data",
        "Create or delete features, update attributes or geometry.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class ManageFeatures
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature layer
        private FeatureLayer _damageLayer;

        // Hold a reference to the feature table.
        private ServiceFeatureTable _damageFeatureTable;

        // Name of the field that will be updated.
        private const string AttributeFieldName = "typdamage";

        // Hold a reference to the selected feature.
        private ArcGISFeature _selectedFeature;

        private Feature _tappedFeature;

        // Create an items source list for the ComboBox.
        private List<string> _methodList = new List<string> { "Create feature", "Delete feature", "Update attribute", "Update geometry" };
        public ManageFeatures()
        {
            InitializeComponent();
            _ = Initialize();
        }

        public async Task Initialize()
        {
            try
            {
                // Create the map with streets basemap.
                MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

                // Create a service geodatabase from the feature service.
                ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(new Uri(FeatureServiceUrl));
                await serviceGeodatabase.LoadAsync();

                // Gets the feature table from the service geodatabase, referring to the Damage Assessment feature service.
                // Creating the feature table from the feature service will cause the service geodatabase to be null.
                _damageFeatureTable = serviceGeodatabase.GetTable(0);

                //// UPDATE ATTRIBUTES - When the table loads, use it to discover the domain of the typdamage field.
                _damageFeatureTable.Loaded += DamageTable_Loaded;

                // Create a feature layer to visualize the features in the table.
                _damageLayer = new FeatureLayer(_damageFeatureTable);

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(_damageLayer);

                // Zoom to the United States.
                _ = MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);

                // Bind the list of method names to the ComboBox.
                MethodPicker.ItemsSource = _methodList;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void DamageTable_Loaded(object sender, EventArgs e)
        {
            // This code needs to work with the UI, so it needs to run on the UI thread.
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                // Get the relevant field from the table.
                ServiceFeatureTable table = (ServiceFeatureTable)sender;
                Field typeDamageField = table.Fields.First(field => field.Name == AttributeFieldName);

                // Get the domain for the field.
                CodedValueDomain attributeDomain = (CodedValueDomain)typeDamageField.Domain;

                // Update the ComboBox with the attribute values.
                DamageTypePicker.ItemsSource = attributeDomain.CodedValues.Select(codedValue => codedValue.Name).ToList();
            });
        }

        private async void OnComboBoxSelectionChanged(object sender, EventArgs e)
        {
            // Clear all currently hooked GeoViewTapped events.
            MyMapView.GeoViewTapped -= MapView_Tapped_CreateFeature;
            MyMapView.GeoViewTapped -= MapView_Tapped_DeleteFeature;
            MyMapView.GeoViewTapped -= MapView_Tapped_UpdateAttribute;
            MyMapView.GeoViewTapped -= MapView_Tapped_UpdateGeometry;

            // Reset UI elements.
            _damageLayer.ClearSelection();
            _selectedFeature = null;
            DeleteButton.IsVisible = false;
            DamageTypePicker.IsVisible = false;

            // Hold a variable for the Picker's selected value.
            Picker picker = (Picker)sender;

            try
            {
                if (picker.SelectedIndex == 0)
                {
                    // Update the label.
                    InstructionLabel.Text = "Tap on the map to create a new feature.";

                    // Update the currently hooked GeoViewTapped event.
                    MyMapView.GeoViewTapped += MapView_Tapped_CreateFeature;

                }
                else if (picker.SelectedIndex == 1)
                {
                    // Update the label.
                    InstructionLabel.Text = "Tap an existing feature to delete it.";

                    // Update the currently hooked GeoViewTapped event.
                    MyMapView.GeoViewTapped += MapView_Tapped_DeleteFeature;
                }
                else if (picker.SelectedIndex == 2)
                {
                    // Update the label.
                    InstructionLabel.Text = "Tap an existing feature to edit its attribute.";

                    // Update the currently hooked GeoViewTapped event.
                    MyMapView.GeoViewTapped += MapView_Tapped_UpdateAttribute;
                }
                else if (picker.SelectedIndex == 3)
                {
                    // Update the label.
                    InstructionLabel.Text = "Tap an existing feature to select it, tap the map to move it to a new position.";

                    // Update the currently hooked GeoViewTapped event.
                    MyMapView.GeoViewTapped += MapView_Tapped_UpdateGeometry;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "OK");
            }

        }

        private async void MapView_Tapped_CreateFeature(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Create the feature.
                ArcGISFeature feature = (ArcGISFeature)_damageFeatureTable.CreateFeature();

                // Get the normalized geometry for the tapped location and use it as the feature's geometry.
                MapPoint tappedPoint = (MapPoint)e.Location.NormalizeCentralMeridian();
                feature.Geometry = tappedPoint;

                // Set feature attributes.
                feature.SetAttributeValue("typdamage", "Minor");
                feature.SetAttributeValue("primcause", "Earthquake");

                // Add the feature to the table.
                await _damageFeatureTable.AddFeatureAsync(feature);

                // Apply the edits to the service geodatabase.
                await _damageFeatureTable.ServiceGeodatabase.ApplyEditsAsync();

                // Update the feature to get the updated objectid - a temporary ID is used before the feature is added.
                feature.Refresh();

                // Confirm feature addition.
                await Application.Current.MainPage.DisplayAlert("Success", $"Created feature {feature.Attributes["objectid"]}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error adding feature", ex.ToString(), "OK");
            }
        }

        private async void MapView_Tapped_DeleteFeature(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Reconfigure the button.
            DeleteButton.IsVisible = false;
            DeleteButton.Text = "Delete feature";

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 5, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Otherwise, get the ID of the first result.
                long featureId = (long)identifyResult.GeoElements.First().Attributes["objectid"];

                // Get the feature by constructing a query and running it.
                QueryParameters qp = new QueryParameters();
                qp.ObjectIds.Add(featureId);
                FeatureQueryResult queryResult = await _damageLayer.FeatureTable.QueryFeaturesAsync(qp);
                _tappedFeature = queryResult.First();

                // Select the feature.
                _damageLayer.SelectFeature(_tappedFeature);

                // Update the button to allow deleting the feature.
                ConfigureDeletionButton(_tappedFeature);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void ConfigureDeletionButton(Feature tappedFeature)
        {
            // Create a button for deleting the feature.
            DeleteButton.Text = $"Delete incident {tappedFeature.Attributes["objectid"]}";
            DeleteButton.IsVisible = true;
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            // Reconfigure the button.
            DeleteButton.IsVisible = false;
            DeleteButton.Text = "Delete feature";

            // Guard against null.
            if (_tappedFeature == null)
            {
                return;
            }

            try
            {
                // Delete the feature.
                await _damageLayer.FeatureTable.DeleteFeatureAsync(_tappedFeature);

                // Sync the change with the service geodatabase.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_damageLayer.FeatureTable;
                await serviceTable.ServiceGeodatabase.ApplyEditsAsync();

                // Show a message confirming the deletion.
                await Application.Current.MainPage.DisplayAlert("Success", $"Deleted feature with ID {_tappedFeature.Attributes["objectid"]}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private async void MapView_Tapped_UpdateAttribute(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Reset the picker.
            DamageTypePicker.IsVisible = false;
            DamageTypePicker.SelectedIndex = -1;

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 8, false);

                // Reset the instruction label and return if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    InstructionLabel.Text = "Tap an existing feature to edit its attribute.";
                    return;
                }

                // Get the tapped feature.
                _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);

                // Update instruction label.
                InstructionLabel.Text = "Select damage type:";

                // Update the UI for the selection.
                UpdateUIForSelectedFeature();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error selecting feature", ex.ToString(), "OK");
            }
        }

        private void UpdateUIForSelectedFeature()
        {
            // Get the current value.
            string currentAttributeValue = _selectedFeature.Attributes[AttributeFieldName].ToString();

            // Update the combobox selection without triggering the event.
            DamageTypePicker.SelectedIndexChanged -= DamageType_Changed;
            DamageTypePicker.SelectedItem = currentAttributeValue;
            DamageTypePicker.SelectedIndexChanged += DamageType_Changed;

            // Enable the combobox.
            DamageTypePicker.IsVisible = true;
        }

        private async void DamageType_Changed(object sender, EventArgs e)
        {
            // Skip if nothing is selected.
            if (DamageTypePicker.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                // Get the new value.
                string selectedAttributeValue = DamageTypePicker.SelectedItem.ToString();

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the attribute value.
                _selectedFeature.SetAttributeValue(AttributeFieldName, selectedAttributeValue);

                // Update the table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Update the service geodatabase.
                ServiceFeatureTable table = (ServiceFeatureTable)_selectedFeature.FeatureTable;
                await table.ServiceGeodatabase.ApplyEditsAsync();

                await Application.Current.MainPage.DisplayAlert("Success", $"Edited feature {_selectedFeature.Attributes["objectid"]}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Failed to edit feature", ex.ToString(), "OK");
            }
        }

        private void MapView_Tapped_UpdateGeometry(object sender, GeoViewInputEventArgs e)
        {
            // Select the feature if none selected, move the feature otherwise.
            if (_selectedFeature == null)
            {
                // Select the feature.
                TrySelectFeature(e);
            }
            else
            {
                // Move the feature.
                MoveSelectedFeature(e);
            }
        }

        private async void MoveSelectedFeature(GeoViewInputEventArgs tapEventDetails)
        {
            try
            {
                // Get the MapPoint from the EventArgs for the tap.
                MapPoint destinationPoint = tapEventDetails.Location;

                // Normalize the point - needed when the tapped location is over the international date line.
                destinationPoint = (MapPoint)destinationPoint.NormalizeCentralMeridian();

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the geometry of the selected feature.
                _selectedFeature.Geometry = destinationPoint;

                // Apply the edit to the feature table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Push the update to the service geodatabase.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;
                await serviceTable.ServiceGeodatabase.ApplyEditsAsync();
                await Application.Current.MainPage.DisplayAlert("Success", $"Moved feature {_selectedFeature.Attributes["objectid"]}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error when moving feature", ex.ToString(), "OK");
            }
            finally
            {
                // Reset the selection.
                _damageLayer.ClearSelection();
                _selectedFeature = null;
            }
        }

        private async void TrySelectFeature(GeoViewInputEventArgs tapEventDetails)
        {
            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, tapEventDetails.Position, 10, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Get the tapped feature.
                _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Problem selecting feature", ex.ToString(), "OK");
            }
        }
    }
}