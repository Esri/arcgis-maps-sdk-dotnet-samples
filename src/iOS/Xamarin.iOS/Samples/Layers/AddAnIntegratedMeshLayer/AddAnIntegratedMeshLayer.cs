// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.AddAnIntegratedMeshLayer
{
    [Register("AddAnIntegratedMeshLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add an integrated mesh layer",
        category: "Layers",
        description: "View an integrated mesh layer from a scene service.",
        instructions: "Run the sample and watch the integrated mesh layer load in place of the extruded imagery basemap. Navigate around the scene to visualize the high level of detail on the cliffs and valley floor.",
        tags: new[] { "3D", "integrated mesh", "layers" })]
    public class AddAnIntegratedMeshLayer : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;

        // URLs for the services used by this sample.
        private const string IntegratedMeshLayerUrl =
            "https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/Girona_Spain/SceneServer";

        private const string ElevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public AddAnIntegratedMeshLayer()
        {
            Title = "Add an integrated mesh layer";
        }

        private void Initialize()
        {
            // Create the scene with basemap.
            _mySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Create and use an elevation surface to show terrain.
            Surface baseSurface = new Surface();
            baseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri(ElevationServiceUrl)));
            _mySceneView.Scene.BaseSurface = baseSurface;

            // Create the integrated mesh layer from URL.
            IntegratedMeshLayer meshLayer = new IntegratedMeshLayer(new Uri(IntegratedMeshLayerUrl));

            // Add the layer to the scene's operational layers.
            _mySceneView.Scene.OperationalLayers.Add(meshLayer);

            // Start with camera pointing at the scene.
            _mySceneView.SetViewpointCamera(new Camera(new MapPoint(2.8259, 41.9906, 200.0), 190, 65, 0));
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