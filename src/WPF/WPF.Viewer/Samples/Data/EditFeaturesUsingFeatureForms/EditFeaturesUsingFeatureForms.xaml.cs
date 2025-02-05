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
using Esri.ArcGISRuntime.Toolkit.UI.Controls;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.EditFeaturesUsingFeatureForms
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
            name: "Edit features using feature forms",
            category: "Data",
            description: "Display and edit feature attributes using feature forms.",
            instructions: "Tap a feature on the map to open a sheet displaying the feature form. Select form elements in the list and perform edits to update the field values. Tap the submit icon to commit the changes on the web map.",
            tags: new[] { "edits", "feature", "featureforms", "form", "toolkit" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("516e4d6aeb4c495c87c41e11274c767f")]
    public partial class EditFeaturesUsingFeatureForms
    {
        public EditFeaturesUsingFeatureForms()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
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

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Perform identify operation to get the feature
                var identifyResult = await MyMapView.IdentifyLayersAsync(e.Position, 12, false);
                var feature = identifyResult.SelectMany(result => result.GeoElements).OfType<ArcGISFeature>().FirstOrDefault();

                if (feature != null)
                {
                    // Create a feature form
                    var featureForm = new FeatureForm(feature);

                    // Create a feature form view
                    var featureFormView = new FeatureFormView
                    {
                        FeatureForm = featureForm,
                        Padding = new Thickness(10),
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    };

                    var saveButton = new Button
                    {
                        Content = "Save",
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Margin = new Thickness(10),
                    };

                    saveButton.Click += async (o, args) =>
                    {
                        // Check if there are validation errors
                        if (featureForm.ValidationErrors.Any())
                        {
                            return;
                        }
                        // Finish editing
                        await featureForm.FinishEditingAsync();
                        // Get the service feature table
                        var serviceFeatureTable = (ServiceFeatureTable)feature.FeatureTable;
                        // Get the service geodatabase
                        var serviceGeodatabase = serviceFeatureTable.ServiceGeodatabase;
                        // Check if the service geodatabase can apply edits
                        if (serviceGeodatabase.ServiceInfo?.CanUseServiceGeodatabaseApplyEdits == true)
                        {
                            // Apply edits to the service geodatabase
                            await serviceGeodatabase.ApplyEditsAsync();
                        }
                        else
                        {
                            // Apply edits to the service feature table
                            await serviceFeatureTable.ApplyEditsAsync();
                        }
                    };

                    // Wrap the stack panel in a scroll viewer
                    var scrollViewer = new ScrollViewer
                    {
                        Content = featureFormView,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    };

                    // Create a grid to hold the scroll viewer and the save button
                    var grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    // Add the scroll viewer and the save button to the grid
                    Grid.SetRow(scrollViewer, 0);
                    Grid.SetRow(saveButton, 1);
                    grid.Children.Add(scrollViewer);
                    grid.Children.Add(saveButton);

                    // Display the grid in a window
                    var featureFormWindow = new Window
                    {
                        Content = grid,
                        Width = 400,
                        Height = 600,
                        Title = "Edit Feature",
                    };

                    featureFormWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}