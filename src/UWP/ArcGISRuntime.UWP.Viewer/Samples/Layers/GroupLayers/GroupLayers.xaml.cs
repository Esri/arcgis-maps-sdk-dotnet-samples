// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntime.UWP.Samples.GroupLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Group layers",
        "Layers",
        "Group a collection of layers together and toggle their visibility as a group.",
        "")]
    public partial class GroupLayers
    {
        public GroupLayers()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the layers.
            ArcGISSceneLayer devOne = new ArcGISSceneLayer(new Uri("https://scenesampleserverdev.arcgis.com/arcgis/rest/services/Hosted/DevA_Trees/SceneServer/layers/0"));
            ArcGISSceneLayer devTwo = new ArcGISSceneLayer(new Uri("https://scenesampleserverdev.arcgis.com/arcgis/rest/services/Hosted/DevA_Pathways/SceneServer/layers/0"));
            ArcGISSceneLayer devThree = new ArcGISSceneLayer(new Uri("https://scenesampleserverdev.arcgis.com/arcgis/rest/services/Hosted/DevA_BuildingShell_Textured/SceneServer/layers/0"));
            ArcGISSceneLayer nonDevOne = new ArcGISSceneLayer(new Uri("https://scenesampleserverdev.arcgis.com/arcgis/rest/services/Hosted/PlannedDemo_BuildingShell/SceneServer/layers/0"));
            FeatureLayer nonDevTwo = new FeatureLayer(new Uri("https://services.arcgis.com/P3ePLMYs2RVChkJx/arcgis/rest/services/DevelopmentProjectArea/FeatureServer/0"));

            // Create the group layer and add sublayers.
            GroupLayer gLayer = new GroupLayer();
            gLayer.Name = "Group: Dev A";
            gLayer.Layers.Add(devOne);
            gLayer.Layers.Add(devTwo);
            gLayer.Layers.Add(devThree);

            // Create the scene with a basemap.
            MySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add the top-level layers to the scene.
            MySceneView.Scene.OperationalLayers.Add(gLayer);
            MySceneView.Scene.OperationalLayers.Add(nonDevOne);
            MySceneView.Scene.OperationalLayers.Add(nonDevTwo);

            // Wait for all of the layers in the group layer to load.
            await Task.WhenAll(gLayer.Layers.ToList().Select(m => m.LoadAsync()).ToList());

            // Zoom to the extent of the group layer.
            MySceneView.SetViewpoint(new Viewpoint(gLayer.FullExtent));
        }
    }
}