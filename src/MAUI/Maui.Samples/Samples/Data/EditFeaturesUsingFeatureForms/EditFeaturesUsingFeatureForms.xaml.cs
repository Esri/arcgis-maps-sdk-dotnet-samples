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

namespace ArcGIS.Samples.EditFeaturesUsingFeatureForms
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Edit features using feature forms",
        category: "Data",
        description: "Display and edit feature attributes using feature forms.",
        instructions: "Tap a feature on the map to open a sheet displaying the feature form. Select form elements in the list and perform edits to update the field values. Tap the submit icon to commit the changes on the web map.",
        tags: new[] { "edits", "feature", "featureforms", "form", "toolkit" })]
    public partial class EditFeaturesUsingFeatureForms : ContentPage
    {
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
                var portal = await ArcGISPortal.CreateAsync();

                // Get the Naperville water web map item using its ID.
                var webmapItem = await PortalItem.CreateAsync(portal, "516e4d6aeb4c495c87c41e11274c767f");

                // Create a map from the web map item.
                var onlineMap = new Map(webmapItem);

                // Display the map in the MapView.
                MyMapView.Map = onlineMap;
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
            {
                // Perform identify operation to get the feature.
                var identifyResult = await MyMapView.IdentifyLayersAsync(e.Position, 12, false);
                var feature = identifyResult.SelectMany(result => result.GeoElements).OfType<ArcGISFeature>().FirstOrDefault();

                if (feature != null)
                {
                    // Create a feature form.
                    _featureForm = new FeatureForm(feature);

                    // Add the feature form view to the scroll viewer.
                    FeatureFormViewPanel.FeatureForm = _featureForm;

                    // Show the feature form panel.
                    FeatureFormPanel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Check if there are validation errors.
                if (_featureForm.ValidationErrors.Any())
                {
                    var errors = _featureForm.ValidationErrors;
                    var errorMessages = errors.SelectMany(kvp => kvp.Value.Select(ex => $"{kvp.Key}: {ex.Message}"));
                    string errorMessage = string.Join("\n", errorMessages);
                    throw new Exception($"Validation errors exist.\n{errorMessage}");
                }

                // Finish editing.
                await _featureForm.FinishEditingAsync();

                // Get the service feature table.
                var serviceFeatureTable = (ServiceFeatureTable)_featureForm.Feature.FeatureTable;

                // Get the service geodatabase.
                var serviceGeodatabase = serviceFeatureTable.ServiceGeodatabase;

                // Check if the service geodatabase can apply edits
                if (serviceGeodatabase.ServiceInfo.CanUseServiceGeodatabaseApplyEdits)
                {
                    // Apply edits to the service geodatabase.
                    await serviceGeodatabase.ApplyEditsAsync();
                }
                else
                {
                    // Apply edits to the service feature table.
                    await serviceFeatureTable.ApplyEditsAsync();
                }
                FeatureFormPanel.IsVisible = false;
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            // Cancel editing.
            _featureForm.DiscardEdits();

            // Hide the feature form panel.
            FeatureFormPanel.IsVisible = false;
        }
    }
}