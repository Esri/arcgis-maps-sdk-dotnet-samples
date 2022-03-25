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
using Android.Speech.Tts;
using Android.Views;
using Android.Widget;
using ArcGISRuntimeXamarin.Samples.ARToolkit.Controls;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    [Activity(Label = "RouteViewerAR")]
    public class RouteViewerAR : Activity
    {
        // Hold references to UI controls.
        private ARSceneView _arSceneView;
        private TextView _helpLabel;
        private Button _calibrateButton;
        private Button _navigateButton;
        private View _calibrationView;
        private JoystickSeekBar _headingSlider;
        private JoystickSeekBar _altitudeSlider;

        // Static field for sharing route between views.
        public static RouteResult PassedRouteResult;

        // Objects for navigation.
        private RouteTracker _routeTracker;
        private TextToSpeech _textToSpeech;

        // Scene content.
        private GraphicsOverlay _routeOverlay;
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;
        private Scene _scene;

        // Custom location data source that enables calibration and returns
        // values relative to mean sea level rather than the WGS84 ellipsoid.
        private MslAdjustedARLocationDataSource _locationDataSource;

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
                    // Show the surface semitransparent for calibration.
                    _scene.BaseSurface.Opacity = 0.5;
                    _calibrationView.Visibility = ViewStates.Visible;
                }
                else
                {
                    // Hide the scene when not calibrating.
                    _scene.BaseSurface.Opacity = 0;
                    _calibrationView.Visibility = ViewStates.Gone;
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (PassedRouteResult == null)
            {
                ShowMessage("Didn't get route from previous activity", "Can't start", true);
            }

            // Create the layout.
            CreateLayout();

            // Configure the AR scene view.
            Initialize();
        }

        private void CreateLayout()
        {
            // Load the view.
            SetContentView(ArcGISRuntime.Resource.Layout.NavigateARNavigator);

            // Set up control references.
            _arSceneView = FindViewById<ARSceneView>(ArcGISRuntime.Resource.Id.arView);
            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.helpLabel);
            _calibrateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.calibrateButton);
            _navigateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.navigateStartButton);
            _calibrationView = FindViewById(ArcGISRuntime.Resource.Id.calibrationView);
            _headingSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.headingJoystick);
            _altitudeSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.altitudeJoystick);

            // Create the custom location data source and configure the AR scene view to use it.
            _locationDataSource = new MslAdjustedARLocationDataSource(this);
            _locationDataSource.AltitudeMode = MslAdjustedARLocationDataSource.AltitudeAdjustmentMode.NmeaParsedMsl;
            _arSceneView.LocationDataSource = _locationDataSource;

            // Listen for location changes to enable route tracking.
            _locationDataSource.LocationChanged += LocationDataSource_LocationChanged;

            // Disable plane rendering and visualization.
            _arSceneView.ArSceneView.PlaneRenderer.Enabled = false;
            _arSceneView.ArSceneView.PlaneRenderer.Visible = false;

            // Configure button click events.
            _navigateButton.Click += (o, e) => StartTurnByTurn();
            _calibrateButton.Click += (o, e) => IsCalibrating = !IsCalibrating;

            // Configure calibration sliders.
            _headingSlider.DeltaProgressChanged += HeadingSlider_DeltaProgressChanged;
            _altitudeSlider.DeltaProgressChanged += AltitudeSlider_DeltaProgressChanged;
        }

        private void AltitudeSlider_DeltaProgressChanged(object sender, DeltaChangedEventArgs e)
        {
            // Add the new value to the existing altitude offset.
            _altitudeOffset += e.DeltaProgress;

            // Update the altitude offset on the custom location data source.
            _locationDataSource.AltitudeOffset = _altitudeOffset;
        }

        private void HeadingSlider_DeltaProgressChanged(object sender, DeltaChangedEventArgs e)
        {
            // Get the old camera.
            Camera camera = _arSceneView.OriginCamera;

            // Calculate the new heading by applying the offset to the old camera's heading.
            double heading = camera.Heading + e.DeltaProgress;

            // Create a new camera by rotating the old camera to the new heading.
            Camera newCamera = camera.RotateTo(heading, camera.Pitch, camera.Roll);

            // Use the new camera as the origin camera.
            _arSceneView.OriginCamera = newCamera;
        }

        private async void LocationDataSource_LocationChanged(object sender, Location e)
        {
            if (_routeTracker != null)
            {
                try
                {
                    // Pass location change to the route tracker.
                    await _routeTracker.TrackLocationAsync(e);
                }
                catch (Exception ex)
                {
                    ShowMessage("Failed to update current location", "error");
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        private void Initialize()
        {
            Toast.MakeText(this,
                "Calibrate your heading before navigating!",
                ToastLength.Long).Show();

            // Create the scene and show it.
            _scene = new Scene(BasemapStyle.ArcGISImageryStandard);
            _arSceneView.Scene = _scene;

            // Create and add the elevation surface.
            _elevationSource = new ArcGISTiledElevationSource(new Uri(
                "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            _arSceneView.Scene.BaseSurface = _elevationSurface;

            // Hide the surface in AR.
            _elevationSurface.NavigationConstraint = NavigationConstraint.None;
            _elevationSurface.Opacity = 0;

            // Create and add a graphics overlay for showing routes.
            _routeOverlay = new GraphicsOverlay();
            _arSceneView.GraphicsOverlays.Add(_routeOverlay);

            // Configure the graphics overlay to render graphics as yellow 3D tubes.
            SolidStrokeSymbolLayer strokeSymbolLayer = new SolidStrokeSymbolLayer(1, System.Drawing.Color.Yellow, null,
                StrokeSymbolLayerLineStyle3D.Tube);
            strokeSymbolLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;
            MultilayerPolylineSymbol tubeSymbol = new MultilayerPolylineSymbol(new[] {strokeSymbolLayer});
            _routeOverlay.Renderer = new SimpleRenderer(tubeSymbol);

            // Configure the space and atmosphere effects for AR.
            _arSceneView.SpaceEffect = SpaceEffect.None;
            _arSceneView.AtmosphereEffect = AtmosphereEffect.None;

            // Set up the route graphic.
            SetRoute(PassedRouteResult.Routes[0]);
        }

        private void SetRoute(Route inputRoute)
        {
            // Clear any existing route graphics.
            _routeOverlay.Graphics.Clear();

            // Create a new route graphic and show it in the view.
            Graphic routeGraphic = new Graphic(inputRoute.RouteGeometry);
            _routeOverlay.Graphics.Add(routeGraphic);

            // Configure the overlay to show routes 3 meters above the ground.
            _routeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _routeOverlay.SceneProperties.AltitudeOffset = 3;
        }

        private void StartTurnByTurn()
        {
            // Create the route tracker.
            _routeTracker = new RouteTracker(PassedRouteResult, 0);

            // Create the speech synthesizer.
            _textToSpeech = new TextToSpeech(this, null, "com.google.android.tts");

            // Listen for updated guidance.
            _routeTracker.NewVoiceGuidance += RouteTracker_VoiceGuidanceChanged;
            _routeTracker.TrackingStatusChanged += RouteTracker_TrackingStatusChanged;

            // Prevent re-navigating.
            _navigateButton.Enabled = false;
        }

        private void RouteTracker_TrackingStatusChanged(object sender, RouteTrackerTrackingStatusChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                // Update the help label with new guidance.
                _helpLabel.Text = _routeTracker.GenerateVoiceGuidance().Text;
            });
        }

        private void RouteTracker_VoiceGuidanceChanged(object sender, RouteTrackerNewVoiceGuidanceEventArgs e)
        {
            RunOnUiThread(() =>
            {
                // Update the help label with the new guidance.
                _helpLabel.Text = e.VoiceGuidance.Text;

                // Stop any currently running speech.
                _textToSpeech.Stop();

                // Speak the new guidance.
                _textToSpeech.Speak(e.VoiceGuidance.Text, QueueMode.Flush, null, null);
            });
        }

        private void ShowMessage(string message, string title, bool closeApp = false)
        {
            // Show a message and then exit after if needed.
            var dialog = new AlertDialog.Builder(this).SetMessage(message).SetTitle(title).Create();
            if (closeApp)
            {
                dialog.SetButton("OK", (o, e) => { Finish(); });
            }

            dialog.Show();
        }

        protected override async void OnPause()
        {
            base.OnPause();
            await _arSceneView.StopTrackingAsync();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            // Start AR tracking without location updates.
            await _arSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_routeTracker != null)
            {
                _routeTracker.NewVoiceGuidance -= RouteTracker_VoiceGuidanceChanged;
                _routeTracker.TrackingStatusChanged -= RouteTracker_TrackingStatusChanged;
            }
        }
    }
}