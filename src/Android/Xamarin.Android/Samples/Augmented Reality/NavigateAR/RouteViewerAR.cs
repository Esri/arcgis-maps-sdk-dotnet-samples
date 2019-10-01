// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Speech.Tts;
using Android.Views;
using Android.Widget;
using ArcgisRuntime.Samples.ARToolkit.Controls;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    [Activity(Label = "RouteViewerAR")]
    public class RouteViewerAR : Activity
    {
        private ARSceneView _arSceneView;
        private TextView _helpLabel;
        private Button _calibrateButton;
        private Button _navigateButton;
        private View _calibrationView;
        private JoystickSeekBar _headingSlider;
        private JoystickSeekBar _altitudeSlider;

        // TODO - is there a better way to pass this data?
        public static Route PassedRoute;
        public static RouteParameters PassedRouteParameters;
        public static RouteResult PassedRouteResult;
        public static RouteTask PassedRouteTask;

        private RouteTracker _routeTracker;
        private TextToSpeech _textToSpeech;

        private GraphicsOverlay _routeOverlay;
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;
        private Scene _scene;

        // Calibration state fields.
        private bool _isCalibrating = false;
        private double _altitudeOffset = 0;

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
                    _scene.BaseSurface.Opacity = 0.5;
                    _calibrationView.Visibility = ViewStates.Visible;
                } else
                {
                    _scene.BaseSurface.Opacity = 0;
                    _calibrationView.Visibility = ViewStates.Gone;
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (PassedRoute == null || PassedRouteParameters == null || PassedRouteResult == null || PassedRouteTask == null)
            {
                ShowMessage("Didn't get route from previous activity", "Can't start", true);
            }

            SetContentView(ArcGISRuntime.Resource.Layout.NavigateARNavigator);

            _arSceneView = FindViewById<ARSceneView>(ArcGISRuntime.Resource.Id.arView);
            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.helpLabel);
            _calibrateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.calibrateButton);
            _navigateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.navigateStartButton);
            _calibrationView = FindViewById(ArcGISRuntime.Resource.Id.calibrationView);
            _headingSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.headingJoystick);
            _altitudeSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.altitudeJoystick);

            MSLAdjustedARLocationDataSource arLocationDataSource = new MSLAdjustedARLocationDataSource(this);
            arLocationDataSource.AltitudeMode = MSLAdjustedARLocationDataSource.AltitudeAdjustmentMode.NmeaParsedMsl;
            _arSceneView.LocationDataSource = arLocationDataSource;
            //arSceneView.LocationDataSource = new SystemLocationDataSource();

            _arSceneView.ArSceneView.PlaneRenderer.Enabled = false;
            _arSceneView.ArSceneView.PlaneRenderer.Visible = false;

            _navigateButton.Click += (o, e) => StartTurnByTurn();

            _calibrateButton.Click += (o, e) => IsCalibrating = !IsCalibrating;

            _headingSlider.DeltaProgressChanged += (o, e) =>
            {
                Camera camera = _arSceneView.OriginCamera;

                double heading = camera.Heading + e.deltaProgress;

                Camera newCamera = camera.RotateTo(heading, camera.Pitch, camera.Roll);

                _arSceneView.OriginCamera = newCamera;
            };

            _altitudeSlider.DeltaProgressChanged += (o, e) =>
            {
                _altitudeOffset += e.deltaProgress;
                arLocationDataSource.AltitudeOffset = _altitudeOffset;
            };

            SetupArView();
        }

        private void SetupArView()
        {
            Toast.MakeText(this,
                "Calibrate your heading before navigating!",
                ToastLength.Long).Show();

            _scene = new Scene(Basemap.CreateImageryWithLabels());
            _arSceneView.Scene = _scene;

            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            _arSceneView.Scene.BaseSurface = _elevationSurface;

            _elevationSurface.NavigationConstraint = NavigationConstraint.None;
            _elevationSurface.Opacity = 0;

            _routeOverlay = new GraphicsOverlay();
            _arSceneView.GraphicsOverlays.Add(_routeOverlay);

            SolidStrokeSymbolLayer strokeSymbolLayer = new SolidStrokeSymbolLayer(1, System.Drawing.Color.Yellow, null, StrokeSymbolLayerLineStyle3D.Tube);
            strokeSymbolLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;
            MultilayerPolylineSymbol tubeSymbol = new MultilayerPolylineSymbol(new[] { strokeSymbolLayer });
            _routeOverlay.Renderer = new SimpleRenderer(tubeSymbol);

            SystemLocationDataSource trackingDataSource = new SystemLocationDataSource();
            trackingDataSource.LocationChanged += TrackingDataSource_LocationChanged;
            trackingDataSource.StartAsync();

            _arSceneView.SpaceEffect = SpaceEffect.None;
            _arSceneView.AtmosphereEffect = AtmosphereEffect.None;

            SetRoute(PassedRouteResult.Routes[0]);
        }

        private void SetRoute(Route inputRoute)
        {
            _routeOverlay.Graphics.Clear();

            Graphic routeGraphic = new Graphic(inputRoute.RouteGeometry);

            _routeOverlay.Graphics.Add(routeGraphic);

            _routeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _routeOverlay.SceneProperties.AltitudeOffset = 3;
        }

        private async void TrackingDataSource_LocationChanged(object sender, Location e)
        {
            if (_routeTracker != null)
            {
                try
                {
                    await _routeTracker.TrackLocationAsync(e);
                }
                catch (Exception ex)
                {
                    ShowMessage("Failed to update current location", "error");
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        private void StartTurnByTurn()
        {
            _routeTracker = new RouteTracker(PassedRouteResult, 0);

            _textToSpeech = new TextToSpeech(this, null, "com.google.android.tts");

            _routeTracker.NewVoiceGuidance += (o, e) =>
            {
                _helpLabel.Text = e.VoiceGuidance.Text;

                _textToSpeech.Stop();
                _textToSpeech.Speak(e.VoiceGuidance.Text, QueueMode.Flush, null, null);
            };

            _routeTracker.TrackingStatusChanged += (o, e) =>
            {

            };

            if (PassedRouteTask.RouteTaskInfo.SupportsRerouting)
            {
                _routeTracker.RerouteStarted += (o, e) =>
                {
                    _helpLabel.Text = "Rerouting...";
                };

                _routeTracker.RerouteCompleted += (o, e) =>
                {
                    Route newRoute = e.TrackingStatus.RouteResult.Routes.First();

                    if (!newRoute.Equals(PassedRoute))
                    {
                        SetRoute(newRoute);
                    }
                };

                _routeTracker.EnableReroutingAsync(PassedRouteTask, PassedRouteParameters, ReroutingStrategy.ToNextStop, true);
            }
        }

        private void ShowMessage(string message, string title, bool closeApp = false)
        {
            var dialog = new AlertDialog.Builder(this).SetMessage(message).SetTitle(title).Create();
            if (closeApp)
            {
                dialog.SetButton("OK", (o, e) =>
                {
                    Finish();
                });
            }
            dialog.Show();
        }

        protected override void OnPause()
        {
            _arSceneView.StopTracking();
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            _arSceneView.StopTracking();
            base.OnDestroy();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            // Start AR tracking without location updates.
            await _arSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
        }
    }
}
