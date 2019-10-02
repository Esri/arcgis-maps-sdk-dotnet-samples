using CoreGraphics;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntime.Samples.Augmented_reality.CollectDataAR
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
        private ARSceneView _arView;
        private UILabel _helpLabel;
        private UIBarButtonItem _calibrateButton;
        private UIBarButtonItem _addButton;
        private CalibrationViewController _calibrationVC;
        private UISegmentedControl _realScalePicker;

        private bool _changingScale;

        private FeatureLayer _featureLayer;
        private FeatureTable _featureTable = new ServiceFeatureTable(new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/AR_Tree_Survey/FeatureServer/0"));

        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        private GraphicsOverlay _graphicsOverlay;
        private SimpleMarkerSceneSymbol _tappedPointSymbol = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Diamond, System.Drawing.Color.Orange, 0.5, 0.5, 0.5, SceneSymbolAnchorPosition.Center);

        private bool _isCalibrating = false;

        private bool IsCalibrating
        {
            get => _isCalibrating;
            set
            {
                _isCalibrating = value;
                if (_isCalibrating)
                {
                    _arView.Scene.BaseSurface.IsEnabled = true;
                    ShowCalibrationPopover();
                }
                else
                {
                    _arView.Scene.BaseSurface.IsEnabled = false;
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
            if(_changingScale)
            {
                return;
            }
            _changingScale = true;
            if (((UISegmentedControl)sender).SelectedSegment == 0)
            {
                _arView.StopTracking();
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
            }
            else
            {
                _arView.StopTracking();
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Initial);
            }
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
            var scene = new Scene(BasemapType.ImageryWithLabels);

            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            scene.BaseSurface.ElevationSources.Add(_elevationSource);
            scene.BaseSurface.NavigationConstraint = NavigationConstraint.None;
            scene.BaseSurface.Opacity = 0.5;

            _featureLayer = new FeatureLayer(_featureTable);
            scene.OperationalLayers.Add(_featureLayer);
            _featureLayer.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;

            _arView.SpaceEffect = SpaceEffect.None;
            _arView.AtmosphereEffect = AtmosphereEffect.None;

            _arView.Scene = scene;
            _graphicsOverlay = new GraphicsOverlay();
            _graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _graphicsOverlay.Renderer = new SimpleRenderer(_tappedPointSymbol);
            _arView.GraphicsOverlays.Add(_graphicsOverlay);

            _arView.GeoViewTapped += arViewTapped;
        }

        private void arViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Try to get the real-world position of that tapped AR plane.
            var planeLocation = _arView.ARScreenToLocation(e.Position);

            // Remove any existing graphics.
            _graphicsOverlay.Graphics.Clear();

            if (planeLocation != null)
            {
                _graphicsOverlay.Graphics.Add(new Graphic(planeLocation));
                _addButton.Enabled = true;
                _helpLabel.Text = "Placed relative to ARKit plane";
            }
            else
            {
                new UIAlertView("Error", "Din't find anything, try again.", (IUIAlertViewDelegate)null, "OK", null).Show();
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
                pc.Delegate = new ppDelegate();
            }

            PresentViewController(_calibrationVC, true, null);
        }

        private void AddButtonPressed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
            _arView.StartTrackingAsync(ARLocationTrackingMode.Initial);
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
        private UISegmentedControl _realScalePicker;

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
            _headingSlider.TouchUpInside -= TouchUpHeading;
            _headingSlider.TouchUpOutside -= TouchUpHeading;

            _elevationSlider.ValueChanged -= ElevationChanged;
            _elevationSlider.TouchUpInside -= TouchUpElevation;
            _elevationSlider.TouchUpOutside -= TouchUpElevation;
        }
    }
}