// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.AddPointSceneLayer
{
    [Register("AddPointSceneLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add a point scene layer",
        category: "Layers",
        description: "View a point scene layer from a scene service.",
        instructions: "Pan around the scene and zoom in. Notice how many thousands of additional features appear at each successive zoom scale.",
        tags: new[] { "3D", "layers", "point scene layer" })]
    public class AddPointSceneLayer : UIViewController
    {
        // URL for the service with the point scene layer.
        private const string PointSceneServiceUri = "https://tiles.arcgis.com/tiles/V6ZHFr6zdgNZuVG0/arcgis/rest/services/Airports_PointSceneLayer/SceneServer/layers/0";

        // Hold references to UI controls.
        private SceneView _mySceneView;

        public AddPointSceneLayer()
        {
            Title = "Add a point scene layer";
        }

        private void Initialize()
        {
            // Create the scene.
            _mySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Create the layer.
            ArcGISSceneLayer pointSceneLayer = new ArcGISSceneLayer(new Uri(PointSceneServiceUri));

            // Show the layer in the scene.
            _mySceneView.Scene.OperationalLayers.Add(pointSceneLayer);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_mySceneView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}