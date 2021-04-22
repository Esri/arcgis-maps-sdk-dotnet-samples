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
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using System;

namespace ArcGISRuntime.Samples.ViewshedCamera
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Viewshed for camera",
        category: "Analysis",
        description: "Analyze the viewshed for a camera. A viewshed shows the visible and obstructed areas from an observer's vantage point. ",
        instructions: "The sample will start with a viewshed created from the initial camera location, so only the visible (green) portion of the viewshed will be visible. Move around the scene to see the obstructed (red) portions. Tap the button to update the viewshed to the current camera position.",
        tags: new[] { "3D", "Scene", "integrated mesh", "viewshed", "visibility analysis" })]
    public class ViewshedCamera : Activity
    {
        // Hold a reference to the scene view
        private SceneView _mySceneView;

        // URL for a scene service of buildings in Girona.
        private string _gironaMeshUrl = "https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/Girona_Spain/SceneServer";

        // URL for an image service to use as an elevation source.
        private string _elevationSourceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        // Location viewshed analysis to show visible and obstructed areas from the camera
        private LocationViewshed _viewshedForCamera;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Viewshed camera";

            // Create the UI
            CreateLayout();

            // Create the Scene, basemap, camera, and location viewshed analysis
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Scene with an imagery basemap
            Scene myScene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Create a scene layer to show buildings in the Scene
            IntegratedMeshLayer meshLayer = new IntegratedMeshLayer(new Uri(_gironaMeshUrl));
            myScene.OperationalLayers.Add(meshLayer);

            // Create an elevation source for the Scene
            ArcGISTiledElevationSource elevationSrc = new ArcGISTiledElevationSource(new Uri(_elevationSourceUrl));
            myScene.BaseSurface.ElevationSources.Add(elevationSrc);

            // Add the Scene to the SceneView
            _mySceneView.Scene = myScene;

            // Set the viewpoint with a new camera focused on the cathedral in Girona.
            Camera observerCamera = new Camera(new MapPoint(2.82691, 41.985, 124.987, SpatialReferences.Wgs84), 332.131, 82.4732, 0.0);
            _mySceneView.SetViewpointCameraAsync(observerCamera);

            // Create a LocationViewshed analysis using the camera as the observer
            _viewshedForCamera = new LocationViewshed(observerCamera, 1, 1000);

            // Create an analysis overlay to contain the viewshed analysis results
            AnalysisOverlay viewshedOverlay = new AnalysisOverlay();

            // Add the location viewshed analysis to the analysis overlay, then add the overlay to the scene view
            viewshedOverlay.Analyses.Add(_viewshedForCamera);
            _mySceneView.AnalysisOverlays.Add(viewshedOverlay);
        }

        private void UpdateObserverWithCamera(object sender, EventArgs e)
        {
            // Use the current camera to update the observer for the location viewshed analysis
            _viewshedForCamera.UpdateFromCamera(_mySceneView.Camera);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a button to update the viewshed using the current camera
            Button updateViewshedButton = new Button(this)
            {
                Text = "Viewshed from here"
            };
            updateViewshedButton.Click += UpdateObserverWithCamera;

            // Add the button and scene view to the layout
            layout.AddView(updateViewshedButton);
            _mySceneView = new SceneView(this);
            layout.AddView(_mySceneView);

            // Show the layout in the app
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