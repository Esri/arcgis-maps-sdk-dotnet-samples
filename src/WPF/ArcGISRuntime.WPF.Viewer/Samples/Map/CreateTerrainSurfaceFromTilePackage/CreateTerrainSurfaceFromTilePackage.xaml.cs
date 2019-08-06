﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;

namespace ArcGISRuntime.WPF.Samples.CreateTerrainSurfaceFromTilePackage
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create terrain surface from a tile package",
        "Map",
        "Use a terrain surface with elevation described by a local tile package.",
        "", "Featured")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("cce37043eb0440c7a5c109cf8aad5500")]
    public partial class CreateTerrainSurfaceFromTilePackage
    {
        public CreateTerrainSurfaceFromTilePackage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene.
            MySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Get the path to the elevation tile package.
            string packagePath = DataManager.GetDataFolder("cce37043eb0440c7a5c109cf8aad5500", "MontereyElevation.tpk");

            // Create the elevation source from the tile cache.
            TileCache elevationCache = new TileCache(packagePath);
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(elevationCache);

            // Create a surface to display the elevation source.
            Surface elevationSurface = new Surface();

            // Add the elevation source to the surface.
            elevationSurface.ElevationSources.Add(elevationSource);

            // Add the surface to the scene.
            MySceneView.Scene.BaseSurface = elevationSurface;

            // Set an initial camera viewpoint.
            Camera camera = new Camera(36.525, -121.80, 300.0, 180, 80.0, 0.0);
            MySceneView.SetViewpointCamera(camera);
        }
    }
}