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
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("95392f99970d4a71bd25951beb34a508")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Stretch renderer",
        "Layers",
        "Use a stretch renderer to enhance the visual contrast of raster data for analysis.",
        "Choose one of the stretch parameter types:",
        "analysis", "deviation", "histogram", "imagery", "interpretation", "min-max", "percent clip", "pixel", "raster", "stretch", "symbology", "visualization")]
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
            LinearLayout subLayout1 = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create label that displays the 1st parameter used by the stretch renderer
            _Label_Parameter1 = new TextView(this)
            {
                Text = "Minimum value (0 - 255):"
            };
            subLayout1.AddView(_Label_Parameter1);

            // Create text field for 1st parameter used by the stretch renderer that the user can modify 
            _Input_Parameter1 = new EditText(this)
            {
                Text = "10"
            };
            subLayout1.AddView(_Input_Parameter1);

            // Add the sub layout to the main layout
            layout.AddView(subLayout1);

            // ------------

            // Create a sub layout to organize the label and 2nd parameter on a single horizontal line
            LinearLayout subLayout2 = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create label that displays the 2nd parameter used by the stretch renderer
            _Label_Parameter2 = new TextView(this)
            {
                Text = "Maximum value (0 - 255):"
            };
            subLayout2.AddView(_Label_Parameter2);

            // Create text field for 2nd parameter used by the stretch renderer that the user can modify 
            _Input_Parameter2 = new EditText(this)
            {
                Text = "150"
            };
            subLayout2.AddView(_Input_Parameter2);

            // Add the sub layout to the main layout
            layout.AddView(subLayout2);

            // ------------

            // Create button to change stretch renderer of the raster
            _UpdateRenderer = new Button(this)
            {
                Text = "Update Renderer"
            };
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
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Get the file name
            string filepath = GetRasterPath();

            // Load the raster file
            Raster myRasterFile = new Raster(filepath);

            // Create the layer
            RasterLayer myRasterLayer = new RasterLayer(myRasterFile);

            // Add the layer to the map
            _myMapView.Map.OperationalLayers.Add(myRasterLayer);

            try
            {
                // Wait for the layer to load
                await myRasterLayer.LoadAsync();

                // Set the viewpoint
                await _myMapView.SetViewpointGeometryAsync(myRasterLayer.FullExtent);
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private void OnUpdateRendererClicked(object sender, EventArgs e)
        {
            // This function acquires the user selection of the stretch renderer from the table view
            // along with the parameters specified, then a stretch renderer is created and applied to 
            // the raster layer

            // Convert the input text to doubles and return if they're invalid.
            double input1;
            double input2;
            try
            {
                input1 = Convert.ToDouble(_Input_Parameter1.Text);
                input2 = Convert.ToDouble(_Input_Parameter2.Text);
            }
            catch (Exception ex)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetMessage(ex.Message).SetTitle("Alert").Show();
                return;
            }

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
                    IEnumerable<double> myMinValues = new List<double> { input1 };

                    // Create an IEnumerable from a list of double max stretch value doubles
                    IEnumerable<double> myMaxValues = new List<double> { input2 };

                    // Create a new MinMaxStretchParameters based on the user choice for min and max stretch values
                    MinMaxStretchParameters myMinMaxStretchParameters = new MinMaxStretchParameters(myMinValues, myMaxValues);

                    // Create the stretch renderer based on the user defined min/max stretch values, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new StretchRenderer(myMinMaxStretchParameters, myGammaValues, true, myColorRamp);

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