// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.ChangeAtmosphereEffect
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change atmosphere effect",
        category: "Scene",
        description: "Changes the appearance of the atmosphere in a scene.",
        instructions: "Select one of the three available atmosphere effects. The sky will change to display the selected atmosphere effect. ",
        tags: new[] { "atmosphere", "horizon", "sky" })]
    public class ChangeAtmosphereEffect : Activity
    {
        // Hold references to the UI controls.
        private SceneView _mySceneView;
        private Button _realisticOption;
        private Button _horizonOnlyOption;
        private Button _noneOption;

        private readonly string _elevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Change atmosphere effect";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene with a basemap.
            _mySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);
            
            // Add an elevation source to the scene.
            Surface elevationSurface = new Surface();
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            elevationSurface.ElevationSources.Add(elevationSource);
            _mySceneView.Scene.BaseSurface = elevationSurface;

            // Set the initial viewpoint.
            Camera initialCamera = new Camera(64.416919, -14.483728, 100, 318, 105, 0);
            _mySceneView.SetViewpointCamera(initialCamera);

            // Apply the selected atmosphere effect option.
            _realisticOption.Click += (o, e) => _mySceneView.AtmosphereEffect = AtmosphereEffect.Realistic;
            _horizonOnlyOption.Click += (o, e) => _mySceneView.AtmosphereEffect = AtmosphereEffect.HorizonOnly;
            _noneOption.Click += (o, e) => _mySceneView.AtmosphereEffect = AtmosphereEffect.None;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a new horizontal layout to hold buttons for all three options.
            var buttonContainer = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Layout parameters allow buttons to share space equally in the view.
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );

            // Create the scene view.
            _mySceneView = new SceneView(this);

            // Create the buttons and add them to the button container.
            _realisticOption = new Button(this);
            _realisticOption.Text = "Realistic";
            _realisticOption.LayoutParameters = layoutParams;

            _horizonOnlyOption = new Button(this);
            _horizonOnlyOption.Text = "Horizon only";
            _horizonOnlyOption.LayoutParameters = layoutParams;

            _noneOption = new Button(this);
            _noneOption.Text = "None";
            _noneOption.LayoutParameters = layoutParams;

            buttonContainer.AddView(_realisticOption);
            buttonContainer.AddView(_horizonOnlyOption);
            buttonContainer.AddView(_noneOption);

            // Add the views to the layout.
            layout.AddView(buttonContainer);
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Remove the sceneview
            (_mySceneView.Parent as ViewGroup).RemoveView(_mySceneView);
            _mySceneView.Dispose();
            _mySceneView = null;
        }
    }
}
