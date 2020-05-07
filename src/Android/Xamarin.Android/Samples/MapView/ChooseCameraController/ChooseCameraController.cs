// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.ChooseCameraController
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Choose camera controller",
        "MapView",
        "Control the behavior of the camera in a scene.",
        "The application loads with the \"Orbit camera around plane\" option (i.e. camera will now be fixed to the plane). Choose the \"Orbit camera around location\" option to rotate and center the scene around the location of the Upheaval Dome crater structure, or choose the \"Free pan round the globe\" option to go to default free navigation.",
        "3D", "camera", "camera controller")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("ChooseCameraController.axml")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("681d6f7694644709a7c830ec57a2d72b")]
    public class ChooseCameraController : Activity
    {
        // View for the scene.
        private SceneView _mySceneView;

        // Path for elevation data.
        private readonly Uri _elevationUri = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Path for the plane model.
        private readonly Uri _modelUri = new Uri(DataManager.GetDataFolder("681d6f7694644709a7c830ec57a2d72b", "Bristol.dae"));

        // Geo element camera controller.
        private OrbitGeoElementCameraController _orbitPlaneCameraController;

        // Location camera controller.
        private OrbitLocationCameraController _orbitCraterCameraController;

        // Globe camera controller.
        private readonly GlobeCameraController _globeCameraController = new GlobeCameraController();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Choose camera controller";

            CreateLayout();
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
            _mySceneView.GraphicsOverlays.Add(sceneGraphicsOverlay);

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
                CreateErrorDialog("Loading plane model failed. Sample failed to initialize.");
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
            _mySceneView.CameraController = _orbitPlaneCameraController;

            // Add the scene to the view.
            _mySceneView.Scene = myScene;
        }

        private void ChangeCameraController(string camera)
        {
            switch (camera)
            {
                case "Orbit camera around plane":
                    // Switch to the plane camera controller.
                    _mySceneView.CameraController = _orbitPlaneCameraController;
                    break;

                case "Orbit camera around crater":
                    // Switch to the crater camera controller.
                    _mySceneView.CameraController = _orbitCraterCameraController;
                    break;

                case "Free pan around the globe":
                    // Switch to a globe camera controller, which is free pan.
                    _mySceneView.CameraController = _globeCameraController;
                    break;
            }
        }

        private void CreateLayout()
        {
            // Load the layout for the sample from the .axml file.
            SetContentView(Resource.Layout.ChooseCameraController);

            // Update control references to point to the controls defined in the layout.
            _mySceneView = FindViewById<SceneView>(Resource.Id.SceneView);
            RadioButton planeButton = FindViewById<RadioButton>(Resource.Id.PlaneButton);
            RadioButton craterButton = FindViewById<RadioButton>(Resource.Id.CraterButton);
            RadioButton freeButton = FindViewById<RadioButton>(Resource.Id.FreeButton);

            // Select the first option.
            planeButton.Checked = true;

            // Add listeners to all of the buttons.
            planeButton.Click += Setting_Checked;
            craterButton.Click += Setting_Checked;
            freeButton.Click += Setting_Checked;
        }

        private void Setting_Checked(object sender, EventArgs e)
        {
            ChangeCameraController(((RadioButton)sender).Text);
        }

        private void CreateErrorDialog(string message)
        {
            // Create a dialog to show message to user.
            AlertDialog alert = new AlertDialog.Builder(this).Create();
            alert.SetMessage(message);
            alert.Show();
        }
    }
}