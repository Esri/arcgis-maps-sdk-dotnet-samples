// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.


using CoreGraphics;
using CoreImage;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.CollectDataAR
{
    [Register("CollectDataAR")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Collect data in AR",
        "Augmented reality",
        "Tap on real-world objects to collect data.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class CollectDataAR : UIViewController
    {
        // UI controls.
        private ARSceneView _arView;
        private UILabel _helpLabel;
        private UIBarButtonItem _calibrateButton;
        private UIBarButtonItem _addButton;
        private CalibrationViewController _calibrationVC;
        private UISegmentedControl _realScalePicker;

        // Track when user is changing between AR and GPS localization.
        private bool _changingScale;

        // Track whether or not the user is calibrating the AR scene view.
        private bool _isCalibrating = false;

        // Feature table for collected data about trees.
        private ServiceFeatureTable _featureTable = new ServiceFeatureTable(new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/AR_Tree_Survey/FeatureServer/0"));

        // Elevation source for AR scene calibration.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        // Graphics for tapped points in the scene.
        private GraphicsOverlay _graphicsOverlay;
        private SimpleMarkerSceneSymbol _tappedPointSymbol = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Diamond, System.Drawing.Color.Orange, 0.5, 0.5, 0.5, SceneSymbolAnchorPosition.Center);

        // Location data source for AR and route tracking.
        private AdjustableLocationDataSource _locationSource = new AdjustableLocationDataSource();

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
                    ShowCalibrationPopover();
                }
                else
                {
                    // Hide the base surface.
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

            _calibrationVC = new CalibrationViewController(_arView, _locationSource);

            _calibrateButton = new UIBarButtonItem("Calibrate", UIBarButtonItemStyle.Plain, ToggleCalibration);
            _addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, AddButtonPressed) { Enabled = false };

            _realScalePicker = new UISegmentedControl("Roaming", "Local");
            _realScalePicker.SelectedSegment = 0;
            _realScalePicker.ValueChanged += RealScaleValueChanged;

            toolbar.Items = new[]
            {
                _calibrateButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(){CustomView = _realScalePicker},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _addButton
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
            }
            else
            {
                await _arView.StopTrackingAsync();

                // Start AR tracking without using a GPS signal.
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Ignore);
                _calibrationVC.SetIsUsingContinuousPositioning(false);
            }

            // Re-enable the UI control.
            ((UISegmentedControl)sender).Enabled = true;
            _changingScale = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
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

            // Configure scene view display for real-scale AR: no space effect or atmosphere effect.
            _arView.SpaceEffect = SpaceEffect.None;
            _arView.AtmosphereEffect = AtmosphereEffect.None;

            // Add a graphics overlay for displaying points in AR.
            _graphicsOverlay = new GraphicsOverlay();
            _graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _graphicsOverlay.Renderer = new SimpleRenderer(_tappedPointSymbol);
            _arView.GraphicsOverlays.Add(_graphicsOverlay);

            // Add the event for the user tapping the screen.
            _arView.GeoViewTapped += arViewTapped;
        }

        private void arViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Don't add features when calibrating the AR view.
            if(_isCalibrating)
            {
                return;
            }

            // Try to get the real-world position of that tapped AR plane.
            MapPoint planeLocation = _arView.ARScreenToLocation(e.Position);

            // Remove any existing graphics.
            _graphicsOverlay.Graphics.Clear();


            // Check if a Map Point was identified.
            if (planeLocation != null)
            {
                // Add a graphic at the tapped location.
                _graphicsOverlay.Graphics.Add(new Graphic(planeLocation));
                _addButton.Enabled = true;
                _helpLabel.Text = "Placed relative to ARKit plane";
            }
            else
            {
                new UIAlertView("Error", "Didn't find anything, try again.", (IUIAlertViewDelegate)null, "OK", null).Show();
                _addButton.Enabled = false;
            }
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
                pc.PassthroughViews = new UIView[]{ View };
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

        private async void AddButtonPressed(object sender, EventArgs e)
        {
            // Check if the user has already tapped a point.
            if (!_graphicsOverlay.Graphics.Any())
            {
                new UIAlertView("Error", "Didn't find anything, try again.", (IUIAlertViewDelegate)null, "OK", null).Show();
                return;
            }
            try
            {
                // Prevent the user from changing the tapped feature.
                _arView.GeoViewTapped -= arViewTapped;

                // Prompt the user for the health value of the tree.
                int healthValue = await GetTreeHealthValue();

                // Get the video buffer for the camera.
                CoreVideo.CVPixelBuffer coreVideoBuffer = _arView.ARSCNView.Session.CurrentFrame?.CapturedImage;
                if (coreVideoBuffer != null)
                {
                    // Get the current frame from the video buffer.
                    CIImage coreImage = new CIImage(coreVideoBuffer);

                    // Rotate the image to the right. (This assumes that the device is in portrait mode.)
                    coreImage = coreImage.CreateByApplyingOrientation(ImageIO.CGImagePropertyOrientation.Right);

                    // Create a CoreGraphics image using the CoreImage image.
                    CGImage imageRef = new CIContext().CreateCGImage(coreImage, new CGRect(0, 0, coreVideoBuffer.Height, coreVideoBuffer.Width));

                    // Create a UIImage using the CoreImage image.
                    UIImage rotatedImage = new UIImage(imageRef);

                    // Create a new ArcGIS feature and add it to the feature service.
                    await CreateFeature(rotatedImage, healthValue);
                }
                else
                {
                    new UIAlertView("Error", "Didn't get image for tap.", (IUIAlertViewDelegate)null, "OK", null).Show();
                }
            }
            // This exception is thrown when the user cancels out of the prompt.
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                // Restore the event listener for adding new features.
                _arView.GeoViewTapped += arViewTapped;
            }
        }

        private async Task<int> GetTreeHealthValue()
        {
            // Create a new copmletion source for the prompt.
            TaskCompletionSource<int> _healthCompletionSource = new TaskCompletionSource<int>();

            // Create prompt for health of the tree.
            UIAlertController prompt = UIAlertController.Create("Take picture and add tree", "How healthy is this tree?", UIAlertControllerStyle.ActionSheet);
            prompt.AddAction(UIAlertAction.Create("Dead", UIAlertActionStyle.Default, (o) => _healthCompletionSource.TrySetResult(0)));
            prompt.AddAction(UIAlertAction.Create("Distressed", UIAlertActionStyle.Default, (o) => _healthCompletionSource.TrySetResult(5)));
            prompt.AddAction(UIAlertAction.Create("Healthy", UIAlertActionStyle.Default, (o) => _healthCompletionSource.TrySetResult(10)));
            prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (o) => _healthCompletionSource.TrySetCanceled()));

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.DidDismiss += (s, ee) => _healthCompletionSource.TrySetCanceled();
            }

            // Present the prompt to the user.
            PresentViewController(prompt, true, null);

            // Return the selected health value.
            return await _healthCompletionSource.Task;
        }

        private async Task CreateFeature(UIImage capturedImage, int healthValue)
        {
            _helpLabel.Text = "Adding feature...";

            try
            {
                // Get the geometry of the feature.
                MapPoint featurePoint = _graphicsOverlay.Graphics.First().Geometry as MapPoint;

                // Create attributes for the feature using the user selected health value.
                IEnumerable<KeyValuePair<string, object>> featureAttributes = new Dictionary<string, object>() { { "Health", (short)healthValue }, { "Height", 3.2 }, { "Diameter", 1.2 } };

                // Ensure that the feature table is loaded.
                if(_featureTable.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded )
                {
                    await _featureTable.LoadAsync();
                }

                // Create the new feature
                ArcGISFeature newFeature = _featureTable.CreateFeature(featureAttributes, featurePoint) as ArcGISFeature;

                // Convert the image into a byte array.
                Stream imageStream = capturedImage.AsJPEG().AsStream();
                byte[] attachmentData = new byte[imageStream.Length];
                imageStream.Read(attachmentData, 0, attachmentData.Length);

                // Add the attachment.
                // The contentType string is the MIME type for JPEG files, image/jpeg.
                await newFeature.AddAttachmentAsync("tree.jpg", "image/jpeg", attachmentData);

                // Add the newly created feature to the feature table.
                await _featureTable.AddFeatureAsync(newFeature);

                // Apply the edits to the service feature table.
                await _featureTable.ApplyEditsAsync();

                // Reset the user interface.
                _helpLabel.Text = "Tap to create a feature";
                _graphicsOverlay.Graphics.Clear();
                _addButton.Enabled = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                new UIAlertView("Error", "Could not create feature", (IUIAlertViewDelegate)null, "OK", null).Show();
            }
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

        public void SetIsUsingContinuousPositioning(bool continuous)
        {
            _isContinuous = continuous;
            if(_isContinuous)
            {
                _elevationSlider.Enabled = true;
            }
            else
            {
                _elevationSlider.Enabled = false;
            }
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
                    var newValue = _locationSource.AltitudeOffset += JoystickConverter(_elevationSlider.Value * 3.0);

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