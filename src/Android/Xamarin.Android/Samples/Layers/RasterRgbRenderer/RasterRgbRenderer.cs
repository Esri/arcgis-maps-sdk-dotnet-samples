// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.RasterRgbRenderer
{
    [Activity(Label = "RasterRgbRenderer")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster RGB renderer",
        "Layers",
        "This sample demonstrates how to use an RGB renderer on a raster layer. An RGB renderer is used to adjust the color bands of a multi-spectral image.",
        "Choose one of the stretch parameter types. The other options will adjust based on the chosen type. Add your inputs and press the Apply button to update the renderer.")]
    public class RasterRgbRenderer : Activity
    {
        // Reference to the MapView used in the sample.
        private MapView _myMapView;

        // Reference to the raster layer to render.
        private RasterLayer _rasterLayer;

        // Layout panels to contain the UI for entering different renderer parameters.
        private LinearLayout _minMaxLayout;
        private LinearLayout _percentClipLayout;
        private LinearLayout _stdDeviationLayout;

        // Spinner for choosing the type of parameters input to use.
        private Spinner _parameterInputTypeSpinner;

        // Input controls for minimum and maximum RGB parameter values.
        private Spinner _minRedSpinner;
        private Spinner _minGreenSpinner;
        private Spinner _minBlueSpinner;
        private Spinner _maxRedSpinner;
        private Spinner _maxGreenSpinner;
        private Spinner _maxBlueSpinner;

        // Input controls for minimum and maximum percent clip values.
        private SeekBar _minPercentClipSlider;
        private SeekBar _maxPercentClipSlider;

        // Input control for the standard deviation factor value.
        private Spinner _stdDeviationFactorSpinner;

        // Button to apply the renderer.
        private Button _applyRendererButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Raster RGB renderer";

            // Create the UI for inputting renderer parameters.
            CreateLayout();

            // Initialize the map and raster layer.
            Initialize();
        }

        private void CreateLayout()
        {
            #region UI for selecting parameter type and applying the renderer
            // Create a vertical layout for the page.
            LinearLayout mainLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Add some padding to the layout.
            mainLayout.SetPadding(20, 0, 0, 5);

            // Create a horizontal layout for the parameter type list and button to apply the renderer.
            LinearLayout parameterTypeLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            string[] stretchTypes = { "Min Max", "Percent Clip", "Standard Deviation" };
            ArrayAdapter<string> stretchTypesAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, stretchTypes);

            // Create the spinner control for choosing the stretch parameter input type and handle its ItemSelected event.
            _parameterInputTypeSpinner = new Spinner(this)
            {
                Adapter = stretchTypesAdapter,
                DropDownWidth = 340
            };
            _parameterInputTypeSpinner.ItemSelected += ParameterInputTypeSpinner_ItemSelected;

            // Create the button that applies the renderer and handle its Click event.
            _applyRendererButton = new Button(this)
            {
                Text = "Apply",
                Enabled = false
            };
            _applyRendererButton.Click += ApplyRendererButton_Click;

            // Add a label, parameter type spinner control, and the apply button.
            TextView parameterTypeTextView = new TextView(this)
            {
                Text = "Stretch type: "
            };
            parameterTypeLayout.AddView(parameterTypeTextView);
            parameterTypeLayout.AddView(_parameterInputTypeSpinner);
            parameterTypeLayout.AddView(_applyRendererButton);

            // Add the parameter UI choice controls to the main layout.
            mainLayout.AddView(parameterTypeLayout);
            #endregion

            #region UI for defining min/max RGB values
            // Create a horizontal layout for the min/max RGB inputs.
            _minMaxLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            
            // Create a range of values from 0-255 and an adapter to display them.
            int[] minMaxValues = Enumerable.Range(0, 256).ToArray();
            ArrayAdapter<int> minMaxValuesAdapter = new ArrayAdapter<int>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, minMaxValues);

            // Get the width of the current device (in pixels).
            int widthPixels = Resources.DisplayMetrics.WidthPixels;

            // Use 1/5 of the device width for the drop down.
            int dropDownWidth = widthPixels / 5;

            // Create controls for specifying the minimum and maximum red values (0-255).
            _minRedSpinner = new Spinner(this, SpinnerMode.Dropdown)
            {
                Adapter = minMaxValuesAdapter,
                DropDownWidth = dropDownWidth
            };
            _minRedSpinner.SetSelection(0);
            _maxRedSpinner = new Spinner(this, SpinnerMode.Dropdown)
            {
                Adapter = minMaxValuesAdapter,
                DropDownWidth = dropDownWidth
            };
            _maxRedSpinner.SetSelection(255);
            
            // Set the background color to indicate which values the inputs are for.
            _minRedSpinner.SetBackgroundColor(Android.Graphics.Color.DarkRed);
            _maxRedSpinner.SetBackgroundColor(Android.Graphics.Color.DarkRed);

            // Create controls for specifying the minimum and maximum green values (0-255).
            _minGreenSpinner = new Spinner(this, SpinnerMode.Dropdown)
            {
                Adapter = minMaxValuesAdapter,
                DropDownWidth = dropDownWidth
            };
            _minGreenSpinner.SetSelection(0);
            _maxGreenSpinner = new Spinner(this, SpinnerMode.Dropdown)
            {
                Adapter = minMaxValuesAdapter,
                DropDownWidth = dropDownWidth
            };
            _maxGreenSpinner.SetSelection(255);

            // Set the background color to indicate which values the inputs are for.
            _minGreenSpinner.SetBackgroundColor(Android.Graphics.Color.DarkGreen);
            _maxGreenSpinner.SetBackgroundColor(Android.Graphics.Color.DarkGreen);

            // Create controls for specifying the minimum and maximum blue values (0-255).
            _minBlueSpinner = new Spinner(this, SpinnerMode.Dropdown)
            {
                Adapter = minMaxValuesAdapter,
                DropDownWidth = dropDownWidth
            };
            _minBlueSpinner.SetSelection(0);
            _maxBlueSpinner = new Spinner(this, SpinnerMode.Dropdown)
            {
                Adapter = minMaxValuesAdapter,
                DropDownWidth = dropDownWidth
            };
            _maxBlueSpinner.SetSelection(255);

            // Set the background color to indicate which values the inputs are for.
            _minBlueSpinner.SetBackgroundColor(Android.Graphics.Color.DarkBlue);
            _maxBlueSpinner.SetBackgroundColor(Android.Graphics.Color.DarkBlue);

            // Create vertical layouts for the color inputs (so they align properly).
            LinearLayout redInputsLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };
            LinearLayout greenInputsLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };
            LinearLayout blueInputsLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Add the color inputs to their corresponding layout.
            redInputsLayout.AddView(_minRedSpinner);
            redInputsLayout.AddView(_maxRedSpinner);
            greenInputsLayout.AddView(_minGreenSpinner);
            greenInputsLayout.AddView(_maxGreenSpinner);
            blueInputsLayout.AddView(_minBlueSpinner);
            blueInputsLayout.AddView(_maxBlueSpinner);

            // Add the vertical color inputs to the horizontal parent layout.
            _minMaxLayout.SetPadding(50, 10, 0, 10);
            _minMaxLayout.AddView(redInputsLayout);
            _minMaxLayout.AddView(greenInputsLayout);
            _minMaxLayout.AddView(blueInputsLayout);

            // Add the UI layouts to the main layout
            mainLayout.AddView(_minMaxLayout);
            #endregion

            #region UI for defining percent clip values
            // Create a (hidden) vertical layout for the min/max percent clip sliders.
            _percentClipLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
                Visibility = Android.Views.ViewStates.Gone
            };

            // Apply some padding for the layout.
            _percentClipLayout.SetPadding(20, 5, 0, 20);

            // Create the minimum and maximum percent sliders (SeekBar).
            _minPercentClipSlider = new SeekBar(this)
            {
                Max = 100,
                Progress = 0
            };
            _maxPercentClipSlider = new SeekBar(this)
            {
                Max = 100,
                Progress = 0
            };

            // Set the SeekBar dimensions and a left margin.
            ActionBar.LayoutParams layoutParamsSeekBar = new ActionBar.LayoutParams(400, 30)
            {
                LeftMargin = 5
            };
            _minPercentClipSlider.LayoutParameters = layoutParamsSeekBar;
            _maxPercentClipSlider.LayoutParameters = layoutParamsSeekBar;

            // Create labels for minimum and maximum percent.
            TextView minimumSliderLabel = new TextView(this)
            {
                Text = "Min: "
            };
            TextView maximumSliderLabel = new TextView(this)
            {
                Text = "Max: "
            };

            // Create horizontal layouts for the minimum and maximum controls.
            LinearLayout minPercentClipLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            LinearLayout maxPercentClipLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };

            // Add min and max percent controls to their corresponding layouts.
            minPercentClipLayout.AddView(minimumSliderLabel);
            minPercentClipLayout.AddView(_minPercentClipSlider);
            maxPercentClipLayout.AddView(maximumSliderLabel);
            maxPercentClipLayout.AddView(_maxPercentClipSlider);

            // Add the slider layouts to the percent clip layout.
            _percentClipLayout.AddView(minPercentClipLayout);
            _percentClipLayout.AddView(maxPercentClipLayout);

            // Add the percent clip UI to the main layout.
            mainLayout.AddView(_percentClipLayout);
            #endregion

            #region UI for defining standard deviation factor
            // Create a range of values from 0-5 (in 0.5 increments) and an adapter to display them.
            IEnumerable<int> wholeStdDevs = Enumerable.Range(1, 10);
            List<double> halfStdDevs = wholeStdDevs.Select(i => (double)i / 2).ToList();
            ArrayAdapter<double> stdDevFactorsAdapter = new ArrayAdapter<double>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, halfStdDevs);

            // Create a drop down (Spinner) control for specifying the standard deviation factor (0.0 - 5.0).
            _stdDeviationFactorSpinner = new Spinner(this, SpinnerMode.Dropdown)
            {
                Adapter = stdDevFactorsAdapter,
                DropDownWidth = dropDownWidth
            };

            // Set the default selection to the 4th item (value of 2.0)
            _stdDeviationFactorSpinner.SetSelection(4);

            // Create a label (TextView) for the Spinner.
            TextView factorLabel = new TextView(this)
            {
                Text = "Factor: "
            };

            // Create a horizontal layout for the controls.
            LinearLayout stdDevFactorLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };

            // Add the controls for selecting a standard deviation factor.
            stdDevFactorLayout.AddView(factorLabel);
            stdDevFactorLayout.AddView(_stdDeviationFactorSpinner);

            // Create the standard deviation factor layout and add the controls.
            _stdDeviationLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
                Visibility = Android.Views.ViewStates.Gone
            };
            _stdDeviationLayout.SetPadding(50, 5, 0, 5);
            _stdDeviationLayout.AddView(stdDevFactorLayout);

            // Add the standard deviation layout to the main layout.
            mainLayout.AddView(_stdDeviationLayout);
            #endregion

            // Create the map view control.
            _myMapView = new MapView(this);

            // Add the map view to the layout.
            mainLayout.AddView(_myMapView);

            // Set the layout as the sample view.
            SetContentView(mainLayout);
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap.
            Map myMap = new Map(Basemap.CreateStreets());

            // Get the file name for the local raster dataset.
            String filepath = GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            // Create and load a new raster layer to show the image.
            _rasterLayer = new RasterLayer(rasterFile);
            await _rasterLayer.LoadAsync();

            // Once the layer is loaded, enable the button to apply a new renderer.
            _applyRendererButton.Enabled = true;
            
            // Create a viewpoint with the raster's full extent.
            Viewpoint fullRasterExtent = new Viewpoint(_rasterLayer.FullExtent);

            // Set the initial viewpoint for the map.
            myMap.InitialViewpoint = fullRasterExtent;

            // Add the layer to the map.
            myMap.OperationalLayers.Add(_rasterLayer);

            // Add the map to the map view.
            _myMapView.Map = myMap;
        }

        private void ParameterInputTypeSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // See which parameter (stretch) type was selected.
            string selectedStretchType = (e.View as TextView).Text;

            // Hide all UI controls for the input parameters.
            _minMaxLayout.Visibility = Android.Views.ViewStates.Gone;
            _percentClipLayout.Visibility = Android.Views.ViewStates.Gone;
            _stdDeviationLayout.Visibility = Android.Views.ViewStates.Gone;

            // See which type was selected and show the corresponding input controls.
            switch (selectedStretchType)
            {
                case "Min Max":
                    _minMaxLayout.Visibility = Android.Views.ViewStates.Visible;
                    break;
                case "Percent Clip":
                    _percentClipLayout.Visibility = Android.Views.ViewStates.Visible;
                    break;
                case "Standard Deviation":
                    _stdDeviationLayout.Visibility = Android.Views.ViewStates.Visible;
                    break;
            }
        }

        private void ApplyRendererButton_Click(object sender, EventArgs e)
        {
            // Create the correct type of StretchParameters with the corresponding user inputs.
            StretchParameters stretchParameters = null;

            // See which type is selected and apply the corresponding input parameters to create the renderer.
            switch (_parameterInputTypeSpinner.SelectedItem.ToString()) 
            {
                case "Min Max":
                    // Read the minimum and maximum values for the red, green, and blue bands.
                    double minRed = Convert.ToDouble(_minRedSpinner.SelectedItem);
                    double minGreen = Convert.ToDouble(_minGreenSpinner.SelectedItem);
                    double minBlue = Convert.ToDouble(_minBlueSpinner.SelectedItem);
                    double maxRed = Convert.ToDouble(_maxRedSpinner.SelectedItem);
                    double maxGreen = Convert.ToDouble(_maxGreenSpinner.SelectedItem);
                    double maxBlue = Convert.ToDouble(_maxBlueSpinner.SelectedItem);

                    // Create an array of the minimum and maximum values.
                    double[] minValues = { minRed, minGreen, minBlue };
                    double[] maxValues = { maxRed, maxGreen, maxBlue };

                    // Create a new MinMaxStretchParameters with the values.
                    stretchParameters = new MinMaxStretchParameters(minValues, maxValues);
                    break;
                case "Percent Clip":
                    // Get the percentile cutoff below which values in the raster dataset are to be clipped.
                    double minimumPercent = Convert.ToDouble(_minPercentClipSlider.Progress);

                    // Get the percentile cutoff above which pixel values in the raster dataset are to be clipped.
                    double maximumPercent = Convert.ToDouble(_maxPercentClipSlider.Progress);

                    // Create a new PercentClipStretchParameters with the inputs.
                    stretchParameters = new PercentClipStretchParameters(minimumPercent, maximumPercent);
                    break;
                case "Standard Deviation":
                    // Read the standard deviation factor (the number of standard deviations used to define the range of pixel values).
                    double standardDeviationFactor = Convert.ToDouble(_stdDeviationFactorSpinner.SelectedItem);

                    // Create a new StandardDeviationStretchParameters with the selected number of standard deviations.
                    stretchParameters = new StandardDeviationStretchParameters(standardDeviationFactor);
                    break;
            }

            // Create an array to specify the raster bands (red, green, blue).
            int[] bands = { 0, 1, 2 };

            // Create the RgbRenderer with the stretch parameters created above, then apply it to the raster layer.
            RgbRenderer rasterRenderer = new RgbRenderer(stretchParameters, bands, null, true);
            _rasterLayer.Renderer = rasterRenderer;
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");
        }
    }
}