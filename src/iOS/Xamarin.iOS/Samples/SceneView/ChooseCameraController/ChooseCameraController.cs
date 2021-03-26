// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChooseCameraController
{
    [Register("ChooseCameraController")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Choose camera controller",
        category: "SceneView",
        description: "Control the behavior of the camera in a scene.",
        instructions: "The application loads with the \"Orbit camera around plane\" option (i.e. camera will now be fixed to the plane). Choose the \"Orbit camera around location\" option to rotate and center the scene around the location of the Upheaval Dome crater structure, or choose the \"Free pan round the globe\" option to go to default free navigation.",
        tags: new[] { "3D", "camera", "camera controller" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("681d6f7694644709a7c830ec57a2d72b")]
    public class ChooseCameraController : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;

        // Segmented control for selecting camera controller.
        private UISegmentedControl _cameraPicker;

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

        public ChooseCameraController()
        {
            Title = "Choose camera controller";
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

        private void ChangeCameraController(string setting)
        {
            // Switch on the selected text.
            switch (setting)
            {
                case "Orbit plane":
                    // Switch to the plane camera controller.
                    _mySceneView.CameraController = _orbitPlaneCameraController;
                    break;

                case "Orbit crater":
                    // Switch to the crater camera controller.
                    _mySceneView.CameraController = _orbitCraterCameraController;
                    break;

                case "Free pan":
                    // Switch to a globe camera controller, which is free pan.
                    _mySceneView.CameraController = _globeCameraController;
                    break;
            }
        }

        public override void LoadView()
        {
            // Create the view.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            // Create the scene.
            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add a segmented control for selecting the camera controller.
            _cameraPicker = new UISegmentedControl("Orbit plane", "Orbit crater", "Free pan");
            _cameraPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            _cameraPicker.SelectedSegment = 0;

            // Toolbar for the segmented control.
            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem {CustomView = _cameraPicker}
            };

            // Add the views.
            View.AddSubviews(_mySceneView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _cameraPicker.ValueChanged += Segment_Selected;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _cameraPicker.ValueChanged -= Segment_Selected;
        }

        private void Segment_Selected(object sender, EventArgs e)
        {
            // Get the string of the selected item.
            UISegmentedControl control = (UISegmentedControl)sender;

            // Change the camera controller.
            ChangeCameraController(control.TitleAt(control.SelectedSegment));
        }

        private void CreateErrorDialog(string message)
        {
            // Create Alert.
            UIAlertController okAlertController = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);

            // Add Action.
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Present Alert.
            PresentViewController(okAlertController, true, null);
        }
    }
}