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
        "Raster RGB renderer",
        "Layers",
        "This sample demonstrates how to use an RGB renderer on a raster layer. An RGB renderer is used to adjust the color bands of a multi-spectral image.",
        "Choose one of the stretch parameter types. The other options will adjust based on the chosen type. Add your inputs and press the Apply button to update the renderer.")]
    public class RasterRgbRenderer : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UISegmentedControl _segmentButton = new UISegmentedControl();
        private readonly UIToolbar _toolbar = new UIToolbar();

        // Overlay with entry controls for applying a new raster renderer.
        private UpdateRendererDialogOverlay _updateRendererUi;

        // Reference to the raster layer to render.
        private RasterLayer _rasterLayer;

        public RasterRgbRenderer()
        {
            Title = "Raster RGB renderer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI for inputting renderer parameters.
            CreateLayout();

            // Initialize the map and raster layer.
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _segmentButton.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);

                if (_updateRendererUi != null)
                {
                    _updateRendererUi.Bounds = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height - topMargin);
                    _updateRendererUi.Frame = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height - topMargin);
                    _updateRendererUi.Center = View.Center;
                }

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap.
            Map myMap = new Map(Basemap.CreateStreets());

            // Get the file name for the local raster dataset.
            string filepath = GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            // Create a new raster layer to show the image.
            _rasterLayer = new RasterLayer(rasterFile);
            await _rasterLayer.LoadAsync();

            // Once the layer is loaded, enable the button for changing the renderer.
            _segmentButton.Enabled = true;

            // Set the initial viewpoint for the map to the raster's full extent.
            myMap.InitialViewpoint = new Viewpoint(_rasterLayer.FullExtent);

            // Add the layer to the map.
            myMap.OperationalLayers.Add(_rasterLayer);

            // Add the map to the map view.
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Configure segmented button control.
            _segmentButton.BackgroundColor = UIColor.White;
            _segmentButton.InsertSegment("Min Max RGB", 0, false);
            _segmentButton.InsertSegment("% Clip", 1, false);
            _segmentButton.InsertSegment("Std Dev", 2, false);
            _segmentButton.Enabled = false;

            // Handle the "click" for each segment (new segment is selected).
            _segmentButton.ValueChanged += SegmentButtonClicked;

            // Add the MapView and segmented button to the page.
            View.AddSubviews(_myMapView, _toolbar, _segmentButton);
        }

        private void UpdateRenderer(object sender, StretchParametersEventArgs e)
        {
            // Create an array to specify the raster bands (red, green, blue).
            int[] bands = {0, 1, 2};

            // Create the RgbRenderer with the stretch parameters passed in, then apply it to the raster layer.
            _rasterLayer.Renderer = new RgbRenderer(e.StretchParams, bands, null, true);

            // Remove the parameter input UI.
            _updateRendererUi.Hide();
            _updateRendererUi = null;
        }

        private void SegmentButtonClicked(object sender, EventArgs e)
        {
            // Get the segmented button control that raised the event.
            UISegmentedControl buttonControl = (UISegmentedControl)sender;

            // Get the type of stretch inputs to show (title of the selected button).
            string stretchType = buttonControl.TitleAt(buttonControl.SelectedSegment);

            // Show the UI and pass the type of parameter inputs to show.
            ShowRendererParamsUi(stretchType);

            // Deselect all segments (user might want to click the same control twice)
            buttonControl.SelectedSegment = -1;
        }

        private void ShowRendererParamsUi(string stretchType)
        {
            if (_updateRendererUi != null)
            {
                return;
            }

            // Create a view to show map item info entry controls over the map view
            CGRect ovBounds = new CGRect(0, 60, View.Bounds.Width, View.Bounds.Height);
            _updateRendererUi = new UpdateRendererDialogOverlay(ovBounds, 0.9f, UIColor.White, stretchType);

            // Handle the OnSearchMapsTextEntered event to get the info entered by the user
            _updateRendererUi.OnStretchInputsEntered += UpdateRenderer;

            // Handle the cancel event when the user closes the dialog without choosing to search
            _updateRendererUi.OnCanceled += (s, e) =>
            {
                // Remove the search input UI
                _updateRendererUi.Hide();
                _updateRendererUi = null;
            };

            // Add the search UI view (will display semi-transparent over the map view)
            View.Add(_updateRendererUi);
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");
        }
    }

    #region UI for entering raster renderer properties.

    // View containing renderer parameter input controls (for three stretch types).
    public class UpdateRendererDialogOverlay : UIView
    {
        // Event to provide the user inputs when the view is dismissed.
        public event EventHandler<StretchParametersEventArgs> OnStretchInputsEntered;

        // Event to report that the input was canceled.
        public event EventHandler OnCanceled;

        // Field to store the type of stretch inputs.
        private readonly string _stretchParamsType;

        // Fields for controls that will be referenced later.
        private UIPickerView _minRgbPicker;
        private UIPickerView _maxRgbPicker;
        private UISlider _minPercentSlider;
        private UISlider _maxPercentSlider;
        private UIPickerView _stdDevPicker;

        public UpdateRendererDialogOverlay(CGRect frame, nfloat transparency, UIColor color, string stretchType) : base(frame)
        {
            // Store the type of stretch parameters to define.
            _stretchParamsType = stretchType;

            // Create a semi-transparent overlay with the specified background color.
            BackgroundColor = color;
            Alpha = transparency;

            // Label to describe the type of input.
            UILabel descriptionLabel = new UILabel
            {
                Text = "Stretch parameters (" + _stretchParamsType + ")",
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.Black
            };

            // Button to create the stretch parameters and pass it back to the main page.
            UIButton inputStretchParamsButton = new UIButton();
            inputStretchParamsButton.SetTitle("Apply", UIControlState.Normal);
            inputStretchParamsButton.SetTitleColor(TintColor, UIControlState.Normal);
            inputStretchParamsButton.TouchUpInside += InputStretchParamsButton_Click;

            // Button to cancel the inputs.
            UIButton cancelButton = new UIButton();
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled?.Invoke(this, null); };

            // Create inputs that are specific for the stretch type.
            switch (_stretchParamsType)
            {
                case "Min Max RGB":
                    CreateRgbInputUi(inputStretchParamsButton, cancelButton, descriptionLabel);
                    break;
                case "% Clip":
                    CreatePercentInputUi(inputStretchParamsButton, cancelButton, descriptionLabel);
                    break;
                case "Std Dev":
                    CreateStdDevInputUi(inputStretchParamsButton, cancelButton, descriptionLabel);
                    break;
            }
        }

        private void CreateRgbInputUi(UIButton applyButton, UIButton cancelButton, UILabel description)
        {
            // Set size and spacing for controls.
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat columnSpace = 15;
            nfloat buttonWidth = 60;

            // Store the total height and width.
            nfloat totalWidth = Frame.Width - 60;

            // Find the center x and y of the view.
            nfloat centerX = Frame.Width / 2;

            // Find the start x and y for the control layout.
            nfloat controlX = centerX - totalWidth / 2;
            nfloat controlY = 200;

            // Position the input description label.
            description.Frame = new CGRect(controlX, controlY, totalWidth, controlHeight);

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Create a label for the minimum RGB input.
            UILabel minRgbLabel = new UILabel(new CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Minimum (Red | Green | Blue)",
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 15;

            // Create a picker for minimum RGB values (all are 0 by default).
            _minRgbPicker = new UIPickerView(new CGRect(controlX, controlY, 200, 100))
            {
                Model = new RgbValuePickerModel(0, 0, 0)
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Create a label for the maximum RGB input.
            UILabel maxRgbLabel = new UILabel(new CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Maximum (Red | Green | Blue)",
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 15;

            // Create a picker for the maximum RGB values.
            _maxRgbPicker = new UIPickerView(new CGRect(controlX, controlY, 200, 100))
            {
                Model = new RgbValuePickerModel(255, 255, 255)
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Set the frame for the apply button.
            applyButton.Frame = new CGRect(controlX, controlY, buttonWidth, controlHeight);

            // Adjust the X position for the next control.
            controlX = controlX + buttonWidth + columnSpace;

            // Set the frame for the cancel button.
            cancelButton.Frame = new CGRect(controlX, controlY, buttonWidth, controlHeight);

            // Add the input controls.
            AddSubviews(description,
                minRgbLabel, _minRgbPicker,
                maxRgbLabel, _maxRgbPicker,
                applyButton, cancelButton);

            // Select 255 as the default for red, green, and blue.
            _maxRgbPicker.Select(255, 0, false);
            _maxRgbPicker.Select(255, 1, false);
            _maxRgbPicker.Select(255, 2, false);
        }

        private void CreatePercentInputUi(UIButton applyButton, UIButton cancelButton, UILabel description)
        {
            // Set size and spacing for controls.
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat columnSpace = 15;
            nfloat buttonWidth = 60;

            // Store the total height and width.
            nfloat totalWidth = Frame.Width - 60;

            // Find the center x and y of the view.
            nfloat centerX = Frame.Width / 2;

            // Find the start x and y for the control layout.
            nfloat controlX = centerX - totalWidth / 2;
            nfloat controlY = 200;

            // Position the input description label.
            description.Frame = new CGRect(controlX, controlY, totalWidth, controlHeight);

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Create a label for the minimum percent input.
            UILabel minPercentLabel = new UILabel(new CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Minimum: ",
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 15;

            // Create a slider for minimum percent clip value.
            _minPercentSlider = new UISlider(new CGRect(controlX, controlY, 200, 100))
            {
                MinValue = 0,
                MaxValue = 100
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Create a label for the maximum percent clip input.
            UILabel maxPercentLabel = new UILabel(new CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Maximum: ",
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 15;

            // Create a picker for the maximum RGB values.
            _maxPercentSlider = new UISlider(new CGRect(controlX, controlY, 200, 100))
            {
                MinValue = 0,
                MaxValue = 100
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Set the frame for the apply button.
            applyButton.Frame = new CGRect(controlX, controlY, buttonWidth, controlHeight);

            // Adjust the X position for the next control.
            controlX = controlX + buttonWidth + columnSpace;

            // Set the frame for the cancel button.
            cancelButton.Frame = new CGRect(controlX, controlY, buttonWidth, controlHeight);

            // Add the input controls.
            AddSubviews(description,
                minPercentLabel, _minPercentSlider,
                maxPercentLabel, _maxPercentSlider,
                applyButton, cancelButton);
        }

        private void CreateStdDevInputUi(UIButton applyButton, UIButton cancelButton, UILabel description)
        {
            // Set size and spacing for controls.
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat columnSpace = 15;
            nfloat buttonWidth = 60;

            // Store the total height and width.
            nfloat totalWidth = Frame.Width - 60;

            // Find the center x and y of the view.
            nfloat centerX = Frame.Width / 2;

            // Find the start x and y for the control layout.
            nfloat controlX = centerX - totalWidth / 2;
            nfloat controlY = 200;

            // Position the input description label.
            description.Frame = new CGRect(controlX, controlY, totalWidth, controlHeight);

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Create a label for the standard deviation factor input.
            UILabel factorLabel = new UILabel(new CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Factor: ",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 15;

            // Create a picker for the standard deviation factor.
            _stdDevPicker = new UIPickerView(new CGRect(controlX, controlY, 200, 100))
            {
                Model = new StdDevFactorPickerModel()
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Set the frame for the apply button.
            applyButton.Frame = new CGRect(controlX, controlY, buttonWidth, controlHeight);

            // Adjust the X position for the next control.
            controlX = controlX + buttonWidth + columnSpace;

            // Set the frame for the cancel button.
            cancelButton.Frame = new CGRect(controlX, controlY, buttonWidth, controlHeight);

            // Add the input controls.
            AddSubviews(description,
                factorLabel, _stdDevPicker,
                applyButton, cancelButton);
        }

        // Animate increasing transparency to completely hide the view, then remove it
        public void Hide()
        {
            // Action to make the view transparent
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view
            Action removeViewAction = RemoveFromSuperview;

            // Time to complete the animation (seconds)
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void InputStretchParamsButton_Click(object sender, EventArgs e)
        {
            // Fire the OnStretchInputsEntered event and provide the stretch parameter input.
            if (OnStretchInputsEntered != null)
            {
                // Create a new StretchParametersEventArgs to contain the input values.
                StretchParameters inputStretchParams = null;

                // Create the right type of stretch parameters defined by the user.
                switch (_stretchParamsType)
                {
                    // - Minimum and maximum RGB values.
                    case "Min Max RGB":
                        // Get the models that contains the min/max red, green, and blue picker choices.
                        RgbValuePickerModel minRgbModel = (RgbValuePickerModel)_minRgbPicker.Model;
                        RgbValuePickerModel maxRgbModel = (RgbValuePickerModel)_maxRgbPicker.Model;

                        // Read min/max RGB values that were chosen.
                        double[] minValues = {minRgbModel.SelectedRed, minRgbModel.SelectedGreen, minRgbModel.SelectedBlue};
                        double[] maxValues = {maxRgbModel.SelectedRed, maxRgbModel.SelectedGreen, maxRgbModel.SelectedBlue};

                        // Create a new MinMaxStretchParameters object with the values.
                        inputStretchParams = new MinMaxStretchParameters(minValues, maxValues);

                        break;

                    // - Minimum and maximum percent clip values.
                    case "% Clip":
                        // Read min/max percent values that were chosen.
                        double minPercent = _minPercentSlider.Value;
                        double maxPercent = _maxPercentSlider.Value;
                        inputStretchParams = new PercentClipStretchParameters(minPercent, maxPercent);

                        break;
                    // Standard deviation factor.
                    case "Std Dev":
                        // Get the model that contains the standard deviation factor choices.
                        StdDevFactorPickerModel factorModel = (StdDevFactorPickerModel)_stdDevPicker.Model;

                        // Get the selected factor.
                        double standardDevFactor = factorModel.SelectedFactor;
                        inputStretchParams = new StandardDeviationStretchParameters(standardDevFactor);

                        break;
                }

                // Create a new StretchParametersEventArgs and provide the stretch parameters.
                StretchParametersEventArgs inputParamsEventArgs = new StretchParametersEventArgs(inputStretchParams);

                // Raise the event with the custom arguments.
                OnStretchInputsEntered(sender, inputParamsEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold raster renderer inputs.
    public class StretchParametersEventArgs : EventArgs
    {
        // Property to store raster stretch parameters.
        public StretchParameters StretchParams { get; }

        // Store parameter info passed into the constructor.
        public StretchParametersEventArgs(StretchParameters stretchParams)
        {
            StretchParams = stretchParams;
        }
    }

    // Class that defines a view model for showing color values (0-255 for RGB) in a picker control.
    public class RgbValuePickerModel : UIPickerViewModel
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
            SelectedRed = (int)pickerView.SelectedRowInComponent(0);
            SelectedGreen = (int)pickerView.SelectedRowInComponent(1);
            SelectedBlue = (int)pickerView.SelectedRowInComponent(2);
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

    // Class that defines a view model for showing standard deviation factor values (0.5-4.50) in a picker control.
    public class StdDevFactorPickerModel : UIPickerViewModel
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

    #endregion
}