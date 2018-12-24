// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.TerrainExaggeration
{
    [Register("TerrainExaggeration")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Terrain exaggeration",
        "Map",
        "Configure the vertical exaggeration of terrain (the ground surface) in a scene.",
        "",
        "Elevation", "terrain", "DTM", "DEM", "surface", "3D", "scene")]
    public class TerrainExaggeration : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UISlider _terrainSlider;

        private readonly string _elevationServiceUrl = "http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public TerrainExaggeration()
        {
            Title = "Terrain exaggeration";
        }

        private void Initialize()
        {
            // Configure the scene with National Geographic basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateNationalGeographic());

            // Add the base surface for elevation data.
            Surface surface = new Surface();
            surface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl)));

            // Add the surface to the scene.
            _mySceneView.Scene.BaseSurface = surface;

            // Set the initial camera.
            MapPoint initialLocation = new MapPoint(-119.9489, 46.7592, 0, SpatialReferences.Wgs84);
            Camera camera = new Camera(initialLocation, 15000, 40, 60, 0);
            _mySceneView.SetViewpointCamera(camera);

            // Update terrain exaggeration based on the slider value.
            _terrainSlider.ValueChanged += (sender, e) => { surface.ElevationExaggeration = _terrainSlider.Value; };
        }

        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _terrainSlider = new UISlider();
            _terrainSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _terrainSlider.MaxValue = 3;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(_mySceneView, toolbar);

            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(_terrainSlider) { Width = 250 },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _mySceneView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor).Active = true;
            _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

            toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}
