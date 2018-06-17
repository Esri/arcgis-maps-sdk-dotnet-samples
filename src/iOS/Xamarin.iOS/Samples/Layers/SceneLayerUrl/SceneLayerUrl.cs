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

namespace ArcGISRuntime.Samples.SceneLayerUrl
{
    [Register("SceneLayerUrl")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "ArcGIS scene layer (URL)",
        "Layers",
        "This sample demonstrates how to add an ArcGISSceneLayer as a layer in a Scene.",
        "")]
    public class SceneLayerUrl : UIViewController
    {
        // Create and hold a reference to the SceneView.
        private readonly SceneView _mySceneView = new SceneView();

        // URL for a service to use as an elevation source.
        private readonly Uri _elevationSourceUrl = new Uri("https://scene.arcgis.com/arcgis/rest/services/BREST_DTM_1M/ImageServer");

        // URL for the scene layer.
        private readonly Uri _serviceUri = new Uri("https://scene.arcgis.com/arcgis/rest/services/Hosted/Buildings_Brest/SceneServer/0");

        public SceneLayerUrl()
        {
            Title = "ArcGIS scene layer (URL)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _mySceneView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _mySceneView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Create new Scene with basemap.
            Scene myScene = new Scene(Basemap.CreateImagery());

            // Create and add an elevation source for the Scene.
            ArcGISTiledElevationSource elevationSrc = new ArcGISTiledElevationSource(_elevationSourceUrl);
            myScene.BaseSurface.ElevationSources.Add(elevationSrc);

            // Create new scene layer from the URL.
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_serviceUri);

            // Add created layer to the operational layers collection.
            myScene.OperationalLayers.Add(sceneLayer);

            // Create a camera with coordinates showing layer data.
            Camera camera = new Camera(48.378, -4.494, 200, 345, 65, 0);

            // Assign the Scene to the SceneView.
            _mySceneView.Scene = myScene;

            // Set view point of scene view using camera.
            _mySceneView.SetViewpointCameraAsync(camera);
        }

        private void CreateLayout()
        {
            // Add SceneView to the page.
            View.AddSubviews(_mySceneView);
        }
    }
}