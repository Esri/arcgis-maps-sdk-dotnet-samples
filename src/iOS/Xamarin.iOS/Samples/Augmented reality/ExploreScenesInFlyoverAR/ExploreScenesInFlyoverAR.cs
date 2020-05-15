// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ARKit;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ExploreScenesInFlyoverAR
{
    [Register("ExploreScenesInFlyoverAR")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Explore scenes in flyover AR",
        category: "Augmented reality",
        description: "Use augmented reality (AR) to quickly explore a scene more naturally than you could with a touch or mouse interface.",
        instructions: "When you open the sample, you'll be viewing the scene from above. You can walk around, using your device as a window into the scene. Try moving vertically to get closer to the ground. Touch gestures which ",
        tags: new[] { "augmented reality", "bird's eye", "birds-eye-view", "fly over", "flyover", "mixed reality", "translation factor" })]
    public class ExploreScenesInFlyoverAR : UIViewController
    {
        // Hold references to UI controls.
        private ARSceneView _arSceneView;
        private UILabel _arKitStatusLabel;

        public ExploreScenesInFlyoverAR()
        {
            Title = "Explore scenes in flyover AR";
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

            // Add the views.
            View.AddSubviews(_arSceneView, _arKitStatusLabel);

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
                _arKitStatusLabel.HeightAnchor.ConstraintEqualTo(40)
            });

            // Listen for tracking status changes and provide feedback to the user.
            _arSceneView.ARSCNViewCameraDidChangeTrackingState += CameraTrackingStateChanged;
        }

        private async void Initialize()
        {
            // Create the scene with a basemap.
            Scene flyoverScene = new Scene(Basemap.CreateImagery());

            // Create the integrated mesh layer and add it to the scene.
            IntegratedMeshLayer meshLayer =
                new IntegratedMeshLayer(
                    new Uri("https://www.arcgis.com/home/item.html?id=dbc72b3ebb024c848d89a42fe6387a1b"));
            flyoverScene.OperationalLayers.Add(meshLayer);

            try
            {
                // Wait for the layer to load so that extent is available.
                await meshLayer.LoadAsync();

                // Start with the camera at the center of the mesh layer.
                Envelope layerExtent = meshLayer.FullExtent;
                Camera originCamera = new Camera(layerExtent.GetCenter().Y, layerExtent.GetCenter().X, 600, 0, 90, 0);
                _arSceneView.OriginCamera = originCamera;

                // set the translation factor to enable rapid movement through the scene.
                _arSceneView.TranslationFactor = 1000;

                // Enable atmosphere and space effects for a more immersive experience.
                _arSceneView.SpaceEffect = SpaceEffect.Stars;
                _arSceneView.AtmosphereEffect = AtmosphereEffect.Realistic;

                // Display the scene.
                await flyoverScene.LoadAsync();
                _arSceneView.Scene = flyoverScene;
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", "Failed to start AR", (IUIAlertViewDelegate) null, "OK", null).Show();
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void CameraTrackingStateChanged(object sender, ARSCNViewCameraTrackingStateEventArgs e)
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
                await _arSceneView?.StopTrackingAsync();
            }
        }
    }
}