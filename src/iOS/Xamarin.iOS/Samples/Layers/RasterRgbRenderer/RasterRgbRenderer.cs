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

namespace ArcGISRuntime.Samples.RasterRgbRenderer
{
    [Register("RasterRgbRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "RGB renderer",
        category: "Layers",
        description: "Apply an RGB renderer to a raster layer to enhance feature visibility.",
        instructions: "Choose one of the stretch parameter types. The other options will adjust based on the chosen type. Add your inputs and select the 'Update' button to update the renderer.",
        tags: new[] { "analysis", "color", "composite", "imagery", "multiband", "multispectral", "pan-sharpen", "photograph", "raster", "spectrum", "stretch", "visualization" })]
    public class RasterRgbRenderer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private MinMaxSettingsController _minMaxController;
        private PercentClipSettingsController _percentClipController;
        private StandardDeviationSettingsController _stdDevController;
        private UIBarButtonItem _minMaxButton;
        private UIBarButtonItem _percentClipButton;
        private UIBarButtonItem _stdDevButton;

        // Reference to the raster layer to render.
        private RasterLayer _rasterLayer;

        public RasterRgbRenderer()
        {
            Title = "Raster RGB renderer";
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap.
            Map myMap = new Map(Basemap.CreateStreets());

            // Get the file name for the local raster dataset.
            string filepath = GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            try
            {
                // Create a new raster layer to show the image.
                _rasterLayer = new RasterLayer(rasterFile);
                await _rasterLayer.LoadAsync();

                // Set the initial viewpoint for the map to the raster's full extent.
                myMap.InitialViewpoint = new Viewpoint(_rasterLayer.FullExtent);

                // Add the layer to the map.
                myMap.OperationalLayers.Add(_rasterLayer);

                // Add the map to the map view.
                _myMapView.Map = myMap;

                // Create the settings view controllers.
                _minMaxController = new MinMaxSettingsController(_rasterLayer);
                _percentClipController = new PercentClipSettingsController(_rasterLayer);
                _stdDevController = new StandardDeviationSettingsController(_rasterLayer);
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");
        }

        private void MinMax_Clicked(object sender, EventArgs e)
        {
            ShowPopover(_minMaxController, (UIBarButtonItem) sender);
        }

        private void PercentClip_Clicked(object sender, EventArgs e)
        {
            ShowPopover(_percentClipController, (UIBarButtonItem) sender);
        }

        private void StdDev_Clicked(object sender, EventArgs e)
        {
            ShowPopover(_stdDevController, (UIBarButtonItem) sender);
        }

        private void ShowPopover(UIViewController controller, UIBarButtonItem sender)
        {
            UINavigationController navController = new UINavigationController(controller);
            navController.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            UIPopoverPresentationController pc = navController.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Unknown;
                pc.Delegate = new PpDelegate();
            }

            PresentViewController(navController, true, null);
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

            _minMaxButton = new UIBarButtonItem();
            _minMaxButton.Title = "Min/Max";

            _percentClipButton = new UIBarButtonItem();
            _percentClipButton.Title = "% Clip";

            _stdDevButton = new UIBarButtonItem();
            _stdDevButton.Title = "Std. Dev.";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _minMaxButton,
                _percentClipButton,
                _stdDevButton
            };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

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

