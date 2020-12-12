// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DisplayScenesInTabletopAR
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display scenes in tabletop AR",
        "Augmented reality",
        "Use augmented reality (AR) to pin a scene to a table or desk for easy exploration.",
        "")]
    public partial class DisplayScenesInTabletopAR : ContentPage, IARSample
    {
        // Scene to be displayed on the tabletop.
        private Scene _tabletopScene;

        public DisplayScenesInTabletopAR()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Display an empty scene to enable tap-to-place.
            MyARSceneView.Scene = new Scene(SceneViewTilingScheme.WebMercator);

            // Render the scene invisible to start.
            MyARSceneView.Scene.BaseSurface.Opacity = 0;

            // Get notification when planes are detected
            MyARSceneView.PlanesDetectedChanged += ARSceneView_PlanesDetectedChanged;
        }

        // This method is called by the sample viewer when the AR sample has appeared.
        async void IARSample.StartAugmentedReality()
        {
            // Start device tracking.
            try
            {
                await MyARSceneView.StartTrackingAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void ARSceneView_PlanesDetectedChanged(object sender, bool planeDetected)
        {
            if (planeDetected)
            {
                EnableTapToPlace();
                MyARSceneView.PlanesDetectedChanged -= ARSceneView_PlanesDetectedChanged;
            }
        }

        private void EnableTapToPlace()
        {
            // Show the help label.
            Device.BeginInvokeOnMainThread(() =>
            {
                HelpLabel.Text = "Tap to place the scene.";
            });

            // Wait for the user to tap.
            MyARSceneView.GeoViewTapped += ARGeoViewTapped;
        }

        private void ARGeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            // If the tapped position is valid, display the scene.
            if (MyARSceneView.SetInitialTransformation(e.Position)) DisplayScene();
        }

        private async void DisplayScene()
        {
            // Hide the help label.
            Device.BeginInvokeOnMainThread(() =>
            {
                HelpLabel.IsVisible = false;
            });

            if (_tabletopScene == null)
            {
                // Load a scene from ArcGIS Online.
                _tabletopScene = new Scene(new Uri("https://www.arcgis.com/home/item.html?id=31874da8a16d45bfbc1273422f772270"));
                await _tabletopScene.LoadAsync();

                // Hide the base surface.
                _tabletopScene.BaseSurface.Opacity = 0;

                // Enable subsurface navigation. This allows you to look at the scene from below.
                _tabletopScene.BaseSurface.NavigationConstraint = NavigationConstraint.None;

                // Set the AR scene to the tabletop scene.
                MyARSceneView.Scene = _tabletopScene;
            }

            // Create a camera at the bottom and center of the scene.
            //    This camera is the point at which the scene is pinned to the real-world surface.
            Camera originCamera = new Camera(52.52083, 13.40944, 8.813445091247559, 0, 90, 0);

            // Set the origin camera.
            MyARSceneView.OriginCamera = originCamera;

            // The width of the scene content is about 800 meters.
            double geographicContentWidth = 800;

            // Set the clipping distance for the scene.
            MyARSceneView.ClippingDistance = geographicContentWidth / 2;

            // The desired physical width of the scene is 1 meter.
            double tableContainerWidth = 1;

            // Set the translation factor based on the scene content width and desired physical size.
            MyARSceneView.TranslationFactor = geographicContentWidth / tableContainerWidth;
        }

        void IARSample.StopAugmentedReality()
        {
            MyARSceneView.StopTrackingAsync();
        }
    }
}