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
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System;
using System.Diagnostics;
using System.Threading;
using Xamarin.Forms;
using static Xamarin.Essentials.TextToSpeech;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

#if __IOS__
using UIKit;
#endif

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    public partial class RouteViewer : ContentPage, IDisposable
    {
        // Static field for sharing route between views.
        public static RouteResult PassedRouteResult;

        // Objects for navigation.
        private RouteTracker _routeTracker;

        // Cancellation token for speech synthesizer.
        private CancellationTokenSource _speechToken = new CancellationTokenSource();

        // Scene content.
        private GraphicsOverlay _routeOverlay;
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;
        private Scene _scene;

        // Custom location data source that enables calibration and returns values relative to mean sea level rather than the WGS84 ellipsoid.
        private ARLocationDataSource _locationDataSource;

        // Calibration state fields.
        private bool _isCalibrating;
        private double _altitudeOffset;

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
                    _scene.BaseSurface.Opacity = 0.5;

                    // Show the calibration controls.
                    CalibrationGrid.IsVisible = true;
                }
                else
                {
                    // Hide the scene when not calibrating.
                    _scene.BaseSurface.Opacity = 0;

                    // Hide the calibration controls.
                    CalibrationGrid.IsVisible = false;
                }
            }
        }

        public RouteViewer()
        {
            InitializeComponent();
            Initialize();
        }

        private void CalibrateButton_Clicked(object sender, EventArgs e) { IsCalibrating = !IsCalibrating; }

        private void ElevationSlider_DeltaProgressChanged(object sender, Forms.Resources.DeltaChangedEventArgs e)
        {
            // Add the new value to the existing altitude offset.
            _altitudeOffset += e.DeltaProgress;

            // Update the altitude offset on the custom location data source.
            _locationDataSource.AltitudeOffset = _altitudeOffset;
        }

        private void HeadingSlider_DeltaProgressChanged(object sender, Forms.Resources.DeltaChangedEventArgs e)
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

        private async void LocationDataSource_LocationChanged(object sender, Location e)
        {
            if (_routeTracker != null)
            {
                // Correct any invalid velocity value.
                if (e.Velocity < 0)
                {
                    e = new Location(e.Position, e.HorizontalAccuracy, 0.0, e.Course, e.IsLastKnown);
                }

                try
                {
                    // Pass location change to the route tracker.
                    await _routeTracker.TrackLocationAsync(e);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to update current location", "OK");
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void Initialize()
        {
            // Create the custom location data source and configure the AR scene view to use it.
#if XAMARIN_ANDROID
            _locationDataSource = new ARLocationDataSource(Android.App.Application.Context);
            _locationDataSource.AltitudeMode = ARLocationDataSource.AltitudeAdjustmentMode.NmeaParsedMsl;
#elif __IOS__
            _locationDataSource = new ARLocationDataSource();
#endif
            MyARSceneView.LocationDataSource = _locationDataSource;
            _locationDataSource.LocationChanged += LocationDataSource_LocationChanged;

            // Create the scene and show it.
            _scene = new Scene(Basemap.CreateImagery());
            MyARSceneView.Scene = _scene;

            // Create and add the elevation surface.
            _elevationSource = new ArcGISTiledElevationSource(new Uri(
                "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            MyARSceneView.Scene.BaseSurface = _elevationSurface;

            // Hide the surface in AR.
            _elevationSurface.NavigationConstraint = NavigationConstraint.None;
            _elevationSurface.Opacity = 0;

            // Create and add a graphics overlay for showing routes.
            _routeOverlay = new GraphicsOverlay();
            MyARSceneView.GraphicsOverlays.Add(_routeOverlay);

            // Configure the graphics overlay to render graphics as yellow 3D tubes.
            SolidStrokeSymbolLayer strokeSymbolLayer = new SolidStrokeSymbolLayer(1, System.Drawing.Color.Yellow, null,
                StrokeSymbolLayerLineStyle3D.Tube);
            strokeSymbolLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;
            MultilayerPolylineSymbol tubeSymbol = new MultilayerPolylineSymbol(new[] { strokeSymbolLayer });
            _routeOverlay.Renderer = new SimpleRenderer(tubeSymbol);

            // Configure the space and atmosphere effects for AR.
            MyARSceneView.SpaceEffect = SpaceEffect.None;
            MyARSceneView.AtmosphereEffect = AtmosphereEffect.None;

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

        private void NavigateButton_Clicked(object sender, EventArgs e)
        {
            // Create the route tracker.
            _routeTracker = new RouteTracker(PassedRouteResult, 0);

            // Listen for updated guidance.
            _routeTracker.NewVoiceGuidance += RouteTracker_VoiceGuidanceChanged;

            // Disable rerouting
            _routeTracker.DisableRerouting();

            Device.BeginInvokeOnMainThread(() =>
            {
                // Prevent re-navigating.
                NavigateButton.IsEnabled = false;
            });
        }

        private void RouteTracker_VoiceGuidanceChanged(object sender, RouteTrackerNewVoiceGuidanceEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Update the help label with the new guidance.
                HelpLabel.Text = e.VoiceGuidance.Text;
            });

            try
            {
                // Say the direction using voice synthesis.
                if (e.VoiceGuidance.Text?.Length > 0)
                {
                    _speechToken.Cancel();
                    _speechToken = new CancellationTokenSource();
                    SpeakAsync(e.VoiceGuidance.Text, _speechToken.Token);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            if (_routeTracker != null)
            {
                _routeTracker.NewVoiceGuidance -= RouteTracker_VoiceGuidanceChanged;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Start device tracking.
            try
            {
                await MyARSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MyARSceneView.StopTrackingAsync();
            Dispose();
        }
    }
}