            _minMaxButton.Clicked += MinMax_Clicked;
            _percentClipButton.Clicked += PercentClip_Clicked;
            _stdDevButton.Clicked += StdDev_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            _minMaxButton.Clicked -= MinMax_Clicked;
            _percentClipButton.Clicked -= PercentClip_Clicked;
            _stdDevButton.Clicked -= StdDev_Clicked;
        }

        // Used to force popovers to appear on iPhone.
        private class PpDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }
    }

    public class MinMaxSettingsController : UIViewController
    {
        // Hold references to UI controls.
        private RgbValuePickerModel _minPickerModel;
        private RgbValuePickerModel _maxPickerModel;
        private UIPickerView _maxPicker;

        // Hold a reference to the raster layer.
        private RasterLayer _rasterLayer;

        public MinMaxSettingsController(RasterLayer rasterLayer)
        {
            _rasterLayer = rasterLayer;
            Title = "Min/Max RGB Renderer";
        }

        private void ApplyButton_Clicked(object sender, EventArgs e)
        {
            double[] minValues =
                {_minPickerModel.SelectedRed, _minPickerModel.SelectedBlue, _minPickerModel.SelectedGreen};
            double[] maxValues =
                {_maxPickerModel.SelectedRed, _maxPickerModel.SelectedBlue, _maxPickerModel.SelectedGreen};
            MinMaxStretchParameters parameters = new MinMaxStretchParameters(minValues, maxValues);

            int[] bands = {0, 1, 2};
            _rasterLayer.Renderer = new RgbRenderer(parameters, bands, null, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _maxPicker.Select(255, 0, false);
            _maxPicker.Select(255, 1, false);
            _maxPicker.Select(255, 2, false);

            NavigationController.PreferredContentSize = new CGSize(300, 250);
        }

        public override void LoadView()
        {
            View = new UIView() { BackgroundColor = UIColor.White };

            UIScrollView scrollView = new UIScrollView();
            scrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(scrollView);

            UIStackView formContainer = new UIStackView();
            formContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.Spacing = 8;
            formContainer.LayoutMarginsRelativeArrangement = true;
            formContainer.Alignment = UIStackViewAlignment.Center;
            formContainer.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formContainer.Axis = UILayoutConstraintAxis.Vertical;

            // Add controls here.
            UILabel minLabel = new UILabel();
            minLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            minLabel.Text = "Minimum (Red | Green | Blue)";
            minLabel.TextAlignment = UITextAlignment.Center;
            formContainer.AddArrangedSubview(minLabel);

            UIPickerView minPicker = new UIPickerView();
            minPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            _minPickerModel = new RgbValuePickerModel(0, 0, 0);
            minPicker.HeightAnchor.ConstraintEqualTo(60).Active = true;
            minPicker.Model = _minPickerModel;
            formContainer.AddArrangedSubview(minPicker);

            UILabel maxLabel = new UILabel();
            maxLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            maxLabel.Text = "Maximum (Red | Green | Blue)";
            maxLabel.TextAlignment = UITextAlignment.Center;
            formContainer.AddArrangedSubview(maxLabel);

            _maxPicker = new UIPickerView();
            _maxPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            _maxPickerModel = new RgbValuePickerModel(255, 255, 255);
            _maxPicker.HeightAnchor.ConstraintEqualTo(60).Active = true;
            _maxPicker.Model = _maxPickerModel;
            formContainer.AddArrangedSubview(_maxPicker);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Apply", UIBarButtonItemStyle.Plain, ApplyButton_Clicked);

            scrollView.AddSubview(formContainer);

            scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            formContainer.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor).Active = true;
            formContainer.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor).Active = true;
            formContainer.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor).Active = true;
            formContainer.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor).Active = true;
            formContainer.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor).Active = true;
        }

        // Class that defines a view model for showing color values (0-255 for RGB) in a picker control.
        private class RgbValuePickerModel : UIPickerViewModel
        {
            // Constructor takes the default values for RGB.
            public RgbValuePickerModel(int defaultRed, int defaultGreen, int defaultBlue)
            {
                SelectedRed = defaultRed;
                SelectedGreen = defaultGreen;
                SelectedBlue = defaultBlue;
            }

            // Property to expose the currently selected red value in the picker.
            public int SelectedRed { get; private set; } = 0;

            // Property to expose the currently selected green value in the picker.
            public int SelectedGreen { get; private set; } = 0;

            // Property to expose the currently selected blue value in the picker.
            public int SelectedBlue { get; private set; } = 0;

            // Return the number of picker components (three sections: red, green, and blue values).
            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 3;
            }

            // Return the number of rows in each of the sections (0-255 = 256).
            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return 256;
            }

            // Get the title to display in each picker component.
            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return row.ToString();
            }

            // Handle the selection event for the picker.
            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                // Get the selected RGB values.
                SelectedRed = (int) pickerView.SelectedRowInComponent(0);
                SelectedGreen = (int) pickerView.SelectedRowInComponent(1);
                SelectedBlue = (int) pickerView.SelectedRowInComponent(2);
            }

            // Return the desired width for each component in the picker.
            public override nfloat GetComponentWidth(UIPickerView pickerView, nint component)
            {
                // All components display the same range of values (largest is 3 digits).
                return 60f;
            }

            // Return the desired height for rows in the picker.
            public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
            {
                return 30f;
            }
        }
    }

    public class PercentClipSettingsController : UIViewController
    {
        // Hold a reference to the raster layer.
        private readonly RasterLayer _rasterLayer;

        // Hold references to UI controls.
        private UISlider _minSlider;
        private UISlider _maxSlider;

        public PercentClipSettingsController(RasterLayer rasterLayer)
        {
            _rasterLayer = rasterLayer;
            Title = "% Clip Renderer";
        }

        private void ApplyButton_Clicked(object sender, EventArgs e)
        {
            PercentClipStretchParameters parameters =
                new PercentClipStretchParameters(_minSlider.Value, _maxSlider.Value);

            int[] bands = {0, 1, 2};
            _rasterLayer.Renderer = new RgbRenderer(parameters, bands, null, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.PreferredContentSize = new CGSize(250, 100);
        }

        public override void LoadView()
        {
            View = new UIView() { BackgroundColor = UIColor.White };

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

            // Add controls here.
            UILabel minLabel = new UILabel();
            minLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            minLabel.Text = "Minimum:";
            _minSlider = new UISlider();
            _minSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _minSlider.MinValue = 0;
            _minSlider.MaxValue = 100;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {minLabel, _minSlider}));
            UILabel maxLabel = new UILabel();
            maxLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            maxLabel.Text = "Maximum:";
            _maxSlider = new UISlider();
            _maxSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _maxSlider.MinValue = 0;
            _maxSlider.MaxValue = 100;
            formContainer.AddArrangedSubview(getRowStackView(new UIView[] {maxLabel, _maxSlider}));

            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Apply", UIBarButtonItemStyle.Plain, ApplyButton_Clicked);

            scrollView.AddSubview(formContainer);

            formContainer.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor).Active = true;
            formContainer.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor).Active = true;
            formContainer.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor).Active = true;
            formContainer.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor).Active = true;
            formContainer.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor).Active = true;
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
    }

    public class StandardDeviationSettingsController : UIViewController
    {
        private readonly RasterLayer _rasterLayer;
        private StdDevFactorPickerModel _pickerModel;

        public StandardDeviationSettingsController(RasterLayer rasterLayer)
        {
            _rasterLayer = rasterLayer;
            Title = "Std. Deviation Renderer";
        }

        private void ApplyButton_Clicked(object sender, EventArgs e)
        {
            StandardDeviationStretchParameters parameters = new StandardDeviationStretchParameters(_pickerModel.SelectedFactor);

            int[] bands = {0, 1, 2};
            _rasterLayer.Renderer = new RgbRenderer(parameters, bands, null, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.PreferredContentSize = new CGSize(200, 175);
        }

        public override void LoadView()
        {
            View = new UIView() { BackgroundColor = UIColor.White };

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
            formContainer.Alignment = UIStackViewAlignment.Center;
            formContainer.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formContainer.Axis = UILayoutConstraintAxis.Vertical;

            // Add controls here.
            UILabel factorLabel = new UILabel();
            factorLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            factorLabel.Text = "Factor:";
            formContainer.AddArrangedSubview(factorLabel);

            UIPickerView picker = new UIPickerView();
            picker.TranslatesAutoresizingMaskIntoConstraints = false;
            _pickerModel = new StdDevFactorPickerModel();
            picker.Model = _pickerModel;
            picker.HeightAnchor.ConstraintEqualTo(90).Active = true;
            formContainer.AddArrangedSubview(picker);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Apply", UIBarButtonItemStyle.Plain, ApplyButton_Clicked);

            scrollView.AddSubview(formContainer);

            formContainer.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor).Active = true;
            formContainer.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor).Active = true;
            formContainer.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor).Active = true;
            formContainer.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor).Active = true;
            formContainer.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor).Active = true;
        }

        // Class that defines a view model for showing standard deviation factor values (0.5-4.50) in a picker control.
        private class StdDevFactorPickerModel : UIPickerViewModel
        {
            // Array of available factor values.
            private readonly double[] _factorValues = {0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5};

            // Property to expose the currently selected factor value in the picker.
            public double SelectedFactor { get; private set; } = 0.5;

            // Return the number of picker components (just one).
            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 1;
            }

            // Return the number of rows in the section (the size of the factor choice array).
            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return _factorValues.Length;
            }

            // Get the title to display in the picker component.
            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return _factorValues[row].ToString();
            }

            // Handle the selection event for the picker.
            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                // Get the selected standard deviation factor.
                SelectedFactor = _factorValues[pickerView.SelectedRowInComponent(0)];
            }

            // Return the desired width for each component in the picker.
            public override nfloat GetComponentWidth(UIPickerView pickerView, nint component)
            {
                return 60f;
            }

            // Return the desired height for rows in the picker.
            public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
            {
                return 30f;
            }
        }
    }
}