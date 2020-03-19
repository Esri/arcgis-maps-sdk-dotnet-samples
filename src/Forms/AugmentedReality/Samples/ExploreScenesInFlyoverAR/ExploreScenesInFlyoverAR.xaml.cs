// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
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
                MyARSceneView.OriginCamera = originCamera;

                // Set the translation factor to enable rapid movement through the scene.
                MyARSceneView.TranslationFactor = 1000;

                // Enable atmosphere and space effects for a more immersive experience.
                MyARSceneView.SpaceEffect = SpaceEffect.Stars;
                MyARSceneView.AtmosphereEffect = AtmosphereEffect.Realistic;

                // Display the scene.
                await flyoverScene.LoadAsync();
                MyARSceneView.Scene = flyoverScene;
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