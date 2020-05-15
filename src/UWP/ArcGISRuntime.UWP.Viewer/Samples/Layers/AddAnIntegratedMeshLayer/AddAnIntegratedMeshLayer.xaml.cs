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

namespace ArcGISRuntime.UWP.Samples.AddAnIntegratedMeshLayer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add an integrated mesh layer",
        category: "Layers",
        description: "View an integrated mesh layer from a scene service.",
        instructions: "Run the sample and watch the integrated mesh layer load in place of the extruded imagery basemap. Navigate around the scene to visualize the high level of detail on the cliffs and valley floor.",
        tags: new[] { "3D", "integrated mesh", "layers" })]
    public partial class AddAnIntegratedMeshLayer
    {
        // URLs for the services used by this sample.
        private const string IntegratedMeshLayerUrl =
            "https://tiles.arcgis.com/tiles/FQD0rKU8X5sAQfh8/arcgis/rest/services/VRICON_Yosemite_Sample_Integrated_Mesh_scene_layer/SceneServer";

        private const string ElevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public AddAnIntegratedMeshLayer()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene with basemap.
            MySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Create and use an elevation surface to show terrain.
            Surface baseSurface = new Surface();
            baseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri(ElevationServiceUrl)));
            MySceneView.Scene.BaseSurface = baseSurface;

            // Create the integrated mesh layer from URL.
            IntegratedMeshLayer meshLayer = new IntegratedMeshLayer(new Uri(IntegratedMeshLayerUrl));

            // Add the layer to the scene's operational layers.
            MySceneView.Scene.OperationalLayers.Add(meshLayer);

            // Start with camera pointing at El Capitan.
            MySceneView.SetViewpointCamera(new Camera(new MapPoint(-119.622075, 37.720650, 2104.901239), 315.50368761552056, 78.09465920130114, 0));
        }
    }
}