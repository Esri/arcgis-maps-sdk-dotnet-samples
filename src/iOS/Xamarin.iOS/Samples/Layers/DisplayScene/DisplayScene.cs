// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.DisplayScene
{
    [Register("DisplayScene")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display a scene",
        category: "Layers",
        description: "Display a scene with a terrain surface and some imagery.",
        instructions: "When loaded, the sample will display a scene. Pan and zoom to explore the scene.",
        tags: new[] { "3D", "basemap", "elevation", "scene", "surface" })]
    public class DisplayScene : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;

        public DisplayScene()
        {
            Title = "Display scene";
        }

        private void Initialize()
        {
            // Create a new basemap.
            Basemap imageryBasemap = Basemap.CreateImagery();

            // Create and show a new scene with imagery basemap.
            _mySceneView.Scene = new Scene(imageryBasemap);

            // Create a new surface.
            Surface surface = new Surface();

            // Define the string that points to the elevation image service.
            const string elevationImageService = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_mySceneView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}