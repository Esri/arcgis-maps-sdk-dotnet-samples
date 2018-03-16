// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Collections.Generic;

namespace ArcGISRuntime.UWP.Samples.ChangeBlendRenderer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Blend renderer",
        "Layers",
        "This sample demonstrates how to use blend renderer on a raster layer. You can get a hillshade blended with either a colored raster or color ramp.",
        "Tap on the 'Update Renderer' button to change the settings for the blend renderer. The sample allows you to change the Altitude, Azimuth, SlopeType and ColorRamp. If you use None as the ColorRamp, a standard hill shade raster output is displayed. For all the other ColorRamp types an elevation raster is used.",
        "Featured")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd","caeef9aa78534760b07158bb8e068462")]
    public partial class ChangeBlendRenderer
    {
        public ChangeBlendRenderer()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Get all the ColorRamp names from the PresetColorRampType Enumeration and put them 
            // in an array of strings, then set the ComboBox.ItemSource to the array, and finally 
            // select the first item in the ComboBox
            string[] myPresetColorRampTypes = System.Enum.GetNames(typeof(PresetColorRampType));
            ColorRamps.ItemsSource = myPresetColorRampTypes;
            ColorRamps.SelectedIndex = 0;

            // Get all the SlopeType names from the SlopeType Enumeration and put them 
            // in an array of strings, then set the ComboBox.ItemSource to the array, and finally 
            // select the first item in the ComboBox
            string[] mySlopeTypes = System.Enum.GetNames(typeof(SlopeType));
            SlopeTypes.ItemsSource = mySlopeTypes;
            SlopeTypes.SelectedIndex = 0;

            // Set the altitude slider min/max and initial value
            Altitude_Slider.Minimum = 0;
            Altitude_Slider.Maximum = 90;
            Altitude_Slider.Value = 45;

            // Set the azimuth slider min/max and initial value
            Azimuth_Slider.Minimum = 0;
            Azimuth_Slider.Maximum = 360;
            Azimuth_Slider.Value = 180;

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

            // Zoom in the extent just a bit so that raster layer encompasses the entire viewable area of the map
            myEnvelopBuilder.Expand(0.75);

            // Set the viewpoint of the map to the EnvelopeBuilder's extent
            myMap.InitialViewpoint = new Viewpoint(myEnvelopBuilder.ToGeometry().Extent);

            // Add map to the map view
            MyMapView.Map = myMap;

            // Wait for the map to load
            await myMap.LoadAsync();

            // Enable the 'Update Renderer' button now that the map has loaded
            UpdateRenderer.IsEnabled = true;
        }

        private void OnUpdateRendererClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Define the RasterLayer that will be used to display in the map
            RasterLayer rasterLayer_ForDisplayInMap;

            // Define the ColorRamp that will be used by the BlendRenderer
            ColorRamp myColorRamp;

            // Based on ColorRamp type chosen by the user, create a different
            // RasterLayer and define the appropriate ColorRamp option
            if (ColorRamps.SelectedValue.ToString() == "None")
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
                PresetColorRampType myPresetColorRampType = (PresetColorRampType)Enum.Parse(typeof(PresetColorRampType), ColorRamps.SelectedValue.ToString());
                myColorRamp = ColorRamp.Create(myPresetColorRampType, 256);
            }

            // Define the parameters used by the BlendRenderer constructor
            Raster raster_ForMakingBlendRenderer = new Raster(GetRasterPath_Elevation());
            IEnumerable<double> myOutputMinValues = new List<double> { 9 };
            IEnumerable<double> myOutputMaxValues = new List<double> { 255 };
            IEnumerable<double> mySourceMinValues = new List<double> { };
            IEnumerable<double> mySourceMaxValues = new List<double> { };
            IEnumerable<double> myNoDataValues = new List<double> { };
            IEnumerable<double> myGammas = new List<double> { };
            SlopeType mySlopeType = (SlopeType)Enum.Parse(typeof(SlopeType), SlopeTypes.SelectedValue.ToString());

            BlendRenderer myBlendRenderer = new BlendRenderer(
                raster_ForMakingBlendRenderer, // elevationRaster - Raster based on a elevation source
                myOutputMinValues, // outputMinValues - Output stretch values, one for each band
                myOutputMaxValues, // outputMaxValues - Output stretch values, one for each band
                mySourceMinValues, // sourceMinValues - Input stretch values, one for each band
                mySourceMaxValues, // sourceMaxValues - Input stretch values, one for each band
                myNoDataValues, // noDataValues - NoData values, one for each band
                myGammas, // gammas - Gamma adjustment
                myColorRamp, // colorRamp - ColorRamp object to use, could be null
                Altitude_Slider.Value, // altitude - Altitude angle of the light source
                Azimuth_Slider.Value, // azimuth - Azimuth angle of the light source, measured clockwise from north
                1, // zfactor - Factor to convert z unit to x,y units, default is 1
                mySlopeType, // slopeType - Slope Type
                1, // pixelSizeFactor - Pixel size factor, default is 1
                1, // pixelSizePower - Pixel size power value, default is 1
                8); // outputBitDepth - Output bit depth, default is 8-bi

            // Set the RasterLayer.Renderer to be the BlendRenderer
            rasterLayer_ForDisplayInMap.Renderer = myBlendRenderer;

            // Set the new base map to be the RasterLayer with the BlendRenderer applied
            MyMapView.Map.Basemap = new Basemap(rasterLayer_ForDisplayInMap);
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