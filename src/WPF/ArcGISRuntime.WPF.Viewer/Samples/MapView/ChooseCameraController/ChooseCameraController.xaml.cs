// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.ChooseCameraController
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Choose camera controller",
        "MapView",
        "Control the behavior of the camera in a scene.",
        "Select a radiio button to change the camera controller.")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("290f0c571c394461a8b58b6775d0bd63", "681d6f7694644709a7c830ec57a2d72b", "e87c154fb9c2487f999143df5b08e9b1", "5a9b60cee9ba41e79640a06bcdf8084d", "12509ffdc684437f8f2656b0129d2c13")]
    public partial class ChooseCameraController
    {
        // Path for elevation data.
        private Uri _elevationUri = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Path for the plane model.
        private Uri _modelUri = new Uri(DataManager.GetDataFolder("681d6f7694644709a7c830ec57a2d72b", "Bristol.dae"));

        // Location at the crater.
        private MapPoint _targetLocation = new MapPoint(-109.929589, 38.437304, 1700, SpatialReferences.Wgs84);

        // Geo element camera controller.
        private OrbitGeoElementCameraController _orbitPlaneCameraController;

        // Location camera controller.
        private OrbitLocationCameraController _orbitCraterCameraController;

        public ChooseCameraController()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a scene.
            Scene myScene = new Scene(Basemap.CreateImageryWithLabels());

            // Create a surface for elevation data.
            Surface surface = new Surface();
            surface.ElevationSources.Add(new ArcGISTiledElevationSource(_elevationUri));

            // Add the surface to the scene.
            myScene.BaseSurface = surface;

            // Create a graphics overlay for the scene.
            GraphicsOverlay sceneGraphicsOverlay = new GraphicsOverlay()
            {
                SceneProperties = new LayerSceneProperties(SurfacePlacement.Absolute)
            };
            MySceneView.GraphicsOverlays.Add(sceneGraphicsOverlay);

            // Create the plane symbol and make it 10x larger (to be the right size relative to the scene).
            ModelSceneSymbol planeSymbol = await ModelSceneSymbol.CreateAsync(_modelUri, 10.0);
            planeSymbol.Heading = 45;

            // Create a graphic using the plane symbol.
            Graphic planeGraphic = new Graphic(new MapPoint(_targetLocation.X, _targetLocation.Y, 5000.0, SpatialReferences.Wgs84), planeSymbol);
            sceneGraphicsOverlay.Graphics.Add(planeGraphic);

            // Instantiate a new camera controller which orbits a geo element.
            _orbitPlaneCameraController = new OrbitGeoElementCameraController(planeGraphic, 300.0)
            {
                CameraPitchOffset = 30,
                CameraHeadingOffset = 150
            };

            // Instantiate a new camera controller which orbits a location.
            _orbitCraterCameraController = new OrbitLocationCameraController(_targetLocation, 5000.0)
            {
                CameraPitchOffset = 3,
                CameraHeadingOffset = 150
            };

            // Set the starting camera controller.
            MySceneView.CameraController = _orbitPlaneCameraController;

            // Add events for the radio buttons.
            OrbitPlaneButton.Checked += Setting_Checked;
            OrbitCraterButton.Checked += Setting_Checked;
            FreePanButton.Checked += Setting_Checked;

            // Add the scene to the view.
            MySceneView.Scene = myScene;
        }

        private void Setting_Checked(object sender, RoutedEventArgs e)
        {
            switch (((RadioButton)sender).Name)
            {
                case "OrbitPlaneButton":
                    // Switch to the plane camera controller.
                    MySceneView.CameraController = _orbitPlaneCameraController;
                    break;

                case "OrbitCraterButton":
                    // Switch to the crater camera controller.
                    MySceneView.CameraController = _orbitCraterCameraController;
                    break;

                case "FreePanButton":
                    // Switch to a globe camera controller, which is free pan.
                    MySceneView.CameraController = new GlobeCameraController();
                    break;
            }
        }
    }
}