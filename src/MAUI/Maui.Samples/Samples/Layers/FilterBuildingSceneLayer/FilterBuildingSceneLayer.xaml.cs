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

        // Track the currently selected feature's sublayer for clearing selection.
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
                // Load the building scene layer to access sublayers and statistics.
                await _buildingSceneLayer.LoadAsync();

                // Get the statistics to retrieve floor information.
                var statistics = await _buildingSceneLayer.FetchStatisticsAsync();

                if (statistics.ContainsKey("BldgLevel"))
                {
                    // Get the floor values and sort them in descending order (top floor first).
                    var floorStats = statistics["BldgLevel"];
                    _floorList = floorStats.MostFrequentValues.ToList();
                    _floorList.Sort((a, b) => int.Parse(b).CompareTo(int.Parse(a)));
                }

                // Populate the UI controls.
                PopulateFloorPicker();
                PopulateCategoryControls();

                // Listen for taps on the scene view to identify features.
                MySceneView.GeoViewTapped += MySceneView_GeoViewTapped;
            }
        }

        private void PopulateFloorPicker()
        {
            // Add "All" option followed by each floor.
            FloorPicker.Items.Add("All");
            foreach (var floor in _floorList)
            {
                FloorPicker.Items.Add(floor);
            }

            FloorPicker.SelectedIndex = 0;
        }

        private void FloorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FloorPicker.SelectedIndex < 0) return;

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
            // 1. Solid mode for the selected floor.
            // 2. X-ray mode for floors below (semi-transparent).
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

            _buildingSceneLayer.ActiveFilter = filter;
        }

        private void PopulateCategoryControls()
        {
            CategoriesStackLayout.Children.Clear();

            if (_buildingSceneLayer == null) return;

            // Get the "Full Model" sublayer group which contains all categories.
            var fullModelSublayer = _buildingSceneLayer.Sublayers
                .OfType<BuildingGroupSublayer>()
                .FirstOrDefault(s => s.Name == "Full Model");

            if (fullModelSublayer == null) return;

            // Create expandable UI for each category (e.g., Architectural, Structural, Electrical).
            foreach (BuildingGroupSublayer categorySublayer in fullModelSublayer.Sublayers)
            {
                CategoriesStackLayout.Children.Add(CreateCategoryItem(categorySublayer));
            }
        }

        private View CreateCategoryItem(BuildingGroupSublayer categorySublayer)
        {
            var mainStack = new VerticalStackLayout { Spacing = 4, Margin = new Thickness(0, 4) };

            // Category header with expand button, name, and visibility checkbox.
            var headerLayout = new HorizontalStackLayout { Spacing = 8 };

            var expandButton = new Button
            {
                Text = ">",
                FontSize = 12,
                WidthRequest = 30,
                HeightRequest = 30,
                Padding = 0,
                BackgroundColor = Colors.Transparent
            };

            var categoryCheckBox = new CheckBox { IsChecked = categorySublayer.IsVisible };
            categoryCheckBox.CheckedChanged += (s, e) => categorySublayer.IsVisible = e.Value;

            var nameLabel = new Label
            {
                Text = categorySublayer.Name,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };

            headerLayout.Add(expandButton);
            headerLayout.Add(categoryCheckBox);
            headerLayout.Add(nameLabel);

            mainStack.Children.Add(headerLayout);

            // Component sublayers (collapsed by default).
            var componentsStack = new VerticalStackLayout
            {
                Margin = new Thickness(40, 4, 0, 0),
                Spacing = 4,
                IsVisible = false
            };

            foreach (BuildingComponentSublayer componentSublayer in categorySublayer.Sublayers)
            {
                var componentLayout = new HorizontalStackLayout { Spacing = 8 };

                var componentCheckBox = new CheckBox { IsChecked = componentSublayer.IsVisible };
                componentCheckBox.CheckedChanged += (s, e) => componentSublayer.IsVisible = e.Value;

                var componentLabel = new Label
                {
                    Text = componentSublayer.Name,
                    VerticalOptions = LayoutOptions.Center
                };

                componentLayout.Add(componentCheckBox);
                componentLayout.Add(componentLabel);
                componentsStack.Children.Add(componentLayout);
            }

            mainStack.Children.Add(componentsStack);

            // Toggle expand/collapse when button is clicked.
            expandButton.Clicked += (s, e) =>
            {
                componentsStack.IsVisible = !componentsStack.IsVisible;
                expandButton.Text = componentsStack.IsVisible ? "v" : ">";
            };

            return mainStack;
        }

        private async void MySceneView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Clear any previous selection.
            if (_selectedSublayer != null)
            {
                _selectedSublayer.ClearSelection();
                _selectedSublayer = null;
            }

            // Reset panel visibility.
            FeaturePanel.IsVisible = false;
            SettingsPanel.IsVisible = true;

            if (_buildingSceneLayer == null) return;

            // Identify features at the tapped location.
            var identifyResult = await MySceneView.IdentifyLayerAsync(
                _buildingSceneLayer,
                e.Position,
                5,
                false);

            // Process the first identified feature.
            if (identifyResult.SublayerResults.Any())
            {
                var sublayerResult = identifyResult.SublayerResults.First();

                if (sublayerResult.GeoElements.Any())
                {
                    var identifiedFeature = sublayerResult.GeoElements.First() as Feature;
                    var sublayer = sublayerResult.LayerContent as BuildingComponentSublayer;

                    if (identifiedFeature != null && sublayer != null)
                    {
                        // Select the feature and display its attributes.
                        sublayer.SelectFeature(identifiedFeature);
                        _selectedSublayer = sublayer;
                        DisplayFeatureAttributes(identifiedFeature);
                    }
                }
            }
        }

        private void DisplayFeatureAttributes(Feature feature)
        {
            FeatureAttributesCollection.ItemsSource = feature.Attributes
                .Select(a => new KeyValuePair<string, string>(a.Key, a.Value?.ToString() ?? "N/A"))
                .ToList();

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