// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;

namespace ArcGISRuntime.WPF.Samples.SceneLayerUrl
{
    public partial class SceneLayerUrl
    {
        public SceneLayerUrl()
        {
            InitializeComponent();

            // Execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Scene
            Scene myScene = new Scene();

            // Set Scene's base map property
            myScene.Basemap = Basemap.CreateImagery();

            // Create uri to the scene layer
            var serviceUri = new Uri(
               "https://scene.arcgis.com/arcgis/rest/services/Hosted/Buildings_Brest/SceneServer/0");

            // Create new scene layer from the url
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(serviceUri);

            // Add created layer to the operational layers collection
            myScene.OperationalLayers.Add(sceneLayer);

            // Create a camera with coordinates showing layer data 
            Camera camera = new Camera(48.378, -4.494, 200, 345, 65, 0);

            // Assign the Scene to the SceneView
            MySceneView.Scene = myScene;

            // Set view point of scene view using camera 
            MySceneView.SetViewpointCameraAsync(camera);

        }

    }
}