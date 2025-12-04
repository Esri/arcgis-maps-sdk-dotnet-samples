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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.FilterBuildingSceneLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Filter building scene layer",
        category: "Layers",
        description: "Explore details of a building scene by using filters and sublayer visibility.",
        instructions: "In the filter controls, select floor and category options to filter what parts of the Building Scene Layer are displayed in the scene. Click on any of the building features to identify them.",
        tags: new[] { "3D", "building scene layer", "layers" })]
    public partial class FilterBuildingSceneLayer
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
                // Load the building scene layer.
                await _buildingSceneLayer.LoadAsync();

                // Get the statistics for the building scene layer to retrieve floor information.
                var statistics = await _buildingSceneLayer.FetchStatisticsAsync();

                if (statistics.ContainsKey("BldgLevel"))
                {
                    // Get the floor values and sort them in descending order.
                    var floorStats = statistics["BldgLevel"];
                    _floorList = floorStats.MostFrequentValues.ToList();
                    _floorList.Sort((a, b) => int.Parse(b).CompareTo(int.Parse(a)));
                }

                // Populate the UI with floor options and category tree.
                PopulateFloorComboBox();
                PopulateCategoryTree();
            }

            // Listen for taps on the scene view to identify features.
            MySceneView.GeoViewTapped += MySceneView_GeoViewTapped;
        }

        private void PopulateFloorComboBox()
        {
            // Add "All" option to show all floors.
            FloorComboBox.Items.Add("All");

            // Add each floor to the combo box.
            foreach (var floor in _floorList)
            {
                FloorComboBox.Items.Add(floor);
            }

            // Set the default selection to "All".
            FloorComboBox.SelectedIndex = 0;
        }

        private void FloorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Apply the floor filter based on the selected floor.
            ApplyFloorFilter(FloorComboBox.SelectedItem as string ?? "All");
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

        private void PopulateCategoryTree()
        {
            // Clear any existing items in the tree view.
            CategoriesTreeView.Items.Clear();

            if (_buildingSceneLayer == null) return;

            // Get the "Full Model" sublayer group which contains all categories.
            var fullModelSublayer = _buildingSceneLayer.Sublayers
                .OfType<BuildingGroupSublayer>()
                .FirstOrDefault(s => s.Name == "Full Model");

            if (fullModelSublayer == null) return;

            // Iterate through each category sublayer (e.g., Architectural, Structural, Electrical).
            foreach (BuildingGroupSublayer categorySublayer in fullModelSublayer.Sublayers)
            {
                // Create a tree view item for the category with a checkbox header.
                var categoryItem = new TreeViewItem
                {
                    Header = CreateCategoryHeader(categorySublayer)
                };

                // Add each component sublayer as a child item.
                foreach (BuildingComponentSublayer componentSublayer in categorySublayer.Sublayers)
                {
                    var componentItem = new TreeViewItem
                    {
                        Header = CreateComponentHeader(componentSublayer)
                    };
                    categoryItem.Items.Add(componentItem);
                }

                // Add the category to the tree view.
                CategoriesTreeView.Items.Add(categoryItem);
            }
        }

        private StackPanel CreateCategoryHeader(BuildingGroupSublayer sublayer)
        {
            // Create a horizontal stack panel for the category header.
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            // Add the category name.
            var textBlock = new TextBlock
            {
                Text = sublayer.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            // Add a checkbox to control category visibility.
            var checkBox = new CheckBox
            {
                IsChecked = sublayer.IsVisible,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Update the sublayer visibility when the checkbox is toggled.
            checkBox.Checked += (s, e) => sublayer.IsVisible = true;
            checkBox.Unchecked += (s, e) => sublayer.IsVisible = false;

            panel.Children.Add(textBlock);
            panel.Children.Add(checkBox);

            return panel;
        }

        private StackPanel CreateComponentHeader(BuildingComponentSublayer sublayer)
        {
            // Create a horizontal stack panel for the component header.
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            // Add a checkbox with the component name.
            var checkBox = new CheckBox
            {
                Content = sublayer.Name,
                IsChecked = sublayer.IsVisible,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Update the sublayer visibility when the checkbox is toggled.
            checkBox.Checked += (s, e) => sublayer.IsVisible = true;
            checkBox.Unchecked += (s, e) => sublayer.IsVisible = false;

            panel.Children.Add(checkBox);

            return panel;
        }

        private async void MySceneView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear the previous selection if one exists.
            if (_selectedSublayer != null)
            {
                _selectedSublayer.ClearSelection();
                _selectedSublayer = null;
            }

            // Clear the feature attribute display.
            ClearFeatureDisplay();

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

                        // Display the feature's attributes in the UI.
                        DisplayFeatureAttributes(identifiedFeature);
                    }
                }
            }
        }

        private void DisplayFeatureAttributes(Feature feature)
        {
            // Show the feature info border and hide the instruction text.
            FeatureInfoBorder.Visibility = Visibility.Visible;
            NoFeatureSelectedText.Visibility = Visibility.Collapsed;

            var attributesList = new List<KeyValuePair<string, string>>();
            foreach (var attribute in feature.Attributes)
            {
                var value = attribute.Value?.ToString() ?? "N/A";
                attributesList.Add(new KeyValuePair<string, string>(attribute.Key, value));
            }

            // Bind the attributes to the UI.
            FeatureAttributesPanel.ItemsSource = attributesList;
        }

        private void ClearFeatureDisplay()
        {
            // Hide the feature info border and show the instruction text.
            FeatureInfoBorder.Visibility = Visibility.Collapsed;
            NoFeatureSelectedText.Visibility = Visibility.Visible;

            FeatureAttributesPanel.ItemsSource = null;
        }

        private void CloseFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSublayer != null)
            {
                _selectedSublayer.ClearSelection();
                _selectedSublayer = null;
            }
            ClearFeatureDisplay();
        }
    }
}