// Copyright 2017 Esri.
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
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.SceneLayerUrl
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Scene layer (URL)",
        category: "Layers",
        description: "Display an ArcGIS scene layer from a URL.",
        instructions: "Pan and zoom to explore the scene.",
        tags: new[] { "3D", "Portland", "URL", "buildings", "model", "scene", "service" })]
    public partial class SceneLayerUrl
    {
        // URL for a service to use as an elevation source.
        private readonly Uri _elevationSourceUrl = new Uri(
            "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // URL for the scene layer.
        private readonly Uri _serviceUri = new Uri(
            "https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Portland/SceneServer");

        public SceneLayerUrl()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create new Scene.
            Scene myScene = new Scene { Basemap = new Basemap(BasemapStyle.ArcGISImageryStandard) };

            // Create and add an elevation source for the Scene.
            ArcGISTiledElevationSource elevationSrc = new ArcGISTiledElevationSource(_elevationSourceUrl);
            myScene.BaseSurface.ElevationSources.Add(elevationSrc);

            // Create new scene layer from the URL.
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_serviceUri);

            // Add created layer to the operational layers collection.
            myScene.OperationalLayers.Add(sceneLayer);

            try
            {
                // Load the layer.
                await sceneLayer.LoadAsync();

                // Get the center of the scene layer.
                MapPoint center = (MapPoint)GeometryEngine.Project(sceneLayer.FullExtent.GetCenter(), SpatialReferences.Wgs84);

                // Create a camera with coordinates showing layer data.
                Camera camera = new Camera(center.Y, center.X, 225, 220, 80, 0);

                // Assign the Scene to the SceneView.
                MySceneView.Scene = myScene;

                // Set view point of scene view using camera.
                await MySceneView.SetViewpointCameraAsync(camera);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }
    }
}