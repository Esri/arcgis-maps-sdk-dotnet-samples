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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using UIKit;

namespace ArcGISRuntime.Samples.SceneLayerSelection
{
    [Register("SceneLayerSelection")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Scene layer selection",
        "Layers",
        "Identify GeoElements in a scene layer.",
        "Tap/Click on a building in the scene layer to identify it.",
        "")]
    public class SceneLayerSelection : UIViewController
    {
        // Hold a reference to the UI control.
        private SceneView _mySceneView;

        public SceneLayerSelection()
        {
            Title = "Scene layer selection";
        }

        public override void LoadView()
        {
            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView();
            View.AddSubviews(_mySceneView);

            _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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
            _mySceneView.Scene = scene;

            try
            {
                // Create a camera with an interesting view.
                await buildingsLayer.LoadAsync();
                MapPoint center = (MapPoint) GeometryEngine.Project(buildingsLayer.FullExtent.GetCenter(), SpatialReferences.Wgs84);
                Camera viewCamera = new Camera(center.Y, center.X, 600, 120, 60, 0);

                // Set the viewpoint with the camera.
                _mySceneView.SetViewpointCamera(viewCamera);

                // Listen for taps.
                _mySceneView.GeoViewTapped += SceneViewTapped;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async void SceneViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the scene layer from the scene (first and only operational layer).
            ArcGISSceneLayer sceneLayer = (ArcGISSceneLayer) _mySceneView.Scene.OperationalLayers.First();

            // Clear any existing selection.
            sceneLayer.ClearSelection();

            try
            {
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
                        sceneLayer.SelectFeature((Feature) geoElement);
                    }
                }
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }
    }
}