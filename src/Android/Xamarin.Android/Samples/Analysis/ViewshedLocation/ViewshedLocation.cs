// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: https://www.apache.org/licenses/LICENSE-2.0
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
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntime.Samples.ViewshedLocation
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Viewshed (Location)",
        "Analysis",
        "This sample demonstrates the configurable properties of viewshed analysis, including frustum color, heading, pitch, distances, angles, and location.",
        "Tap anywhere in the scene to change the viewshed observer location.",
        "Featured")]
    public class ViewshedLocation : Activity
    {
        // Create and hold reference to the used SceneView.
        private readonly SceneView _mySceneView = new SceneView();

        // Hold the URL to the elevation source.
        private readonly Uri _localElevationImageService = new Uri("https://scene.arcgis.com/arcgis/rest/services/BREST_DTM_1M/ImageServer");

        // Hold the URL to the buildings scene layer.
        private readonly Uri _buildingsUrl = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");

        // Hold a reference to the viewshed analysis.
        private LocationViewshed _viewshed;

        // Hold a reference to the analysis overlay that will hold the viewshed analysis.
        private AnalysisOverlay _analysisOverlay;

        // References to UI elements.
        private SeekBar _headingSlider;
        private SeekBar _pitchSlider;
        private SeekBar _horizontalAngleSlider;
        private SeekBar _verticalAngleSlider;
        private SeekBar _minimumDistanceSlider;
        private SeekBar _maximumDistanceSlider;
        private Switch _analysisVisibilitySwitch;
        private Switch _frustumVisibilitySwitch;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Viewshed (Location)";

            // Create the layout
            CreateLayout();

            // Initialize the sample
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene with the imagery basemap.
            Scene myScene = new Scene(Basemap.CreateImagery());
            _mySceneView.Scene = myScene;

            // Add the surface elevation.
            Surface mySurface = new Surface();
            mySurface.ElevationSources.Add(new ArcGISTiledElevationSource(_localElevationImageService));
            myScene.BaseSurface = mySurface;

            // Add the scene layer.
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_buildingsUrl);
            myScene.OperationalLayers.Add(sceneLayer);

            // Create the MapPoint representing the initial location.
            MapPoint initialLocation = new MapPoint(-4.5, 48.4, 100.0);

            // Create the location viewshed analysis.
            _viewshed = new LocationViewshed(
                initialLocation,
                _headingSlider.Progress,
                _pitchSlider.Progress,
                _horizontalAngleSlider.Progress,
                _verticalAngleSlider.Progress,
                _minimumDistanceSlider.Progress,
                _maximumDistanceSlider.Progress);

            // Create a camera based on the initial location.
            Camera camera = new Camera(initialLocation, 200.0, 20.0, 70.0, 0.0);

            // Apply the camera to the scene view.
            _mySceneView.SetViewpointCamera(camera);

            // Create an analysis overlay for showing the viewshed analysis.
            _analysisOverlay = new AnalysisOverlay();

            // Add the viewshed analysis to the overlay.
            _analysisOverlay.Analyses.Add(_viewshed);

            // Add the analysis overlay to the SceneView.
            _mySceneView.AnalysisOverlays.Add(_analysisOverlay);

            // Update the frustum outline color.
            // The frustum outline shows the volume in which the viewshed analysis is performed.
            Viewshed.FrustumOutlineColor = System.Drawing.Color.Blue;

            // Subscribe to tap events to enable moving the observer.
            _mySceneView.GeoViewTapped += MySceneViewOnGeoViewTapped;
        }

        private void MySceneViewOnGeoViewTapped(object sender, GeoViewInputEventArgs viewInputEventArgs)
        {
            // Update the viewshed location.
            _viewshed.Location = viewInputEventArgs.Location;
        }

        private void HandleSettingsChange(object sender, EventArgs e)
        {
            // Reset the horizontal and vertical angle if they go out of bounds
            if (_horizontalAngleSlider.Progress < 1) { _horizontalAngleSlider.Progress = 1; }
            if (_verticalAngleSlider.Progress < 1) { _verticalAngleSlider.Progress = 1; }

            // Update the viewshed settings.
            _viewshed.Heading = _headingSlider.Progress;
            _viewshed.Pitch = _pitchSlider.Progress;
            _viewshed.HorizontalAngle = _horizontalAngleSlider.Progress;
            _viewshed.VerticalAngle = _verticalAngleSlider.Progress;
            _viewshed.MinDistance = _minimumDistanceSlider.Progress;
            _viewshed.MaxDistance = _maximumDistanceSlider.Progress;

            // Update visibility of the viewshed analysis.
            _viewshed.IsVisible = _analysisVisibilitySwitch.Checked;

            // Update visibility of the frustum. Note that the frustum will be invisible
            //     regardless of this setting if the viewshed analysis is not visible.
            _viewshed.IsFrustumOutlineVisible = _frustumVisibilitySwitch.Checked;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Heading
            TextView headingLabel = new TextView(this) { Text = "Heading:" };
            _headingSlider = new SeekBar(this) { Max = 360 };
            layout.AddView(headingLabel);
            layout.AddView(_headingSlider);

            // Pitch
            TextView pitchLabel = new TextView(this) { Text = "Pitch:" };
            _pitchSlider = new SeekBar(this) { Max = 180, Progress = 60 };
            layout.AddView(pitchLabel);
            layout.AddView(_pitchSlider);

            // Horizontal Angle
            TextView horizontalAngleLabel = new TextView(this) { Text = "Horizontal Angle:" };
            _horizontalAngleSlider = new SeekBar(this) { Max = 120, Progress = 75 };
            layout.AddView(horizontalAngleLabel);
            layout.AddView(_horizontalAngleSlider);

            // Vertical Angle
            TextView verticalAngleLabel = new TextView(this) { Text = "Vertical Angle:" };
            _verticalAngleSlider = new SeekBar(this) { Max = 120, Progress = 90 };
            layout.AddView(verticalAngleLabel);
            layout.AddView(_verticalAngleSlider);

            // Minimum Distance
            TextView minimumDistanceLabel = new TextView(this) { Text = "Minimum Distance:" };
            _minimumDistanceSlider = new SeekBar(this) { Max = 8999 };
            layout.AddView(minimumDistanceLabel);
            layout.AddView(_minimumDistanceSlider);
            layout.SetHorizontalGravity(GravityFlags.FillHorizontal);

            // Maximum Distance
            TextView maximumDistanceLabel = new TextView(this) { Text = "Maximum Distance:" };
            _maximumDistanceSlider = new SeekBar(this) { Max = 9999, Progress = 1500 };
            layout.AddView(maximumDistanceLabel);
            layout.AddView(_maximumDistanceSlider);

            // Analysis Visibility
            TextView analysisVisibilityLabel = new TextView(this) { Text = "Analysis Visibility:" };
            _analysisVisibilitySwitch = new Switch(this) { Checked = true };
            layout.AddView(analysisVisibilityLabel);
            layout.AddView(_analysisVisibilitySwitch);

            // Frustum Visibility
            TextView frustumVisibilityLabel = new TextView(this) { Text = "Frustum Visibility:" };
            _frustumVisibilitySwitch = new Switch(this) { Checked = false };
            layout.AddView(frustumVisibilityLabel);
            layout.AddView(_frustumVisibilitySwitch);

            // Add the scene view to the layout
            layout.AddView(_mySceneView);

            // Show the layout in the app
            SetContentView(layout);

            // Subscribe to events
            _headingSlider.ProgressChanged += HandleSettingsChange;
            _pitchSlider.ProgressChanged += HandleSettingsChange;
            _horizontalAngleSlider.ProgressChanged += HandleSettingsChange;
            _verticalAngleSlider.ProgressChanged += HandleSettingsChange;
            _minimumDistanceSlider.ProgressChanged += HandleSettingsChange;
            _maximumDistanceSlider.ProgressChanged += HandleSettingsChange;
            _analysisVisibilitySwitch.Click += HandleSettingsChange;
            _frustumVisibilitySwitch.Click += HandleSettingsChange;
        }
    }
}