// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.SceneLayerSelection
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Scene layer selection",
        category: "Layers",
        description: "Identify features in a scene to select.",
        instructions: "Tap on a building in the scene layer to select it. Deselect buildings by clicking away from the buildings.",
        tags: new[] { "3D", "Berlin", "buildings", "identify", "model", "query", "search", "select" })]
    public partial class SceneLayerSelection : ContentPage
    {
        public SceneLayerSelection()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new Scene with an imagery basemap.
            Scene scene = new Scene(Basemap.CreateImagery());

            // Add a base surface with elevation data.
            Surface elevationSurface = new Surface();
            Uri elevationService = new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");
            elevationSurface.ElevationSources.Add(new ArcGISTiledElevationSource(elevationService));
            scene.BaseSurface = elevationSurface;

            // Add a scene layer.
            Uri buildingsService = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Berlin/SceneServer");
            ArcGISSceneLayer buildingsLayer = new ArcGISSceneLayer(buildingsService);
            scene.OperationalLayers.Add(buildingsLayer);

            // Assign the Scene to the SceneView.
            MySceneView.Scene = scene;

            try
            {
                // Create a camera with an interesting view.
                await buildingsLayer.LoadAsync();
                MapPoint center = (MapPoint)GeometryEngine.Project(buildingsLayer.FullExtent.GetCenter(), SpatialReferences.Wgs84);
                Camera viewCamera = new Camera(center.Y, center.X, 600, 120, 60, 0);

                // Set the viewpoint with the camera.
                MySceneView.SetViewpointCamera(viewCamera);
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private async void SceneViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            // Get the scene layer from the scene (first and only operational layer).
            ArcGISSceneLayer sceneLayer = (ArcGISSceneLayer)MySceneView.Scene.OperationalLayers.First();

            // Clear any existing selection.
            sceneLayer.ClearSelection();

            try
            {
                // Identify the layer at the tap point.
                // Use a 10-pixel tolerance around the point and return a maximum of one feature.
                Point tapPoint = new Point(e.Position.X, e.Position.Y);
                IdentifyLayerResult result = await MySceneView.IdentifyLayerAsync(sceneLayer, tapPoint, 10, false, 1);

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
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }
    }
}