// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ViewshedCamera
{
    [Register("ViewshedCamera")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Viewshed for camera",
        "Analysis",
        "This sample demonstrates how to create a `LocationViewshed` to display interactive viewshed results in the scene view. The viewshed observer is defined by the scene view camera to evaluate visible and obstructed areas of the scene from that location.",
        "", "Featured")]
    public class ViewshedCamera : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly SceneView _mySceneView = new SceneView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UIButton _updateViewshedButton;

        // URL for a scene service of buildings in Brest, France.
        private const string BuildingsServiceUrl = "http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0";

        // URL for an image service to use as an elevation source.
        private const string ElevationSourceUrl = "http://scene.arcgis.com/arcgis/rest/services/BREST_DTM_1M/ImageServer";

        // Location viewshed analysis to show visible and obstructed areas from the camera.
        private LocationViewshed _viewshedForCamera;


        public ViewshedCamera()
        {
            Title = "Viewshed from camera";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Scene with an imagery basemap.
            Scene myScene = new Scene(Basemap.CreateImagery());

            // Create a scene layer to show buildings in the Scene.
            myScene.OperationalLayers.Add(new ArcGISSceneLayer(new Uri(BuildingsServiceUrl)));

            // Create an elevation source for the Scene.
            myScene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri(ElevationSourceUrl)));

            // Add the Scene to the SceneView.
            _mySceneView.Scene = myScene;

            // Set the viewpoint with a new camera focused on the castle in Brest.
            Camera observerCamera = new Camera(new MapPoint(-4.49492, 48.3808, 48.2511, SpatialReferences.Wgs84), 344.488, 74.1212, 0.0);
            _mySceneView.SetViewpointCameraAsync(observerCamera);

            // Create a LocationViewshed analysis using the camera as the observer.
            _viewshedForCamera = new LocationViewshed(observerCamera, 1, 1000);

            // Create an analysis overlay to contain the viewshed analysis results.
            AnalysisOverlay viewshedOverlay = new AnalysisOverlay();

            // Add the location viewshed analysis to the analysis overlay, then add the overlay to the scene view.
            viewshedOverlay.Analyses.Add(_viewshedForCamera);
            _mySceneView.AnalysisOverlays.Add(viewshedOverlay);
        }

        private void UpdateObserverWithCamera(object sender, EventArgs e)
        {
            // Use the current camera to update the observer for the location viewshed analysis.
            _viewshedForCamera.UpdateFromCamera(_mySceneView.Camera);
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _mySceneView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _mySceneView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _updateViewshedButton.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void CreateLayout()
        {
            // Create a button to update the viewshed using the current camera.
            _updateViewshedButton = new UIButton();
            _updateViewshedButton.SetTitle("Viewshed from here", UIControlState.Normal);
            _updateViewshedButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _updateViewshedButton.TouchUpInside += UpdateObserverWithCamera;

            // Add controls to the view.
            View.AddSubviews(_mySceneView, _toolbar, _updateViewshedButton);
        }
    }
}