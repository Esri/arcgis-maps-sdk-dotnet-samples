// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System.Collections.ObjectModel;

namespace ArcGIS.Samples.ChangeStretchRenderer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Stretch renderer",
        category: "Layers",
        description: "Use a stretch renderer to enhance the visual contrast of raster data for analysis.",
        instructions: "Choose one of the stretch parameter types:",
        tags: new[] { "analysis", "deviation", "histogram", "imagery", "interpretation", "min-max", "percent clip", "pixel", "raster", "stretch", "symbology", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("95392f99970d4a71bd25951beb34a508")]
    public partial class ChangeStretchRenderer : ContentPage
    {
        public ChangeStretchRenderer()
        {
            InitializeComponent();

            // Call a function to set up the map
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Initialize the GUI controls appearance
            ObservableCollection<string> myObservableCollection_RendererTypes = new ObservableCollection<string>();
            myObservableCollection_RendererTypes.Add("Min Max");
            myObservableCollection_RendererTypes.Add("Percent Clip");
            myObservableCollection_RendererTypes.Add("Standard Deviation");
            RendererTypes.ItemsSource = myObservableCollection_RendererTypes;
            RendererTypes.SelectedItem = "Min Max";

            // Add an imagery basemap
            MyMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);

            // Get the file name
            string filepath = GetRasterPath();

            // Load the raster file
            Raster myRasterFile = new Raster(filepath);

            // Create the layer
            RasterLayer myRasterLayer = new RasterLayer(myRasterFile);

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(myRasterLayer);

            try
            {
                // Wait for the layer to load
                await myRasterLayer.LoadAsync();

                // Set the viewpoint
                await MyMapView.SetViewpointGeometryAsync(myRasterLayer.FullExtent);
            }
            catch (Exception e)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private void RendererTypes_SelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            // Get the user choice for the raster stretch render
            string myRendererTypeChoice = e.SelectedItem.ToString();

            switch (myRendererTypeChoice)
            {
                case "Min Max":

                    // This section displays/resets the user choice options for MinMaxStretchParameters

                    // Make sure all the GUI items are visible
                    Label_Parameter1.IsVisible = true;
                    Label_Parameter2.IsVisible = true;
                    Input_Parameter1.IsVisible = true;
                    Input_Parameter2.IsVisible = true;

                    // Define what values/options the user sees
                    Label_Parameter1.Text = "Minimum value (0 - 255):";
                    Label_Parameter2.Text = "Maximum value (0 - 255):";
                    Input_Parameter1.Text = "10";
                    Input_Parameter2.Text = "150";

                    break;

                case "Percent Clip":

                    // This section displays/resets the user choice options for PercentClipStretchParameters

                    // Make sure all the GUI items are visible
                    Label_Parameter1.IsVisible = true;
                    Label_Parameter2.IsVisible = true;
                    Input_Parameter1.IsVisible = true;
                    Input_Parameter2.IsVisible = true;

                    // Define what values/options the user sees
                    Label_Parameter1.Text = "Minimum (0 - 100):";
                    Label_Parameter2.Text = "Maximum (0 - 100)";
                    Input_Parameter1.Text = "0";
                    Input_Parameter2.Text = "50";

                    break;

                case "Standard Deviation":

                    // This section displays/resets the user choice options for StandardDeviationStretchParameters

                    // Make sure that only the necessary GUI items are visible
                    Label_Parameter1.IsVisible = true;
                    Label_Parameter2.IsVisible = false;
                    Input_Parameter1.IsVisible = true;
                    Input_Parameter2.IsVisible = false;

                    // Define what values/options the user sees
                    Label_Parameter1.Text = "Factor (.25 to 4):";
                    Input_Parameter1.Text = "0.5";

                    break;
            }
        }

        private void OnUpdateRendererClicked(object sender, EventArgs e)
        {
            // Convert the input text to doubles and return if they're invalid.
            double input1;
            double input2;
            try
            {
                input1 = Convert.ToDouble(Input_Parameter1.Text);
                input2 = Convert.ToDouble(Input_Parameter2.Text);
            }
            catch (Exception ex)
            {
                DisplayAlert("Alert", ex.Message, "OK");
                return;
            }

            // Get the user choice for the raster stretch render
            string myRendererTypeChoice = RendererTypes.SelectedItem.ToString();

            // Create an IEnumerable from an empty list of doubles for the gamma values in the stretch render
            IEnumerable<double> myGammaValues = new List<double>();

            // Create a color ramp for the stretch renderer
            ColorRamp myColorRamp = ColorRamp.Create(PresetColorRampType.DemLight, 1000);

            // Create the place holder for the stretch renderer
            StretchRenderer myStretchRenderer = null;

            switch (myRendererTypeChoice)
            {
                case "Min Max":

                    // This section creates a stretch renderer based on a MinMaxStretchParameters
                    // TODO: Add you own logic to ensure that accurate min/max stretch values are used

                    // Create an IEnumerable from a list of double min stretch value doubles
                    IEnumerable<double> myMinValues = new List<double> { input1 };

                    // Create an IEnumerable from a list of double max stretch value doubles
                    IEnumerable<double> myMaxValues = new List<double> { input2 };

                    // Create a new MinMaxStretchParameters based on the user choice for min and max stretch values
                    MinMaxStretchParameters myMinMaxStretchParameters = new MinMaxStretchParameters(myMinValues, myMaxValues);

                    // Create the stretch renderer based on the user defined min/max stretch values, empty gamma values, statistic estimates, and a predefined color ramp
                    myStretchRenderer = new Esri.ArcGISRuntime.Rasters.StretchRenderer(myMinMaxStretchParameters, myGammaValues, true, myColorRamp);

                    break;

                case "Percent Clip":

                    // This section creates a stretch renderer based on a PercentClipStretchParameters
                    // TODO: Add you own logic to ensure that accurate min/max percent clip values are used

                    // Create a new PercentClipStretchParameters based on the user choice for min and max percent clip values
                    PercentClipStretchParameters myPercentClipStretchParameters = new PercentClipStretchParameters(input1, input2);

                    // Create the percent clip renderer based on the user defined min/max percent clip values, empty gamma values, statistic estimates, and a predefined color ramp
                    myStretchRenderer = new StretchRenderer(myPercentClipStretchParameters, myGammaValues, true, myColorRamp);

                    break;

                case "Standard Deviation":

                    // This section creates a stretch renderer based on a StandardDeviationStretchParameters
                    // TODO: Add you own logic to ensure that an accurate standard deviation value is used

                    // Create a new StandardDeviationStretchParameters based on the user choice for standard deviation value
                    StandardDeviationStretchParameters myStandardDeviationStretchParameters = new StandardDeviationStretchParameters(input1);

                    // Create the standard deviation renderer based on the user defined standard deviation value, empty gamma values, statistic estimates, and a predefined color ramp
                    myStretchRenderer = new StretchRenderer(myStandardDeviationStretchParameters, myGammaValues, true, myColorRamp);

                    break;
            }

            // Get the existing raster layer in the map
            RasterLayer myRasterLayer = (RasterLayer)MyMapView.Map.OperationalLayers[0];

            // Apply the stretch renderer to the raster layer
            myRasterLayer.Renderer = myStretchRenderer;
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("95392f99970d4a71bd25951beb34a508", "shasta", "ShastaBW.tif");
        }
    }
}