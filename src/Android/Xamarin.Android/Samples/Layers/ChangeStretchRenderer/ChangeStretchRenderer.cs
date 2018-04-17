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
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.ChangeStretchRenderer
{
    [Activity(Label = "ChangeStretchRenderer")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("95392f99970d4a71bd25951beb34a508")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Stretch renderer",
        "Layers",
        "This sample demonstrates how to use stretch renderer on a raster layer.",
        "Choose a stretch renderer type from the list view to change the settings for the stretch renderer.\nThe sample allows you to change the stretch type and the parameters for each type. Click/tap the 'Update Renderer' button to update the raster.\nExperiment with settings for the various types for stretch parameters. For example, setting the renderer to use stretch parameters:\nMin Max with a min value of 50 and a max value of 200 will stretch between these pixel values. A higher min value will remove more of the lighter pixels values whilst a lower max will remove more of the darker.\nPercent Clip with a min value of 2 and a max value of 98 will stretch from 2% to 98% of the pixel values histogram. A lower min and higher max percentage will render using more of the original raster histogram.\nStandard Deviation with a factor of 2.0 will stretch 2 standard deviations from the mean. A higher factor (further from the mean) will render using more of the original raster histogram.",
        "Featured")]
    public class ChangeStretchRenderer : Activity
    {
        // Global reference to the MapView used in the sample
        private MapView _myMapView;

        // Global reference to a list view of stretch render choices the user can choose from
        private ListView _RendererTypes;

        // Global reference to a label that displays the 1st parameter used by the stretch renderer
        private TextView _Label_Parameter1;

        // Global reference to the 1st parameter used by the stretch renderer that the user can modify 
        private EditText _Input_Parameter1;

        // Global reference to a label that displays the 2nd parameter used by the stretch renderer
        private TextView _Label_Parameter2;

        // Global reference to the 2nd parameter used by the stretch renderer that the user can modify 
        private EditText _Input_Parameter2;

        // Global reference to button the user clicks to change the stretch renderer on the raster 
        private Button _UpdateRenderer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Stretch renderer";

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // This section creates the UI elements and adds them to the layout view of the GUI

            // Create a stack layout
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a list of stretch renderer choices the user can choose from
            _RendererTypes = new ListView(this);
            _RendererTypes.ItemClick += RendererTypes_SelectionChanged;
            layout.AddView(_RendererTypes);

            // ------------

            // Create a sub layout to organize the label and 1st parameter on a single horizontal line
            var subLayout1 = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create label that displays the 1st parameter used by the stretch renderer
            _Label_Parameter1 = new TextView(this);
            _Label_Parameter1.Text = "Minimum value (0 - 255):";
            subLayout1.AddView(_Label_Parameter1);

            // Create text field for 1st parameter used by the stretch renderer that the user can modify 
            _Input_Parameter1 = new EditText(this);
            _Input_Parameter1.Text = "10";
            subLayout1.AddView(_Input_Parameter1);

            // Add the sub layout to the main layout
            layout.AddView(subLayout1);

            // ------------

            // Create a sub layout to organize the label and 2nd parameter on a single horizontal line
            var subLayout2 = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create label that displays the 2nd parameter used by the stretch renderer
            _Label_Parameter2 = new TextView(this);
            _Label_Parameter2.Text = "Maximum value (0 - 255):";
            subLayout2.AddView(_Label_Parameter2);

            // Create text field for 2nd parameter used by the stretch renderer that the user can modify 
            _Input_Parameter2 = new EditText(this);
            _Input_Parameter2.Text = "150";
            subLayout2.AddView(_Input_Parameter2);

            // Add the sub layout to the main layout
            layout.AddView(subLayout2);

            // ------------

            // Create button to change stretch renderer of the raster
            _UpdateRenderer = new Button(this);
            _UpdateRenderer.Text = "Update Renderer";
            // Hook to touch/click event of the button
            _UpdateRenderer.Click += OnUpdateRendererClicked;
            layout.AddView(_UpdateRenderer);

            // Create the map view and add it to the main layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Set the layout as the sample view
            SetContentView(layout);
        }

        private async void Initialize()
        {
            // Create a simple string array of the human-readable stretch renderer names from the ObservableCollection
            string[] myStringArray_RendererTypes = { "Min Max", "Percent Clip", "Standard Deviation" };

            // Create an ArrayAdapter from the simple string array
            ArrayAdapter myArrayAdapter_LayerNamesInTheMap = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myStringArray_RendererTypes);

            // Display the human-readable stretch renderer names to be displayed a ListView
            _RendererTypes.Adapter = myArrayAdapter_LayerNamesInTheMap;

            // Add an imagery basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Wait for the map to load
            await myMap.LoadAsync();

            // Get the file name
            string filepath = GetRasterPath();

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
            _myMapView.Map = myMap;
        }

        private void OnUpdateRendererClicked(object sender, EventArgs e)
        {
            // This function acquires the user selection of the stretch renderer from the table view
            // along with the parameters specified, then a stretch renderer is created and applied to 
            // the raster layer

            // Get the user choice for the raster stretch render
            string myRendererTypeChoice;

            if (_RendererTypes.SelectedItem == null)
            {
                // If the user does not click on a choice in the table but just clicks the
                // button, the selected value will be null so use the initial
                // stretch renderer option
                myRendererTypeChoice = "Min Max";
            }
            else
            {
                // The user clicked on an option in the table and thus the selected value
                // will contain a valid choice
                myRendererTypeChoice = _RendererTypes.SelectedItem.ToString();
            }

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
                    IEnumerable<double> myMinValues = new List<double> { Convert.ToDouble(_Input_Parameter1.Text) };

                    // Create an IEnumerable from a list of double max stretch value doubles
                    IEnumerable<double> myMaxValues = new List<double> { Convert.ToDouble(_Input_Parameter2.Text) };

                    // Create a new MinMaxStretchParameters based on the user choice for min and max stretch values
                    MinMaxStretchParameters myMinMaxStretchParameters = new MinMaxStretchParameters(myMinValues, myMaxValues);

                    // Create the stretch renderer based on the user defined min/max stretch values, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new StretchRenderer(myMinMaxStretchParameters, myGammaValues, true, myColorRamp);

                    break;

                case "Percent Clip":

                    // This section creates a stretch renderer based on a PercentClipStretchParameters
                    // TODO: Add you own logic to ensure that accurate min/max percent clip values are used

                    // Create a new PercentClipStretchParameters based on the user choice for min and max percent clip values
                    PercentClipStretchParameters myPercentClipStretchParameters = new PercentClipStretchParameters(Convert.ToDouble(_Input_Parameter1.Text), Convert.ToDouble(_Input_Parameter2.Text));

                    // Create the percent clip renderer based on the user defined min/max percent clip values, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new StretchRenderer(myPercentClipStretchParameters, myGammaValues, true, myColorRamp);

                    break;

                case "Standard Deviation":

                    // This section creates a stretch renderer based on a StandardDeviationStretchParameters
                    // TODO: Add you own logic to ensure that an accurate standard deviation value is used

                    // Create a new StandardDeviationStretchParameters based on the user choice for standard deviation value
                    StandardDeviationStretchParameters myStandardDeviationStretchParameters = new StandardDeviationStretchParameters(Convert.ToDouble(_Input_Parameter1.Text));

                    // Create the standard deviation renderer based on the user defined standard deviation value, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new StretchRenderer(myStandardDeviationStretchParameters, myGammaValues, true, myColorRamp);

                    break;
            }

            // Get the existing raster layer in the map
            RasterLayer myRasterLayer = (RasterLayer)_myMapView.Map.OperationalLayers[0];

            // Apply the stretch renderer to the raster layer
            myRasterLayer.Renderer = myStretchRenderer;
        }

        private void RendererTypes_SelectionChanged(object sender, AdapterView.ItemClickEventArgs e)
        {

            // This function modifies the UI parameter controls depending on which stretch 
            // renderer is chosen by the user when clicking the table view

            // Get the TextView from the AdapterView.ItemClickEventArgs
            TextView myTextView = (TextView)e.View;

            // Get the user choice for the raster stretch render from the TextView
            string myRendererTypeChoice = myTextView.Text;

            switch (myRendererTypeChoice)
            {
                case "Min Max":

                    // This section displays/resets the user choice options for MinMaxStretchParameters

                    // Make sure all the GUI items are visible
                    _Label_Parameter1.Visibility = Android.Views.ViewStates.Visible;
                    _Label_Parameter2.Visibility = Android.Views.ViewStates.Visible;
                    _Input_Parameter1.Visibility = Android.Views.ViewStates.Visible;
                    _Input_Parameter2.Visibility = Android.Views.ViewStates.Visible;

                    // Define what values/options the user sees
                    _Label_Parameter1.Text = "Minimum value (0 - 255):";
                    _Label_Parameter2.Text = "Maximum value (0 - 255):";
                    _Input_Parameter1.Text = "10";
                    _Input_Parameter2.Text = "150";

                    break;

                case "Percent Clip":

                    // This section displays/resets the user choice options for PercentClipStretchParameters

                    // Make sure all the GUI items are visible
                    _Label_Parameter1.Visibility = Android.Views.ViewStates.Visible;
                    _Label_Parameter2.Visibility = Android.Views.ViewStates.Visible;
                    _Input_Parameter1.Visibility = Android.Views.ViewStates.Visible;
                    _Input_Parameter2.Visibility = Android.Views.ViewStates.Visible;

                    // Define what values/options the user sees
                    _Label_Parameter1.Text = "Minimum (0 - 100):";
                    _Label_Parameter2.Text = "Maximum (0 - 100)";
                    _Input_Parameter1.Text = "0";
                    _Input_Parameter2.Text = "50";

                    break;

                case "Standard Deviation":

                    // This section displays/resets the user choice options for StandardDeviationStretchParameters

                    // Make sure that only the necessary GUI items are visible
                    _Label_Parameter1.Visibility = Android.Views.ViewStates.Visible;
                    _Label_Parameter2.Visibility = Android.Views.ViewStates.Invisible;
                    _Input_Parameter1.Visibility = Android.Views.ViewStates.Visible;
                    _Input_Parameter2.Visibility = Android.Views.ViewStates.Invisible;

                    // Define what values/options the user sees
                    _Label_Parameter1.Text = "Factor (.25 to 4):";
                    _Input_Parameter1.Text = "0.5";

                    break;
            }
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("95392f99970d4a71bd25951beb34a508", "shasta", "ShastaBW.tif");
        }

    }
}