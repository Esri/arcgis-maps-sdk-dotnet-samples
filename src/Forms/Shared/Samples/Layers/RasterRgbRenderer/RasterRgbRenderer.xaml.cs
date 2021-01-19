// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.RasterRgbRenderer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "RGB renderer",
        category: "Layers",
        description: "Apply an RGB renderer to a raster layer to enhance feature visibility.",
        instructions: "Choose one of the stretch parameter types. The other options will adjust based on the chosen type. Add your inputs and select the 'Update' button to update the renderer.",
        tags: new[] { "analysis", "color", "composite", "imagery", "multiband", "multispectral", "pan-sharpen", "photograph", "raster", "spectrum", "stretch", "visualization" })]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd")]
    public partial class RasterRgbRenderer : ContentPage
    {
        // A reference to the raster layer to render.
        private RasterLayer _rasterLayer;

        public RasterRgbRenderer()
        {
            InitializeComponent();

            // Call a function to set up the map
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap.
            Map map = new Map(BasemapStyle.ArcGISStreets);

            // Get the file name for the local raster dataset.
            string filepath = GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            // Create a new raster layer to show the image.
            _rasterLayer = new RasterLayer(rasterFile);

            try
            {
                // Once the layer has loaded, enable the button to change the renderer.
                await _rasterLayer.LoadAsync();
                ApplyRgbRendererButton.IsEnabled = true;

                // Create a viewpoint with the raster's full extent.
                Viewpoint fullRasterExtent = new Viewpoint(_rasterLayer.FullExtent);

                // Set the initial viewpoint for the map.
                map.InitialViewpoint = fullRasterExtent;

                // Add the layer to the map.
                map.OperationalLayers.Add(_rasterLayer);

                // Add the map to the map view.
                MyMapView.Map = map;

                // Add available stretch types to the combo box.
                StretchTypeComboBox.Items.Add("Min Max");
                StretchTypeComboBox.Items.Add("Percent Clip");
                StretchTypeComboBox.Items.Add("Standard Deviation");

                // Select "Min Max" as the stretch type.
                StretchTypeComboBox.SelectedIndex = 0;

                // Create a range of values from 0-255.
                IList minMaxValues = Enumerable.Range(0, 256).ToList();

                // Fill the min and max red combo boxes with the range and set default values.
                MinRedComboBox.ItemsSource = minMaxValues;
                MinRedComboBox.SelectedItem = 0;
                MaxRedComboBox.ItemsSource = minMaxValues;
                MaxRedComboBox.SelectedItem = 255;

                // Fill the min and max green combo boxes with the range and set default values.
                MinGreenComboBox.ItemsSource = minMaxValues;
                MinGreenComboBox.SelectedItem = 0;
                MaxGreenComboBox.ItemsSource = minMaxValues;
                MaxGreenComboBox.SelectedItem = 255;

                // Fill the min and max blue combo boxes with the range and set default values.
                MinBlueComboBox.ItemsSource = minMaxValues;
                MinBlueComboBox.SelectedItem = 0;
                MaxBlueComboBox.ItemsSource = minMaxValues;
                MaxBlueComboBox.SelectedItem = 255;

                // Fill the standard deviation factor combo box and set a default value.
                IEnumerable<int> wholeStdDevs = Enumerable.Range(1, 10);
                List<double> halfStdDevs = wholeStdDevs.Select(i => (double)i / 2).ToList();
                StdDeviationFactorComboBox.ItemsSource = halfStdDevs;
                StdDeviationFactorComboBox.SelectedItem = 2.0;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private void ApplyRgbRendererButton_Clicked(object sender, EventArgs e)
        {
            // Create the correct type of StretchParameters with the corresponding user inputs.
            StretchParameters stretchParameters = null;

            // See which type is selected and apply the corresponding input parameters to create the renderer.
            switch (StretchTypeComboBox.SelectedItem.ToString())
            {
                case "Min Max":
                    // Read the minimum and maximum values for the red, green, and blue bands.
                    double minRed = Convert.ToDouble(MinRedComboBox.SelectedItem);
                    double minGreen = Convert.ToDouble(MinGreenComboBox.SelectedItem);
                    double minBlue = Convert.ToDouble(MinBlueComboBox.SelectedItem);
                    double maxRed = Convert.ToDouble(MaxRedComboBox.SelectedItem);
                    double maxGreen = Convert.ToDouble(MaxGreenComboBox.SelectedItem);
                    double maxBlue = Convert.ToDouble(MaxBlueComboBox.SelectedItem);

                    // Create an array of the minimum and maximum values.
                    double[] minValues = { minRed, minGreen, minBlue };
                    double[] maxValues = { maxRed, maxGreen, maxBlue };

                    // Create a new MinMaxStretchParameters with the values.
                    stretchParameters = new MinMaxStretchParameters(minValues, maxValues);
                    break;
                case "Percent Clip":
                    // Get the percentile cutoff below which values in the raster dataset are to be clipped.
                    double minimumPercent = MinimumValueSlider.Value;

                    // Get the percentile cutoff above which pixel values in the raster dataset are to be clipped.
                    double maximumPercent = MaximumValueSlider.Value;

                    // Create a new PercentClipStretchParameters with the inputs.
                    stretchParameters = new PercentClipStretchParameters(minimumPercent, maximumPercent);
                    break;
                case "Standard Deviation":
                    // Read the standard deviation factor (the number of standard deviations used to define the range of pixel values).
                    double standardDeviationFactor = Convert.ToDouble(StdDeviationFactorComboBox.SelectedItem);

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

        private void StretchTypeSelectionChanged(object sender, EventArgs e)
        {
            // Hide all UI controls for the input parameters.
            MinMaxParametersGrid.IsVisible = false;
            PercentClipParametersGrid.IsVisible = false;
            StdDeviationParametersGrid.IsVisible = false;

            // See which type was selected and show the corresponding input controls.
            switch (StretchTypeComboBox.SelectedItem.ToString())
            {
                case "Min Max":
                    MinMaxParametersGrid.IsVisible = true;
                    break;
                case "Percent Clip":
                    PercentClipParametersGrid.IsVisible = true;
                    break;
                case "Standard Deviation":
                    StdDeviationParametersGrid.IsVisible = true;
                    break;
            }
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");
        }
    }
}