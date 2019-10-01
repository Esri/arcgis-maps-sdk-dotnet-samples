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
using Android.Support.V7.App;
using Android.Widget;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;

namespace ArcGISRuntimeXamarin.Samples.ExploreScenesInFlyoverAR
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Explore scenes in flyover AR",
        "Augmented reality",
        "Use augmented reality (AR) to quickly explore a scene more naturally than you could with a touch or mouse interface.",
        "")]
    public class ExploreScenesInFlyoverAR : AppCompatActivity
    {
        // Hold references to the UI controls.
        private ARSceneView _arSceneView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Explore scenes in flyover AR";

            CreateLayout();
        }

        private void CreateLayout()
        {
            // Create the layout.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the AR scene view.
            _arSceneView = new ARSceneView(this);
            layout.AddView(_arSceneView);

            SetContentView(layout);

            // Request camera permission. Initialize will be called when permissions are granted.
            Initialize();
        }

        private async void Initialize()
        {
            // Create the scene with a basemap.
            Scene flyoverScene = new Scene(Basemap.CreateImagery());

            // Create the integrated mesh layer and add it to the scene.
            IntegratedMeshLayer meshLayer = new IntegratedMeshLayer(new System.Uri("https://www.arcgis.com/home/item.html?id=d4fb271d1cb747e696bb80adca8487fa"));
            flyoverScene.OperationalLayers.Add(meshLayer);

            try
            {
                // Wait for the layer to load so that extent is available.
                await meshLayer.LoadAsync();

                // Start with the camera at the center of the mesh layer.
                Envelope layerExtent = meshLayer.FullExtent;
                Camera originCamera = new Camera(layerExtent.GetCenter().Y, layerExtent.GetCenter().X, 600, 0, 90, 0);
                _arSceneView.OriginCamera = originCamera;

                // Set the translation factor to enable rapid movement through the scene.
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
                ShowMessage("Failed to start AR", "Error starting");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void ShowMessage(string message, string title)
        {
            // Display the message to the user.
            new Android.App.AlertDialog.Builder(this).SetMessage(message).SetTitle(title).Show();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _arSceneView.StopTracking();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            // Start AR tracking without location updates.
            await _arSceneView.StartTrackingAsync(ARLocationTrackingMode.Ignore);
        }

        protected override void OnDestroy()
        {
            _arSceneView.StopTracking();
            base.OnDestroy();
        }
    }
}