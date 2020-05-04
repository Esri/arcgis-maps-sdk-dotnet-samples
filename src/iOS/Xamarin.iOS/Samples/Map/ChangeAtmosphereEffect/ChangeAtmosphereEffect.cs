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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChangeAtmosphereEffect
{
    [Register("ChangeAtmosphereEffect")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change atmosphere effect",
        "Map",
        "Change the appearance of the atmosphere in a scene.",
        "",
        "3D", "AtmosphereEffect", "Scene")]
    public class ChangeAtmosphereEffect : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UISegmentedControl _atmosphereEffectPicker;

        private readonly string _elevationServiceUrl = "http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public ChangeAtmosphereEffect()
        {
            Title = "Change atmosphere effect";
        }

        private void Initialize()
        {
            // Create the scene with a basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add an elevation source to the scene.
            Surface elevationSurface = new Surface();
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            elevationSurface.ElevationSources.Add(elevationSource);
            _mySceneView.Scene.BaseSurface = elevationSurface;

            // Set the initial viewpoint.
            Camera initialCamera = new Camera(64.416919, -14.483728, 100, 318, 105, 0);
            _mySceneView.SetViewpointCamera(initialCamera);
        }

        private void Picker_ValuedChanged(object sender, EventArgs e)
        {
            switch (_atmosphereEffectPicker.SelectedSegment)
            {
                case 0:
                    _mySceneView.AtmosphereEffect = AtmosphereEffect.Realistic;
                    break;
                case 1:
                    _mySceneView.AtmosphereEffect = AtmosphereEffect.HorizonOnly;
                    break;
                case 2:
                    _mySceneView.AtmosphereEffect = AtmosphereEffect.None;
                    break;
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _atmosphereEffectPicker = new UISegmentedControl("Realistic", "Horizon only", "None");
            _atmosphereEffectPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            _atmosphereEffectPicker.SelectedSegment = 1;
            _atmosphereEffectPicker.BackgroundColor = UIColor.White;
            _atmosphereEffectPicker.Layer.CornerRadius = 4;
            _atmosphereEffectPicker.ClipsToBounds = true;

            // Add the views.
            View.AddSubviews(_mySceneView, _atmosphereEffectPicker);

            // Lay out the views.
            _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

            _atmosphereEffectPicker.TopAnchor.ConstraintEqualTo(_mySceneView.TopAnchor, 8).Active = true;
            _atmosphereEffectPicker.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8).Active = true;
            _atmosphereEffectPicker.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _atmosphereEffectPicker.ValueChanged += Picker_ValuedChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _atmosphereEffectPicker.ValueChanged -= Picker_ValuedChanged;
        }
    }
}