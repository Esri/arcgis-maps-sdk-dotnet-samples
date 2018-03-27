// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerRenderingModeScene
{
    [Register("FeatureLayerRenderingModeScene")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer rendering mode (Scene)",
        "Layers",
        "This sample demonstrates how to use load settings to change the preferred rendering mode for a scene. Static rendering mode only redraws features periodically when a sceneview is navigating, while dynamic mode dynamically re-renders as the scene moves.",
        "Press the 'Animated Zoom' button to trigger a zoom. Observe the differences between the two scenes.")]
    public class FeatureLayerRenderingModeScene : UIViewController
    {
        // Create the UI components
        private UILabel _staticLabel = new UILabel { Text = "Static Mode: " };
        private UILabel _dynamicLabel = new UILabel { Text = "Dynamic Mode: " };
        private UIButton _zoomButton = new UIButton();

        // Create the scene views
        private SceneView _myStaticScene = new SceneView();
        private SceneView _myDynamicScene = new SceneView();

        // Points for demonstrating zoom
        private MapPoint _zoomedOutPoint = new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84);
        private MapPoint _zoomedInPoint = new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84);

        // Viewpoints for each zoom level
        private Camera _zoomedOutCamera;
        private Camera _zoomedInCamera;

        // URI for the feature service
        private string _featureService = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/";

        // Hold the current zoom state
        private bool _zoomed;

        public FeatureLayerRenderingModeScene()
        {
            Title = "Feature layer rendering mode (Scene)";
        }

        private void Initialize()
        {
            // Initialize the cameras (viewpoints) with two points
            _zoomedOutCamera = new Camera(_zoomedOutPoint, 42000, 0, 0, 0);
            _zoomedInCamera = new Camera(_zoomedInPoint, 2500, 90, 75, 0);

            // Create the scene for displaying the feature layer in static mode
            Scene staticScene = new Scene(); // Basemap omitted to make it easier to distinguish the rendering modes
            staticScene.LoadSettings.PreferredPointFeatureRenderingMode = FeatureRenderingMode.Static;
            staticScene.LoadSettings.PreferredPolygonFeatureRenderingMode = FeatureRenderingMode.Static;
            staticScene.LoadSettings.PreferredPolylineFeatureRenderingMode = FeatureRenderingMode.Static;

            // Create the scene for displaying the feature layer in dynamic mode
            Scene dynamicScene = new Scene();
            dynamicScene.LoadSettings.PreferredPointFeatureRenderingMode = FeatureRenderingMode.Dynamic;
            dynamicScene.LoadSettings.PreferredPolygonFeatureRenderingMode = FeatureRenderingMode.Dynamic;
            dynamicScene.LoadSettings.PreferredPolylineFeatureRenderingMode = FeatureRenderingMode.Dynamic;

            // Create the service feature tables
            ServiceFeatureTable faultTable = new ServiceFeatureTable(new Uri(_featureService + "0"));
            ServiceFeatureTable contactTable = new ServiceFeatureTable(new Uri(_featureService + "8"));
            ServiceFeatureTable outcropTable = new ServiceFeatureTable(new Uri(_featureService + "9"));

            // Create the feature layers
            FeatureLayer faultLayer = new FeatureLayer(faultTable);
            FeatureLayer contactLayer = new FeatureLayer(contactTable);
            FeatureLayer outcropLayer = new FeatureLayer(outcropTable);

            // Add the layers to each scene
            staticScene.OperationalLayers.Add(faultLayer);
            staticScene.OperationalLayers.Add(contactLayer);
            staticScene.OperationalLayers.Add(outcropLayer);
            dynamicScene.OperationalLayers.Add(faultLayer.Clone());
            dynamicScene.OperationalLayers.Add(contactLayer.Clone());
            dynamicScene.OperationalLayers.Add(outcropLayer.Clone());

            // Add the scenes to the scene views
            _myStaticScene.Scene = staticScene;
            _myDynamicScene.Scene = dynamicScene;

            // Set the initial viewpoints for the scenes
            _myStaticScene.SetViewpointCamera(_zoomedOutCamera);
            _myDynamicScene.SetViewpointCamera(_zoomedOutCamera);
        }

        private void CreateLayout()
        {
            // Set zoom button text
            _zoomButton.SetTitle("Animated Zoom", UIControlState.Normal);

            // Set label and button colors
            _zoomButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _staticLabel.TextColor = UIColor.Red;
            _dynamicLabel.TextColor = UIColor.Red;

            // Subscribe to button press events
            _zoomButton.TouchUpInside += _zoomButton_TouchUpInside;

            // Add views to page
            View.AddSubviews(_myStaticScene, _myDynamicScene, _staticLabel, _dynamicLabel, _zoomButton);

            // Set view background
            View.BackgroundColor = UIColor.White;
        }

        private void _zoomButton_TouchUpInside(object sender, System.EventArgs e)
        {
            // Zoom out if zoomed
            if (_zoomed)
            {
                _myStaticScene.SetViewpointCameraAsync(_zoomedOutCamera, new TimeSpan(0, 0, 5));
                _myDynamicScene.SetViewpointCameraAsync(_zoomedOutCamera, new TimeSpan(0, 0, 5));
            }
            else // Zoom in otherwise
            {
                _myStaticScene.SetViewpointCameraAsync(_zoomedInCamera, new TimeSpan(0, 0, 5));
                _myDynamicScene.SetViewpointCameraAsync(_zoomedInCamera, new TimeSpan(0, 0, 5));
            }

            // Toggle zoom state
            _zoomed = !_zoomed;
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            var topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            var centerLine = (View.Bounds.Height - topMargin) / 2;

            // Setup the visual frames for the views
            _myStaticScene.Frame = new CoreGraphics.CGRect(0, topMargin, View.Bounds.Width, centerLine - 25);
            _myDynamicScene.Frame = new CoreGraphics.CGRect(0, topMargin + centerLine + 25, View.Bounds.Width, centerLine - 25);
            _staticLabel.Frame = new CoreGraphics.CGRect(10, topMargin, View.Bounds.Width / 2, 50);
            _dynamicLabel.Frame = new CoreGraphics.CGRect(10, centerLine + topMargin - 25, View.Bounds.Width / 2, 50);
            _zoomButton.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2, centerLine + topMargin - 25, View.Bounds.Width / 2, 50);

            base.ViewDidLayoutSubviews();
        }
    }
}