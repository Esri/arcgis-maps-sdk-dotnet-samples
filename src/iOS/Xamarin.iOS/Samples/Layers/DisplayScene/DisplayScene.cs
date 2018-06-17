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

namespace ArcGISRuntime.Samples.DisplayScene
{
    [Register("DisplayScene")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display scene",
        "Layers",
        "Demonstrates how to display a scene with an elevation data source. An elevation data source allows objects to be viewed in 3D, like this picture of Mt. Everest.",
        "")]
    public class DisplayScene : UIViewController
    {
        // Create a new SceneView control.
        private readonly SceneView _mySceneView = new SceneView();

        public DisplayScene()
        {
            Title = "Display scene";
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
            // Create and show a new scene with imagery basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Create a new surface.
            Surface surface = new Surface();

            // Define the string that points to the elevation image service.
            string elevationImageService = "http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

            // Create an ArcGIS tiled elevation.
            ArcGISTiledElevationSource myArcGISTiledElevationSource = new ArcGISTiledElevationSource
            {
                // Set the ArcGIS tiled elevation sources property to the Uri of the elevation image service.
                Source = new Uri(elevationImageService)
            };

            // Add the ArcGIS tiled elevation source to the surface's elevated sources collection.
            surface.ElevationSources.Add(myArcGISTiledElevationSource);

            // Set the scene's base surface to the surface with the ArcGIS tiled elevation source.
            _mySceneView.Scene.BaseSurface = surface;

            // Create camera with an initial camera position (Mount Everest in the Alps mountains).
            Camera camera = new Camera(28.4, 83.9, 10010.0, 10.0, 80.0, 300.0);

            // Set the scene view's camera position.
            _mySceneView.SetViewpointCameraAsync(camera);
        }

        private void CreateLayout()
        {
            // Add SceneView to the page.
            View.AddSubviews(_mySceneView);
        }
    }
}