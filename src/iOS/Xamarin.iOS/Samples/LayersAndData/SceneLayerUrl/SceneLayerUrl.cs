// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.SceneLayerUrl
{
    [Register("SceneLayerUrl")]
    public class SceneLayerUrl : UIViewController
    {
        // Constant holding offset where the SceneView control should start
        private const int yPageOffset = 60;

        // Create a new SceneView control
        private SceneView _mySceneView = new SceneView();

        public SceneLayerUrl()
        {
            Title = "ArcGIS scene layer (URL)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Execute initialization 
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the SceneView
            _mySceneView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
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

            // Create a camera with cordinates showing layer data 
            Camera camera = new Camera(48.378, -4.494, 200, 345, 65, 0);

            // Assign the Scene to the SceneView
            _mySceneView.Scene = myScene;

            // Set view point of scene view using camera 
            _mySceneView.SetViewpointCameraAsync(camera);        
        }

        private void CreateLayout()
        {
            // Add SceneView to the page
            View.AddSubviews(_mySceneView);
        }
    }
}