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
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.ChangeBlendRenderer
{
    [Activity(Label = "ChangeBlendRenderer")]
	[Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd","caeef9aa78534760b07158bb8e068462")]
    [Shared.Attributes.Sample(
        "Blend renderer",
        "Layers",
        "This sample demonstrates how to use blend renderer on a raster layer. You can get a hillshade blended with either a colored raster or color ramp.",
        "Tap on the 'Update Renderer' button to change the settings for the blend renderer. The sample allows you to change the Altitude, Azimuth, SlopeType and ColorRamp. If you use None as the ColorRamp, a standard hill shade raster output is displayed. For all the other ColorRamp types an elevation raster is used.",
        "Featured")]
    public class ChangeBlendRenderer : Activity
    {
        // Global reference to a label for Altitude
        private TextView _Label_Altitude;

        // Global reference to the slider (SeekBar) where the user can modify the Altitude
        private SeekBar _Slider_Altitude;

        // Global reference to a label for Azimuth
        private TextView _Label_Azimuth;

        // Global reference to the slider (SeekBar) where the user can modify the Azimuth
        private SeekBar _Slider_Azimuth;

        // Global reference to a label for SlopeTypes
        private TextView _Label_SlopeTypes;

        // Global reference to a label for ColorRamps
        private TextView _Label_ColorRamps;

        // Global reference to button the user clicks to change the stretch renderer on the raster 
        private Button _Button_UpdateRenderer;

        // Global reference to the MapView used in the sample
        private MapView _myMapView;

        // Global variable for the SlopeType being used - changeable when the user make 
        // a selection, set the default value to "Degree"
        private string _mySlopeTypeChoice = "Degree";

        // Global variable for the ColorRamp being used - changeable when the user make 
        // a selection, set the default value to "Elevation"
        private string _myColorRampChoice = "Elevation";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Blend renderer";

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a stack layout
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create label that displays Altitude
            _Label_Altitude = new TextView(this);
            _Label_Altitude.Text = "Altitude";
            layout.AddView(_Label_Altitude);

            // Create a slider (SeekBar) that the user can modify the Altitude
            _Slider_Altitude = new SeekBar(this);
            layout.AddView(_Slider_Altitude);

            // Create label that displays Azimuth
            _Label_Azimuth = new TextView(this);
            _Label_Azimuth.Text = "Azimuth";
            layout.AddView(_Label_Azimuth);

            // Create a slider (SeekBar) that the user can modify the Azimuth
            _Slider_Azimuth = new SeekBar(this);
            layout.AddView(_Slider_Azimuth);
                
            // Create label that displays the SlopeType - set the default label to "Degree"
            _Label_SlopeTypes = new TextView(this);
            _Label_SlopeTypes.Text = _mySlopeTypeChoice;
            layout.AddView(_Label_SlopeTypes);

            // Create button to choose a specific SlopeType
            var slopeTypesButton = new Button(this);
            slopeTypesButton.Text = "SlopeTypes";
            slopeTypesButton.Click += SlopeTypesButton_Click;
            layout.AddView(slopeTypesButton);

            // Create label that displays the ColorRamp  - set the default label to "Elevation"
            _Label_ColorRamps = new TextView(this);
            _Label_ColorRamps.Text = _myColorRampChoice;
            layout.AddView(_Label_ColorRamps);

            // Create button to choose a specific ColorRamp
            var colorRampsButton = new Button(this);
            colorRampsButton.Text = "Color Ramps";
            colorRampsButton.Click += ColorRampsButton_Click;
            layout.AddView(colorRampsButton);

            // Create button to change stretch renderer of the raster, wire-up the touch/click 
            // event handler for the button
            _Button_UpdateRenderer = new Button(this);
            _Button_UpdateRenderer.Text = "Update Renderer";
            _Button_UpdateRenderer.Click += OnUpdateRendererClicked;
            layout.AddView(_Button_UpdateRenderer);
            _Button_UpdateRenderer.Enabled = false;

            // Create a map view and add it to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Set the layout as the sample view
            SetContentView(layout);
        }

        private void ColorRampsButton_Click(object sender, EventArgs e)
        {
            // Create a local variable for the ColorRamps button
            Button myButton_ColorRamps = sender as Button;

            // Create menu to show ColorRamp options
            PopupMenu myPopupMenu_ColorRamps = new PopupMenu(this, myButton_ColorRamps);
            myPopupMenu_ColorRamps.MenuItemClick += OnColorRampsMenuItemClicked;

            // Create a string array of ColorRamp Enumerations the user can pick from
            string[] myColorRamps = Enum.GetNames(typeof(PresetColorRampType));

            // Create menu options from the array of ColorRamp choices
            foreach (string myColorRamp in myColorRamps)
                myPopupMenu_ColorRamps.Menu.Add(myColorRamp);

            // Show the popup menu in the view
            myPopupMenu_ColorRamps.Show();
        }

        private async void Initialize()
        {
            try
            {
                // Set the altitude slider min/max and initial value (minimum is always 0 - do 
                // not set _Altitude_Slider.Min = 0)
                _Slider_Altitude.Max = 90;
                _Slider_Altitude.Progress = 45;

                // Set the azimuth slider min/max and initial value (minimum is always 0 - do 
                // not set _AZimuth_Slider.Min = 0)
                _Slider_Azimuth.Max = 360;
                _Slider_Azimuth.Progress = 180;

                // Load the raster file using a path on disk
                Raster myRasterImagery = new Raster(GetRasterPath_Imagery());

                // Create the raster layer from the raster
                RasterLayer myRasterLayerImagery = new RasterLayer(myRasterImagery);

                // Create a new map using the raster layer as the base map 
                Map myMap = new Map(new Basemap(myRasterLayerImagery));

                // Wait for the layer to load - this enabled being able to obtain the extent information 
                // of the raster layer
                await myRasterLayerImagery.LoadAsync();

                // Create a new EnvelopeBuilder from the full extent of the raster layer 
                EnvelopeBuilder myEnvelopBuilder = new EnvelopeBuilder(myRasterLayerImagery.FullExtent);

                // Zoom in the extent just a bit so that raster layer encompasses the entire viewable 
                // area of the map
                myEnvelopBuilder.Expand(0.75);

                // Set the viewpoint of the map to the EnvelopeBuilder's extent
                myMap.InitialViewpoint = new Viewpoint(myEnvelopBuilder.ToGeometry().Extent);

                // Add map to the map view
                _myMapView.Map = myMap;

                // Wait for the map to load
                await myMap.LoadAsync();

                // Enable the 'Update Renderer' button now that the map has loaded
                _Button_UpdateRenderer.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SlopeTypesButton_Click(object sender, EventArgs e)
        {
            // Create a local variable for the SlopeTypes button
            Button myButton_SlopeTypes = sender as Button;

            // Create menu to show SlopeType options
            PopupMenu myPopupMenu_SlopeTypes = new PopupMenu(this, myButton_SlopeTypes);
            myPopupMenu_SlopeTypes.MenuItemClick += OnSlopeTypesMenuItemClicked;

            // Create a string array of SlopeType Enumerations the user can pick from
            string[] mySlopeTypes = Enum.GetNames(typeof(SlopeType));

            // Create menu options from the array of SlopeType choices
            foreach (string mySlopeType in mySlopeTypes)
                myPopupMenu_SlopeTypes.Menu.Add(mySlopeType);

            // Show the popup menu in the view
            myPopupMenu_SlopeTypes.Show();
        }

        private void OnSlopeTypesMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get user selected SlopeType choice title from the selected item
            _mySlopeTypeChoice = e.Item.TitleCondensedFormatted.ToString();

            // Set the text of the label to be the name of the SlopeType choice
            _Label_SlopeTypes.Text = _mySlopeTypeChoice;
        }

        private void OnColorRampsMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get user selected ColorRamp choice title from the selected item
            _myColorRampChoice = e.Item.TitleCondensedFormatted.ToString();

            // Set the text of the label to be the name of the ColorRamp choice
            _Label_ColorRamps.Text = _myColorRampChoice;
        }

        private void OnUpdateRendererClicked(object sender, EventArgs e)
        {
            // Define the RasterLayer that will be used to display in the map
            RasterLayer rasterLayer_ForDisplayInMap;

            // Define the ColorRamp that will be used by the BlendRenderer
            ColorRamp myColorRamp;

            // Based on ColorRamp type chosen by the user, create a different
            // RasterLayer and define the appropriate ColorRamp option
            if (_myColorRampChoice == "None")
            {
                // The user chose not to use a specific ColorRamp, therefore 
                // need to create a RasterLayer based on general imagery (ie. Shasta.tif)
                // for display in the map and use null for the ColorRamp as one of the
                // parameters in the BlendRenderer constructor

                // Load the raster file using a path on disk
                Raster raster_Imagery = new Raster(GetRasterPath_Imagery());

                // Create the raster layer from the raster
                rasterLayer_ForDisplayInMap = new RasterLayer(raster_Imagery);

                // Set up the ColorRamp as being null
                myColorRamp = null;
            }
            else
            {
                // The user chose a specific ColorRamp (options: are Elevation, DemScreen, DemLight), 
                // therefore create a RasterLayer based on an imagery with elevation 
                // (ie. Shasta_Elevation.tif) for display in the map. Also create a ColorRamp 
                // based on the user choice, translated into an Enumeration, as one of the parameters 
                // in the BlendRenderer constructor

                // Load the raster file using a path on disk
                Raster raster_Elevation = new Raster(GetRasterPath_Elevation());

                // Create the raster layer from the raster
                rasterLayer_ForDisplayInMap = new RasterLayer(raster_Elevation);

                // Create a ColorRamp based on the user choice, translated into an Enumeration
                PresetColorRampType myPresetColorRampType = (PresetColorRampType)Enum.Parse(typeof(PresetColorRampType), _myColorRampChoice);
                myColorRamp = ColorRamp.Create(myPresetColorRampType, 256);
            }

            // Define the parameters used by the BlendRenderer constructor
            Raster raster_ForMakingBlendRenderer = new Raster(GetRasterPath_Elevation());
            IEnumerable<double> myOutputMinValues = new List<double> { 9 };
            IEnumerable<double> myOutputMaxValues = new List<double> { 255 };
            IEnumerable<double> mySourceMinValues = new List<double>();
            IEnumerable<double> mySourceMaxValues = new List<double>();
            IEnumerable<double> myNoDataValues = new List<double>();
            IEnumerable<double> myGammas = new List<double>();
            SlopeType mySlopeType = (SlopeType)Enum.Parse(typeof(SlopeType), _mySlopeTypeChoice);

            BlendRenderer myBlendRenderer = new BlendRenderer(
                raster_ForMakingBlendRenderer, // elevationRaster - Raster based on a elevation source
                myOutputMinValues, // outputMinValues - Output stretch values, one for each band
                myOutputMaxValues, // outputMaxValues - Output stretch values, one for each band
                mySourceMinValues, // sourceMinValues - Input stretch values, one for each band
                mySourceMaxValues, // sourceMaxValues - Input stretch values, one for each band
                myNoDataValues, // noDataValues - NoData values, one for each band
                myGammas, // gammas - Gamma adjustment
                myColorRamp, // colorRamp - ColorRamp object to use, could be null
                _Slider_Altitude.Progress, // altitude - Altitude angle of the light source
                _Slider_Azimuth.Progress, // azimuth - Azimuth angle of the light source, measured clockwise from north
                1, // zfactor - Factor to convert z unit to x,y units, default is 1
                mySlopeType, // slopeType - Slope Type
                1, // pixelSizeFactor - Pixel size factor, default is 1
                1, // pixelSizePower - Pixel size power value, default is 1
                8); // outputBitDepth - Output bit depth, default is 8-bi

            // Set the RasterLayer.Renderer to be the BlendRenderer
            rasterLayer_ForDisplayInMap.Renderer = myBlendRenderer;

            // Set the new base map to be the RasterLayer with the BlendRenderer applied
            _myMapView.Map.Basemap = new Basemap(rasterLayer_ForDisplayInMap);
        }

        private static string GetRasterPath_Imagery()
        {
            return DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");
        }

        private static string GetRasterPath_Elevation()
        {
            return DataManager.GetDataFolder("caeef9aa78534760b07158bb8e068462", "Shasta_Elevation.tif");
        }
    }
}