// Copyright 2019 Esri.
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

namespace ArcGIS.WPF.Samples.ChangeAtmosphereEffect
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Change atmosphere effect",
        category: "Scene",
        description: "Changes the appearance of the atmosphere in a scene.",
        instructions: "Select one of the three available atmosphere effects. The sky will change to display the selected atmosphere effect.",
        tags: new[] { "atmosphere", "horizon", "sky" })]
    public partial class ChangeAtmosphereEffect
    {
        private readonly string _elevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public ChangeAtmosphereEffect()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene with a basemap.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Add an elevation source to the scene.
            Surface elevationSurface = new Surface();
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            elevationSurface.ElevationSources.Add(elevationSource);
            MySceneView.Scene.BaseSurface = elevationSurface;

            // Set the initial viewpoint.
            Camera initialCamera = new Camera(64.416919, -14.483728, 100, 318, 105, 0);
            MySceneView.SetViewpointCamera(initialCamera);

            // Apply the selected atmosphere effect option.
            RealisticOption.Selected += (sender, e) => MySceneView.AtmosphereEffect = AtmosphereEffect.Realistic;
            HorizonOnlyOption.Selected += (sender, e) => MySceneView.AtmosphereEffect = AtmosphereEffect.HorizonOnly;
            NoneOption.Selected += (sender, e) => MySceneView.AtmosphereEffect = AtmosphereEffect.None;
        }
    }
}