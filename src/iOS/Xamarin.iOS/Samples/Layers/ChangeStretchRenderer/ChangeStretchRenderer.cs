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
using System.Collections.Generic;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeStretchRenderer
{
    [Register("ChangeStretchRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("95392f99970d4a71bd25951beb34a508")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Stretch renderer",
        "Layers",
        "This sample demonstrates how to use stretch renderer on a raster layer.",
        "Choose a stretch renderer type from the table to change the settings for the stretch renderer.\nThe sample allows you to change the stretch type and the parameters for each type. Click/tap the 'Update Renderer' button to update the raster.\nExperiment with settings for the various types for stretch parameters. For example, setting the renderer to use stretch parameters:\nMin Max with a min value of 50 and a max value of 200 will stretch between these pixel values. A higher min value will remove more of the lighter pixels values whilst a lower max will remove more of the darker.\nPercent Clip with a min value of 2 and a max value of 98 will stretch from 2% to 98% of the pixel values histogram. A lower min and higher max percentage will render using more of the original raster histogram.\nStandard Deviation with a factor of 2.0 will stretch 2 standard deviations from the mean. A higher factor (further from the mean) will render using more of the original raster histogram.",
        "Featured")]
    public class ChangeStretchRenderer : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UISegmentedControl _rendererTypes;
        private UILabel _labelParameter1;
        private UITextField _inputParameter1;
        private UILabel _labelParameter2;
        private UITextField _inputParameter2;
        private UIButton _updateRendererButton;

        private readonly string[] _rendererChoices = {"Min/Max", "% Clip", "Std. Deviation"};

        public ChangeStretchRenderer()
        {
            Title = "Stretch renderer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topStart = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = 4 * controlHeight + 5 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topStart + toolbarHeight, 0, 0, 0);
                _toolbar.Frame = new CGRect(0, topStart, View.Bounds.Width, toolbarHeight);
                _rendererTypes.Frame = new CGRect(margin, topStart + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _labelParameter1.Frame = new CGRect(margin, topStart + controlHeight + 2 * margin, View.Bounds.Width - 4 * margin - 100, controlHeight);
                _inputParameter1.Frame = new CGRect(View.Bounds.Width - 100 - 2 * margin, topStart + controlHeight + 2 * margin, 100, controlHeight);
                _labelParameter2.Frame = new CGRect(margin, topStart + 2 * controlHeight + 3 * margin, View.Bounds.Width - 4 * margin - 100, controlHeight);
                _inputParameter2.Frame = new CGRect(View.Bounds.Width - 100 - 2 * margin, topStart + 2 * controlHeight + 3 * margin, 100, controlHeight);
                _updateRendererButton.Frame = new CGRect(margin, topStart + 3 * controlHeight + 4 * margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Add an imagery basemap.
            Map map = new Map(Basemap.CreateImagery());

            // Wait for the map to load.
            await map.LoadAsync();

            // Get the file name.
            string filepath = DataManager.GetDataFolder("95392f99970d4a71bd25951beb34a508", "shasta", "ShastaBW.tif");

            // Load the raster file.
            Raster rasterFile = new Raster(filepath);

            // Create the layer.
            RasterLayer rasterLayer = new RasterLayer(rasterFile);

            // Add the layer to the map.
            map.OperationalLayers.Add(rasterLayer);

            // Wait for the layer to load.
            await rasterLayer.LoadAsync();

            // Set the viewpoint.
            map.InitialViewpoint = new Viewpoint(rasterLayer.FullExtent);

            // Add map to the mapview.
            _myMapView.Map = map;
        }

        private void CreateLayout()
        {
            UIColor controlWhite = UIColor.FromWhiteAlpha(1, .8f);

            // Create button to change stretch renderer of the raster.
            _updateRendererButton = new UIButton();
            _updateRendererButton.SetTitle("Update renderer", UIControlState.Normal);
            _updateRendererButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Hook to touch/click event of the button.
            _updateRendererButton.TouchUpInside += UpdateRendererButton_Clicked;

            // Create a list of stretch renderer choices the user can choose from.
            _rendererTypes = new UISegmentedControl(_rendererChoices)
            {
                SelectedSegment = 0
            };
            _rendererTypes.ValueChanged += rendererTypes_ValueChanged;

            // Create label that displays the 1st parameter used by the stretch renderer.
            _labelParameter1 = new UILabel
            {
                Text = "Minimum value (0 - 255):",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Right
            };

            // Create text field for 1st parameter used by the stretch renderer that the user can modify.
            _inputParameter1 = new UITextField
            {
                Text = "10",
                AdjustsFontSizeToFitWidth = true,
                BackgroundColor = controlWhite,
                TextColor = View.TintColor,
                BorderStyle = UITextBorderStyle.RoundedRect,
                TextAlignment = UITextAlignment.Center
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _inputParameter1.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Create label that displays the 2nd parameter used by the stretch renderer.
            _labelParameter2 = new UILabel
            {
                Text = "Maximum value (0 - 255):",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Right
            };

            // Create text field for 2nd parameter used by the stretch renderer that the user can modify.
            _inputParameter2 = new UITextField
            {
                Text = "150",
                AdjustsFontSizeToFitWidth = true,
                BackgroundColor = controlWhite,
                TextColor = View.TintColor,
                BorderStyle = UITextBorderStyle.RoundedRect,
                TextAlignment = UITextAlignment.Center
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _inputParameter2.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Add all of the UI controls to the page.
            View.AddSubviews(_myMapView, _toolbar, _updateRendererButton, _rendererTypes, _labelParameter1, _inputParameter1, _labelParameter2, _inputParameter2);
        }

        private void rendererTypes_ValueChanged(object sender, EventArgs e)
        {
            // This function modifies the UI parameter controls depending on which stretch 
            // renderer is chosen by the user.
            switch (_rendererTypes.SelectedSegment)
            {
                case 0: // Min Max.
                    // Make sure all the GUI items are visible.
                    _labelParameter1.Hidden = false;
                    _labelParameter2.Hidden = false;
                    _inputParameter1.Hidden = false;
                    _inputParameter2.Hidden = false;

                    // Define what values/options the user sees.
                    _labelParameter1.Text = "Minimum value (0 - 255):";
                    _labelParameter2.Text = "Maximum value (0 - 255):";
                    _inputParameter1.Text = "10";
                    _inputParameter2.Text = "150";

                    break;

                case 1: // Percent Clip.
                    // Make sure all the GUI items are visible.
                    _labelParameter1.Hidden = false;
                    _labelParameter2.Hidden = false;
                    _inputParameter1.Hidden = false;
                    _inputParameter2.Hidden = false;

                    // Define what values/options the user sees.
                    _labelParameter1.Text = "Minimum (0 - 100):";
                    _labelParameter2.Text = "Maximum (0 - 100):";
                    _inputParameter1.Text = "0";
                    _inputParameter2.Text = "50";

                    break;

                case 2: // Standard Deviation.
                    // Make sure that only the necessary GUI items are visible.
                    _labelParameter1.Hidden = false;
                    _labelParameter2.Hidden = true;
                    _inputParameter1.Hidden = false;
                    _inputParameter2.Hidden = true;

                    // Define what values/options the user sees.
                    _labelParameter1.Text = "Factor (.25 to 4):";
                    _inputParameter1.Text = "0.5";

                    break;
            }
        }


        private void UpdateRendererButton_Clicked(object sender, EventArgs e)
        {
            // This function acquires the user selection of the stretch renderer from the table view
            // along with the parameters specified, then a stretch renderer is created and applied to 
            // the raster layer.

            // Create an IEnumerable from an empty list of doubles for the gamma values in the stretch render.
            IEnumerable<double> gammaValues = new List<double>();

            // Create a color ramp for the stretch renderer.
            ColorRamp colorRamp = ColorRamp.Create(PresetColorRampType.DemLight, 1000);

            // Create the place holder for the stretch renderer.
            StretchRenderer stretchRenderer = null;

            switch (_rendererTypes.SelectedSegment)
            {
                case 0:

                    // This section creates a stretch renderer based on a MinMaxStretchParameters.
                    // TODO: Add you own logic to ensure that accurate min/max stretch values are used.

                    try
                    {
                        // Create an IEnumerable from a list of double min stretch value doubles.
                        IEnumerable<double> minValues = new List<double> {Convert.ToDouble(_inputParameter1.Text)};

                        // Create an IEnumerable from a list of double max stretch value doubles.
                        IEnumerable<double> maxValues = new List<double> {Convert.ToDouble(_inputParameter2.Text)};

                        // Create a new MinMaxStretchParameters based on the user choice for min and max stretch values.
                        MinMaxStretchParameters minMaxStretchParameters = new MinMaxStretchParameters(minValues, maxValues);

                        // Create the stretch renderer based on the user defined min/max stretch values, empty gamma values, statistic estimates, and a predefined color ramp.
                        stretchRenderer = new StretchRenderer(minMaxStretchParameters, gammaValues, true, colorRamp);
                    }
                    catch (Exception ex)
                    {
                        showMessage("Error configuring renderer.", "Ensure all values are valid and try again.");
                        return;
                    }

                    break;

                case 1:

                    // This section creates a stretch renderer based on a PercentClipStretchParameters.
                    // TODO: Add you own logic to ensure that accurate min/max percent clip values are used.

                    try
                    {
                        // Create a new PercentClipStretchParameters based on the user choice for min and max percent clip values.
                        PercentClipStretchParameters percentClipStretchParameters = new PercentClipStretchParameters(
                            Convert.ToDouble(_inputParameter1.Text), Convert.ToDouble(_inputParameter2.Text));

                        // Create the percent clip renderer based on the user defined min/max percent clip values, empty gamma values, statistic estimates, and a predefined color ramp.
                        stretchRenderer = new StretchRenderer(percentClipStretchParameters, gammaValues, true, colorRamp);
                    }
                    catch (Exception ex)
                    {
                        showMessage("Error configuring renderer.", "Ensure all values are valid and try again.");
                        return;
                    }

                    break;

                case 2:

                    // This section creates a stretch renderer based on a StandardDeviationStretchParameters.
                    // TODO: Add you own logic to ensure that an accurate standard deviation value is used

                    try
                    {
                        // Create a new StandardDeviationStretchParameters based on the user choice for standard deviation value.
                        StandardDeviationStretchParameters standardDeviationStretchParameters = new StandardDeviationStretchParameters(Convert.ToDouble(_inputParameter1.Text));
                        // Create the standard deviation renderer based on the user defined standard deviation value, empty gamma values, statistic estimates, and a predefined color ramp.
                        stretchRenderer = new StretchRenderer(standardDeviationStretchParameters, gammaValues, true, colorRamp);
                    }
                    catch (Exception ex)
                    {
                        showMessage("Error configuring renderer.", "Ensure all values are valid and try again.");
                        return;
                    }

                    break;
            }

            // Get the existing raster layer in the map.
            RasterLayer rasterLayer = (RasterLayer) _myMapView.Map.OperationalLayers[0];

            // Apply the stretch renderer to the raster layer.
            rasterLayer.Renderer = stretchRenderer;
        }

        private void showMessage(string title, string message)
        {
            new UIAlertView(title, message, (IUIAlertViewDelegate) null, "OK", null).Show();
        }
    }
}