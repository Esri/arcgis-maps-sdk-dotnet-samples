// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;

namespace ArcGISRuntime.UWP.Samples.ViewshedLocation
{
    public partial class ViewshedLocation
    {
        // URL to the elevation source
        private readonly Uri _localElevationImageService = new Uri("http://scene.arcgis.com/arcgis/rest/services/BREST_DTM_1M/ImageServer");

        // URL to the buildings scene layer
        private readonly Uri _buildingsUrl = new Uri("http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");

        // Reference to the viewshed analysis
        private LocationViewshed _viewshed;

        // Reference to the analysis overlay that will hold the viewshed analysis
        private AnalysisOverlay _analysisOverlay;

        // Flag indicating if the viewshed will move with the mouse
        private bool _subscribedToMouseMoves;

        public ViewshedLocation()
        {
            InitializeComponent();

            // Setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene with the imagery basemap.
            Scene myScene = new Scene(Basemap.CreateImagery());
            MySceneView.Scene = myScene;

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
                HeadingSlider.Value, 
                PitchSlider.Value, 
                HorizontalAngleSlider.Value, 
                VerticalAngleSlider.Value, 
                MinimumDistanceSlider.Value, 
                MaximumDistanceSlider.Value);

            // Create an initial camera based on the initial location.
            Camera camera = new Camera(initialLocation, 200.0, 20.0, 70.0, 0.0);

            // Apply the camera to the scene view.
            MySceneView.SetViewpointCamera(camera);

            // Create an analysis overlay for showing the viewshed analysis.
            _analysisOverlay = new AnalysisOverlay();

            // Add the viewshed analysis to the overlay.
            _analysisOverlay.Analyses.Add(_viewshed);

            // Add the analysis overlay to the SceneView.
            MySceneView.AnalysisOverlays.Add(_analysisOverlay);

            // Update the frustum outline color.
            Viewshed.FrustumOutlineColor = Color.FromArgb(255, 0, 0, 255);

            // Subscribe to tap events to enable moving the observer. 
            MySceneView.GeoViewTapped += MySceneViewOnGeoViewTapped;
        }

        private void MySceneViewOnGeoViewTapped(object sender, GeoViewInputEventArgs geoViewInputEventArgs)
        {
            // Update the viewshed location.
            _viewshed.Location = geoViewInputEventArgs.Location;
        }

        private void HandleSettingsChange(object sender, RoutedEventArgs e)
        {
            // Return if viewshed hasn't been created yet. This happens when the sample is starting.
            if (_viewshed == null)
            {
                return;
            }
            
            // Update the viewshed settings.
            _viewshed.Heading = HeadingSlider.Value;
            _viewshed.Pitch = PitchSlider.Value;
            _viewshed.HorizontalAngle = HorizontalAngleSlider.Value;
            _viewshed.VerticalAngle = VerticalAngleSlider.Value;
            _viewshed.MinDistance = MinimumDistanceSlider.Value;
            _viewshed.MaxDistance = MaximumDistanceSlider.Value;

            // Return if the checkboxes are in an invalid state.
            if (AnalysisVisibilityCheck.IsChecked == null || FrustumVisibilityCheck.IsChecked == null)
            {
                return;
            }

            // Update visibility of the viewshed analysis.
            if ((bool)AnalysisVisibilityCheck.IsChecked)
            {
                _viewshed.IsVisible = true;
            }
            else if (!(bool) AnalysisVisibilityCheck.IsChecked)
            {
                _viewshed.IsVisible = false;
            }

            // Update visibility of the frustum. Note that the frustum will be invisible 
            //     regardless of this setting if the viewshed analysis is not visible.
            if ((bool)FrustumVisibilityCheck.IsChecked)
            {
                _viewshed.IsFrustumOutlineVisible = true;
            }
            else
            {
                _viewshed.IsFrustumOutlineVisible = false;
            }
        }
    }
}
