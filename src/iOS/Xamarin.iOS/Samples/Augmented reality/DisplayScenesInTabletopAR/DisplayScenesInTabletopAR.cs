// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ARKit;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Mapping;
using Foundation;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplayScenesInTabletopAR
{
    [Register("DisplayScenesInTabletopAR")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display scenes in tabletop AR",
        "Augmented reality",
        "Use augmented reality (AR) to pin a scene to a table or desk for easy exploration.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7dd2f97bb007466ea939160d0de96a9d")]
    public class DisplayScenesInTabletopAR : UIViewController
    {
        // Hold references to UI controls.
        private ARSceneView _arSceneView;
        private UILabel _arKitStatusLabel;
        private UILabel _helpLabel;

        // Scene to be displayed on the tabletop.
        private Scene _tabletopScene;

        public DisplayScenesInTabletopAR()
        {
            Title = "Display scenes in tabletop AR";
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _arSceneView = new ARSceneView();
            _arSceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _arKitStatusLabel = new UILabel();
            _arKitStatusLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _arKitStatusLabel.TextAlignment = UITextAlignment.Center;
            _arKitStatusLabel.TextColor = UIColor.Black;
            _arKitStatusLabel.BackgroundColor = UIColor.FromWhiteAlpha(1.0f, 0.6f);
            _arKitStatusLabel.Text = "Setting up ARKit";

            _helpLabel = new UILabel();
            _helpLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _helpLabel.TextAlignment = UITextAlignment.Center;
            _helpLabel.TextColor = UIColor.White;
            _helpLabel.BackgroundColor = UIColor.FromWhiteAlpha(0f, 0.6f);
            _helpLabel.Text = "Tap to place scene";
            _helpLabel.Hidden = true;

            // Add the views.
            View.AddSubviews(_arSceneView, _arKitStatusLabel, _helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _arSceneView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _arSceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _arSceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _arSceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _arKitStatusLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _arKitStatusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _arKitStatusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _arKitStatusLabel.HeightAnchor.ConstraintEqualTo(40),
                _helpLabel.BottomAnchor.ConstraintEqualTo(_arSceneView.SafeAreaLayoutGuide.BottomAnchor),
                _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });

            // Listen for tracking status changes and provide feedback to the user.
            _arSceneView.ARSCNViewCameraDidChangeTrackingState += ARSceneView_TrackingStateChanged;
        }

        private void Initialize()
        {
            // Display an empty scene to enable tap-to-place.
            _arSceneView.Scene = new Scene(SceneViewTilingScheme.Geographic);

            // Render the scene invisible to start.
            _arSceneView.Scene.BaseSurface.Opacity = 0;

            // Get notification when planes are detected
            _arSceneView.PlanesDetectedChanged += ARSceneView_PlanesDetectedChanged;

            // Configure the AR scene view to render detected planes.
            _arSceneView.RenderPlanes = true;
        }

        private void ARSceneView_PlanesDetectedChanged(object sender, bool planeDetected)
        {
            if (planeDetected)
            {
                BeginInvokeOnMainThread(EnableTapToPlace);
                _arSceneView.PlanesDetectedChanged -= ARSceneView_PlanesDetectedChanged;
            }
        }

        private void EnableTapToPlace()
        {
            // Show the help label.
            _helpLabel.Hidden = false;
            _helpLabel.Text = "Tap to place the scene.";

            // Wait for the user to tap.
            _arSceneView.GeoViewTapped += _arSceneView_GeoViewTapped;
        }

        private void _arSceneView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            if (_arSceneView.SetInitialTransformation(e.Position))
            {
                DisplayScene();
                _arKitStatusLabel.Hidden = true;
            }
        }

        private async void DisplayScene()
        {
            // Hide the help label.
            _helpLabel.Hidden = true;

            if (_tabletopScene == null)
            {
                // Get the downloaded mobile scene package.
                MobileScenePackage package =
                    await MobileScenePackage.OpenAsync(DataManager.GetDataFolder("7dd2f97bb007466ea939160d0de96a9d",
                        "philadelphia.mspk"));

                // Load the package.
                await package.LoadAsync();

                // Get the first scene.
                _tabletopScene = package.Scenes.First();

                // Set the clipping distance for the scene.
                _arSceneView.ClippingDistance = 400;

                // Enable subsurface navigation. This allows you to look at the scene from below.
                _tabletopScene.BaseSurface.NavigationConstraint = NavigationConstraint.None;

                _arSceneView.Scene = _tabletopScene;
            }

            // Create a camera at the bottom and center of the scene.
            //    This camera is the point at which the scene is pinned to the real-world surface.
            Camera originCamera = new Camera(39.95787000283599,
                -75.16996728256345,
                8.813445091247559,
                0, 90, 0);

            // Set the origin camera.
            _arSceneView.OriginCamera = originCamera;

            // The width of the scene content is about 800 meters.
            double geographicContentWidth = 800;

            // The desired physical width of the scene is 1 meter.
            double tableContainerWidth = 1;

            // Set the translation factor based on the scene content width and desired physical size.
            _arSceneView.TranslationFactor = geographicContentWidth / tableContainerWidth;
        }

        private void ARSceneView_TrackingStateChanged(object sender, ARSCNViewCameraTrackingStateEventArgs e)
        {
            // Provide clear feedback to the user in terms they will understand.
            switch (e.Camera.TrackingState)
            {
                case ARTrackingState.Normal:
                    _arKitStatusLabel.Hidden = true;
                    break;

                case ARTrackingState.NotAvailable:
                    _arKitStatusLabel.Hidden = false;
                    _arKitStatusLabel.Text = "ARKit location not available";
                    break;

                case ARTrackingState.Limited:
                    _arKitStatusLabel.Hidden = false;
                    switch (e.Camera.TrackingStateReason)
                    {
                        case ARTrackingStateReason.ExcessiveMotion:
                            _arKitStatusLabel.Text = "Try moving your device more slowly.";
                            break;

                        case ARTrackingStateReason.Initializing:
                            _arKitStatusLabel.Text = "Keep moving your device.";
                            break;

                        case ARTrackingStateReason.InsufficientFeatures:
                            _arKitStatusLabel.Text = "Try turning on more lights and moving around.";
                            break;

                        case ARTrackingStateReason.Relocalizing:
                            // This won't happen as this sample doesn't use relocalization.
                            break;
                    }

                    break;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _arSceneView.StartTrackingAsync();
        }

        public override async void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (_arSceneView != null)
            {
                _arSceneView.PlanesDetectedChanged -= ARSceneView_PlanesDetectedChanged;
                await _arSceneView.StopTrackingAsync();
            }
        }
    }
}