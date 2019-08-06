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
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.RasterHillshade
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster hillshade renderer",
        "Layers",
        "This sample demonstrates how to use a hillshade renderer on a raster layer. Hillshade renderers can adjust a grayscale raster (usually of terrain) according to a hypothetical sun position (azimuth and altitude).",
        "")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("134d60f50e184e8fa56365f44e5ce3fb")]
    public partial class RasterHillshade
    {
        // Constant to store a z-factor (conversion constant) applied to the hillshade.
        // If needed, this can be used to convert z-values to the same unit as the x/y coordinates or to apply a vertical exaggeration.
        private const double ZFactor = 1.0;

        // Constants to store the Pixel Size Power and Pixel Size Factor values.
        // Use these to account for altitude changes (scale) as the viewer zooms in and out (recommended when using worldwide datasets).
        private const double PixelSizePower = 1.0;
        private const double PixelSizeFactor = 1.0;

        // Constant to store the bit depth (pixel depth), which determines the range of values that the hillshade raster can store.
        private const int PixelBitDepth = 8;

        // Store a reference to the layer
        private RasterLayer _rasterLayer; 
        
        public RasterHillshade()
        {
            InitializeComponent();

            // Set up the map and load the raster layer from a local file
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap
            Map map = new Map(Basemap.CreateStreets());

            // Get the file name for the local raster dataset
            string filepath = GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            try
            {
                // Create and load a new raster layer to show the image
                _rasterLayer = new RasterLayer(rasterFile);
                await _rasterLayer.LoadAsync();

                // Enable the apply renderer button when the layer loads.
                ApplyHillshadeButton.IsEnabled = true;

                // Create a viewpoint with the raster's full extent
                Viewpoint fullRasterExtent = new Viewpoint(_rasterLayer.FullExtent);

                // Set the initial viewpoint for the map
                map.InitialViewpoint = fullRasterExtent;

                // Add the layer to the map
                map.OperationalLayers.Add(_rasterLayer);

                // Add the map to the map view
                MyMapView.Map = map;

                // Add slope type values to the combo box
                foreach (object slope in Enum.GetValues(typeof(SlopeType)))
                {
                    SlopeTypeCombo.Items.Add(slope);
                }

                // Select the "Scaled" slope type enum
                SlopeTypeCombo.SelectedValue = SlopeType.Scaled;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private void ApplyHillshadeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get the current parameter values
            double altitude = AltitudeSlider.Value;
            double azimuth = AzimuthSlider.Value;
            SlopeType typeOfSlope = (SlopeType)SlopeTypeCombo.SelectedItem;

            // Create a hillshade renderer that uses the values selected by the user
            HillshadeRenderer hillshadeRenderer = new HillshadeRenderer(altitude, azimuth, ZFactor, typeOfSlope, PixelSizeFactor, PixelSizePower, PixelBitDepth);

            // Apply the new renderer to the raster layer
            _rasterLayer.Renderer = hillshadeRenderer;
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("134d60f50e184e8fa56365f44e5ce3fb", "srtm-hillshade", "srtm.tiff");
        }
    }
}