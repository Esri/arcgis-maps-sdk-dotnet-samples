// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.FeatureForms;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Toolkit.Maui;

namespace ArcGIS.Samples.EditFeaturesUsingFeatureForms
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
            name: "Edit features using feature forms",
            category: "Data",
            description: "Display and edit feature attributes using feature forms.",
            instructions: "Tap a feature on the map to open a sheet displaying the feature form. Select form elements in the list and perform edits to update the field values. Tap the submit icon to commit the changes on the web map.",
            tags: new[] { "edits", "feature", "featureforms", "form", "toolkit" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("516e4d6aeb4c495c87c41e11274c767f")]
    public partial class EditFeaturesUsingFeatureForms : ContentPage
    {
        private ArcGISFeature _currentFeature;
        private FeatureForm _featureForm;

        public EditFeaturesUsingFeatureForms()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create the ArcGIS Online portal.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();
                // Get the Naperville water web map item using its ID.
                PortalItem webmapItem = await PortalItem.CreateAsync(portal, "516e4d6aeb4c495c87c41e11274c767f");
                // Create a map from the web map item.
                Map onlineMap = new Map(webmapItem);
                // Display the map in the MapView.
                MyMapView.Map = onlineMap;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
            {
                // Perform identify operation to get the feature
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(MyMapView.Map.OperationalLayers.First(), e.Position, 12, false);
                _currentFeature = identifyResult.GeoElements.OfType<ArcGISFeature>().FirstOrDefault();
                if (_currentFeature != null)
                {
                    // Create a feature form
                    _featureForm = new FeatureForm(_currentFeature);
                    FeatureFormView.FeatureForm = _featureForm;
                    FeatureFormPanel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Check if there are validation errors
                if (_featureForm.ValidationErrors.Any())
                {
                    return;
                }
                // Finish editing
                await _featureForm.FinishEditingAsync();
                // Get the service feature table
                var serviceFeatureTable = (ServiceFeatureTable)_currentFeature.FeatureTable;
                // Get the service geodatabase
                var serviceGeodatabase = serviceFeatureTable.ServiceGeodatabase;
                // Check if the service geodatabase can apply edits
                if (serviceGeodatabase.ServiceInfo.CanUseServiceGeodatabaseApplyEdits)
                {
                    // Apply edits to the service geodatabase
                    await serviceGeodatabase.ApplyEditsAsync();
                }
                else
                {
                    // Apply edits to the service feature table
                    await serviceFeatureTable.ApplyEditsAsync();
                }
                FeatureFormPanel.IsVisible = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            FeatureFormPanel.IsVisible = false;
        }
    }
}