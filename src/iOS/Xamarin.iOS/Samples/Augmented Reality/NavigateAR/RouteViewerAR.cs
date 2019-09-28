// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using AVFoundation;
using CoreGraphics;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    public class RouteViewerAR : UIViewController
    {
        private ARSceneView _arView;
        private UILabel _helpLabel;
        private UIBarButtonItem _calibrateButton;
        private UIBarButtonItem _navigateButton;
        private CalibrationViewController _calibrationVC;

        public RouteResult _routeResult;
        private RouteTracker _routeTracker;
        private AVSpeechSynthesizer _synthesizer;

        private GraphicsOverlay _routeOverlay;
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        private bool _isCalibrating = false;

        private bool IsCalibrating
        {
            get => _isCalibrating;
            set
            {
                _isCalibrating = value;
                if (_isCalibrating)
                {
                    _arView.Scene.BaseSurface.Opacity = 0.5;
                    ShowCalibrationPopover();
                }
                else
                {
                    _arView.Scene.BaseSurface.Opacity = 0;
                    _calibrationVC.DismissViewController(true, null);
                }
            }
        }

        public override void LoadView()
        {
            View = new UIView { BackgroundColor = UIColor.White };

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _arView = new ARSceneView();
            _arView.TranslatesAutoresizingMaskIntoConstraints = false;

            _helpLabel = new UILabel();
            _helpLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _helpLabel.TextAlignment = UITextAlignment.Center;
            _helpLabel.TextColor = UIColor.White;
            _helpLabel.BackgroundColor = UIColor.FromWhiteAlpha(0, 0.6f);
            _helpLabel.Text = "Adjust calibration before starting";

            _calibrationVC = new CalibrationViewController(_arView);

            _calibrateButton = new UIBarButtonItem("Calibrate", UIBarButtonItemStyle.Plain, ToggleCalibration);
            _navigateButton = new UIBarButtonItem("Navigate", UIBarButtonItemStyle.Plain, StartTurnByTurn);

            _synthesizer = new AVSpeechSynthesizer();

            toolbar.Items = new[]
            {
                _calibrateButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _navigateButton
            };

            View.AddSubviews(_arView, toolbar, _helpLabel);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _arView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _arView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _arView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _arView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        private void ToggleCalibration(object sender, EventArgs e) => IsCalibrating = !IsCalibrating;

        private void Initialize()
        {
            _arView.Scene = new Scene(Basemap.CreateImageryWithLabels());

            _arView.LocationDataSource = new SystemLocationDataSource();

            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            _arView.Scene.BaseSurface = _elevationSurface;

            _elevationSurface.NavigationConstraint = NavigationConstraint.None;
            _elevationSurface.Opacity = 0;

            _routeOverlay = new GraphicsOverlay();
            _arView.GraphicsOverlays.Add(_routeOverlay);

            SolidStrokeSymbolLayer strokeSymbolLayer = new SolidStrokeSymbolLayer(1, System.Drawing.Color.Yellow, null, StrokeSymbolLayerLineStyle3D.Tube);
            strokeSymbolLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;
            MultilayerPolylineSymbol tubeSymbol = new MultilayerPolylineSymbol(new[] { strokeSymbolLayer });
            _routeOverlay.Renderer = new SimpleRenderer(tubeSymbol);

            SystemLocationDataSource trackingDataSource = new SystemLocationDataSource();
            trackingDataSource.LocationChanged += TrackingDataSource_LocationChanged;
            trackingDataSource.StartAsync();

            _arView.SpaceEffect = SpaceEffect.None;
            _arView.AtmosphereEffect = AtmosphereEffect.None;

            SetRoute(_routeResult.Routes[0]);
        }

        private void SetRoute(Route inputRoute)
        {
            _routeOverlay.Graphics.Clear();

            Graphic routeGraphic = new Graphic(inputRoute.RouteGeometry);

            _routeOverlay.Graphics.Add(routeGraphic);

            _routeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _routeOverlay.SceneProperties.AltitudeOffset = 3;
        }

        private void TrackingDataSource_LocationChanged(object sender, Location e)
        {
            if (_routeTracker != null)
            {
                _routeTracker.TrackLocationAsync(e);
            }
        }

        private void StartTurnByTurn(object sender, EventArgs e)
        {
            _routeTracker = new RouteTracker(_routeResult, 0);

            _routeTracker.NewVoiceGuidance += (o, ee) =>
            {
                var utterance = new AVSpeechUtterance(ee.VoiceGuidance.Text);
                utterance.Voice = AVSpeechSynthesisVoice.FromLanguage("en-US");
                _synthesizer.SpeakUtterance(utterance);

                _helpLabel.Text = ee.VoiceGuidance.Text;
            };

            _routeTracker.TrackingStatusChanged += (o, ee) =>
            {
                _helpLabel.Text = _routeTracker.GenerateVoiceGuidance().Text;
            };
        }

        private void ShowCalibrationPopover()
        {
            // Show the table view in a popover.
            _calibrationVC.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            _calibrationVC.PreferredContentSize = new CGSize(360, 120);
            UIPopoverPresentationController pc = _calibrationVC.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = _calibrateButton;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new ppDelegate();
            }

            PresentViewController(_calibrationVC, true, null);
        }

        // Force popover to display on iPhone.
        private class ppDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _arView.StopTracking();
        }
    }

    public class CalibrationViewController : UIViewController
    {
        private UISlider _headingSlider;
        private UISlider _elevationSlider;
        private UILabel elevationLabel;
        private UILabel headingLabel;
        private ARSceneView _arView;
        private NSTimer _headingTimer;
        private NSTimer _elevationTimer;

        public CalibrationViewController(ARSceneView arView)
        {
            this._arView = arView;
        }

        public override void LoadView()
        {
            // Create and add the container views.
            View = new UIView();

            UIStackView formContainer = new UIStackView();
            formContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.Spacing = 8;
            formContainer.LayoutMarginsRelativeArrangement = true;
            formContainer.Alignment = UIStackViewAlignment.Fill;
            formContainer.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formContainer.Axis = UILayoutConstraintAxis.Vertical;
            formContainer.WidthAnchor.ConstraintEqualTo(300).Active = true;

            elevationLabel = new UILabel();
            elevationLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            elevationLabel.Text = "Elevation";
            _elevationSlider = new UISlider { MinValue = -10, MaxValue = 10, Value = 0 };
            _elevationSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] { _elevationSlider, elevationLabel }));

            headingLabel = new UILabel();
            headingLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            headingLabel.Text = "Heading";
            _headingSlider = new UISlider { MinValue = -10, MaxValue = 10, Value = 0 };
            _headingSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] { _headingSlider, headingLabel }));

            // Lay out container and scroll view.
            View.AddSubview(formContainer);
        }

        private UIStackView getRowStackView(UIView[] views)
        {
            UIStackView row = new UIStackView(views);
            row.TranslatesAutoresizingMaskIntoConstraints = false;
            row.Spacing = 8;
            row.Axis = UILayoutConstraintAxis.Horizontal;
            row.Distribution = UIStackViewDistribution.FillEqually;
            return row;
        }

        private void HeadingChanged(object sender, EventArgs e)
        {
            if (_headingTimer == null)
            {
                _headingTimer = new NSTimer(NSDate.Now, 0.1, true, (timer) =>
                {
                    Camera oldCamera = _arView.OriginCamera;
                    var newHeading = oldCamera.Heading + this.JoystickConverter(_headingSlider.Value);
                    _arView.OriginCamera = oldCamera.RotateTo(newHeading, oldCamera.Pitch, oldCamera.Roll);
                    headingLabel.Text = $"Heading: {(int)_arView.OriginCamera.Heading}";
                });
                NSRunLoop.Main.AddTimer(_headingTimer, NSRunLoopMode.Default);
            }
        }

        private void ElevationChanged(object sender, EventArgs e)
        {
            if (_elevationTimer == null)
            {
                _elevationTimer = new NSTimer(NSDate.Now, 0.1, true, (timer) =>
                {
                    Camera oldCamera = _arView.OriginCamera;
                    _arView.OriginCamera = oldCamera.Elevate(JoystickConverter(_elevationSlider.Value * 3.0));
                    elevationLabel.Text = $"Elevation: {(int)_arView.OriginCamera.Location.Z}m";
                });
                NSRunLoop.Main.AddTimer(_elevationTimer, NSRunLoopMode.Default);
            }
        }

        private double JoystickConverter(double value)
        {
            return Math.Pow(value, 2) / 25 * (value < 0 ? -1.0 : 1.0);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Subscribe to events.
            _headingSlider.ValueChanged += HeadingChanged;
            _headingSlider.TouchUpInside += TouchUpHeading;
            _headingSlider.TouchUpOutside += TouchUpHeading;

            _elevationSlider.ValueChanged += ElevationChanged;
            _elevationSlider.TouchUpInside += TouchUpElevation;
            _elevationSlider.TouchUpOutside += TouchUpElevation;
        }

        private void TouchUpHeading(object sender, EventArgs e)
        {
            _headingTimer.Invalidate();
            _headingTimer = null;
            _headingSlider.Value = 0;
        }

        private void TouchUpElevation(object sender, EventArgs e)
        {
            _elevationTimer.Invalidate();
            _elevationTimer = null;
            _elevationSlider.Value = 0;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _headingSlider.ValueChanged -= HeadingChanged;
        }
    }
}