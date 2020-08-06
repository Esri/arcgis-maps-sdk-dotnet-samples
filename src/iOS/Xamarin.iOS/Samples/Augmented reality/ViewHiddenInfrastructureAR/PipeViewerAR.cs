// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using CoreGraphics;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ViewHiddenInfrastructureAR
{
    internal class PipeViewerAR : UIViewController
    {
        // Pipe graphics that have been passed in by the PipePlacer class.
        public IEnumerable<Graphic> _pipeGraphics;

        // Hold references to UI controls.
        private ARSceneView _arView;
        private UILabel _helpLabel;
        private UIBarButtonItem _calibrateButton;
        private CalibrationViewController _calibrationVC;
        private UISegmentedControl _realScalePicker;

        // Elevation for the scene.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        // Track when user is changing between AR and GPS localization.
        private bool _changingScale;

        // Location data source for AR and route tracking.
        private AdjustableLocationDataSource _locationSource = new AdjustableLocationDataSource();

        // Track whether calibration is in progress and update the UI when that changes.
        private bool _isCalibrating = false;

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
                    ShowCalibrationPopover();

                    // Set continuous positioning in case real scale is changed before calibration.
                    _calibrationVC.SetIsUsingContinuousPositioning(_realScalePicker.SelectedSegment == 0);
                }
                else
                {
                    // Hide the base surface.
                    _arView.Scene.BaseSurface.Opacity = 0;

                    // Disable scene interaction.
                    _arView.InteractionOptions.IsEnabled = false;

                    // Hide the calibration controls.
                    _calibrationVC.DismissViewController(true, null);
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

            // Disable the associated UI control while switching.
            ((UISegmentedControl)sender).Enabled = false;

            // Check if using roaming for AR location mode.
            if (((UISegmentedControl)sender).SelectedSegment == 0)
            {
                await _arView.StopTrackingAsync();

                // Start AR tracking using a continuous GPS signal.
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
                _calibrationVC.SetIsUsingContinuousPositioning(true);
                _helpLabel.Text = "Using GPS signal";
            }
            else
            {
                await _arView.StopTrackingAsync();
                
                // Start AR tracking without using a GPS signal.
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Ignore);
                _calibrationVC.SetIsUsingContinuousPositioning(false);
                _helpLabel.Text = "Using ARKit only";
            }

            // Re-enable the UI control.
            ((UISegmentedControl)sender).Enabled = true;
            _changingScale = false;
        }      

        private void ToggleCalibration(object sender, EventArgs e) => IsCalibrating = !IsCalibrating;

        private void Initialize()
        {
            // Create and add the scene.
            _arView.Scene = new Scene(Basemap.CreateImagery());

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
            pipesOverlay.Graphics.AddRange(_pipeGraphics);

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
                ppDelegate popoverDelegate = new ppDelegate();

                // Stop calibration when the popover closes.
                popoverDelegate.UserDidDismissPopover += (o, e) => IsCalibrating = false;
                pc.Delegate = popoverDelegate;
                pc.PassthroughViews = new UIView[] { View };
            }

            PresentViewController(_calibrationVC, true, null);
        }

        // Force popover to display on iPhone.
        private class ppDelegate : UIPopoverPresentationControllerDelegate
        {
            // Public event enables detection of popover close. When the popover closes, calibration should stop.
            public EventHandler UserDidDismissPopover;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;

            public override void DidDismissPopover(UIPopoverPresentationController popoverPresentationController)
            {
                UserDidDismissPopover?.Invoke(this, EventArgs.Empty);
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

            _calibrationVC = new CalibrationViewController(_arView, _locationSource);

            _calibrateButton = new UIBarButtonItem("Calibrate", UIBarButtonItemStyle.Plain, ToggleCalibration) { Enabled = false };

            _realScalePicker = new UISegmentedControl("Roaming", "Local");
            _realScalePicker.SelectedSegment = 0;
            _realScalePicker.ValueChanged += RealScaleValueChanged;

            toolbar.Items = new[]
            {
                _calibrateButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(){CustomView = _realScalePicker},
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

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Start tracking as soon as the view has been shown.
            await _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
        }

        public override async void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Stop ARKit tracking and unsubscribe from events when the view closes.
            await _arView?.StopTrackingAsync();
        }
    }

    public class CalibrationViewController : UIViewController
    {
        private UISlider _headingSlider;
        private UISlider _elevationSlider;
        private UILabel elevationLabel;
        private UILabel headingLabel;
        private ARSceneView _arView;
        private AdjustableLocationDataSource _locationSource;
        private NSTimer _headingTimer;
        private NSTimer _elevationTimer;
        private bool _isContinuous = true;

        public CalibrationViewController(ARSceneView arView, AdjustableLocationDataSource locationSource)
        {
            _arView = arView;
            _locationSource = locationSource;
        }

        public override void LoadView()
        {
            // Create and add the container views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

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

        public void SetIsUsingContinuousPositioning(bool continuous)
        {
            _isContinuous = continuous;
            if (_elevationSlider != null) _elevationSlider.Enabled = continuous;
        }

        private void HeadingSlider_ValueChanged(object sender, EventArgs e)
        {
            if (_headingTimer == null)
            {
                // Use a timer to continuously update elevation while the user is interacting (joystick effect).
                _headingTimer = new NSTimer(NSDate.Now, 0.1, true, (timer) =>
                {
                    // Get the old camera.
                    Camera oldCamera = _arView.OriginCamera;

                    // Calculate the new heading by applying an offset to the old camera's heading.
                    var newHeading = oldCamera.Heading + this.JoystickConverter(_headingSlider.Value);

                    // Set the origin camera by rotating the existing camera to the new heading.
                    _arView.OriginCamera = oldCamera.RotateTo(newHeading, oldCamera.Pitch, oldCamera.Roll);

                    // Update the heading label.
                    headingLabel.Text = $"Heading: {(int)_arView.OriginCamera.Heading}";
                });
                NSRunLoop.Main.AddTimer(_headingTimer, NSRunLoopMode.Default);
            }
        }

        private void ElevationSlider_ValueChanged(object sender, EventArgs e)
        {
            if (_elevationTimer == null && _isContinuous)
            {
                // Use a timer to continuously update elevation while the user is interacting (joystick effect).
                _elevationTimer = new NSTimer(NSDate.Now, 0.1, true, (timer) =>
                {
                    // Calculate the altitude offset
                    var newValue = _locationSource.AltitudeOffset += JoystickConverter(_elevationSlider.Value);

                    // Set the altitude offset on the location data source.
                    _locationSource.AltitudeOffset = newValue;

                    // Update the label
                    elevationLabel.Text = $"Elevation: {(int)_locationSource.AltitudeOffset}m";
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
            _headingSlider.ValueChanged += HeadingSlider_ValueChanged;
            _headingSlider.TouchUpInside += TouchUpHeading;
            _headingSlider.TouchUpOutside += TouchUpHeading;

            _elevationSlider.ValueChanged += ElevationSlider_ValueChanged;
            _elevationSlider.TouchUpInside += TouchUpElevation;
            _elevationSlider.TouchUpOutside += TouchUpElevation;
        }

        private void TouchUpHeading(object sender, EventArgs e)
        {
            _headingTimer?.Invalidate();
            _headingTimer = null;
            _headingSlider.Value = 0;
        }

        private void TouchUpElevation(object sender, EventArgs e)
        {
            _elevationTimer?.Invalidate();
            _elevationTimer = null;
            _elevationSlider.Value = 0;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _headingSlider.ValueChanged -= HeadingSlider_ValueChanged;
            _headingSlider.TouchUpInside -= TouchUpHeading;
            _headingSlider.TouchUpOutside -= TouchUpHeading;

            _elevationSlider.ValueChanged -= ElevationSlider_ValueChanged;
            _elevationSlider.TouchUpInside -= TouchUpElevation;
            _elevationSlider.TouchUpOutside -= TouchUpElevation;
        }
    }
}