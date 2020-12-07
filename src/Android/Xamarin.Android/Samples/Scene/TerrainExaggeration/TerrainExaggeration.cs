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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.TerrainExaggeration
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Terrain exaggeration",
        category: "Scene",
        description: "Vertically exaggerate terrain in a scene.",
        instructions: "Use the slider to update terrain exaggeration.",
        tags: new[] { "3D", "DEM", "DTM", "elevation", "scene", "surface", "terrain" })]
    public class TerrainExaggeration : Activity
    {
        // Hold reference to the UI controls.
        private SceneView _mySceneView;
        private SeekBar _terrainSlider;

        private readonly string _elevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Terrain exaggeration";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Configure the scene with National Geographic basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateNationalGeographic());

            // Add the base surface for elevation data.
            Surface elevationSurface = new Surface();
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            elevationSurface.ElevationSources.Add(elevationSource);

            // Add the surface to the scene.
            _mySceneView.Scene.BaseSurface = elevationSurface;

            // Set the initial camera.
            MapPoint initialLocation = new MapPoint(-119.9489, 46.7592, 0, SpatialReferences.Wgs84);
            Camera initialCamera = new Camera(initialLocation, 15000, 40, 60, 0);
            _mySceneView.SetViewpointCamera(initialCamera);

            // Update terrain exaggeration based on the slider value.
            _terrainSlider.ProgressChanged += (sender, e) =>
            {
                // Values are scaled to enable smoother animation - Android Seekbar has a course step size.
                elevationSurface.ElevationExaggeration = 1 + _terrainSlider.Progress / 20.0;
            };
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the views.
            _mySceneView = new SceneView(this);
            _terrainSlider = new SeekBar(this);
            _terrainSlider.Max = 20;

            TextView terrainLabel = new TextView(this);
            terrainLabel.Text = "Terrain exaggeration:";

            // Add the views to the layout.
            layout.AddView(terrainLabel);
            layout.AddView(_terrainSlider);
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
