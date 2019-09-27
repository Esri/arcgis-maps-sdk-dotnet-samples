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
using SceneKit;
using System;
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

        public DisplayScenesInTabletopAR()
        {
            Title = "Display scenes in tabletop AR";
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();

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
            NSLayoutConstraint.ActivateConstraints(new[]{
                _arSceneView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _arSceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _arSceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _arSceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _arKitStatusLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _arKitStatusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _arKitStatusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _arKitStatusLabel.HeightAnchor.ConstraintEqualTo(40),
                _helpLabel.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });

            // Listen for tracking status changes and provide feedback to the user.
            _arSceneView.PlanesDetectedChanged += (o, e) =>
             {
                 BeginInvokeOnMainThread(EnableTapToPlace);
             };
            _arSceneView.ARSCNViewCameraDidChangeTrackingState += CameraTrackingStateDidChange;
        }

        private void Initialize()
        {
            // Display an empty scene to enable tap-to-place.
            _arSceneView.Scene = new Scene(SceneViewTilingScheme.Geographic);

            // Render the scene invisible to start.
            _arSceneView.Scene.BaseSurface.Opacity = 0;
        }

        private void EnableTapToPlace()
        {
            _helpLabel.Hidden = false;

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

        private async void DisplayScene(double planeWidth = 1)
        {
            // Hide the help label.
            _helpLabel.Hidden = true;

            // Get the downloaded mobile scene package.
            MobileScenePackage package = await MobileScenePackage.OpenAsync(DataManager.GetDataFolder("7dd2f97bb007466ea939160d0de96a9d", "philadelphia.mspk"));

            // Load the package.
            await package.LoadAsync();

            // Get the first scene.
            Scene philadelphiaScene = package.Scenes.First();

            // Hide the base surface.
            philadelphiaScene.BaseSurface.Opacity = 0;

            // Enable subsurface navigation. This allows you to look at the scene from below.
            philadelphiaScene.BaseSurface.NavigationConstraint = NavigationConstraint.None;

            // Display the scene.
            _arSceneView.Scene = philadelphiaScene;

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
            double tableContainerWidth = planeWidth;

            // Set the translation facotr based on the scene content width and desired physical size.
            _arSceneView.TranslationFactor = geographicContentWidth / tableContainerWidth;
        }

        private void CameraTrackingStateDidChange(object sender, ARSCNViewCameraTrackingStateEventArgs e)
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

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _arSceneView?.StopTracking();
        }

        // Delegate object to receive notifications from ARKit.
        private class SessionDelegate : ARSCNViewDelegate
        {
            private SCNMaterial _planeRenderingMaterial;

            public bool ShouldRenderPlanes { get; set; } = true;
            public bool HasFoundPlane { get; set; } = false;

            public SessionDelegate()
            {
                _planeRenderingMaterial = new SCNMaterial();
                _planeRenderingMaterial.DoubleSided = false;
                _planeRenderingMaterial.Diffuse.ContentColor = UIColor.FromRGBA(0.5f, 0, 0, 0.5f);
            }

            // Expose an event for listening for camera changes specifically.
            public event EventHandler<ARTrackingStateEventArgs> CameraTrackingStateDidChange;

            public event EventHandler FirstPlaneFound;

            public override void CameraDidChangeTrackingState(ARSession session, ARCamera camera) => CameraTrackingStateDidChange?.Invoke(this, new ARTrackingStateEventArgs { Camera = camera, Session = session });

            public override void DidUpdateNode(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
            {
                if (!ShouldRenderPlanes)
                {
                    node.RemoveFromParentNode();
                    return;
                }

                if (anchor is ARPlaneAnchor planeAnchor)
                {
                    ARPlaneGeometry geometry = planeAnchor.Geometry;

                    ARSCNPlaneGeometry scenePlaneGeometry = ARSCNPlaneGeometry.Create(renderer.GetDevice());

                    scenePlaneGeometry.Update(geometry);

                    scenePlaneGeometry.Materials = new[] { _planeRenderingMaterial };

                    node.ChildNodes.First().Geometry = scenePlaneGeometry;
                }
            }

            public override void DidAddNode(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
            {
                if (!HasFoundPlane)
                {
                    HasFoundPlane = true;
                    FirstPlaneFound?.Invoke(this, EventArgs.Empty);
                }

                if (!ShouldRenderPlanes)
                {
                    return;
                }

                if (anchor is ARPlaneAnchor planeAnchor)
                {
                    ARPlaneGeometry geometry = planeAnchor.Geometry;

                    ARSCNPlaneGeometry scenePlaneGeometry = ARSCNPlaneGeometry.Create(renderer.GetDevice());

                    scenePlaneGeometry.Update(geometry);

                    SCNNode newNode = SCNNode.FromGeometry(scenePlaneGeometry);

                    node.AddChildNode(newNode);

                    scenePlaneGeometry.Materials = new[] { _planeRenderingMaterial };

                    node.Geometry = scenePlaneGeometry;
                }
            }
        }

        private class ARTrackingStateEventArgs
        {
            public ARSession Session { get; set; }
            public ARCamera Camera { get; set; }
        }
    }
}