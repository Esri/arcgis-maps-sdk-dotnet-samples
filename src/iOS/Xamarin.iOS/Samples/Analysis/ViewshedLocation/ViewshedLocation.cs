// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ViewshedLocation
{
    [Register("ViewshedLocation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Viewshed (location)",
        category: "Analysis",
        description: "Perform a viewshed analysis from a defined vantage point. ",
        instructions: "Use the sliders to change the properties (heading, pitch, etc.), of the viewshed and see them updated in real time.",
        tags: new[] { "3D", "LocationViewshed", "Scene", "frustum", "viewshed", "visibility analysis" })]
    public class ViewshedLocation : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private ViewshedLocationSettingsController _settingsVC;
        private UIBarButtonItem _settingsButton;

        // Hold the URL to the elevation source.
        private readonly Uri _localElevationImageService = new Uri("https://scene.arcgis.com/arcgis/rest/services/BREST_DTM_1M/ImageServer");

        // Hold the URL to the buildings scene layer.
        private readonly Uri _buildingsUrl = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");

        // Hold a reference to the viewshed analysis.
        private LocationViewshed _viewshed;

        // Hold a reference to the analysis overlay that will hold the viewshed analysis.
        private AnalysisOverlay _analysisOverlay;

        // Graphics overlay for viewpoint symbol.
        private GraphicsOverlay _viewpointOverlay;

        // Symbol for viewpoint.
        private SimpleMarkerSceneSymbol _viewpointSymbol;

        public ViewshedLocation()
        {
            Title = "Viewshed (location)";
        }

        private void Initialize()
        {
            // Create the scene with the imagery basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add the surface elevation.
            Surface mySurface = new Surface();
            mySurface.ElevationSources.Add(new ArcGISTiledElevationSource(_localElevationImageService));
            _mySceneView.Scene.BaseSurface = mySurface;

            // Add the scene layer.
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_buildingsUrl);
            _mySceneView.Scene.OperationalLayers.Add(sceneLayer);

            // Create the MapPoint representing the initial location.
            MapPoint initialLocation = new MapPoint(-4.5, 48.4, 56.0);

            // Create the location viewshed analysis.
            _viewshed = new LocationViewshed(
                initialLocation,
                0,
                60,
                75,
                90,
                11,
                1500);

            _settingsVC = new ViewshedLocationSettingsController(_viewshed);
            _settingsButton.Enabled = true;

            // Create a camera based on the initial location.
            Camera camera = new Camera(initialLocation, 200.0, 20.0, 70.0, 0.0);

            // Apply the camera to the scene view.
            _mySceneView.SetViewpointCamera(camera);

            // Create an analysis overlay for showing the viewshed analysis.
            _analysisOverlay = new AnalysisOverlay();

            // Add the viewshed analysis to the overlay.
            _analysisOverlay.Analyses.Add(_viewshed);

            // Create a symbol for the viewpoint.
            _viewpointSymbol = SimpleMarkerSceneSymbol.CreateSphere(Color.Blue, 10, SceneSymbolAnchorPosition.Center);

            // Add the symbol to the viewpoint overlay.
            _viewpointOverlay = new GraphicsOverlay
            {
                SceneProperties = new LayerSceneProperties(SurfacePlacement.Absolute)
            };
            _viewpointOverlay.Graphics.Add(new Graphic(initialLocation, _viewpointSymbol));

            // Add the analysis overlay to the SceneView.
            _mySceneView.AnalysisOverlays.Add(_analysisOverlay);

            // Add the graphics overlay
            _mySceneView.GraphicsOverlays.Add(_viewpointOverlay);

            // Update the frustum outline color.
            // The frustum outline shows the volume in which the viewshed analysis is performed.
            Viewshed.FrustumOutlineColor = Color.Blue;
        }

        private void MySceneView_GeoViewTapped(object sender, GeoViewInputEventArgs viewInputEventArgs)
        {
            // Sample isn't ready yet, return.
            if (_viewshed == null) return;

            if (viewInputEventArgs.Location == null)
            {
                // User clicked on the sky - don't update the location with invalid value.
                return;
            }

            // Update the viewshed location.
            _viewshed.Location = viewInputEventArgs.Location;

            // Move the location off of the ground.
            _viewshed.Location = new MapPoint(_viewshed.Location.X, _viewshed.Location.Y, _viewshed.Location.Z + 10.0);

            // Update the viewpoint symbol.
            _viewpointOverlay.Graphics.Clear();
            _viewpointOverlay.Graphics.Add(new Graphic(_viewshed.Location, _viewpointSymbol));
        }

        private void HandleSettings_Clicked(object sender, EventArgs e)
        {
            UINavigationController controller = new UINavigationController(_settingsVC);
            controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            controller.PreferredContentSize = new CGSize(300, 320);
            UIPopoverPresentationController pc = controller.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem) sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new PpDelegate();
            }

            PresentViewController(controller, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _settingsButton = new UIBarButtonItem();
            _settingsButton.Title = "Edit settings";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _settingsButton
            };

            // Add the views.
            View.AddSubviews(_mySceneView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        // Force popover to display on iPhone.
        private class PpDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _mySceneView.GeoViewTapped += MySceneView_GeoViewTapped;
            _settingsButton.Clicked += HandleSettings_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _mySceneView.GeoViewTapped -= MySceneView_GeoViewTapped;
            _settingsButton.Clicked -= HandleSettings_Clicked;
        }
    }

    public class ViewshedLocationSettingsController : UIViewController
    {
        private readonly LocationViewshed _viewshed;
        private UISlider _headingSlider;
        private UISlider _pitchSlider;
        private UISlider _horizontalAngleSlider;
        private UISlider _verticalAngleSlider;
        private UISlider _minimumDistanceSlider;
        private UISlider _maximumDistanceSlider;
        private UISwitch _analysisVisibilitySwitch;
        private UISwitch _frustumVisibilitySwitch;

        ~ViewshedLocationSettingsController()
        {
            System.Diagnostics.Debug.WriteLine($"Finalized {nameof(ViewshedLocationSettingsController)}");
        }

        public ViewshedLocationSettingsController(LocationViewshed viewshed)
        {
            _viewshed = viewshed;
            Title = "Viewshed settings";
        }

        private void HandleSettingsChange(object sender, EventArgs e)
        {
            // Update the viewshed settings.
            _viewshed.Heading = _headingSlider.Value;
            _viewshed.Pitch = _pitchSlider.Value;
            _viewshed.HorizontalAngle = _horizontalAngleSlider.Value;
            _viewshed.VerticalAngle = _verticalAngleSlider.Value;
            _viewshed.MinDistance = _minimumDistanceSlider.Value;
            _viewshed.MaxDistance = _maximumDistanceSlider.Value;

            // Update visibility of the viewshed analysis.
            _viewshed.IsVisible = _analysisVisibilitySwitch.On;

            // Update visibility of the frustum. Note that the frustum will be invisible
            //     regardless of this setting if the viewshed analysis is not visible.
            _viewshed.IsFrustumOutlineVisible = _frustumVisibilitySwitch.On;
        }

        public override void LoadView()
        {
            // Create and add the container views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            UIScrollView scrollView = new UIScrollView();
            scrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(scrollView);

            scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            UIStackView formContainer = new UIStackView();
            formContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.Spacing = 8;
            formContainer.LayoutMarginsRelativeArrangement = true;
            formContainer.Alignment = UIStackViewAlignment.Fill;
            formContainer.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formContainer.Axis = UILayoutConstraintAxis.Vertical;
            formContainer.WidthAnchor.ConstraintEqualTo(300).Active = true;

            // Create and add each row.
            UILabel analysisLabel = new UILabel();
            analysisLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            analysisLabel.Text = "Analysis overlay";
            _analysisVisibilitySwitch = new UISwitch();
            _analysisVisibilitySwitch.On = _viewshed.IsVisible;
            _analysisVisibilitySwitch.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {analysisLabel, _analysisVisibilitySwitch}));

            UILabel frustumLabel = new UILabel();
            frustumLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            frustumLabel.Text = "Frustum";
            _frustumVisibilitySwitch = new UISwitch();
            _frustumVisibilitySwitch.On = _viewshed.IsFrustumOutlineVisible;
            _frustumVisibilitySwitch.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {frustumLabel, _frustumVisibilitySwitch}));

            UILabel headingLabel = new UILabel();
            headingLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            headingLabel.Text = "Heading";
            _headingSlider = new UISlider {MinValue = 0, MaxValue = 360, Value = (float) _viewshed.Heading};
            _headingSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {headingLabel, _headingSlider}));

            UILabel pitchLabel = new UILabel();
            pitchLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            pitchLabel.Text = "Pitch";
            _pitchSlider = new UISlider {MinValue = 0, MaxValue = 180, Value = (float) _viewshed.Pitch};
            _pitchSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {pitchLabel, _pitchSlider}));

            UILabel horizontalLabel = new UILabel();
            horizontalLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            horizontalLabel.Text = "Horizontal";
            _horizontalAngleSlider = new UISlider {MinValue = 1, MaxValue = 120, Value = (float) _viewshed.HorizontalAngle};
            _horizontalAngleSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {horizontalLabel, _horizontalAngleSlider}));

            UILabel verticalLabel = new UILabel();
            verticalLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            verticalLabel.Text = "Vertical";
            _verticalAngleSlider = new UISlider {MinValue = 1, MaxValue = 120, Value = (float) _viewshed.VerticalAngle};
            _verticalAngleSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {verticalLabel, _verticalAngleSlider}));

            UILabel minLabel = new UILabel();
            minLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            minLabel.Text = "Min";
            _minimumDistanceSlider = new UISlider {MinValue = 11, MaxValue = 8999, Value = (float) _viewshed.MinDistance};
            _minimumDistanceSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {minLabel, _minimumDistanceSlider}));

            UILabel maxLabel = new UILabel();
            maxLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            maxLabel.Text = "Max";
            _maximumDistanceSlider = new UISlider {MinValue = 0, MaxValue = 9999, Value = (float) _viewshed.MaxDistance};
            _maximumDistanceSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {maxLabel, _maximumDistanceSlider}));

            // Lay out container and scroll view.
            scrollView.AddSubview(formContainer);

            formContainer.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor).Active = true;
            formContainer.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor).Active = true;
            formContainer.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor).Active = true;
            formContainer.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor).Active = true;
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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Subscribe to events.
            _headingSlider.ValueChanged += HandleSettingsChange;
            _pitchSlider.ValueChanged += HandleSettingsChange;
            _horizontalAngleSlider.ValueChanged += HandleSettingsChange;
            _verticalAngleSlider.ValueChanged += HandleSettingsChange;
            _minimumDistanceSlider.ValueChanged += HandleSettingsChange;
            _maximumDistanceSlider.ValueChanged += HandleSettingsChange;
            _analysisVisibilitySwitch.ValueChanged += HandleSettingsChange;
            _frustumVisibilitySwitch.ValueChanged += HandleSettingsChange;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _headingSlider.ValueChanged -= HandleSettingsChange;
            _pitchSlider.ValueChanged -= HandleSettingsChange;
            _horizontalAngleSlider.ValueChanged -= HandleSettingsChange;
            _verticalAngleSlider.ValueChanged -= HandleSettingsChange;
            _minimumDistanceSlider.ValueChanged -= HandleSettingsChange;
            _maximumDistanceSlider.ValueChanged -= HandleSettingsChange;
            _analysisVisibilitySwitch.ValueChanged -= HandleSettingsChange;
            _frustumVisibilitySwitch.ValueChanged -= HandleSettingsChange;
        }
    }
}