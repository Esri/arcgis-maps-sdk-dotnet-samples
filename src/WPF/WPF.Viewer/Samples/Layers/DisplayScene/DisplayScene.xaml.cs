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

namespace ArcGIS.WPF.Samples.DisplayScene
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display a scene",
        category: "Layers",
        description: "Display a scene with a terrain surface and some imagery.",
        instructions: "When loaded, the sample will display a scene. Pan and zoom to explore the scene.",
        tags: new[] { "3D", "basemap", "elevation", "scene", "surface" })]
    public partial class DisplayScene
    {
        public DisplayScene()
        {
            InitializeComponent();

            // Execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Create a new scene.
            Scene myScene = new Scene();

            // Crate a new base map using the static/shared create imagery method.
            Basemap myBaseMap = new Basemap(BasemapStyle.ArcGISImageryStandard);

            // Add the imagery basemap to the scene's base map property.
            myScene.Basemap = myBaseMap;

            // Add scene (with an imagery basemap) to the scene view's scene property.
            MySceneView.Scene = myScene;

            // Create a new surface.
            Surface mySurface = new Surface();

            // Define the string that points to the elevation image service.
            string myElevationImageService = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

            // Create a Uri from the elevation image service string.
            Uri myUri = new Uri(myElevationImageService);

            // Create an ArcGIS tiled elevation.
            ArcGISTiledElevationSource myArcGISTiledElevationSource = new ArcGISTiledElevationSource(myUri);

            // Add the ArcGIS tiled elevation source to the surface's elevated sources collection.
            mySurface.ElevationSources.Add(myArcGISTiledElevationSource);

            // Set the scene's base surface to the surface with the ArcGIS tiled elevation source.
            myScene.BaseSurface = mySurface;

            // Create camera with an initial camera position (Mount Everest in the Alps mountains).
            Camera myCamera = new Camera(28.4, 83.9, 10010.0, 10.0, 80.0, 0);

            // Set the scene view's camera position.
            MySceneView.SetViewpointCameraAsync(myCamera);
        }
    }
}