// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.ChangeBlendRenderer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Blend renderer",
        category: "Layers",
        description: "Blend a hillshade with a raster by specifying the elevation data. The resulting raster looks similar to the original raster, but with some terrain shading, giving it a textured look.",
        instructions: "Choose and adjust the altitude, azimuth, slope type, and color ramp type settings to update the image.",
        tags: new[] { "color ramp", "elevation", "hillshade", "image", "raster", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd",
        "caeef9aa78534760b07158bb8e068462")]
    public partial class ChangeBlendRenderer
    {
        public ChangeBlendRenderer()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Update the preset color ramp type options and select the first one.
            ColorRamps.ItemsSource = Enum.GetNames(typeof(PresetColorRampType));
            ColorRamps.SelectedIndex = 0;

            // Update the slope options and select the first one.
            SlopeTypes.ItemsSource = Enum.GetNames(typeof(SlopeType));
            SlopeTypes.SelectedIndex = 0;

            // Load the raster file using a path on disk.
            Raster myRasterImagery = new Raster(GetRasterPath_Imagery());

            // Create the raster layer from the raster.
            RasterLayer myRasterLayerImagery = new RasterLayer(myRasterImagery);

            // Create a new map using the raster layer as the base map.
            Map myMap = new Map(new Basemap(myRasterLayerImagery));

            try
            {
                // Wait for the layer to load - this enabled being able to obtain the extent information
                // of the raster layer.
                await myRasterLayerImagery.LoadAsync();

                // Create a new EnvelopeBuilder from the full extent of the raster layer.
                EnvelopeBuilder myEnvelopBuilder = new EnvelopeBuilder(myRasterLayerImagery.FullExtent);

                // Zoom in the extent just a bit so that raster layer encompasses the entire viewable area of the map.
                myEnvelopBuilder.Expand(0.75);

                // Set the viewpoint of the map to the EnvelopeBuilder's extent.
                myMap.InitialViewpoint = new Viewpoint(myEnvelopBuilder.ToGeometry().Extent);

                // Add map to the map view.
                MyMapView.Map = myMap;

                // Wait for the map to load.
                await myMap.LoadAsync();

                // Enable the 'Update Renderer' button now that the map has loaded.
                UpdateRenderer.IsEnabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private void OnUpdateRendererClicked(object sender, RoutedEventArgs e)
        {
            // Define the RasterLayer that will be used to display in the map.
            RasterLayer rasterLayerForDisplayInMap;

            // Define the ColorRamp that will be used by the BlendRenderer.
            ColorRamp myColorRamp;

            // Based on ColorRamp type chosen by the user, create a different
            // RasterLayer and define the appropriate ColorRamp option.
            if (ColorRamps.SelectedValue.ToString() == "None")
            {
                // The user chose not to use a specific ColorRamp, therefore
                // need to create a RasterLayer based on general imagery (i.e. Shasta.tif)
                // for display in the map and use null for the ColorRamp as one of the
                // parameters in the BlendRenderer constructor.

                // Load the raster file using a path on disk.
                Raster rasterImagery = new Raster(GetRasterPath_Imagery());

                // Create the raster layer from the raster.
                rasterLayerForDisplayInMap = new RasterLayer(rasterImagery);

                // Set up the ColorRamp as being null.
                myColorRamp = null;
            }
            else
            {
                // The user chose a specific ColorRamp (options: are Elevation, DemScreen, DemLight),
                // therefore create a RasterLayer based on an imagery with elevation
                // (i.e. Shasta_Elevation.tif) for display in the map. Also create a ColorRamp
                // based on the user choice, translated into an Enumeration, as one of the parameters
                // in the BlendRenderer constructor.

                // Load the raster file using a path on disk.
                Raster rasterElevation = new Raster(GetRasterPath_Elevation());

                // Create the raster layer from the raster.
                rasterLayerForDisplayInMap = new RasterLayer(rasterElevation);

                // Create a ColorRamp based on the user choice, translated into an Enumeration.
                PresetColorRampType myPresetColorRampType =
                    (PresetColorRampType)Enum.Parse(typeof(PresetColorRampType), ColorRamps.SelectedValue.ToString());
                myColorRamp = ColorRamp.Create(myPresetColorRampType, 256);
            }

            // Define the parameters used by the BlendRenderer constructor.
            Raster rasterForMakingBlendRenderer = new Raster(GetRasterPath_Elevation());
            IEnumerable<double> myOutputMinValues = new List<double> { 9 };
            IEnumerable<double> myOutputMaxValues = new List<double> { 255 };
            IEnumerable<double> mySourceMinValues = new List<double>();
            IEnumerable<double> mySourceMaxValues = new List<double>();
            IEnumerable<double> myNoDataValues = new List<double>();
            IEnumerable<double> myGammas = new List<double>();
            SlopeType mySlopeType = (SlopeType)Enum.Parse(typeof(SlopeType), SlopeTypes.SelectedValue.ToString());

            BlendRenderer myBlendRenderer = new BlendRenderer(
                rasterForMakingBlendRenderer, // elevationRaster - Raster based on a elevation source.
                myOutputMinValues, // outputMinValues - Output stretch values, one for each band.
                myOutputMaxValues, // outputMaxValues - Output stretch values, one for each band.
                mySourceMinValues, // sourceMinValues - Input stretch values, one for each band.
                mySourceMaxValues, // sourceMaxValues - Input stretch values, one for each band.
                myNoDataValues, // noDataValues - NoData values, one for each band.
                myGammas, // gammas - Gamma adjustment.
                myColorRamp, // colorRamp - ColorRamp object to use, could be null.
                AltitudeSlider.Value, // altitude - Altitude angle of the light source.
                AzimuthSlider.Value, // azimuth - Azimuth angle of the light source, measured clockwise from north.
                1, // zfactor - Factor to convert z unit to x,y units, default is 1.
                mySlopeType, // slopeType - Slope Type.
                1, // pixelSizeFactor - Pixel size factor, default is 1.
                1, // pixelSizePower - Pixel size power value, default is 1.
                8); // outputBitDepth - Output bit depth, default is 8-bi.

            // Set the RasterLayer.Renderer to be the BlendRenderer.
            rasterLayerForDisplayInMap.Renderer = myBlendRenderer;

            // Set the new base map to be the RasterLayer with the BlendRenderer applied.
            MyMapView.Map.Basemap = new Basemap(rasterLayerForDisplayInMap);
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