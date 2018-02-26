// Copyright 2018 Esri.
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

namespace ArcGISRuntime.WPF.Samples.FeatureLayerRenderingModeScene
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer rendering mode (Scene)",
        "Layers",
        "This sample demonstrates how to use load settings to change the preferred rendering mode for a scene. Static rendering mode only redraws features periodically when a sceneview is navigating, while dynamic mode dynamically re-renders as the scene moves.",
        "Press the 'Animated Zoom' button to trigger a zoom. Observe the differences between the two scenes.")]
    public partial class FeatureLayerRenderingModeScene
    {
        // Points for demonstrating zoom
        private MapPoint _zoomedOutPoint = new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84);
        private MapPoint _zoomedInPoint = new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84);

        // Viewpoints for each zoom level
        private Camera _zoomedOutCamera;
        private Camera _zoomedInCamera;

        // URI for the feature service
        private string _featureService = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/";

        // Hold the current zoom state
        private bool _zoomed;

        public FeatureLayerRenderingModeScene()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Initialize the cameras (viewpoints) with two points
            _zoomedOutCamera = new Camera(_zoomedOutPoint, 42000, 0, 0, 0);
            _zoomedInCamera = new Camera(_zoomedInPoint, 2500, 90, 75, 0);

            // Create the scene for displaying the feature layer in static mode
            Scene staticScene = new Scene(); // Basemap omitted to make it easier to distinguish the rendering modes
            staticScene.LoadSettings.PreferredPointFeatureRenderingMode = FeatureRenderingMode.Static;
            staticScene.LoadSettings.PreferredPolygonFeatureRenderingMode = FeatureRenderingMode.Static;
            staticScene.LoadSettings.PreferredPolylineFeatureRenderingMode = FeatureRenderingMode.Static;

            // Create the scene for displaying the feature layer in dynamic mode
            Scene dynamicScene = new Scene();
            dynamicScene.LoadSettings.PreferredPointFeatureRenderingMode = FeatureRenderingMode.Dynamic;
            dynamicScene.LoadSettings.PreferredPolygonFeatureRenderingMode = FeatureRenderingMode.Dynamic;
            dynamicScene.LoadSettings.PreferredPolylineFeatureRenderingMode = FeatureRenderingMode.Dynamic;

            // Create the service feature tables
            ServiceFeatureTable faultTable = new ServiceFeatureTable(new Uri(_featureService + "0"));
            ServiceFeatureTable contactTable = new ServiceFeatureTable(new Uri(_featureService + "8"));
            ServiceFeatureTable outcropTable = new ServiceFeatureTable(new Uri(_featureService + "9"));

            // Create the feature layers
            FeatureLayer faultLayer = new FeatureLayer(faultTable);
            FeatureLayer contactLayer = new FeatureLayer(contactTable);
            FeatureLayer outcropLayer = new FeatureLayer(outcropTable);

            // Add the layers to each scene
            staticScene.OperationalLayers.Add(faultLayer);
            staticScene.OperationalLayers.Add(contactLayer);
            staticScene.OperationalLayers.Add(outcropLayer);
            dynamicScene.OperationalLayers.Add(faultLayer.Clone());
            dynamicScene.OperationalLayers.Add(contactLayer.Clone());
            dynamicScene.OperationalLayers.Add(outcropLayer.Clone());

            // Add the scenes to the scene views
            MyStaticSceneView.Scene = staticScene;
            MyDynamicSceneView.Scene = dynamicScene;

            // Set the initial viewpoints for the scenes
            MyStaticSceneView.SetViewpointCamera(_zoomedOutCamera);
            MyDynamicSceneView.SetViewpointCamera(_zoomedOutCamera);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Zoom out if zoomed
            if (_zoomed)
            {
                MyStaticSceneView.SetViewpointCameraAsync(_zoomedOutCamera, new TimeSpan(0, 0, 5));
                MyDynamicSceneView.SetViewpointCameraAsync(_zoomedOutCamera, new TimeSpan(0, 0, 5));
            }
            else // Zoom in otherwise
            {
                MyStaticSceneView.SetViewpointCameraAsync(_zoomedInCamera, new TimeSpan(0, 0, 5));
                MyDynamicSceneView.SetViewpointCameraAsync(_zoomedInCamera, new TimeSpan(0, 0, 5));
            }

            // Toggle zoom state
            _zoomed = !_zoomed;
        }
    }
}