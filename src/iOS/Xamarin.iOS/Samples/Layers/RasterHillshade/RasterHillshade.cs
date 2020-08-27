// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.RasterHillshade
{
    [Register("RasterHillshade")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("134d60f50e184e8fa56365f44e5ce3fb")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Raster hillshade renderer",
        category: "Layers",
        description: "Use a hillshade renderer on a raster.",
        instructions: "Configure the options for rendering, then tap 'Apply hillshade'.",
        tags: new[] { "Visualization", "hillshade", "raster", "shadow", "slope" })]
    public class RasterHillshade : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private HillshadeSettingsController _settingsVC;
        private UIBarButtonItem _configureButton;

        // Store a reference to the raster layer.
        private RasterLayer _rasterLayer;

        public RasterHillshade()
        {
            Title = "Raster hillshade";
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap.
            Map map = new Map(Basemap.CreateStreetsVector());

            // Get the file name for the local raster dataset.
            string filepath =
                DataManager.GetDataFolder("134d60f50e184e8fa56365f44e5ce3fb", "srtm-hillshade", "srtm.tiff");

            // Load the raster file.
            Raster rasterFile = new Raster(filepath);

            try
            {
                // Create and load a new raster layer to show the image.
                _rasterLayer = new RasterLayer(rasterFile);
                await _rasterLayer.LoadAsync();

                // Set up the settings controls.
                _settingsVC = new HillshadeSettingsController(_rasterLayer);

                // Set the initial viewpoint to the raster's full extent.
                map.InitialViewpoint = new Viewpoint(_rasterLayer.FullExtent);

                // Add the layer to the map.
                map.OperationalLayers.Add(_rasterLayer);

                // Add the map to the map view.
                _myMapView.Map = map;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void HandleSettings_Clicked(object sender, EventArgs e)
        {
            UINavigationController controller = new UINavigationController(_settingsVC);
            controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            controller.PreferredContentSize = new CGSize(300, 250);
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
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _configureButton = new UIBarButtonItem();
            _configureButton.Title = "Configure hillshade";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _configureButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _configureButton.Clicked += HandleSettings_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _configureButton.Clicked -= HandleSettings_Clicked;
        }

        // Force popover to display on iPhone.
        private class PpDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }
    }

    public class HillshadeSettingsController : UIViewController
    {
        private readonly RasterLayer _rasterLayer;
        private UISegmentedControl _slopeTypePicker;
        private UISlider _altitudeSlider;
        private UISlider _azimuthSlider;

        public HillshadeSettingsController(RasterLayer rasterLayer)
        {
            _rasterLayer = rasterLayer;
            Title = "Hillshade settings";
        }

        public override void LoadView()
        {
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

            UILabel slopeTypeLabel = new UILabel();
            slopeTypeLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            slopeTypeLabel.Text = "Slope type:";
            formContainer.AddArrangedSubview(slopeTypeLabel);

            _slopeTypePicker = new UISegmentedControl("Degree", "% Rise", "Scaled", "None");
            _slopeTypePicker.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.AddArrangedSubview(_slopeTypePicker);

            UILabel altitudeLabel = new UILabel();
            altitudeLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            altitudeLabel.Text = "Altitude:";
            formContainer.AddArrangedSubview(altitudeLabel);

            _altitudeSlider = new UISlider();
            _altitudeSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _altitudeSlider.MinValue = 0;
            _altitudeSlider.MaxValue = 90;
            _altitudeSlider.Value = 45;
            formContainer.AddArrangedSubview(_altitudeSlider);

            UILabel azimuthLabel = new UILabel();
            azimuthLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            azimuthLabel.Text = "Azimuth:";
            formContainer.AddArrangedSubview(azimuthLabel);

            _azimuthSlider = new UISlider();
            _azimuthSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _azimuthSlider.MinValue = 0;
            _azimuthSlider.MaxValue = 360;
            _azimuthSlider.Value = 270;
            formContainer.AddArrangedSubview(_azimuthSlider);

            scrollView.AddSubview(formContainer);

            formContainer.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor).Active = true;
            formContainer.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor).Active = true;
            formContainer.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor).Active = true;
            formContainer.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor).Active = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _azimuthSlider.ValueChanged += UpdateSettings;
            _altitudeSlider.ValueChanged += UpdateSettings;
            _slopeTypePicker.ValueChanged += UpdateSettings;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _azimuthSlider.ValueChanged -= UpdateSettings;
            _altitudeSlider.ValueChanged -= UpdateSettings;
            _slopeTypePicker.ValueChanged -= UpdateSettings;
        }

        private void UpdateSettings(object sender, EventArgs e)
        {
            SlopeType type = SlopeType.None;
            switch (_slopeTypePicker.SelectedSegment)
            {
                case 0:
                    type = SlopeType.Degree;
                    break;
                case 1:
                    type = SlopeType.PercentRise;
                    break;
                case 2:
                    type = SlopeType.Scaled;
                    break;
            }

            HillshadeRenderer renderer = new HillshadeRenderer(
                altitude: _altitudeSlider.Value,
                azimuth: _azimuthSlider.Value,
                zfactor: 1,
                slopeType: type,
                pixelSizeFactor: 1,
                pixelSizePower: 1,
                nbits: 8);
            _rasterLayer.Renderer = renderer;
        }
    }
}