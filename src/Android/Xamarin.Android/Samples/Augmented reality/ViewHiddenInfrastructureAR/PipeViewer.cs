// Copyright 2019 Esri.
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
using ArcGISRuntimeXamarin.Samples.ARToolkit.Controls;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.ViewHiddenInfrastructureAR
{
    [Activity(Label = "PipeViewer")]
    public class PipeViewer : Activity
    {
        // Pipe graphics that have been passed in by the PipePlacer class.
        public static IEnumerable<Graphic> PipeGraphics;

        // Hold references to UI controls.
        private ARSceneView _arView;
        private TextView _helpLabel;
        private Button _calibrateButton;
        private Button _localButton;
        private Button _roamingButton;
        private View _calibrationView;
        private JoystickSeekBar _headingSlider;
        private JoystickSeekBar _altitudeSlider;

        // Scene content.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        // Track when user is changing between AR and GPS localization.
        private bool _changingScale;

        // Custom location data source that enables calibration and returns
        // values relative to mean sea level rather than the WGS84 ellipsoid.
        private MslAdjustedARLocationDataSource _locationSource;

        // Calibration state fields.
        private bool _isCalibrating;
        private double _altitudeOffset;

        private bool IsCalibrating
        {
            get => _isCalibrating;
            set
            {
                _isCalibrating = value;
                if (_isCalibrating)
                {
                    // Show the base surface so that the user can calibrate using the base surface on top of the real world.
                    _arView.Scene.BaseSurface.Opacity = 0.5;

                    // Enable scene interaction.
                    _arView.InteractionOptions.IsEnabled = true;

                    // Show the calibration controls.
                    _calibrationView.Visibility = ViewStates.Visible;
                }
                else
                {
                    // Hide the base surface.
                    _arView.Scene.BaseSurface.Opacity = 0;

                    // Disable scene interaction.
                    _arView.InteractionOptions.IsEnabled = false;

                    // Hide the calibration controls.
                    _calibrationView.Visibility = ViewStates.Gone;
                }
            }
        }

        private async void RealScaleValueChanged(bool roaming)
        {
            // Prevent this from being called concurrently
            if (_changingScale)
            {
                return;
            }
            _changingScale = true;

            // Disable the associated UI controls while switching.
            _roamingButton.Enabled = false;
            _localButton.Enabled = false;

            // Check if using roaming for AR location mode.
            if (roaming)
            {
                await _arView.StopTrackingAsync();

                // Start AR tracking using a continuous GPS signal.
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
                _altitudeSlider.Enabled = true;
                _localButton.Enabled = true;
                _helpLabel.Text = "Using GPS signal";
            }
            else
            {
                await _arView.StopTrackingAsync();

                // Start AR tracking without using a GPS signal.
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Ignore);
                _altitudeSlider.Enabled = false;
                _roamingButton.Enabled = true;
                _helpLabel.Text = "Using ARCore only";
            }
            _changingScale = false;
        }

        private void Initialize()
        {
            // Create and add the scene.
            _arView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Add the location data source to the AR view.
            _arView.LocationDataSource = _locationSource;

            // Create and add the elevation source.
            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            _arView.Scene.BaseSurface = _elevationSurface;

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
            _arView.SpaceEffect = SpaceEffect.None;
            _arView.AtmosphereEffect = AtmosphereEffect.None;

            // Add the graphics overlay to the scene.
            _arView.GraphicsOverlays.Add(pipesOverlay);

            // Disable scene interaction.
            _arView.InteractionOptions = new SceneViewInteractionOptions() { IsEnabled = false };

            // Enable the calibration button.
            _calibrateButton.Enabled = true;
        }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (PipeGraphics == null)
            {
                // Show a message and then exit after if needed.
                var dialog = new AlertDialog.Builder(this).SetMessage("Didn't get data from previous activity").SetTitle("Can't start").Create();
                dialog.SetButton("OK", (o, e) => { Finish(); });
                dialog.Show();
            }

            // Create the layout.
            CreateLayout();

            // Configure the AR scene view.
            Initialize();
        }

        private void CreateLayout()
        {
            // Load the view.
            SetContentView(ArcGISRuntime.Resource.Layout.ViewHiddenARPipeViewer);

            // Set up control references.
            _arView = FindViewById<ARSceneView>(ArcGISRuntime.Resource.Id.arView);
            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.helpLabel);
            _calibrateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.calibrateButton);
            _roamingButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.roamingButton);
            _localButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.localButton);

            _calibrationView = FindViewById(ArcGISRuntime.Resource.Id.calibrationView);
            _headingSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.headingJoystick);
            _altitudeSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.altitudeJoystick);

            // Create the custom location data source and configure the AR scene view to use it.
            _locationSource = new MslAdjustedARLocationDataSource(this);
            _locationSource.AltitudeMode = MslAdjustedARLocationDataSource.AltitudeAdjustmentMode.NmeaParsedMsl;
            _arView.LocationDataSource = _locationSource;

            // Disable plane rendering and visualization.
            _arView.ArSceneView.PlaneRenderer.Enabled = false;
            _arView.ArSceneView.PlaneRenderer.Visible = false;

            // Configure button click events.
            _calibrateButton.Click += (o, e) => IsCalibrating = !IsCalibrating;
            _roamingButton.Click += (o, e) => RealScaleValueChanged(true);
            _localButton.Click += (o, e) => RealScaleValueChanged(false);

            // Configure calibration sliders.
            _headingSlider.DeltaProgressChanged += HeadingSlider_DeltaProgressChanged;
            _altitudeSlider.DeltaProgressChanged += AltitudeSlider_DeltaProgressChanged;
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
            Camera camera = _arView.OriginCamera;

            // Calculate the new heading by applying the offset to the old camera's heading.
            double heading = camera.Heading + e.DeltaProgress;

            // Create a new camera by rotating the old camera to the new heading.
            Camera newCamera = camera.RotateTo(heading, camera.Pitch, camera.Roll);

            // Use the new camera as the origin camera.
            _arView.OriginCamera = newCamera;
        }

        protected override async void OnPause()
        {
            base.OnPause();
            await _arView.StopTrackingAsync();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            // Start AR tracking without location updates.
            await _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
        }
    }
}