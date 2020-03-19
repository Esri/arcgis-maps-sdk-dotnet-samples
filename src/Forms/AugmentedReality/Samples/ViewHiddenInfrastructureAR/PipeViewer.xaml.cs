// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Converters;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Forms.Resources;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

#if __IOS__
using UIKit;
#endif

namespace ArcGISRuntimeXamarin.Samples.ViewHiddenInfrastructureAR
{
    public partial class PipeViewer : ContentPage
    {
        // Pipe graphics that have been passed in by the PipePlacer class.
        public static IEnumerable<Graphic> PipeGraphics;

        // Scene content.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        // Track when user is changing between AR and GPS localization.
        private bool _changingScale;

        // Custom location data source that enables calibration and returns values relative to mean sea level rather than the WGS84 ellipsoid.
        private ARLocationDataSource _locationSource;

        // Calibration state fields.
        private bool _isCalibrating;
        private double _altitudeOffset;

        public PipeViewer()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create and add the scene.
            MyARSceneView.Scene = new Scene(Basemap.CreateImagery());

            // Create the custom location data source and configure the AR scene view to use it.
#if XAMARIN_ANDROID
            _locationSource = new ARLocationDataSource(Android.App.Application.Context);
            _locationSource.AltitudeMode = ARLocationDataSource.AltitudeAdjustmentMode.NmeaParsedMsl;
#elif __IOS__
            _locationSource = new ARLocationDataSource();
#endif
            MyARSceneView.LocationDataSource = _locationSource;

            // Create and add the elevation source.
            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            MyARSceneView.Scene.BaseSurface = _elevationSurface;

            // Configure the surface for AR: no navigation constraint and hidden by default.
            _elevationSurface.NavigationConstraint = NavigationConstraint.None;
            _elevationSurface.Opacity = 0;

            // Create a graphics overlay for the pipes.
            GraphicsOverlay pipesOverlay = new GraphicsOverlay();

            // Use absolute surface placement to see the graphics at the correct altitude.
            pipesOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;

            // Add graphics for the pipes.
            pipesOverlay.Graphics.AddRange(PipeGraphics);

            // Display routes as red 3D tubes.
            SolidStrokeSymbolLayer strokeSymbolLayer = new SolidStrokeSymbolLayer(0.3, System.Drawing.Color.Red, null, StrokeSymbolLayerLineStyle3D.Tube) { CapStyle = StrokeSymbolLayerCapStyle.Round };
            MultilayerPolylineSymbol tubeSymbol = new MultilayerPolylineSymbol(new[] { strokeSymbolLayer });
            pipesOverlay.Renderer = new SimpleRenderer(tubeSymbol);

            // Configure scene view display for real-scale AR: no space effect or atmosphere effect.
            MyARSceneView.SpaceEffect = SpaceEffect.None;
            MyARSceneView.AtmosphereEffect = AtmosphereEffect.None;

            // Add the graphics overlay to the scene.
            MyARSceneView.GraphicsOverlays.Add(pipesOverlay);

            // Disable scene interaction.
            MyARSceneView.InteractionOptions = new SceneViewInteractionOptions() { IsEnabled = false };

            // Enable the calibration button.
            CalibrateButton.IsEnabled = true;
        }

        private void AltitudeSlider_DeltaProgressChanged(object sender, DeltaChangedEventArgs e)
        {
            // Add the new value to the existing altitude offset.
            _altitudeOffset += e.DeltaProgress;

            // Update the altitude offset on the custom location data source.
            _locationSource.AltitudeOffset = _altitudeOffset;
        }

        private void HeadingSlider_DeltaProgressChanged(object sender, DeltaChangedEventArgs e)
        {
            // Get the old camera.
            Camera camera = MyARSceneView.OriginCamera;

            // Calculate the new heading by applying the offset to the old camera's heading.
            double heading = camera.Heading + e.DeltaProgress;

            // Create a new camera by rotating the old camera to the new heading.
            Camera newCamera = camera.RotateTo(heading, camera.Pitch, camera.Roll);

            // Use the new camera as the origin camera.
            MyARSceneView.OriginCamera = newCamera;
        }

        private void CalibrateButtonPressed(object sender, EventArgs e) { IsCalibrating = !IsCalibrating; }

        private bool IsCalibrating
        {
            get
            {
                return _isCalibrating;
            }
            set
            {
                _isCalibrating = value;
                if (_isCalibrating)
                {
                    // Show the surface semitransparent for calibration.
                    MyARSceneView.Scene.BaseSurface.Opacity = 0.5;

                    // Enable scene interaction.
                    MyARSceneView.InteractionOptions.IsEnabled = true;
                    CalibrationGrid.IsVisible = true;
                }
                else
                {
                    // Hide the scene when not calibrating.
                    MyARSceneView.Scene.BaseSurface.Opacity = 0;

                    // Disable scene interaction.
                    MyARSceneView.InteractionOptions.IsEnabled = false;
                    CalibrationGrid.IsVisible = false;
                }
            }
        }

        private async void RealScaleValueChanged(object sender, EventArgs e)
        {
            // Prevent this from being called concurrently
            if (_changingScale)
            {
                return;
            }
            _changingScale = true;

            // Disable the associated UI controls while switching.
            RoamingButton.IsEnabled = false;
            LocalButton.IsEnabled = false;

            // Check if using roaming for AR location mode.
            if (((Button)sender).Text == "GPS")
            {
                await MyARSceneView.StopTrackingAsync();

                // Start AR tracking using a continuous GPS signal.
                await MyARSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
                ElevationSlider.IsEnabled = true;
                LocalButton.IsEnabled = true;
            }
            else
            {
                await MyARSceneView.StopTrackingAsync();

                // Start AR tracking without using a GPS signal.
                await MyARSceneView.StartTrackingAsync(ARLocationTrackingMode.Ignore);
                ElevationSlider.IsEnabled = false;
                RoamingButton.IsEnabled = true;
            }
            _changingScale = false;
        }

        protected override async void OnAppearing()
        {
            // Start device tracking.
            try
            {
                await MyARSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        protected override void OnDisappearing()
        {
            MyARSceneView.StopTrackingAsync();
        }
    }
}