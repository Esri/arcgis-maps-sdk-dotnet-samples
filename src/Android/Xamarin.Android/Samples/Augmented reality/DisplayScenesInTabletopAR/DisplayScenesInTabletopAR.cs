// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Mapping;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.DisplayScenesInTabletopAR
{
    [Activity(ConfigurationChanges =
        Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display scenes in tabletop AR",
        "Augmented reality",
        "Use augmented reality (AR) to pin a scene to a table or desk for easy exploration.",
        "You'll see a feed from the camera when you open the sample. Tap on any flat, horizontal surface (like a desk or table) to place the scene. With the scene placed, you can move the camera around the scene to explore. You can also pan and zoom with touch to adjust the position of the scene.",
        "augmented reality", "drop", "mixed reality", "model", "pin", "place", "table-top", "tabletop")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7dd2f97bb007466ea939160d0de96a9d")]
    public class DisplayScenesInTabletopAR : AppCompatActivity
    {
        // Hold references to the UI controls.
        private ARSceneView _arSceneView;
        private TextView _helpLabel;
        private Scene _tabletopScene;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display scenes in tabletop AR";

            CreateLayout();
        }

        private void CreateLayout()
        {
            // Load the layout.
            SetContentView(ArcGISRuntime.Resource.Layout.DisplayScenesInTabletopAR);

            // Get references to the UI controls.
            _arSceneView = FindViewById<ARSceneView>(ArcGISRuntime.Resource.Id.arSceneView);
            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.helpLabel);

            // Request camera permission. Initialize will be called when permissions are granted.
            Initialize();
        }

        private void Initialize()
        {
            // Display an empty scene to enable tap-to-place.
            _arSceneView.Scene = new Scene(SceneViewTilingScheme.Geographic);

            // Render the scene invisible to start.
            _arSceneView.Scene.BaseSurface.Opacity = 0;

            // Wait for the user to tap.
            _arSceneView.GeoViewTapped += ArSceneView_GeoViewTapped;

            // Enable plane rendering.
            _arSceneView.ArSceneView.PlaneRenderer.Enabled = true;
            _arSceneView.ArSceneView.PlaneRenderer.Visible = true;

            // Wait for planes to be detected.
            _arSceneView.PlanesDetectedChanged += ARSceneView_PlaneDetectedChanged;

            _helpLabel.Text = "Keep moving your phone to find a surface.";
        }

        private void ARSceneView_PlaneDetectedChanged(object sender, bool planeDetected)
        {
            if (planeDetected)
            {
                _arSceneView.PlanesDetectedChanged -= ARSceneView_PlaneDetectedChanged;

                _helpLabel.Text = "Tap the dots to place scene.";
            }
        }

        private void ArSceneView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Get the tapped screen point.
            PointF screenPoint = e.Position;

            if (_arSceneView.SetInitialTransformation(screenPoint))
            {
                DisplayScene();
                _helpLabel.Visibility = ViewStates.Gone;
            }
            else
            {
                Toast.MakeText(this, "ARCore doesn't recognize that point as a plane.", ToastLength.Short);
            }
        }

        private async void DisplayScene()
        {
            try
            {
                if (_tabletopScene == null)
                {
                    // Get the downloaded mobile scene package.
                    MobileScenePackage package = await MobileScenePackage.OpenAsync(
                        DataManager.GetDataFolder("7dd2f97bb007466ea939160d0de96a9d", "philadelphia.mspk"));

                    // Load the package.
                    await package.LoadAsync();

                    // Get the first scene.
                    _tabletopScene = package.Scenes.First();

                    // Set the clipping distance for the scene.
                    _arSceneView.ClippingDistance = 400;

                    // Enable subsurface navigation. This allows you to look at the scene from below.
                    _tabletopScene.BaseSurface.NavigationConstraint = NavigationConstraint.None;
                }

                // Display the scene.
                _arSceneView.Scene = _tabletopScene;

                // Create a camera at the bottom and center of the scene.
                //    This camera is the point at which the scene is pinned to the real-world surface.
                var originCamera = new Esri.ArcGISRuntime.Mapping.Camera(39.95787000283599,
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
            catch (System.Exception ex)
            {
                new Android.App.AlertDialog.Builder(this).SetMessage("Failed to load scene").SetTitle("Error").Show();
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        protected override async void OnPause()
        {
            base.OnPause();
            await _arSceneView.StopTrackingAsync();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            // Start AR tracking without location updates.
            await _arSceneView.StartTrackingAsync();
        }
    }
}