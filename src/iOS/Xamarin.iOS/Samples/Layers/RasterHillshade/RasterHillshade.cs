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
        "Raster hillshade renderer",
        "Layers",
        "This sample demonstrates how to use a hillshade renderer on a raster layer. Hillshade renderers can adjust a grayscale raster (usually of terrain) according to a hypothetical sun position (azimuth and altitude).",
        "",
        "Featured")]
    public class RasterHillshade : UIViewController
    {
        // Hold references to the UI controls.
        private UIButton _applyHillshadeButton;
        private ApplyHillshadeRendererDialogOverlay _applyHillshadeRendererUi;
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
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat barHeight = 40;

                // Reposition thew views.
                _myMapView.Frame = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height - topMargin - barHeight);
                _applyHillshadeButton.Frame = new CGRect(0, topMargin + _myMapView.Frame.Height, View.Bounds.Width, barHeight);

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
            Map map = new Map(Basemap.CreateStreetsVector());

            // Get the file name for the local raster dataset.
            string filepath = DataManager.GetDataFolder("134d60f50e184e8fa56365f44e5ce3fb", "srtm-hillshade", "srtm.tiff");

            // Load the raster file.
            Raster rasterFile = new Raster(filepath);

            // Create and load a new raster layer to show the image.
            _rasterLayer = new RasterLayer(rasterFile);
            await _rasterLayer.LoadAsync();

            // Enable the apply renderer button when the layer loads.
            _applyHillshadeButton.Enabled = true;

            // Set the initial viewpoint to the raster's full extent.
            map.InitialViewpoint = new Viewpoint(_rasterLayer.FullExtent);

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
            _applyHillshadeButton.SetTitle("Apply hillshade", UIControlState.Normal);
            _applyHillshadeButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Handle the button tap to show the hillshade renderer inputs.
            _applyHillshadeButton.TouchUpInside += ApplyHillshade_Click;

            // Add MapView and UI controls to the page.
            View.AddSubviews(_myMapView, _applyHillshadeButton);
        }

        private void ApplyHillshade_Click(object sender, EventArgs e)
        {
            if (_applyHillshadeRendererUi != null)
            {
                return;
            }

            // Create a view to show map item info entry controls over the map view.
            var ovBounds = new CGRect(0, 60, View.Bounds.Width, View.Bounds.Height);
            _applyHillshadeRendererUi = new ApplyHillshadeRendererDialogOverlay(ovBounds, 0.9f, UIColor.White);

            // Handle the OnHillshadeInputsEntered event to get the new renderer defined by the user.
            _applyHillshadeRendererUi.OnHillshadeInputsEntered += (s, hillshadeArgs) =>
            {
                // Get the new hillshade renderer.
                HillshadeRenderer newHillshadeRenderer = hillshadeArgs.HillshadeRasterRenderer;

                // If it's not null, apply the new renderer to the layer.
                if (newHillshadeRenderer != null)
                {
                    _rasterLayer.Renderer = newHillshadeRenderer;
                }

                // Remove the parameters input UI.
                _applyHillshadeRendererUi.Hide();
                _applyHillshadeRendererUi = null;
            };

            // Handle the cancel event when the user closes the dialog without entering hillshade parameters.
            _applyHillshadeRendererUi.OnCanceled += (s, args) =>
            {
                // Remove the parameters input UI.
                _applyHillshadeRendererUi.Hide();
                _applyHillshadeRendererUi = null;
            };

            // Add the input UI view (will display semi-transparent over the map view).
            View.Add(_applyHillshadeRendererUi);
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
        private UISegmentedControl _slopeTypePicker;
        private UISlider _altitudeSlider;
        private UISlider _azimuthSlider;

        public ApplyHillshadeRendererDialogOverlay(CGRect frame, nfloat transparency, UIColor color) : base(frame)
        {
            // Create a semi-transparent overlay with the specified background color.
            BackgroundColor = color;
            Alpha = transparency;

            // Button to create the hillshade parameters and pass them back to the main page.
            UIButton inputHillshadeParamsButton = new UIButton();
            inputHillshadeParamsButton.SetTitle("Apply", UIControlState.Normal);
            inputHillshadeParamsButton.SetTitleColor(TintColor, UIControlState.Normal);
            inputHillshadeParamsButton.TouchUpInside += InputHillshadeParamsButton_Click;

            // Button to cancel the input.
            UIButton cancelButton = new UIButton();
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled.Invoke(this, null); };

            CreateHillshadeInputUi(inputHillshadeParamsButton, cancelButton);
        }

        private void CreateHillshadeInputUi(UIButton applyButton, UIButton cancelButton)
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
            nfloat controlY = 30;


            // Create a label for the slope type input.
            UILabel slopeTypeLabel = new UILabel(new CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Slope type:",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 30;

            // Adjust the Y position for the next control.
            _slopeTypePicker = new UISegmentedControl(Enum.GetNames(typeof(SlopeType)));
            _slopeTypePicker.ApportionsSegmentWidthsByContent = true;
            _slopeTypePicker.Frame = new CGRect(5, controlY, Bounds.Width - 10, 30);
            controlY += 35;

            // Create a label for the altitude input.
            UILabel altitudeLabel = new UILabel(new CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Altitude: ",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 5;

            // Create a slider for altitude value.
            _altitudeSlider = new UISlider(new CGRect(5, controlY, Bounds.Width - 10, 100))
            {
                MinValue = 0,
                MaxValue = 90,
                Value = 45
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Create a label for the azimuth input.
            UILabel azimuthLabel = new UILabel(new CGRect(controlX, controlY, totalWidth, controlHeight))
            {
                Text = "Azimuth: ",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 5;

            // Create a picker for the azimuth value.
            _azimuthSlider = new UISlider(new CGRect(5, controlY, Bounds.Width - 10, 100))
            {
                MinValue = 0,
                MaxValue = 360,
                Value = 270
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
            AddSubviews(slopeTypeLabel, _slopeTypePicker,
                altitudeLabel, _altitudeSlider,
                azimuthLabel, _azimuthSlider,
                applyButton, cancelButton);

            // Set the default value for the slope type.
            _slopeTypePicker.SelectedSegment = 0;
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
                nint selected = _slopeTypePicker.SelectedSegment;
                SlopeType selectedSlopeType = ((SlopeType[]) Enum.GetValues(typeof(SlopeType)))[selected];

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
        public HillshadeRenderer HillshadeRasterRenderer { get; }

        // Store the hillshade renderer passed into the constructor.
        public HillshadeParametersEventArgs(HillshadeRenderer renderer)
        {
            HillshadeRasterRenderer = renderer;
        }
    }

    #endregion
}