// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DisplayDimensions
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display dimensions",
        category: "Layers",
        description: "Display dimension features from a mobile map package.",
        instructions: "When the sample loads, it will automatically display the map containing dimension features from the mobile map package. The name of the dimension layer containing the dimension features is displayed in the controls box. Control the visibility of the dimension layer with the \"Dimension Layer visibility\" check box, and apply a definition expression to show dimensions of greater than or equal to 450m in length using the \"Definition Expression\" checkbox.",
        tags: new[] { "dimension", "layer", "mmpk", "mobile map package", "utility", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("f5ff6f5556a945bca87ca513b8729a1e")]
    public partial class DisplayDimensions : ContentPage
    {
        // Dimension layer, the operational layer. 
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
                // Load the mobile map package.
                MobileMapPackage mobileMapPackage = new MobileMapPackage(DataManager.GetDataFolder("f5ff6f5556a945bca87ca513b8729a1e", "Edinburgh_Pylon_Dimensions.mmpk"));
                await mobileMapPackage.LoadAsync();

                // Set the mapview to display the map from the package.
                MyMapView.Map = mobileMapPackage.Maps.First();

                // Set the minimum scale range of the sample to maintain readability of dimension features.
                MyMapView.Map.MinScale = 35000;

                // Get the dimension layer from the MapView operational layers.
                _dimensionLayer = (DimensionLayer)MyMapView.Map.OperationalLayers.Where(layer => layer is DimensionLayer).First();

                // Load the dimension layer.
                await _dimensionLayer.LoadAsync();

                // Enable the switches.
                DimensionLayerSwitch.IsEnabled = true;
                DefinitionExpressionSwitch.IsEnabled = true;

                // Set the label content.
                PylonLabel.Text = _dimensionLayer.Name;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void DimensionLayerSwitchChanged(object sender, ToggledEventArgs e)
        {
            // Set the visibility of the dimension layer.
            if (_dimensionLayer != null) _dimensionLayer.IsVisible = DimensionLayerSwitch.IsToggled == true;
        }

        private void DefinitionExpressionSwitchChanged(object sender, ToggledEventArgs e)
        {
            // Set a definition expression to show dimension lengths of greater than or equal to 450m when the checkbox is selected,
            // or to reset the definition expression to show all dimension lengths when unselected.
            string definitionExpression = DefinitionExpressionSwitch.IsToggled == true ? "DIMLENGTH >= 450" : "";
            if (_dimensionLayer != null) _dimensionLayer.DefinitionExpression = definitionExpression;
        }
    }
}
