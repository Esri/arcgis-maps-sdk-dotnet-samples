// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;

namespace ArcGIS.WPF.Samples.TerrainExaggeration
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Terrain exaggeration",
        category: "Scene",
        description: "Vertically exaggerate terrain in a scene.",
        instructions: "Use the slider to update terrain exaggeration.",
        tags: new[] { "3D", "DEM", "DTM", "elevation", "scene", "surface", "terrain" })]
    public partial class TerrainExaggeration
    {
        private readonly string _elevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public TerrainExaggeration()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Configure the scene with National Geographic basemap.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISTopographic);

            // Add the base surface for elevation data.
            Surface elevationSurface = new Surface();
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            elevationSurface.ElevationSources.Add(elevationSource);

            // Add the surface to the scene.
            MySceneView.Scene.BaseSurface = elevationSurface;

            // Set the initial camera.
            MapPoint initialLocation = new MapPoint(-119.9489, 46.7592, 0, SpatialReferences.Wgs84);
            Camera initialCamera = new Camera(initialLocation, 15000, 40, 60, 0);
            MySceneView.SetViewpointCamera(initialCamera);

            // Update terrain exaggeration based on the slider value.
            TerrainSlider.ValueChanged += (sender, e) =>
            {
                elevationSurface.ElevationExaggeration = TerrainSlider.Value;
            };
        }
    }
}