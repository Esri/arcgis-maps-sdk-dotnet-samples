// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace ArcGISRuntime.Samples.ViewshedLocation
{
    [Register("ViewshedLocation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Viewshed (Location)",
        "Analysis",
        "This sample demonstrates the configurable properties of viewshed analysis, including frustum color, heading, pitch, distances, angles, and location.",
        "Tap anywhere in the scene to change the viewshed observer location.",
        "Featured")]
    public class ViewshedLocation : UIViewController
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

        // Create the UI controls.
        private readonly UISlider _headingSlider = new UISlider() { MinValue = 0, MaxValue = 360, Value = 0 };
        private readonly UISlider _pitchSlider = new UISlider() { MinValue = 0, MaxValue = 180, Value = 60 };
        private readonly UISlider _horizontalAngleSlider = new UISlider() { MinValue = 1, MaxValue = 120, Value = 75 };
        private readonly UISlider _verticalAngleSlider = new UISlider { MinValue = 1, MaxValue = 120, Value = 90 };
        private readonly UISlider _minimumDistanceSlider = new UISlider() { MinValue = 0, MaxValue = 8999, Value = 0 };
        private readonly UISlider _maximumDistanceSlider = new UISlider() { MinValue = 0, MaxValue = 9999, Value = 1500 };
        private readonly UISwitch _analysisVisibilitySwitch = new UISwitch() { On = true };
        private readonly UISwitch _frustumVisibilitySwitch = new UISwitch() { On = false };

        // Create labels for the UI controls.
        private readonly UILabel _headingLabel = new UILabel() { Text = "Heading:", TextColor = UIColor.Red };
        private readonly UILabel _pitchLabel = new UILabel() { Text = "Pitch", TextColor = UIColor.Red };
        private readonly UILabel _horizontalAngleLabel = new UILabel() { Text = "Horiz. Angle:", TextColor = UIColor.Red };
        private readonly UILabel _verticalAngleLabel = new UILabel() { Text = "Vert. Angle:", TextColor = UIColor.Red };
        private readonly UILabel _minimumDistanceLabel = new UILabel() { Text = "Min. Dist.:", TextColor = UIColor.Red };
        private readonly UILabel _maximumDistanceLabel = new UILabel() { Text = "Max. Dist.:", TextColor = UIColor.Red };
        private readonly UILabel _analysisVisibilityLabel = new UILabel() { Text = "Show Analysis:", TextColor = UIColor.Red };
        private readonly UILabel _frustumVisibilityLabel = new UILabel() { Text = "Show Frustum:", TextColor = UIColor.Red };

        public ViewshedLocation()
        {
            Title = "Viewshed (Location)";
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
                _headingSlider.Value,
                _pitchSlider.Value,
                _horizontalAngleSlider.Value,
                _verticalAngleSlider.Value,
                _minimumDistanceSlider.Value,
                _maximumDistanceSlider.Value);

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
            Viewshed.FrustumOutlineColor = Color.Blue;

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
            // Update the viewshed settings.
            _viewshed.Heading = _headingSlider.Value;
            _viewshed.Pitch = _pitchSlider.Value;
            _viewshed.HorizontalAngle = _horizontalAngleSlider.Value;
            _viewshed.VerticalAngle = _verticalAngleSlider.Value;
            _viewshed.MinDistance = _minimumDistanceSlider.Value;
            _viewshed.MaxDistance = _maximumDistanceSlider.Value;

            // Update visibility of the viewshed analysis.
            _viewshed.IsVisible = _analysisVisibilitySwitch.On;

            // Update visibility of the frustum. Note that the frustum will be invisible
            //     regardless of this setting if the viewshed analysis is not visible.
            _viewshed.IsFrustumOutlineVisible = _frustumVisibilitySwitch.On;
        }

        private void CreateLayout()
        {
            // Add SceneView to the page
            View.AddSubviews(_mySceneView);

            // Add the labels
            View.AddSubviews(_headingLabel, _pitchLabel, _horizontalAngleLabel, _verticalAngleLabel,
                _minimumDistanceLabel, _maximumDistanceLabel, _analysisVisibilityLabel, _frustumVisibilityLabel);

            // Add the controls
            View.AddSubviews(_headingSlider, _pitchSlider, _horizontalAngleSlider, _verticalAngleSlider,
                _minimumDistanceSlider, _maximumDistanceSlider, _analysisVisibilitySwitch, _frustumVisibilitySwitch);

            // Subscribe to events
            _headingSlider.ValueChanged += HandleSettingsChange;
            _pitchSlider.ValueChanged += HandleSettingsChange;
            _horizontalAngleSlider.ValueChanged += HandleSettingsChange;
            _verticalAngleSlider.ValueChanged += HandleSettingsChange;
            _minimumDistanceSlider.ValueChanged += HandleSettingsChange;
            _maximumDistanceSlider.ValueChanged += HandleSettingsChange;
            _analysisVisibilitySwitch.ValueChanged += HandleSettingsChange;
            _frustumVisibilitySwitch.ValueChanged += HandleSettingsChange;
        }

        public override void ViewDidLoad()
        {
            // Create the layout.
            CreateLayout();

            // Initialize the sample.
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the SceneView
            _mySceneView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Top of visible area is offset by the height of the navigation controller bar and the height of the status bar
            nfloat topMargin = NavigationController.NavigationBar.Frame.Height +
                               UIApplication.SharedApplication.StatusBarFrame.Height;

            // Each row will have consistent height.
            nfloat rowHeight = 30;

            // Labels will be of consistent width.
            nfloat labelWidth = 100;

            // Heading
            topMargin += rowHeight;
            _headingLabel.Frame = new CGRect(10, topMargin, labelWidth, rowHeight);
            _headingSlider.Frame = new CGRect(labelWidth + 10, topMargin, View.Bounds.Width - labelWidth - 10, rowHeight);

            // Pitch
            topMargin += rowHeight;
            _pitchLabel.Frame = new CGRect(10, topMargin, labelWidth, rowHeight);
            _pitchSlider.Frame = new CGRect(labelWidth + 10, topMargin, View.Bounds.Width - labelWidth - 10, rowHeight);

            // Horizontal Angle
            topMargin += rowHeight;
            _horizontalAngleLabel.Frame = new CGRect(10, topMargin, labelWidth, rowHeight);
            _horizontalAngleSlider.Frame = new CGRect(labelWidth + 10, topMargin, View.Bounds.Width - labelWidth - 10, rowHeight);

            // Vertical Angle
            topMargin += rowHeight;
            _verticalAngleLabel.Frame = new CGRect(10, topMargin, labelWidth, rowHeight);
            _verticalAngleSlider.Frame = new CGRect(labelWidth + 10, topMargin, View.Bounds.Width - labelWidth - 10, rowHeight);

            // Min Distance
            topMargin += rowHeight;
            _minimumDistanceLabel.Frame = new CGRect(10, topMargin, labelWidth, rowHeight);
            _minimumDistanceSlider.Frame = new CGRect(labelWidth + 10, topMargin, View.Bounds.Width - labelWidth - 10, rowHeight);

            // Max Distance
            topMargin += rowHeight;
            _maximumDistanceLabel.Frame = new CGRect(10, topMargin, labelWidth, rowHeight);
            _maximumDistanceSlider.Frame = new CGRect(labelWidth + 10, topMargin, View.Bounds.Width - labelWidth - 10, rowHeight);

            // Analysis Visibility
            topMargin += rowHeight;
            _analysisVisibilityLabel.Frame = new CGRect(10, topMargin, View.Bounds.Width - labelWidth - 10, rowHeight);
            _analysisVisibilitySwitch.Frame = new CGRect(labelWidth * 2, topMargin, labelWidth, rowHeight);

            // Frustum Visibility
            topMargin += rowHeight;
            _frustumVisibilityLabel.Frame = new CGRect(10, topMargin, View.Bounds.Width - labelWidth - 10, rowHeight);
            _frustumVisibilitySwitch.Frame = new CGRect(labelWidth * 2, topMargin, labelWidth, rowHeight);

            base.ViewDidLayoutSubviews();
        }
    }
}