// Copyright 2019 Esri.
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
using System;
using System.Linq;
using System.Threading.Tasks;


namespace ArcGISRuntimeMaui.Samples.UpdateAttributes
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Update attributes (feature service)",
        category: "Data",
        description: "Update feature attributes in an online feature service.",
        instructions: "To change the feature's damage property, tap the feature to select it, and update the damage type using the drop down.",
        tags: new[] { "amend", "attribute", "details", "edit", "editing", "information", "value" })]
    public partial class UpdateAttributes : ContentPage
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Name of the field that will be updated.
        private const string AttributeFieldName = "typdamage";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        // Hold a reference to the selected feature.
        private ArcGISFeature _selectedFeature;

        public UpdateAttributes()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create the map with streets basemap.
                MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

                // Create a service geodatabase from the feature service.
                ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(new Uri(FeatureServiceUrl));
                await serviceGeodatabase.LoadAsync();

                // Gets the feature table from the service geodatabase, referring to the Damage Assessment feature service.
                // Creating a feature table from the feature service will cause the service geodatbase to be null.
                ServiceFeatureTable damageTable = serviceGeodatabase.GetTable(0);

                // When the table loads, use it to discover the domain of the typdamage field.
                damageTable.Loaded += DamageTable_Loaded;

                // Create a feature layer to visualize the features in the table.
                _damageLayer = new FeatureLayer(damageTable);

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(_damageLayer);

                // Listen for user taps on the map - this will select the feature.
                MyMapView.GeoViewTapped += MapView_Tapped;

                // Zoom to the United States.
                _ = MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void DamageTable_Loaded(object sender, EventArgs e)
        {
            // This code needs to work with the UI, so it needs to run on the UI thread.
            Device.BeginInvokeOnMainThread(() =>
            {
                // Get the relevant field from the table.
                ServiceFeatureTable table = (ServiceFeatureTable)sender;
                Field typeDamageField = table.Fields.First(field => field.Name == AttributeFieldName);

                // Get the domain for the field.
                CodedValueDomain attributeDomain = (CodedValueDomain)typeDamageField.Domain;

                // Update the combobox with the attribute values.
                DamageTypePicker.ItemsSource = attributeDomain.CodedValues.Select(codedValue => codedValue.Name).ToList();
            });
        }

        private async void MapView_Tapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Dismiss any existing callouts.
            MyMapView.DismissCallout();

            // Reset the picker.
            DamageTypePicker.IsEnabled = false;
            DamageTypePicker.SelectedIndex = -1;

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 8, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Get the tapped feature.
                _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);

                // Update the UI for the selection.
                UpdateUiForSelectedFeature();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error selecting feature.", ex.ToString(), "OK");
            }
        }

        private void UpdateUiForSelectedFeature()
        {
            // Get the current value.
            string currentAttributeValue = _selectedFeature.Attributes[AttributeFieldName].ToString();

            // Update the combobox selection without triggering the event.
            DamageTypePicker.SelectedIndexChanged -= DamageType_Changed;
            DamageTypePicker.SelectedItem = currentAttributeValue;
            DamageTypePicker.SelectedIndexChanged += DamageType_Changed;

            // Enable the combobox.
            DamageTypePicker.IsEnabled = true;
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

                await Application.Current.MainPage.DisplayAlert("Success!", $"Edited feature {_selectedFeature.Attributes["objectid"]}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Failed to edit feature", ex.ToString(), "OK");
            }
            finally
            {
                // Clear the selection.
                _damageLayer.ClearSelection();
                _selectedFeature = null;

                // Update the UI.
                DamageTypePicker.IsEnabled = false;
                DamageTypePicker.SelectedIndex = -1;
            }
        }
    }
}