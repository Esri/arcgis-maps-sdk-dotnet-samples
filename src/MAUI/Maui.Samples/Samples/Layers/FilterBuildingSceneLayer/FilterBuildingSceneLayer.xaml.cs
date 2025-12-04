// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.Samples.FilterBuildingSceneLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Filter building scene layer",
        category: "Layers",
        description: "Explore details of a building scene by using filters and sublayer visibility.",
        instructions: "In the filter controls, select floor and category options to filter what parts of the Building Scene Layer are displayed in the scene. Tap on any of the building features to identify them.",
        tags: new[] { "3D", "building scene layer", "layers" })]
    public partial class FilterBuildingSceneLayer : ContentPage
    {
        // Hold a reference to the building scene layer.
        private BuildingSceneLayer _buildingSceneLayer;

        // Store the list of floors in the building.
        private List<string> _floorList = new List<string>();

        // Track the currently selected feature's sublayer.
        private BuildingComponentSublayer _selectedSublayer;

        public FilterBuildingSceneLayer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a scene from a web scene portal item.
            var sceneUri = new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=b7c387d599a84a50aafaece5ca139d44");
            var scene = new Scene(sceneUri);

            // Load the scene.
            await scene.LoadAsync();

            // Set the scene to the scene view.
            MySceneView.Scene = scene;

            // Get the building scene layer from the scene's operational layers.
            _buildingSceneLayer = scene.OperationalLayers
                .OfType<BuildingSceneLayer>()
                .FirstOrDefault();

            if (_buildingSceneLayer != null)
            {
                // Get the statistics for the building scene layer to retrieve floor information.
                var statistics = await _buildingSceneLayer.FetchStatisticsAsync();

                if (statistics.ContainsKey("BldgLevel"))
                {
                    // Get the floor values and sort them in descending order.
                    var floorStats = statistics["BldgLevel"];
                    _floorList = floorStats.MostFrequentValues.ToList();
                    _floorList.Sort((a, b) => int.Parse(b).CompareTo(int.Parse(a)));
                }

                // Populate the UI with floor options and category controls.
                PopulateFloorPicker();
                PopulateCategoryControls();
            }

            // Listen for taps on the scene view to identify features.
            MySceneView.GeoViewTapped += MySceneView_GeoViewTapped;
        }

        private void PopulateFloorPicker()
        {
            // Add "All" option to show all floors.
            FloorPicker.Items.Add("All");

            // Add each floor to the picker.
            foreach (var floor in _floorList)
            {
                FloorPicker.Items.Add(floor);
            }

            // Set the default selection to "All".
            FloorPicker.SelectedIndex = 0;
        }

        private void FloorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FloorPicker.SelectedIndex < 0) return;

            // Apply the floor filter based on the selected floor.
            ApplyFloorFilter(FloorPicker.Items[FloorPicker.SelectedIndex]);
        }

        private void ApplyFloorFilter(string selectedFloor)
        {
            // If "All" is selected, remove any active filter.
            if (string.IsNullOrEmpty(selectedFloor) || selectedFloor == "All")
            {
                _buildingSceneLayer.ActiveFilter = null;
                return;
            }

            // Create a building filter with two blocks:
            // 1. Show the selected floor in solid mode
            // 2. Show floors below in x-ray mode (semi-transparent)
            var filter = new BuildingFilter(
                "Floor filter",
                "Show selected floor and x-ray filter for lower floors.",
                new[]
                {
                    new BuildingFilterBlock(
                        "solid block",
                        $"BldgLevel = {selectedFloor}",
                        new BuildingSolidFilterMode()
                    ),
                    new BuildingFilterBlock(
                        "xray block",
                        $"BldgLevel < {selectedFloor}",
                        new BuildingXrayFilterMode()
                    )
                });

            // Apply the filter to the building scene layer.
            _buildingSceneLayer.ActiveFilter = filter;
        }

        private void PopulateCategoryControls()
        {
            // Clear any existing items.
            CategoriesStackLayout.Children.Clear();

            if (_buildingSceneLayer == null) return;

            // Get the "Full Model" sublayer group which contains all categories.
            var fullModelSublayer = _buildingSceneLayer.Sublayers
                .OfType<BuildingGroupSublayer>()
                .FirstOrDefault(s => s.Name == "Full Model");

            if (fullModelSublayer == null) return;

            // Create UI for each category.
            foreach (BuildingGroupSublayer categorySublayer in fullModelSublayer.Sublayers)
            {
                var categoryItem = CreateCategoryItem(categorySublayer);
                CategoriesStackLayout.Children.Add(categoryItem);
            }
        }

        private VerticalStackLayout CreateCategoryItem(BuildingGroupSublayer categorySublayer)
        {
            var mainStack = new VerticalStackLayout { Spacing = 4, Margin = new Thickness(0, 4) };

            // Category header with expand button, name, and checkbox.
            var headerGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };

            var expandButton = new Button
            {
                Text = "▶",
                FontSize = 10,
                WidthRequest = 30,
                HeightRequest = 30,
                Padding = 0,
                BackgroundColor = Colors.Transparent
            };

            var nameLabel = new Label
            {
                Text = categorySublayer.Name,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };

            var visibilityCheckBox = new CheckBox
            {
                IsChecked = categorySublayer.IsVisible,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 24,
                HeightRequest = 24
            };

            visibilityCheckBox.CheckedChanged += (s, e) => categorySublayer.IsVisible = e.Value;

            Grid.SetColumn(expandButton, 0);
            Grid.SetColumn(nameLabel, 1);
            Grid.SetColumn(visibilityCheckBox, 2);

            headerGrid.Add(expandButton);
            headerGrid.Add(nameLabel);
            headerGrid.Add(visibilityCheckBox);

            mainStack.Children.Add(headerGrid);

            var componentsStack = new VerticalStackLayout
            {
                Margin = new Thickness(20, 5, 0, 0),
                Spacing = 2,
                IsVisible = false 
            };

            foreach (BuildingComponentSublayer componentSublayer in categorySublayer.Sublayers)
            {
                var componentGrid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    }
                };

                var componentLabel = new Label
                {
                    Text = componentSublayer.Name,
                    FontSize = 12,
                    VerticalOptions = LayoutOptions.Center
                };
                var componentCheckBox = new CheckBox
                {
                    IsChecked = componentSublayer.IsVisible,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 24,
                    HeightRequest = 24
                };

                componentCheckBox.CheckedChanged += (s, e) => componentSublayer.IsVisible = e.Value;

                componentGrid.Add(componentLabel, 0);
                componentGrid.Add(componentCheckBox, 1);

                componentsStack.Children.Add(componentGrid);
            }

            mainStack.Children.Add(componentsStack);

            // Handle expand/collapse.
            expandButton.Clicked += (s, e) =>
            {
                componentsStack.IsVisible = !componentsStack.IsVisible;
                expandButton.Text = componentsStack.IsVisible ? "▼" : "▶";
            };

            return mainStack;
        }

        private async void MySceneView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Clear the previous selection if one exists.
            if (_selectedSublayer != null)
            {
                _selectedSublayer.ClearSelection();
                _selectedSublayer = null;
            }

            // Hide the feature panel and show settings.
            FeaturePanel.IsVisible = false;
            SettingsPanel.IsVisible = true;

            if (_buildingSceneLayer == null) return;

            // Identify features at the tapped location.
            var identifyResult = await MySceneView.IdentifyLayerAsync(
                _buildingSceneLayer,
                e.Position,
                5,
                false);

            // Check if any features were identified.
            if (identifyResult.SublayerResults.Any())
            {
                var sublayerResult = identifyResult.SublayerResults.First();

                if (sublayerResult.GeoElements.Any())
                {
                    // Get the first identified feature and its sublayer.
                    var identifiedFeature = sublayerResult.GeoElements.First() as Feature;
                    var sublayer = sublayerResult.LayerContent as BuildingComponentSublayer;

                    if (identifiedFeature != null && sublayer != null)
                    {
                        // Select the feature and store the sublayer reference.
                        sublayer.SelectFeature(identifiedFeature);
                        _selectedSublayer = sublayer;

                        // Display the feature's attributes.
                        DisplayFeatureAttributes(identifiedFeature);
                    }
                }
            }
        }

        private void DisplayFeatureAttributes(Feature feature)
        {
            // Create a list of key-value pairs from the feature's attributes.
            var attributesList = new List<KeyValuePair<string, string>>();
            foreach (var attribute in feature.Attributes)
            {
                var value = attribute.Value?.ToString() ?? "N/A";
                attributesList.Add(new KeyValuePair<string, string>(attribute.Key, value));
            }

            // Bind the attributes to the collection view and show the panel.
            FeatureAttributesCollection.ItemsSource = attributesList;
            SettingsPanel.IsVisible = false;
            FeaturePanel.IsVisible = true;
        }

        private void CloseFeatureButton_Clicked(object sender, EventArgs e)
        {
            if (_selectedSublayer != null)
            {
                _selectedSublayer.ClearSelection();
                _selectedSublayer = null;
            }

            FeaturePanel.IsVisible = false;
            FeatureAttributesCollection.ItemsSource = null;
            SettingsPanel.IsVisible = true;
        }
    }
}