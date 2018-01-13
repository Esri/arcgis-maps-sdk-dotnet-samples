// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using ArcGISRuntimeXamarin.Managers;
using System;
using Xamarin.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ArcGISRuntimeXamarin.Samples.RasterHillshade
{
    public partial class RasterHillshade : ContentPage
    {
        // Constant to store a z-factor (conversion constant) applied to the hillshade.
        // If needed, this can be used to convert z-values to the same unit as the x/y coordinates or to apply a vertical exaggeration.
        private const double Z_FACTOR = 1.0;

        // Constants to store the Pixel Size Power and Pixel Size Factor values.
        // Use these to account for altitude changes (scale) as the viewer zooms in and out (recommended when using worldwide datasets).
        private const double PIXEL_SIZE_POWER = 1.0;
        private const double PIXEL_SIZE_FACTOR = 1.0;

        // Constant to store the bit depth (pixel depth), which determines the range of values that the hillshade raster can store.
        private const int PIXEL_BIT_DEPTH = 8;

        // Store a reference to the layer
        RasterLayer _rasterLayer;

        // Store a dictionary of slope types
        Dictionary<string, SlopeType> _slopeTypeValues = new Dictionary<string, SlopeType>();

        public RasterHillshade()
        {
            InitializeComponent();

            // Call a function to set up the map
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap
            Map map = new Map(Basemap.CreateStreets());

            // Get the file name for the local raster dataset
            string filepath = await GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            // Create and load a new raster layer to show the image
            _rasterLayer = new RasterLayer(rasterFile);
            await _rasterLayer.LoadAsync();

            // Create a viewpoint with the raster's full extent
            Viewpoint fullRasterExtent = new Viewpoint(_rasterLayer.FullExtent);

            // Set the initial viewpoint for the map
            map.InitialViewpoint = fullRasterExtent;

            // Add the layer to the map
            map.OperationalLayers.Add(_rasterLayer);

            // Add the map to the map view
            MyMapView.Map = map;

            // Add slope type values to the dictionary and picker
            foreach (var slope in Enum.GetValues(typeof(SlopeType)))
            {
                _slopeTypeValues.Add(slope.ToString(), (SlopeType)slope);
                SlopeTypePicker.Items.Add(slope.ToString());
            }

            // Select the "Scaled" slope type enum
            SlopeTypePicker.SelectedIndex = 2;
        }

        private void ApplyHillshade_Click(object sender, EventArgs e)
        {
            // Get the current parameter values
            double altitude = AltitudeSlider.Value;
            double azimuth = AzimuthSlider.Value;
            SlopeType typeOfSlope = _slopeTypeValues[SlopeTypePicker.SelectedItem.ToString()];

            // Create a hillshade renderer that uses the values selected by the user
            HillshadeRenderer hillshadeRenderer = new HillshadeRenderer(altitude, azimuth, Z_FACTOR, typeOfSlope, PIXEL_SIZE_FACTOR, PIXEL_SIZE_POWER, PIXEL_BIT_DEPTH);

            // Apply the new renderer to the raster layer
            _rasterLayer.Renderer = hillshadeRenderer;
        }

        private async Task<string> GetRasterPath()
        {
            #region offlinedata

            // The desired raster is expected to be called srtm.tiff
            string filename = "srtm.tiff";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "RasterHillshade", "srtm-hillshade", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData("134d60f50e184e8fa56365f44e5ce3fb", "RasterHillshade");
            }
            return filepath;

            #endregion offlinedata
        }
    }
}