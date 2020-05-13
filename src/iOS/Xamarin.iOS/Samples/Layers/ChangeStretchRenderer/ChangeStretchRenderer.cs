// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeStretchRenderer
{
    [Register("ChangeStretchRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("95392f99970d4a71bd25951beb34a508")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Stretch renderer",
        category: "Layers",
        description: "Use a stretch renderer to enhance the visual contrast of raster data for analysis.",
        instructions: "Choose one of the stretch parameter types:",
        tags: new[] { "analysis", "deviation", "histogram", "imagery", "interpretation", "min-max", "percent clip", "pixel", "raster", "stretch", "symbology", "visualization" })]
    public class ChangeStretchRenderer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISegmentedControl _rendererTypes;
        private UILabel _labelParameter1;
        private UITextField _inputParameter1;
        private UILabel _labelParameter2;
        private UITextField _inputParameter2;
        private UIButton _updateRendererButton;

        public ChangeStretchRenderer()
        {
            Title = "Stretch renderer";
        }

        private async void Initialize()
        {
            // Add an imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Get the file name.
            string filepath = DataManager.GetDataFolder("95392f99970d4a71bd25951beb34a508", "shasta", "ShastaBW.tif");

            // Load the raster file.
            Raster rasterFile = new Raster(filepath);

            // Create the layer.
            RasterLayer rasterLayer = new RasterLayer(rasterFile);

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(rasterLayer);

            try
            {
                // Wait for the layer to load.
                await rasterLayer.LoadAsync();

                // Set the viewpoint
                await _myMapView.SetViewpointGeometryAsync(rasterLayer.FullExtent);
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
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

            // Convert the input text to doubles and return if they're invalid.
            double input1;
            double input2;
            try
            {
                input1 = Convert.ToDouble(_inputParameter1.Text);
                input2 = Convert.ToDouble(_inputParameter2.Text);
            }
            catch (Exception ex)
            {
                new UIAlertView("alert", ex.Message, (IUIAlertViewDelegate) null, "OK", null).Show();
                return;
            }

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
                        IEnumerable<double> minValues = new List<double> {input1};

                        // Create an IEnumerable from a list of double max stretch value doubles.
                        IEnumerable<double> maxValues = new List<double> {input2};

                        // Create a new MinMaxStretchParameters based on the user choice for min and max stretch values.
                        MinMaxStretchParameters minMaxStretchParameters = new MinMaxStretchParameters(minValues, maxValues);

                        // Create the stretch renderer based on the user defined min/max stretch values, empty gamma values, statistic estimates, and a predefined color ramp.
                        stretchRenderer = new StretchRenderer(minMaxStretchParameters, gammaValues, true, colorRamp);
                    }
                    catch (ArgumentException)
                    {
                        ShowMessage("Error configuring renderer.", "Ensure all values are valid and try again.");
                        return;
                    }

                    break;

                case 1:

                    // This section creates a stretch renderer based on a PercentClipStretchParameters.
                    // TODO: Add you own logic to ensure that accurate min/max percent clip values are used.

                    try
                    {
                        // Create a new PercentClipStretchParameters based on the user choice for min and max percent clip values.
                        PercentClipStretchParameters percentClipStretchParameters = new PercentClipStretchParameters(input1, input2);

                        // Create the percent clip renderer based on the user defined min/max percent clip values, empty gamma values, statistic estimates, and a predefined color ramp.
                        stretchRenderer = new StretchRenderer(percentClipStretchParameters, gammaValues, true, colorRamp);
                    }
                    catch (Exception)
                    {
                        ShowMessage("Error configuring renderer.", "Ensure all values are valid and try again.");
                        return;
                    }

                    break;

                case 2:

                    // This section creates a stretch renderer based on a StandardDeviationStretchParameters.
                    // TODO: Add you own logic to ensure that an accurate standard deviation value is used

                    try
                    {
                        // Create a new StandardDeviationStretchParameters based on the user choice for standard deviation value.
                        StandardDeviationStretchParameters standardDeviationStretchParameters = new StandardDeviationStretchParameters(input1);
                        // Create the standard deviation renderer based on the user defined standard deviation value, empty gamma values, statistic estimates, and a predefined color ramp.
                        stretchRenderer = new StretchRenderer(standardDeviationStretchParameters, gammaValues, true, colorRamp);
                    }
                    catch (Exception)
                    {
                        ShowMessage("Error configuring renderer.", "Ensure all values are valid and try again.");
                        return;
                    }

                    break;
            }

            // Get the existing raster layer in the map.
            RasterLayer rasterLayer = (RasterLayer) _myMapView.Map.OperationalLayers[0];

            // Apply the stretch renderer to the raster layer.
            rasterLayer.Renderer = stretchRenderer;
        }

        private void ShowMessage(string title, string message)
        {
            new UIAlertView(title, message, (IUIAlertViewDelegate) null, "OK", null).Show();
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

            UIView formContainer = new UIView();
            formContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.BackgroundColor = UIColor.White;

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _inputParameter1 = new UITextField();
            _inputParameter1.TranslatesAutoresizingMaskIntoConstraints = false;
            _inputParameter1.BorderStyle = UITextBorderStyle.RoundedRect;

            _inputParameter2 = new UITextField();
            _inputParameter2.TranslatesAutoresizingMaskIntoConstraints = false;
            _inputParameter2.BorderStyle = UITextBorderStyle.RoundedRect;

            _labelParameter1 = new UILabel();
            _labelParameter1.TextAlignment = UITextAlignment.Right;
            _labelParameter1.TranslatesAutoresizingMaskIntoConstraints = false;

            _labelParameter2 = new UILabel();
            _labelParameter2.TextAlignment = UITextAlignment.Right;
            _labelParameter2.TranslatesAutoresizingMaskIntoConstraints = false;

            _updateRendererButton = new UIButton();
            _updateRendererButton.SetTitle("Update renderer", UIControlState.Normal);
            _updateRendererButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _updateRendererButton.TranslatesAutoresizingMaskIntoConstraints = false;

            _rendererTypes = new UISegmentedControl("Min/Max", "% Clip", "Std. Deviation");
            _rendererTypes.SelectedSegment = 0;
            _rendererTypes.TintColor = View.TintColor;
            _rendererTypes.TranslatesAutoresizingMaskIntoConstraints = false;

            // Call a function to configure the labels appropriately
            rendererTypes_ValueChanged(_rendererTypes, null);

            // Add the views.
            View.AddSubviews(_myMapView, formContainer, _rendererTypes, _inputParameter1,
                _inputParameter2, _labelParameter1, _labelParameter2, _updateRendererButton);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _rendererTypes.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8),
                _rendererTypes.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8),
                _rendererTypes.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),

                _inputParameter1.TopAnchor.ConstraintEqualTo(_rendererTypes.BottomAnchor, 8),
                _inputParameter1.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),
                _inputParameter1.WidthAnchor.ConstraintEqualTo(72),

                _inputParameter2.TopAnchor.ConstraintEqualTo(_inputParameter1.BottomAnchor, 8),
                _inputParameter2.TrailingAnchor.ConstraintEqualTo(_inputParameter1.TrailingAnchor),
                _inputParameter2.WidthAnchor.ConstraintEqualTo(_inputParameter1.WidthAnchor),

                _updateRendererButton.TopAnchor.ConstraintEqualTo(_inputParameter2.BottomAnchor, 8),
                _updateRendererButton.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _updateRendererButton.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),

                _labelParameter1.TopAnchor.ConstraintEqualTo(_inputParameter1.TopAnchor),
                _labelParameter1.LeadingAnchor.ConstraintEqualTo(_rendererTypes.LeadingAnchor),
                _labelParameter1.TrailingAnchor.ConstraintEqualTo(_inputParameter1.LeadingAnchor, -8),
                _labelParameter1.BottomAnchor.ConstraintEqualTo(_inputParameter1.BottomAnchor),

                _labelParameter2.TopAnchor.ConstraintEqualTo(_inputParameter2.TopAnchor),
                _labelParameter2.BottomAnchor.ConstraintEqualTo(_inputParameter2.BottomAnchor),
                _labelParameter2.LeadingAnchor.ConstraintEqualTo(_labelParameter1.LeadingAnchor),
                _labelParameter2.TrailingAnchor.ConstraintEqualTo(_labelParameter1.TrailingAnchor),

                formContainer.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                formContainer.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                formContainer.BottomAnchor.ConstraintEqualTo(_updateRendererButton.BottomAnchor, 8),
                formContainer.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _myMapView.TopAnchor.ConstraintEqualTo(formContainer.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }

        private bool HandleTextField(UITextField textField)
        {
            // This method allows pressing 'return' to dismiss the software keyboard.
            textField.ResignFirstResponder();
            return true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _inputParameter1.ShouldReturn += HandleTextField;
            _inputParameter2.ShouldReturn += HandleTextField;
            _updateRendererButton.TouchUpInside += UpdateRendererButton_Clicked;
            _rendererTypes.ValueChanged += rendererTypes_ValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _inputParameter1.ShouldReturn -= HandleTextField;
            _inputParameter2.ShouldReturn -= HandleTextField;
            _updateRendererButton.TouchUpInside -= UpdateRendererButton_Clicked;
            _rendererTypes.ValueChanged -= rendererTypes_ValueChanged;
        }
    }
}