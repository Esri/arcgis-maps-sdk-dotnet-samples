﻿// Copyright 2019 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.UpdateAttributes
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Update attributes (feature service)",
        category: "Data",
        description: "Update feature attributes in an online feature service.",
        instructions: "To change the feature's damage property, tap the feature to select it, and update the damage type using the drop down.",
        tags: new[] { "attribute", "coded value", "coded value domain", "domain", "editing", "value" })]
    public partial class UpdateAttributes
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
            Initialize();
        }

        private void Initialize()
        {
            // Create the map with streets basemap.
            MyMapView.Map = new Map(Basemap.CreateStreets());

            // Create the feature table, referring to the Damage Assessment feature service.
            ServiceFeatureTable damageTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // When the table loads, use it to discover the domain of the typdamage field.
            damageTable.Loaded += DamageTable_Loaded;

            // Create a feature layer to visualize the features in the table.
            _damageLayer = new FeatureLayer(damageTable);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(_damageLayer);

            // Listen for user taps on the map - this will select the feature.
            MyMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private async void DamageTable_Loaded(object sender, EventArgs e)
        {
            // This code needs to work with the UI, so it needs to run on the UI thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Get the relevant field from the table.
                ServiceFeatureTable table = (ServiceFeatureTable) sender;
                Field typeDamageField = table.Fields.First(field => field.Name == AttributeFieldName);

                // Get the domain for the field.
                CodedValueDomain attributeDomain = (CodedValueDomain) typeDamageField.Domain;

                // Update the combobox with the attribute values.
                DamageTypeDropDown.ItemsSource = attributeDomain.CodedValues.Select(codedValue => codedValue.Name);
            });
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Dismiss any existing callouts.
            MyMapView.DismissCallout();

            // Reset the dropdown.
            DamageTypeDropDown.IsEnabled = false;
            DamageTypeDropDown.SelectedIndex = -1;

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 2, false);

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
                await new MessageDialog(ex.ToString(), "Error selecting feature.").ShowAsync();
            }
        }

        private void UpdateUiForSelectedFeature()
        {
            // Get the current value.
            string currentAttributeValue = _selectedFeature.Attributes[AttributeFieldName].ToString();

            // Update the combobox selection without triggering the event.
            DamageTypeDropDown.SelectionChanged -= DamageType_Changed;
            DamageTypeDropDown.SelectedValue = currentAttributeValue;
            DamageTypeDropDown.SelectionChanged += DamageType_Changed;

            // Enable the combobox.
            DamageTypeDropDown.IsEnabled = true;
        }

        private async void DamageType_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Skip if nothing is selected.
            if (DamageTypeDropDown.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                // Get the new value.
                string selectedAttributeValue = DamageTypeDropDown.SelectedValue.ToString();

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the attribute value.
                _selectedFeature.SetAttributeValue(AttributeFieldName, selectedAttributeValue);

                // Update the table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Update the service.
                ServiceFeatureTable table = (ServiceFeatureTable) _selectedFeature.FeatureTable;
                await table.ApplyEditsAsync();

                await new MessageDialog($"Edited feature {_selectedFeature.Attributes["objectid"]}", "Success!").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString(), "Failed to edit feature.").ShowAsync();
            }
            finally
            {
                // Clear the selection.
                _damageLayer.ClearSelection();
                _selectedFeature = null;

                // Update the UI.
                DamageTypeDropDown.IsEnabled = false;
                DamageTypeDropDown.SelectedIndex = -1;
            }
        }
    }
}
