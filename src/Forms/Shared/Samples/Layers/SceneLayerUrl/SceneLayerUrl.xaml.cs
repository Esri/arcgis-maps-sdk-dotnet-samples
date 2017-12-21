﻿// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.SceneLayerUrl
{
    public partial class SceneLayerUrl : ContentPage
    {
        // URL for a service to use as an elevation source
        private Uri _elevationSourceUrl = new Uri(@"https://scene.arcgis.com/arcgis/rest/services/BREST_DTM_1M/ImageServer");

        // URL for the scene layer
        private Uri _serviceUri = new Uri(
               "https://scene.arcgis.com/arcgis/rest/services/Hosted/Buildings_Brest/SceneServer/0");

        public SceneLayerUrl()
        {
            InitializeComponent();

            Title = "ArcGIS scene layer (URL)";

            // Execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Scene
            Scene myScene = new Scene();

            // Set Scene's base map property
            myScene.Basemap = Basemap.CreateImagery();

            // Create and add an elevation source for the Scene
            ArcGISTiledElevationSource elevationSrc = new ArcGISTiledElevationSource(_elevationSourceUrl);
            myScene.BaseSurface.ElevationSources.Add(elevationSrc);

            // Create new scene layer from the url
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_serviceUri);

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