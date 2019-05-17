// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntimeXamarin.Samples.CreateTerrainSurfaceFromRaster
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create terrain surface from a local raster",
        "Map",
        "Use a terrain surface with elevation described by a local raster file.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("98092369c4ae4d549bbbd45dba993ebc")]
    public class CreateTerrainSurfaceFromRaster : Activity
    {
        // Hold references to the UI controls.
        private SceneView _mySceneView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Create terrain surface from a local raster";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Get the path to the elevation raster.
            string packagePath = DataManager.GetDataFolder("98092369c4ae4d549bbbd45dba993ebc", "MontereyElevation.dt2");

            // Create the elevation source from a list of paths.
            RasterElevationSource elevationSource = new RasterElevationSource(new[] {packagePath});

            // Create a surface to display the elevation source.
            Surface elevationSurface = new Surface();

            // Add the elevation source to the surface.
            elevationSurface.ElevationSources.Add(elevationSource);

            // Add the surface to the scene.
            _mySceneView.Scene.BaseSurface = elevationSurface;

            // Set an initial camera viewpoint.
            Camera camera = new Camera(36.51, -121.80, 300.0, 0, 70.0, 0.0);
            _mySceneView.SetViewpointCamera(camera);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Add the map view to the layout.
            _mySceneView = new SceneView();
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}