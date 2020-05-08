// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.GroupLayers
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Group layers",
        category: "Layers",
        description: "Group a collection of layers together and toggle their visibility as a group.",
        instructions: "The layers in the map will be displayed in a table of contents. Toggle the checkbox next to a layer's name to change its visibility. Turning a group layer's visibility off will override the visibility of its child layers.",
        tags: new[] { "group layer", "layers" })]
    public class GroupLayers : Activity
    {
        // Hold references to the UI controls.
        private SceneView _mySceneView;
        private LinearLayout _layerListView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Group layers";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
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
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add the top-level layers to the scene.
            _mySceneView.Scene.OperationalLayers.Add(gLayer);
            _mySceneView.Scene.OperationalLayers.Add(nonDevOne);
            _mySceneView.Scene.OperationalLayers.Add(nonDevTwo);

            // Wait for all of the layers in the group layer to load.
            await Task.WhenAll(gLayer.Layers.ToList().Select(m => m.LoadAsync()).ToList());

            // Zoom to the extent of the group layer.
            _mySceneView.SetViewpoint(new Viewpoint(gLayer.FullExtent));

            // Add the layer list to the UI.
            foreach (Layer layer in _mySceneView.Scene.OperationalLayers)
            {
                AddLayersToUI(layer);
            }
        }

        private async void AddLayersToUI(Layer layer, int nestLevel = 0)
        {
            // Wait for the layer to load - ensures that the UI will be up-to-date.
            await layer.LoadAsync();

            // Add a row for the current layer.
            _layerListView.AddView(ViewForLayer(layer, nestLevel));

            // Add rows for any children of this layer if it is a group layer.
            if (layer is GroupLayer layerGroup)
            {
                foreach (Layer child in layerGroup.Layers)
                {
                    AddLayersToUI(child, nestLevel + 1);
                }
            }
        }

        private View ViewForLayer(Layer layer, int nestLevel = 0)
        {
            // Create the view that holds the row.
            LinearLayout rowContainer = new LinearLayout(this);
            rowContainer.Orientation = Orientation.Horizontal;

            // Padding implements the nesting/hieararchy display.
            rowContainer.SetPadding(32 * nestLevel, 0, 0, 0);

            // Create and configure the visibility toggle.
            CheckBox toggle = new CheckBox(this);
            toggle.Checked = true;
            toggle.CheckedChange += (sender, args) => layer.IsVisible = toggle.Checked;
            rowContainer.AddView(toggle);

            // Add a label for the layer.
            rowContainer.AddView(new TextView(this) {Text = layer.Name});

            return rowContainer;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            _layerListView = new LinearLayout(this);
            _layerListView.Orientation = Orientation.Vertical;

            layout.AddView(_layerListView);

            // Add the map view to the layout.
            _mySceneView = new SceneView(this);
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}