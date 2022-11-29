// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Threading.Tasks;

namespace ArcGIS.WPF.Samples.ViewPointCloudDataOffline
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "View point cloud data offline",
        category: "Data",
        description: "Display local 3D point cloud data.",
        instructions: "The sample starts with a point cloud layer loaded and draped on top of a scene. Pan and zoom to explore the scene and see the detail of the point cloud layer.",
        tags: new[] { "3D", "lidar", "point cloud" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("34da965ca51d4c68aa9b3a38edb29e00")]
    public partial class ViewPointCloudDataOffline
    {
        // Hold the URL to the elevation service.
        private const string ElevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public ViewPointCloudDataOffline()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the scene with basemap.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Create a surface and add the elevation service to it.
            Surface groundSurface = new Surface();
            groundSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri(ElevationServiceUrl)));

            // Add the surface to the scene.
            MySceneView.Scene.BaseSurface = groundSurface;

            // Get the path to the local point cloud data.
            string pointCloudPath = DataManager.GetDataFolder("34da965ca51d4c68aa9b3a38edb29e00", "sandiego-north-balboa-pointcloud.slpk");

            // Create the point cloud layer.
            PointCloudLayer balboaPointCloud = new PointCloudLayer(new Uri(pointCloudPath));

            // Add the point cloud to the scene.
            MySceneView.Scene.OperationalLayers.Add(balboaPointCloud);

            // Wait for the layer to load.
            await balboaPointCloud.LoadAsync();

            // Zoom to the extent of the point cloud.
            await MySceneView.SetViewpointAsync(new Viewpoint(balboaPointCloud.FullExtent));
        }
    }
}