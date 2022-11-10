// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISMapsSDK.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

namespace ArcGISMapsSDKMaui.Samples.ChooseCameraController
{
    [ArcGISMapsSDK.Samples.Shared.Attributes.Sample(
        name: "Choose camera controller",
        category: "SceneView",
        description: "Control the behavior of the camera in a scene.",
        instructions: "The application loads with the \"Orbit camera around plane\" option (i.e. camera will now be fixed to the plane). Choose the \"Orbit camera around location\" option to rotate and center the scene around the location of the Upheaval Dome crater structure, or choose the \"Free pan round the globe\" option to go to default free navigation.",
        tags: new[] { "3D", "camera", "camera controller" })]
    [ArcGISMapsSDK.Samples.Shared.Attributes.OfflineData("681d6f7694644709a7c830ec57a2d72b")]
    public partial class ChooseCameraController : ContentPage
    {
        // Path for elevation data.
        private readonly Uri _elevationUri = new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Path for the plane model.
        private readonly Uri _modelUri = new Uri(DataManager.GetDataFolder("681d6f7694644709a7c830ec57a2d72b", "Bristol.dae"));

        // Geo element camera controller.
        private OrbitGeoElementCameraController _orbitPlaneCameraController;

        // Location camera controller.
        private OrbitLocationCameraController _orbitCraterCameraController;

        // Globe camera controller.
        private readonly GlobeCameraController _globeCameraController = new GlobeCameraController();

        // Text labels for the user interface.
        private readonly string[] _controllers = { "Orbit camera around plane", "Orbit camera around crater", "Free pan around the globe" };

        public ChooseCameraController()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a scene.
            Scene myScene = new Scene(BasemapStyle.ArcGISImagery);

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

            // Location at the crater.
            MapPoint craterLocation = new MapPoint(-109.929589, 38.437304, 1700, SpatialReferences.Wgs84);

            // Create the plane symbol and make it 10x larger (to be the right size relative to the scene).
            ModelSceneSymbol planeSymbol;
            try
            {
                planeSymbol = await ModelSceneSymbol.CreateAsync(_modelUri, 10.0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                await Application.Current.MainPage.DisplayAlert("Error", "Loading plane model failed. Sample failed to initialize.", "OK");
                return;
            }

            // Create a graphic using the plane symbol.
            Graphic planeGraphic = new Graphic(new MapPoint(craterLocation.X, craterLocation.Y, 5000.0, SpatialReferences.Wgs84), planeSymbol);
            sceneGraphicsOverlay.Graphics.Add(planeGraphic);

            // Instantiate a new camera controller which orbits a geo element.
            _orbitPlaneCameraController = new OrbitGeoElementCameraController(planeGraphic, 300.0)
            {
                CameraPitchOffset = 30,
                CameraHeadingOffset = 150
            };

            // Instantiate a new camera controller which orbits a location.
            _orbitCraterCameraController = new OrbitLocationCameraController(craterLocation, 6000.0)
            {
                CameraPitchOffset = 3,
                CameraHeadingOffset = 150
            };

            // Set the starting camera controller.
            MySceneView.CameraController = _orbitPlaneCameraController;

            // Add the scene to the view.
            MySceneView.Scene = myScene;
        }

        private void ChangeCameraController(string cameraControllerLabel)
        {
            switch (cameraControllerLabel)
            {
                case "Orbit camera around plane":
                    // Switch to the plane camera controller.
                    MySceneView.CameraController = _orbitPlaneCameraController;
                    break;

                case "Orbit camera around crater":
                    // Switch to the crater camera controller.
                    MySceneView.CameraController = _orbitCraterCameraController;
                    break;

                case "Free pan around the globe":
                    // Switch to a globe camera controller, which is free pan.
                    MySceneView.CameraController = _globeCameraController;
                    break;
            }
        }

        private async void OnButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Show sheet and get the camera controller from the selection.
                string selectedCamera =
                    await Application.Current.MainPage.DisplayActionSheet("Select camera controller", "Cancel", null, _controllers);

                // If selected cancel then do nothing.
                if (selectedCamera == "Cancel") return;

                // Change the camera controller to the selected item.
                ChangeCameraController(selectedCamera);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }
    }
}