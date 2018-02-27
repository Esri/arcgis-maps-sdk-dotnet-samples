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
        // Constant holding offset where the SceneView control should start
        private const int yPageOffset = 60;

        // Create a new SceneView control
        private SceneView _mySceneView = new SceneView();

        public DisplayScene()
        {
            Title = "Display scene";
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
            // Create a new scene
            Scene myScene = new Scene();

            // Crate a new base map using the static/shared create imagery method
            Basemap myBaseMap = Basemap.CreateImagery();

            // Add the imagery basemap to the scene's base map property
            myScene.Basemap = myBaseMap;

            // Add scene (with an imagery basemap) to the scene view's scene property
            _mySceneView.Scene = myScene;

            // Create a new surface
            Surface mySurface = new Surface();

            // Define the string that points to the elevation image service
            string myElevationImageService = "http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

            // Create a Uri from the elevation image service string
            Uri myUri = new Uri(myElevationImageService);

            // Create an ArcGIS tiled elevation 
            ArcGISTiledElevationSource myArcGISTiledElevationSource = new ArcGISTiledElevationSource();

            // Set the ArcGIS tiled elevation sources property to the Uri of the elevation image service
            myArcGISTiledElevationSource.Source = myUri;

            // Add the ArcGIS tiled elevation source to the surface's elevated sources collection
            mySurface.ElevationSources.Add(myArcGISTiledElevationSource);

            // Set the scene's base surface to the surface with the ArcGIS tiled elevation source
            myScene.BaseSurface = mySurface;

            // Create camera with an initial camera position (Mount Everest in the Alps mountains)
            Camera myCamera = new Camera(28.4, 83.9, 10010.0, 10.0, 80.0, 300.0);

            // Set the scene view's camera position
            _mySceneView.SetViewpointCameraAsync(myCamera);
        }

        private void CreateLayout()
        {
            // Add SceneView to the page
            View.AddSubviews(_mySceneView);
        }
    }
}