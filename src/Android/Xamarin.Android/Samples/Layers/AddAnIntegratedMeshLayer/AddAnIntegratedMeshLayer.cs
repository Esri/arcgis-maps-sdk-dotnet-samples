// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.AddAnIntegratedMeshLayer
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add an integrated mesh layer",
        category: "Layers",
        description: "View an integrated mesh layer from a scene service.",
        instructions: "After launching the sample, watch the integrated mesh layer load in place. Navigate around the scene to visualize the high level of detail on the buildings.",
        tags: new[] { "3D", "integrated mesh", "layers" })]
    public class AddAnIntegratedMeshLayer : Activity
    {
        // Hold a reference to the scene view.
        private SceneView _mySceneView;

        // URLs for the services used by this sample.
        private const string IntegratedMeshLayerUrl =
            "https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/Girona_Spain/SceneServer";

        private const string ElevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Add an integrated mesh layer";

            CreateLayout();
            Initialize();
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

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            _mySceneView = new SceneView(this);
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