// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ChangeAtmosphereEffect
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change atmosphere effect",
        category: "Scene",
        description: "Changes the appearance of the atmosphere in a scene.",
        instructions: "Select one of the three available atmosphere effects. The sky will change to display the selected atmosphere effect. ",
        tags: new[] { "atmosphere", "horizon", "sky" })]
    public partial class ChangeAtmosphereEffect : ContentPage
    {
        private readonly string _elevationServiceUrl = "http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public ChangeAtmosphereEffect()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene with a basemap.
            MySceneView.Scene = new Scene(Basemap.CreateImagery());
            
            // Add an elevation source to the scene.
            Surface elevationSurface = new Surface();
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            elevationSurface.ElevationSources.Add(elevationSource);
            MySceneView.Scene.BaseSurface = elevationSurface;

            // Set the initial viewpoint.
            Camera initialCamera = new Camera(64.416919, -14.483728, 100, 318, 105, 0);
            MySceneView.SetViewpointCamera(initialCamera);

            // Configure the picker.
            AtmosphereEffectPicker.ItemsSource = new [] {"Realistic", "Horizon only", "None"};
            AtmosphereEffectPicker.SelectedIndex = 1;

            // Apply the selected atmosphere effect option.
            AtmosphereEffectPicker.SelectedIndexChanged += (o, e) =>
            {
                switch (AtmosphereEffectPicker.SelectedIndex)
                {
                    case 0:
                        MySceneView.AtmosphereEffect = AtmosphereEffect.Realistic;
                        break;
                    case 1:
                        MySceneView.AtmosphereEffect = AtmosphereEffect.HorizonOnly;
                        break;
                    case 2:
                        MySceneView.AtmosphereEffect = AtmosphereEffect.None;
                        break;
                }
            };
        }
    }
}
