// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntime.Samples.SceneLayerSelection
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Scene layer selection",
        "Layers",
        "This sample demonstrates how to identify geoelements in a scene layer.",
        "Tap/Click on a building in the scene layer to identify it.",
        "Scene, Identify")]
    public class SceneLayerSelection : Activity
    {
        // A SceneView control to show the scene layer.
        private SceneView _mySceneView = new SceneView();
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Scene layer selection";

            // Create the UI (SceneView control).
            CreateLayout();

            // Create the scene and display it in the scene view.
            Initialize();
        }
        
        private void Initialize()
        {
            // Create a new Scene with an imagery basemap.
            Scene scene = new Scene(Basemap.CreateImagery());

            // Add a base surface with elevation data.
            Surface elevationSurface = new Surface();
            Uri elevationService = new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");
            elevationSurface.ElevationSources.Add(new ArcGISTiledElevationSource(elevationService));
            scene.BaseSurface = elevationSurface;

            // Add a scene layer of buildings in Brest, France.
            Uri buildingsService = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");
            ArcGISSceneLayer buildingsLayer = new ArcGISSceneLayer(buildingsService);
            scene.OperationalLayers.Add(buildingsLayer);

            // Assign the Scene to the SceneView.
            _mySceneView.Scene = scene;

            // Create a camera targeting the buildings in Brest.
            Camera brestCamera = new Camera(48.378, -4.494, 200, 345, 65, 0);

            // Set the viewpoint with the camera.
            _mySceneView.SetViewpointCameraAsync(brestCamera);

            // Listen for taps.
            _mySceneView.GeoViewTapped += SceneViewTapped;
        }

        private async void SceneViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the scene layer from the scene (first and only operational layer).
            ArcGISSceneLayer sceneLayer = (ArcGISSceneLayer)_mySceneView.Scene.OperationalLayers.First();

            // Clear any existing selection.
            sceneLayer.ClearSelection();

            // Identify the layer at the tap point.
            // Use a 10-pixel tolerance around the point and return a maximum of one feature.
            IdentifyLayerResult result = await _mySceneView.IdentifyLayerAsync(sceneLayer, e.Position, 10, false, 1);

            // Get the GeoElements that were identified (will be 0 or 1 element).
            IReadOnlyList<GeoElement> geoElements = result.GeoElements;

            // If a GeoElement was identified, select it in the scene.
            if (geoElements.Any())
            {
                GeoElement geoElement = geoElements.FirstOrDefault();
                if (geoElement != null)
                {
                    // Select the feature to highlight it in the scene view.
                    sceneLayer.SelectFeature((Feature)geoElement);
                }
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create and add a help label.
            TextView helpLabel = new TextView(this)
            {
                Text = "Tap to select buildings."
            };
            layout.AddView(helpLabel);

            // Add the scene view to the layout.
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}