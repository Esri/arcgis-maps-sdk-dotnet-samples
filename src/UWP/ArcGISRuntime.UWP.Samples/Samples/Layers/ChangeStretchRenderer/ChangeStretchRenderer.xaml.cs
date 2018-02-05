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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.ChangeStretchRenderer
{
    public partial class ChangeStretchRenderer
    {
        public ChangeStretchRenderer()
        {
            this.InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Initialize the GUI controls appearance
            RendererTypes.Items.Add("Min Max");
            RendererTypes.Items.Add("Percent Clip");
            RendererTypes.Items.Add("Standard Deviation");
            RendererTypes.SelectedIndex = 0;

            // Add an imagery basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Wait for the map to load
            await myMap.LoadAsync();

            // Get the file name
            String filepath = await GetRasterPath();

            // Load the raster file
            Raster myRasterFile = new Raster(filepath);

            // Create the layer
            RasterLayer myRasterLayer = new RasterLayer(myRasterFile);

            // Add the layer to the map
            myMap.OperationalLayers.Add(myRasterLayer);

            // Wait for the layer to load
            await myRasterLayer.LoadAsync();

            // Set the viewpoint
            myMap.InitialViewpoint = new Viewpoint(myRasterLayer.FullExtent);

            // Add map to the mapview
            MyMapView.Map = myMap;

        }

        private void OnUpdateRendererClicked(object sender, RoutedEventArgs e)
        {
            // Get the user choice for the raster stretch render
            string myRendererTypeChoice = RendererTypes.SelectedValue.ToString();

            // Create an IEnumerable from an empty list of doubles for the gamma values in the stretch render
            IEnumerable<double> myGammaValues = new List<double> { };

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
                    IEnumerable<double> myMinValues = new List<double> { Convert.ToDouble(Input_Parameter1.Text) };

                    // Create an IEnumerable from a list of double max stretch value doubles
                    IEnumerable<double> myMaxValues = new List<double> { Convert.ToDouble(Input_Parameter2.Text) };

                    // Create a new MinMaxStretchParameters based on the user choice for min and max stretch values
                    MinMaxStretchParameters myMinMaxStretchParameters = new MinMaxStretchParameters(myMinValues, myMaxValues);

                    // Create the stretch renderer based on the user defined min/max stretch values, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new Esri.ArcGISRuntime.Rasters.StretchRenderer(myMinMaxStretchParameters, myGammaValues, true, myColorRamp);

                    break;

                case "Percent Clip":

                    // This section creates a stretch renderer based on a PercentClipStretchParameters
                    // TODO: Add you own logic to ensure that accurate min/max percent clip values are used

                    // Create a new PercentClipStretchParameters based on the user choice for min and max percent clip values
                    PercentClipStretchParameters myPercentClipStretchParameters = new PercentClipStretchParameters(Convert.ToDouble(Input_Parameter1.Text), Convert.ToDouble(Input_Parameter2.Text));

                    // Create the percent clip renderer based on the user defined min/max percent clip values, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new Esri.ArcGISRuntime.Rasters.StretchRenderer(myPercentClipStretchParameters, myGammaValues, true, myColorRamp);

                    break;

                case "Standard Deviation":

                    // This section creates a stretch renderer based on a StandardDeviationStretchParameters
                    // TODO: Add you own logic to ensure that an accurate standard deviation value is used

                    // Create a new StandardDeviationStretchParameters based on the user choice for standard deviation value
                    StandardDeviationStretchParameters myStandardDeviationStretchParameters = new StandardDeviationStretchParameters(Convert.ToDouble(Input_Parameter1.Text));

                    // Create the standard deviation renderer based on the user defined standard deviation value, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new Esri.ArcGISRuntime.Rasters.StretchRenderer(myStandardDeviationStretchParameters, myGammaValues, true, myColorRamp);

                    break;
            }

            // Get the existing raster layer in the map
            RasterLayer myRasterLayer = (RasterLayer)MyMapView.Map.OperationalLayers[0];

            // Apply the stretch renderer to the raster layer
            myRasterLayer.Renderer = myStretchRenderer;
        }

        private void RendererTypes_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            // Get the user choice for the raster stretch render
            string myRendererTypeChoice = e.AddedItems[0].ToString();

            switch (myRendererTypeChoice)
            {
                case "Min Max":

                    // This section displays/resets the user choice options for MinMaxStretchParameters

                    // Make sure all the GUI items are visible
                    Label_Parameter1.Visibility = Visibility.Visible;
                    Label_Parameter2.Visibility = Visibility.Visible;
                    Input_Parameter1.Visibility = Visibility.Visible;
                    Input_Parameter2.Visibility = Visibility.Visible;

                    // Define what values/options the user sees
                    Label_Parameter1.Text = "Minimum value (0 - 255):";
                    Label_Parameter2.Text = "Maximum value (0 - 255):";
                    Input_Parameter1.Text = "10";
                    Input_Parameter2.Text = "150";

                    break;

                case "Percent Clip":

                    // This section displays/resets the user choice options for PercentClipStretchParameters

                    // Make sure all the GUI items are visible
                    Label_Parameter1.Visibility = Visibility.Visible;
                    Label_Parameter2.Visibility = Visibility.Visible;
                    Input_Parameter1.Visibility = Visibility.Visible;
                    Input_Parameter2.Visibility = Visibility.Visible;

                    // Define what values/options the user sees
                    Label_Parameter1.Text = "Minimum (0 - 100):";
                    Label_Parameter2.Text = "Maximum (0 - 100)";
                    Input_Parameter1.Text = "0";
                    Input_Parameter2.Text = "50";

                    break;

                case "Standard Deviation":

                    // This section displays/resets the user choice options for StandardDeviationStretchParameters

                    // Make sure that only the necessary GUI items are visible
                    Label_Parameter1.Visibility = Visibility.Visible;
                    Label_Parameter2.Visibility = Visibility.Collapsed;
                    Input_Parameter1.Visibility = Visibility.Visible;
                    Input_Parameter2.Visibility = Visibility.Collapsed;

                    // Define what values/options the user sees
                    Label_Parameter1.Text = "Factor (.25 to 4):";
                    Input_Parameter1.Text = "0.5";

                    break;
            }

        }

        private async Task<string> GetRasterPath()
        {
            #region offlinedata

            // The desired raster is expected to be called ShastaBW.tif
            string filename = "ShastaBW.tif";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "ChangeStretchRenderer", "shasta", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData("95392f99970d4a71bd25951beb34a508", "ChangeStretchRenderer");
            }
            return filepath;

            #endregion offlinedata
        }

    }
}
