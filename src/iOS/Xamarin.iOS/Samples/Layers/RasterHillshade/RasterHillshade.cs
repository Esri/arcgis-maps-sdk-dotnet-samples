// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using ArcGISRuntime.Samples.Managers;
using UIKit;

namespace ArcGISRuntime.Samples.RasterHillshade
{
    [Register("RasterHillshade")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("134d60f50e184e8fa56365f44e5ce3fb")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster hillshade renderer",
        "Layers",
        "This sample demonstrates how to use a hillshade renderer on a raster layer. Hillshade renderers can adjust a grayscale raster (usually of terrain) according to a hypothetical sun position (azimuth and altitude).",
        "", "Featured")]
    public class RasterHillshade : UIViewController
    {
        // Button to show the hillshade parameters inputs.
        private UIButton _applyHillshadeButton;

        // Overlay with entry controls for applying a new hillshade renderer.
        private ApplyHillshadeRendererDialogOverlay _applyHillshadeRendererUI;

        // Store a reference to the map view control.
        private MapView _myMapView;

        // Store a reference to the raster layer.
        private RasterLayer _rasterLayer;

        public RasterHillshade()
        {
            Title = "Raster hillshade";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, initialize the map, and load a local raster image. 
            CreateLayout();
            Initialize();
        }
        
        public override void ViewDidLayoutSubviews()
        {
            // Get height of status bar and navigation bar.
            nfloat pageOffset = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            
            // Setup the visual frame for the MapView.
            _myMapView.Frame = new CoreGraphics.CGRect(0, pageOffset, View.Bounds.Width, View.Bounds.Height - pageOffset - 40);

            // Setup the visual frame for the hillshade button.
            _applyHillshadeButton.Frame = new CoreGraphics.CGRect(0, pageOffset + _myMapView.Frame.Height, View.Bounds.Width, 40);

            base.ViewDidLayoutSubviews();
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap.
            Map map = new Map(Basemap.CreateStreetsVector());

            // Get the file name for the local raster dataset.
            string filepath = GetRasterPath();

            // Load the raster file.
            Raster rasterFile = new Raster(filepath);

            // Create and load a new raster layer to show the image.
            _rasterLayer = new RasterLayer(rasterFile);            
            await _rasterLayer.LoadAsync();

            // Enable the apply renderer button when the layer loads.
            _applyHillshadeButton.Enabled = true;

            // Create a viewpoint with the raster's full extent.
            Viewpoint fullRasterExtent = new Viewpoint(_rasterLayer.FullExtent);

            // Set the initial viewpoint for the map.
            map.InitialViewpoint = fullRasterExtent;

            // Add the layer to the map.
            map.OperationalLayers.Add(_rasterLayer);

            // Add the map to the map view.
            _myMapView.Map = map;
        }
        
        private void CreateLayout()
        {
            View.BackgroundColor = UIColor.White;

            // Create the map view control.
            _myMapView = new MapView();

            // Create a button to apply the hillshade settings.
            _applyHillshadeButton = new UIButton
            {
                BackgroundColor = UIColor.White,
                Enabled = false
            };
            _applyHillshadeButton.SetTitle("Apply Hillshade", UIControlState.Normal);
            _applyHillshadeButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Handle the button tap to show the hillshade renderer inputs.
            _applyHillshadeButton.TouchUpInside += ApplyHillshade_Click;

            // Add MapView and UI controls to the page.
            View.AddSubviews(_myMapView, _applyHillshadeButton);
        }     

        private void ApplyHillshade_Click(object sender, EventArgs e)
        {
            if (_applyHillshadeRendererUI != null) { return; }

            // Create a view to show map item info entry controls over the map view.
            var ovBounds = new CoreGraphics.CGRect(0, 60, View.Bounds.Width, View.Bounds.Height);
            _applyHillshadeRendererUI = new ApplyHillshadeRendererDialogOverlay(ovBounds, 0.75f, UIColor.White);

            // Handle the OnHillshadeInputsEntered event to get the new renderer defined by the user.
            _applyHillshadeRendererUI.OnHillshadeInputsEntered += (s, hillshadeArgs) => 
            {
                // Get the new hillshade renderer.
                HillshadeRenderer newHillshadeRenderer = hillshadeArgs.HillshadeRasterRenderer;

                // If it's not null, apply the new renderer to the layer.
                if (newHillshadeRenderer != null)
                {
                    _rasterLayer.Renderer = newHillshadeRenderer;
                }

                // Remove the parameters input UI.
                _applyHillshadeRendererUI.Hide();
                _applyHillshadeRendererUI = null;
            };

            // Handle the cancel event when the user closes the dialog without entering hillshade params.
            _applyHillshadeRendererUI.OnCanceled += (s, args) =>
            {
                // Remove the parameters input UI.
                _applyHillshadeRendererUI.Hide();
                _applyHillshadeRendererUI = null;
            };

            // Add the input UI view (will display semi-transparent over the map view).
            View.Add(_applyHillshadeRendererUI);
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("134d60f50e184e8fa56365f44e5ce3fb", "srtm-hillshade", "srtm.tiff");
        }
    }

    #region UI for entering raster hillshade renderer properties.
    // View containing hillshade renderer parameter input controls (altitude and azimuth).
    public class ApplyHillshadeRendererDialogOverlay : UIView
    {
        // Constant to store a z-factor (conversion constant) applied to the hillshade.
        // If needed, this can be used to convert z-values to the same unit as the x/y coordinates or to apply a vertical exaggeration.
        private const double ZFactor = 1.0;

        // Constants to store the Pixel Size Power and Pixel Size Factor values.
        // Use these to account for altitude changes (scale) as the viewer zooms in and out (recommended when using worldwide datasets).
        private const double PixelSizePower = 1.0;
        private const double PixelSizeFactor = 1.0;

        // Constant to store the bit depth (pixel depth), which determines the range of values that the hillshade raster can store.
        private const int PixelBitDepth = 8;

        // Event to provide the user inputs when the view is dismissed.
        public event EventHandler<HillshadeParametersEventArgs> OnHillshadeInputsEntered;

        // Event to report that the input was canceled.
        public event EventHandler OnCanceled;

        // Fields for controls that will be referenced later.
        private UIPickerView _slopeTypePicker;
        private UISlider _altitudeSlider;
        private UISlider _azimuthSlider;

        public ApplyHillshadeRendererDialogOverlay(CoreGraphics.CGRect frame, nfloat transparency, UIColor color) : base(frame)
        {
            // Create a semi-transparent overlay with the specified background color.
            BackgroundColor = color;
            Alpha = transparency;

            // Button to create the hillshade parameters and pass them back to the main page.
            UIButton inputHillshadeParamsButton = new UIButton();
            inputHillshadeParamsButton.SetTitle("Apply", UIControlState.Normal);
            inputHillshadeParamsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            inputHillshadeParamsButton.TouchUpInside += InputHillshadeParamsButton_Click;

            // Button to cancel the input.
            UIButton cancelButton = new UIButton();
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled.Invoke(this, null); };

            CreateHillshadeInputUI(inputHillshadeParamsButton, cancelButton);
        }        

        private void CreateHillshadeInputUI(UIButton applyButton, UIButton cancelButton)
        {
            // Set size and spacing for controls.
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat columnSpace = 15;
            nfloat buttonWidth = 60;

            // Store the total height and width.
            nfloat totalHeight = Frame.Height - 120;
            nfloat totalWidth = Frame.Width - 60;

            // Find the center x and y of the view.
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // Find the start x and y for the control layout.
            nfloat leftMargin = centerX - (totalWidth / 2);
            nfloat controlX = leftMargin;
            nfloat controlY = 30;            

            // Create a label for the slope type input.
            UILabel slopeTypeLabel = new UILabel(new CoreGraphics.CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Slope type:",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Blue
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 10;

            // Create a picker for slope type.
            _slopeTypePicker = new UIPickerView(new CoreGraphics.CGRect(controlX, controlY, 260, 80))
            {
                Model = new SlopeTypesPickerModel()
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 80 + rowSpace;

            // Create a label for the altitude input.
            UILabel altitudeLabel = new UILabel(new CoreGraphics.CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Altitude: ",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Blue
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 5;

            // Create a slider for altitude value.
            _altitudeSlider = new UISlider(new CoreGraphics.CGRect(controlX, controlY, 200, 100))
            {
                MinValue = 0,
                MaxValue = 90,
                Value = 45
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Create a label for the azimuth input.
            UILabel azimuthLabel = new UILabel(new CoreGraphics.CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Azimuth: ",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Blue
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 5;

            // Create a picker for the azimuth value.
            _azimuthSlider = new UISlider(new CoreGraphics.CGRect(controlX, controlY, 200, 100))
            {
                MinValue = 0,
                MaxValue = 360,
                Value = 270
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Set the frame for the apply button.
            applyButton.Frame = new CoreGraphics.CGRect(controlX, controlY, buttonWidth, controlHeight);

            // Adjust the X position for the next control.
            controlX = controlX + buttonWidth + columnSpace;

            // Set the frame for the cancel button.
            cancelButton.Frame = new CoreGraphics.CGRect(controlX, controlY, buttonWidth, controlHeight);

            // Add the input controls.
            AddSubviews(slopeTypeLabel, _slopeTypePicker,
                altitudeLabel, _altitudeSlider,
                azimuthLabel, _azimuthSlider,
                applyButton, cancelButton);

            // Set the default value for the slope type.
            _slopeTypePicker.Select(0, 0, false);
        }
        
        // Animate increasing transparency to completely hide the view, then remove it
        public void Hide()
        {
            // Action to make the view transparent
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view
            Action removeViewAction = () => RemoveFromSuperview();

            // Time to complete the animation (seconds)
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void InputHillshadeParamsButton_Click(object sender, EventArgs e)
        {
            // Fire the OnHillshadeInputsEntered event and provide the hillshade renderer.
            if (OnHillshadeInputsEntered != null)
            {
                // Read the inputs provided by the user.
                // - Altitude and azimuth.
                        double altitude = _altitudeSlider.Value;
                        double azimuth = _azimuthSlider.Value;
                // - Get the model from the slope type picker and read the selected type.
                SlopeTypesPickerModel model = _slopeTypePicker.Model as SlopeTypesPickerModel;
                SlopeType selectedSlopeType = model.SelectedSlopeType;

                // Create a new HillshadeRenderer using the input values and constants.
                HillshadeRenderer hillshade = new HillshadeRenderer(altitude, azimuth, ZFactor, selectedSlopeType, PixelSizeFactor, PixelSizePower, PixelBitDepth);

                // Create a new HillshadeParametersEventArgs and provide the new renderer.
                HillshadeParametersEventArgs inputParamsEventArgs = new HillshadeParametersEventArgs(hillshade);

                // Raise the event with the custom arguments.
                OnHillshadeInputsEntered(sender, inputParamsEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold hillshade renderer based on the inputs.
    public class HillshadeParametersEventArgs : EventArgs
    {
        // Property to store raster stretch parameters.
        public HillshadeRenderer HillshadeRasterRenderer { get; set; }

        // Store the hillshade renderer passed into the constructor.
        public HillshadeParametersEventArgs(HillshadeRenderer renderer)
        {
            HillshadeRasterRenderer = renderer;
        }
    }

    // Class that defines a view model for showing available hillshade slope types in a picker control.
    public class SlopeTypesPickerModel : UIPickerViewModel
    {
        // Array of available slope values.
        private Array _slopeTypeValues = Enum.GetValues(typeof(SlopeType));

        // Store the selected slope type value.
        private SlopeType _selectedSlopeType;

        // Default constructor.
        public SlopeTypesPickerModel()
        {
            
        }
        
        // Property to expose the currently selected slope type value in the picker.
        public SlopeType SelectedSlopeType
        {
            get { return _selectedSlopeType; }
        }

        // Return the number of picker components (just one).
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        // Return the number of rows in the section (the size of the slope type array).
        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return _slopeTypeValues.Length;
        }

        // Get the title to display in the picker component.
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return _slopeTypeValues.GetValue(row).ToString();
        }

        // Handle the selection event for the picker.
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Get the selected standard deviation factor.
            _selectedSlopeType = (SlopeType)_slopeTypeValues.GetValue(pickerView.SelectedRowInComponent(0));
        }

        // Return the desired width for each component in the picker.
        public override nfloat GetComponentWidth(UIPickerView picker, nint component)
        {
            return 240f;
        }

        // Return the desired height for rows in the picker.
        public override nfloat GetRowHeight(UIPickerView picker, nint component)
        {
            return 30f;
        }
    }
    #endregion
}