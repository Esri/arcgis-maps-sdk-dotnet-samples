// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Windows;

namespace ArcGIS.WPF.Samples.FeatureLayerRenderingModeScene
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Feature layer rendering mode (scene)",
        category: "Layers",
        description: "Render features in a scene statically or dynamically by setting the feature layer rendering mode.",
        instructions: "Click the button to trigger the same zoom animation on both static and dynamicly rendered scenes.",
        tags: new[] { "3D", "dynamic", "feature layer", "features", "rendering", "static" })]
    public partial class FeatureLayerRenderingModeScene
    {
        // Points for demonstrating zoom.
        private readonly MapPoint _zoomedOutPoint = new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84);
        private readonly MapPoint _zoomedInPoint = new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84);

        // Viewpoints for each zoom level.
        private Camera _zoomedOutCamera;
        private Camera _zoomedInCamera;

        // URI for the feature service.
        private string _featureService = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/";

        // Hold the current zoom state.
        private bool _zoomed;

        public FeatureLayerRenderingModeScene()
        {
            InitializeComponent();

            // Initialize the sample.
            Initialize();
        }

        private void Initialize()
        {
            // Initialize the cameras (viewpoints) with two points.
            _zoomedOutCamera = new Camera(_zoomedOutPoint, 42000, 0, 0, 0);
            _zoomedInCamera = new Camera(_zoomedInPoint, 2500, 90, 75, 0);

            // Create the scene for displaying the feature layer in static mode.
            Scene staticScene = new Scene
            {
                InitialViewpoint = new Viewpoint(_zoomedOutPoint, _zoomedOutCamera)
            };

            // Create the scene for displaying the feature layer in dynamic mode.
            Scene dynamicScene = new Scene
            {
                InitialViewpoint = new Viewpoint(_zoomedOutPoint, _zoomedOutCamera)
            };

            foreach (string identifier in new[] { "0", "9", "8" })
            {
                // Create the table.
                ServiceFeatureTable serviceTable = new ServiceFeatureTable(new Uri(_featureService + identifier));

                // Create and add the static layer.
                FeatureLayer staticLayer = new FeatureLayer(serviceTable)
                {
                    RenderingMode = FeatureRenderingMode.Static
                };
                staticScene.OperationalLayers.Add(staticLayer);

                // Create and add the dynamic layer.
                FeatureLayer dynamicLayer = new FeatureLayer(new ServiceFeatureTable(serviceTable.Source));
                dynamicLayer.RenderingMode = FeatureRenderingMode.Dynamic;
                dynamicScene.OperationalLayers.Add(dynamicLayer);
            }

            // Add the scenes to the scene views.
            MyStaticSceneView.Scene = staticScene;
            MyDynamicSceneView.Scene = dynamicScene;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Zoom out if zoomed.
            if (_zoomed)
            {
                MyStaticSceneView.SetViewpointCameraAsync(_zoomedOutCamera, new TimeSpan(0, 0, 5));
                MyDynamicSceneView.SetViewpointCameraAsync(_zoomedOutCamera, new TimeSpan(0, 0, 5));
            }
            else // Zoom in otherwise.
            {
                MyStaticSceneView.SetViewpointCameraAsync(_zoomedInCamera, new TimeSpan(0, 0, 5));
                MyDynamicSceneView.SetViewpointCameraAsync(_zoomedInCamera, new TimeSpan(0, 0, 5));
            }

            // Toggle zoom state.
            _zoomed = !_zoomed;
        }
    }
}