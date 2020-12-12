// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ExploreScenesInFlyoverAR
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Explore scenes in flyover AR",
        "Augmented reality",
        "Use augmented reality (AR) to quickly explore a scene more naturally than you could with a touch or mouse interface.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class ExploreScenesInFlyoverAR : ContentPage, IARSample
    {
        public ExploreScenesInFlyoverAR()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the scene.
            Scene flyoverScene = new Scene(new Uri("https://www.arcgis.com/home/item.html?id=76ffb1a9e26b4602a04c209146bf2cd3"));

            try
            {
                // Display the scene.
                await flyoverScene.LoadAsync();
                MyARSceneView.Scene = flyoverScene;

                // Start with the camera at the scenes initial viewpoint.
                Camera originCamera = new Camera(flyoverScene.InitialViewpoint.Camera.Location.Y, flyoverScene.InitialViewpoint.Camera.Location.X, 200, 0, 90, 0);
                MyARSceneView.OriginCamera = originCamera;

                // Set the translation factor to enable rapid movement through the scene.
                MyARSceneView.TranslationFactor = 1000;

                // Enable atmosphere and space effects for a more immersive experience.
                MyARSceneView.SpaceEffect = SpaceEffect.Stars;
                MyARSceneView.AtmosphereEffect = AtmosphereEffect.Realistic;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

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

        void IARSample.StopAugmentedReality()
        {
            MyARSceneView.StopTrackingAsync();
        }
    }
}