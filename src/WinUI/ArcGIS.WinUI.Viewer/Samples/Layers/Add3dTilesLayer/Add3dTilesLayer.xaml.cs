// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.Add3dTilesLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Add 3d tiles layer",
        "Layers",
        "Add a layer to visualize 3D tiles data that conforms to the OGC 3D Tiles specification.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class Add3dTilesLayer
    {
        // Create a new elevation source from Terrain3D REST service.
        private readonly ArcGISTiledElevationSource _elevationSource = new ArcGISTiledElevationSource
            (new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));

        // Create a new scene with a dark gray basemap.
        private readonly Scene _scene = new Scene(BasemapStyle.ArcGISDarkGray);

        // Create a new 3D tiles layer from a REST endpoint.
        private readonly Ogc3DTilesLayer _3dTilesLayer = new Ogc3DTilesLayer
            (new Uri("https://tiles.arcgis.com/tiles/ZQgQTuoyBrtmoGdP/arcgis/rest/services/Stuttgart/3DTilesServer/tileset.json"));

        // Create a camera with an initial viewpoint.
        // Camera constructor parameters: latitude, longitude, altitude, heading, pitch, and roll.
        private readonly Camera sceneCamera = new Camera(48.8418, 9.1536, 1325.0, 48.35, 57.84, 0.0);

        public Add3dTilesLayer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Add the elevation source to the scene.
            _scene.BaseSurface.ElevationSources.Add(_elevationSource);

            // Add the 3D tiles layer to the scene.
            _scene.OperationalLayers.Add(_3dTilesLayer);

            // Set the scene on the SceneView to visualize the 3D tiles layer.
            MySceneView.Scene = _scene;

            // Set the viewpoint with the camera.
            await MySceneView.SetViewpointCameraAsync(sceneCamera);
        }
    }
}