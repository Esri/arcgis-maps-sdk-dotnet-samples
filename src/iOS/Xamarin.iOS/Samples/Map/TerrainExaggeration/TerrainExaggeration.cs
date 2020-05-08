// Copyright 2019 Esri.
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
        name: "Terrain exaggeration",
        category: "Map",
        description: "Vertically exaggerate terrain in a scene.",
        instructions: "Use the slider to update terrain exaggeration.",
        tags: new[] { "3D", "DEM", "DTM", "elevation", "scene", "surface", "terrain" })]
    public class TerrainExaggeration : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UISlider _terrainSlider;

        // Hold a reference to the elevation surface.
        Surface _elevationSurface;

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
            _elevationSurface = new Surface();
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            _elevationSurface.ElevationSources.Add(elevationSource);

            // Add the surface to the scene.
            _mySceneView.Scene.BaseSurface = _elevationSurface;

            // Set the initial camera.
            MapPoint initialLocation = new MapPoint(-119.9489, 46.7592, 0, SpatialReferences.Wgs84);
            Camera initialCamera = new Camera(initialLocation, 15000, 40, 60, 0);
            _mySceneView.SetViewpointCamera(initialCamera);
        }

        private void TerrainSlider_ValueChanged(object sender, EventArgs e) => _elevationSurface.ElevationExaggeration = _terrainSlider.Value;

        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _terrainSlider = new UISlider();
            _terrainSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _terrainSlider.MinValue = 1;
            _terrainSlider.MaxValue = 3;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(_mySceneView, toolbar);

            // Put the slider in the center with a fixed width.
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(_terrainSlider) {Width = 250},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            // Set the layout constraints.
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _terrainSlider.ValueChanged += TerrainSlider_ValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _terrainSlider.ValueChanged -= TerrainSlider_ValueChanged;
        }
    }
}