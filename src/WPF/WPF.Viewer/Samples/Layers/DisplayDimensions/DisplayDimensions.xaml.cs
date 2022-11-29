// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.DisplayDimensions
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display dimensions",
        category: "Layers",
        description: "Display dimension features from a mobile map package.",
        instructions: "When the sample loads, it will automatically display the map containing dimension features from the mobile map package. The name of the dimension layer containing the dimension features is displayed in the controls box. Control the visibility of the dimension layer with the \"Dimension Layer visibility\" check box, and apply a definition expression to show dimensions of greater than or equal to 450m in length using the \"Definition Expression\" checkbox.",
        tags: new[] { "dimension", "layer", "mmpk", "mobile map package", "utility" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("f5ff6f5556a945bca87ca513b8729a1e")]
    public partial class DisplayDimensions
    {
        // Hold a reference to the Dimension layer for use in event handlers
        private DimensionLayer _dimensionLayer;

        public DisplayDimensions()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Get the path to the map package. DataManager is a sample viewer tool, not part of ArcGIS Maps SDK for .NET.
                var dataPath = DataManager.GetDataFolder("f5ff6f5556a945bca87ca513b8729a1e", "Edinburgh_Pylon_Dimensions.mmpk");

                // Load the mobile map package.
                MobileMapPackage mobileMapPackage = new MobileMapPackage(dataPath);
                await mobileMapPackage.LoadAsync();

                // Set the mapview to display the map from the package.
                MyMapView.Map = mobileMapPackage.Maps.First();

                // Set the minimum scale range of the sample to maintain readability of dimension features.
                MyMapView.Map.MinScale = 35000;

                // Get the dimension layer from the MapView operational layers.
                _dimensionLayer = MyMapView.Map.OperationalLayers.OfType<DimensionLayer>().First();

                // Enable the check boxes.
                DimensionLayerCheckBox.IsEnabled = true;
                DefinitionExpressionCheckBox.IsEnabled = true;

                // Set the label content.
                PylonLabel.Content = _dimensionLayer.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DimensionLayerCheckBoxChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            // Check if dimension layer has been instantiated.
            if (_dimensionLayer != null)
            {
                // Set the visibility of the dimension layer.
                _dimensionLayer.IsVisible = DimensionLayerCheckBox.IsChecked == true;
            }
        }

        private void DefinitionExpressionCheckBoxChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create a definition expression to show dimension lengths of greater than or equal to 450m when the checkbox is selected,
            // or to reset the definition expression to show all dimension lengths when unselected.
            string definitionExpression = DefinitionExpressionCheckBox.IsChecked == true ? "DIMLENGTH >= 450" : "";

            // Check if dimension layer has been instantiated.
            if (_dimensionLayer != null)
            {
                // Set the definition expression of the dimension layer.
                _dimensionLayer.DefinitionExpression = definitionExpression;
            }
        }
    }
}