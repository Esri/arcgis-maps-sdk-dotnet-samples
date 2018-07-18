// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
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
        // Create and hold references to the UI controls.
        private readonly SceneView _mySceneView = new SceneView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UISlider _headingSlider = new UISlider {MinValue = 0, MaxValue = 360, Value = 0};
        private readonly UISlider _pitchSlider = new UISlider {MinValue = 0, MaxValue = 180, Value = 60};
        private readonly UISlider _horizontalAngleSlider = new UISlider {MinValue = 1, MaxValue = 120, Value = 75};
        private readonly UISlider _verticalAngleSlider = new UISlider {MinValue = 1, MaxValue = 120, Value = 90};
        private readonly UISlider _minimumDistanceSlider = new UISlider {MinValue = 11, MaxValue = 8999, Value = 11};
        private readonly UISlider _maximumDistanceSlider = new UISlider {MinValue = 0, MaxValue = 9999, Value = 1500};
        private readonly UISwitch _analysisVisibilitySwitch = new UISwitch {On = true};
        private readonly UISwitch _frustumVisibilitySwitch = new UISwitch {On = false};
        private readonly UILabel _headingLabel = new UILabel {Text = "Heading:", TextAlignment = UITextAlignment.Right};
        private readonly UILabel _pitchLabel = new UILabel {Text = "Pitch:", TextAlignment = UITextAlignment.Right};
        private readonly UILabel _horizontalAngleLabel = new UILabel {Text = "Horiz. Angle:", TextAlignment = UITextAlignment.Right};
        private readonly UILabel _verticalAngleLabel = new UILabel {Text = "Vert. Angle:", TextAlignment = UITextAlignment.Right};
        private readonly UILabel _minimumDistanceLabel = new UILabel {Text = "Min. Dist.:", TextAlignment = UITextAlignment.Right};
        private readonly UILabel _maximumDistanceLabel = new UILabel {Text = "Max. Dist.:", TextAlignment = UITextAlignment.Right};
        private readonly UILabel _analysisVisibilityLabel = new UILabel {Text = "Show Analysis:", TextAlignment = UITextAlignment.Right};
        private readonly UILabel _frustumVisibilityLabel = new UILabel {Text = "Show Frustum:", TextAlignment = UITextAlignment.Right};

        // Hold the URL to the elevation source.
        private readonly Uri _localElevationImageService = new Uri("https://scene.arcgis.com/arcgis/rest/services/BREST_DTM_1M/ImageServer");

        // Hold the URL to the buildings scene layer.
        private readonly Uri _buildingsUrl = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");

        // Hold a reference to the viewshed analysis.
        private LocationViewshed _viewshed;

        // Hold a reference to the analysis overlay that will hold the viewshed analysis.
        private AnalysisOverlay _analysisOverlay;

        // Graphics overlay for viewpoint symbol.
        private GraphicsOverlay _viewpointOverlay;

        // Symbol for viewpoint.
        private SimpleMarkerSceneSymbol _viewpointSymbol;

        public ViewshedLocation()
        {
            Title = "Viewshed (location)";
        }

        private void Initialize()
        {
            // Create the scene with the imagery basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add the surface elevation.
            Surface mySurface = new Surface();
            mySurface.ElevationSources.Add(new ArcGISTiledElevationSource(_localElevationImageService));
            _mySceneView.Scene.BaseSurface = mySurface;

            // Add the scene layer.
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_buildingsUrl);
            _mySceneView.Scene.OperationalLayers.Add(sceneLayer);

            // Create the MapPoint representing the initial location.
            MapPoint initialLocation = new MapPoint(-4.5, 48.4, 56.0);

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

            // Create a symbol for the viewpoint.
            _viewpointSymbol = SimpleMarkerSceneSymbol.CreateSphere(Color.Blue, 10, SceneSymbolAnchorPosition.Center);

            // Add the symbol to the viewpoint overlay.
            _viewpointOverlay = new GraphicsOverlay
            {
                SceneProperties = new LayerSceneProperties(SurfacePlacement.Absolute)
            };
            _viewpointOverlay.Graphics.Add(new Graphic(initialLocation, _viewpointSymbol));

            // Add the analysis overlay to the SceneView.
            _mySceneView.AnalysisOverlays.Add(_analysisOverlay);

            // Add the graphics overlay
            _mySceneView.GraphicsOverlays.Add(_viewpointOverlay);

            // Update the frustum outline color.
            // The frustum outline shows the volume in which the viewshed analysis is performed.
            Viewshed.FrustumOutlineColor = Color.Blue;

            // Subscribe to tap events to enable moving the observer.
            _mySceneView.GeoViewTapped += MySceneView_GeoViewTapped;
        }

        private void MySceneView_GeoViewTapped(object sender, GeoViewInputEventArgs viewInputEventArgs)
        {
            // Update the viewshed location.
            _viewshed.Location = viewInputEventArgs.Location;

            // Move the location off of the ground.
            _viewshed.Location = new MapPoint(_viewshed.Location.X, _viewshed.Location.Y, _viewshed.Location.Z + 10.0);

            // Update the viewpoint symbol.
            _viewpointOverlay.Graphics.Clear();
            _viewpointOverlay.Graphics.Add(new Graphic(_viewshed.Location, _viewpointSymbol));
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
            // Add SceneView to the page.
            View.AddSubviews(_mySceneView, _toolbar);

            // Add the labels.
            View.AddSubviews(_headingLabel, _pitchLabel, _horizontalAngleLabel, _verticalAngleLabel,
                _minimumDistanceLabel, _maximumDistanceLabel, _analysisVisibilityLabel, _frustumVisibilityLabel);

            // Add the controls.
            View.AddSubviews(_headingSlider, _pitchSlider, _horizontalAngleSlider, _verticalAngleSlider,
                _minimumDistanceSlider, _maximumDistanceSlider, _analysisVisibilitySwitch, _frustumVisibilitySwitch);

            // Subscribe to events.
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
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat rowHeight = 30;
                nfloat labelWidth = 125;
                nfloat margin = 5;

                // Reposition the controls.
                _mySceneView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, rowHeight * 8 + margin * 9);
                _mySceneView.ViewInsets = new UIEdgeInsets(_toolbar.Frame.Bottom, 0, 0, 0);

                // Heading
                topMargin += margin;
                _headingLabel.Frame = new CGRect(margin, topMargin, labelWidth - 2 * margin, rowHeight);
                _headingSlider.Frame = new CGRect(labelWidth + margin, topMargin, View.Bounds.Width - labelWidth - 2 * margin, rowHeight);

                // Pitch
                topMargin += rowHeight + margin;
                _pitchLabel.Frame = new CGRect(margin, topMargin, labelWidth - 2 * margin, rowHeight);
                _pitchSlider.Frame = new CGRect(labelWidth + margin, topMargin, View.Bounds.Width - labelWidth - 2 * margin, rowHeight);

                // Horizontal Angle
                topMargin += rowHeight + margin;
                _horizontalAngleLabel.Frame = new CGRect(margin, topMargin, labelWidth - 2 * margin, rowHeight);
                _horizontalAngleSlider.Frame = new CGRect(labelWidth + margin, topMargin, View.Bounds.Width - labelWidth - 2 * margin, rowHeight);

                // Vertical Angle
                topMargin += rowHeight + margin;
                _verticalAngleLabel.Frame = new CGRect(margin, topMargin, labelWidth - 2 * margin, rowHeight);
                _verticalAngleSlider.Frame = new CGRect(labelWidth + margin, topMargin, View.Bounds.Width - labelWidth - 2 * margin, rowHeight);

                // Min Distance
                topMargin += rowHeight + margin;
                _minimumDistanceLabel.Frame = new CGRect(margin, topMargin, labelWidth - 2 * margin, rowHeight);
                _minimumDistanceSlider.Frame = new CGRect(labelWidth + margin, topMargin, View.Bounds.Width - labelWidth - 2 * margin, rowHeight);

                // Max Distance
                topMargin += rowHeight + margin;
                _maximumDistanceLabel.Frame = new CGRect(margin, topMargin, labelWidth - 2 * margin, rowHeight);
                _maximumDistanceSlider.Frame = new CGRect(labelWidth + margin, topMargin, View.Bounds.Width - labelWidth - 2 * margin, rowHeight);

                // Analysis Visibility
                topMargin += rowHeight + margin;
                _analysisVisibilityLabel.Frame = new CGRect(margin, topMargin, labelWidth - 2 * margin, rowHeight);
                _analysisVisibilitySwitch.Frame = new CGRect(labelWidth + margin, topMargin, View.Bounds.Width - labelWidth - 2 * margin, rowHeight);

                // Frustum Visibility
                topMargin += rowHeight + margin;
                _frustumVisibilityLabel.Frame = new CGRect(margin, topMargin, labelWidth - 2 * margin, rowHeight);
                _frustumVisibilitySwitch.Frame = new CGRect(labelWidth + margin, topMargin, View.Bounds.Width - labelWidth - 2 * margin, rowHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}