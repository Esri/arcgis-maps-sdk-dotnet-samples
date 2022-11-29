// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.Samples.GroupLayers
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Group layers",
        category: "Layers",
        description: "Group a collection of layers together and toggle their visibility as a group.",
        instructions: "The layers in the map will be displayed in a table of contents. Toggle the checkbox next to a layer's name to change its visibility. Turning a group layer's visibility off will override the visibility of its child layers.",
        tags: new[] { "group layer", "layers" })]
    public partial class GroupLayers : ContentPage
    {
        public GroupLayers()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the layers.
            ArcGISSceneLayer devOne = new ArcGISSceneLayer(new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/DevA_Trees/SceneServer"));
            FeatureLayer devTwo = new FeatureLayer(new Uri("https://services.arcgis.com/P3ePLMYs2RVChkJx/arcgis/rest/services/DevA_Pathways/FeatureServer/1"));
            ArcGISSceneLayer devThree = new ArcGISSceneLayer(new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/DevB_BuildingShells/SceneServer"));
            ArcGISSceneLayer nonDevOne = new ArcGISSceneLayer(new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/DevA_BuildingShells/SceneServer"));
            FeatureLayer nonDevTwo = new FeatureLayer(new Uri("https://services.arcgis.com/P3ePLMYs2RVChkJx/arcgis/rest/services/DevelopmentProjectArea/FeatureServer/0"));

            // Create the group layer and add sublayers.
            GroupLayer gLayer = new GroupLayer();
            gLayer.Name = "Group: Dev A";
            gLayer.Layers.Add(devOne);
            gLayer.Layers.Add(devTwo);
            gLayer.Layers.Add(devThree);

            // Create the scene with a basemap.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Add the top-level layers to the scene.
            MySceneView.Scene.OperationalLayers.Add(gLayer);
            MySceneView.Scene.OperationalLayers.Add(nonDevOne);
            MySceneView.Scene.OperationalLayers.Add(nonDevTwo);

            // Wait for all of the layers in the group layer to load.
            await Task.WhenAll(gLayer.Layers.ToList().Select(m => m.LoadAsync()).ToList());

            // Zoom to the extent of the group layer.
            MySceneView.SetViewpoint(new Viewpoint(gLayer.FullExtent));

            // Add the layer list to the UI.
            foreach (Layer layer in MySceneView.Scene.OperationalLayers)
            {
                _ = AddLayersToUI(layer);
            }
        }

        private async Task AddLayersToUI(Layer layer, int nestLevel = 0)
        {
            try
            {
                // Wait for the layer to load - ensures that the UI will be up-to-date.
                await layer.LoadAsync();

                // Add a row for the current layer.
                LayersView.Children.Add(ViewForLayer(layer, nestLevel));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }

            // Add rows for any children of this layer if it is a group layer.
            if (layer is GroupLayer layerGroup)
            {
                foreach (Layer child in layerGroup.Layers)
                {
                    _ = AddLayersToUI(child, nestLevel + 1);
                }
            }
        }

        private View ViewForLayer(Layer layer, int nestLevel = 0)
        {
            // Create the view that holds the row.
            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Horizontal;

            // Create and configure the visibility toggle.
            Switch toggleSwitch = new Switch();
            toggleSwitch.VerticalOptions = LayoutOptions.Center;
            toggleSwitch.IsToggled = true;
            toggleSwitch.Toggled += (sender, args) => layer.IsVisible = !layer.IsVisible;

            // Implement nesting/hiearchy display.
            container.Margin = new Thickness(nestLevel * 30, 5, 10, 5);
            container.Children.Add(toggleSwitch);

            // Add the label to the row.
            Label layerLabel = new Label();
            layerLabel.Text = layer.Name;
            layerLabel.VerticalOptions = LayoutOptions.Center;
            layerLabel.VerticalTextAlignment = TextAlignment.Center;
            container.Children.Add(layerLabel);

            return container;
        }
    }
}