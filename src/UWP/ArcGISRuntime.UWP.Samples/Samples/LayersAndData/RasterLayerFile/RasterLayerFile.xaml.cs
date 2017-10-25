﻿// Copyright 2017 Esri.
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
using System.IO;
using System.Threading.Tasks;

namespace ArcGISRuntime.UWP.Samples.RasterLayerFile
{
    public partial class RasterLayerFile
    {
        public RasterLayerFile()
        {
            InitializeComponent();

            // Call a function to set up the map
            Initialize();
        }

        private async void Initialize()
        {
            // Apply an imagery basemap
            MyMapView.Map = new Map(Basemap.CreateImagery());

            // Get the file name
            String filepath = await GetRasterPath();

            // Load the raster file
            Raster myRasterFile = new Raster(filepath);

            // Create the layer
            RasterLayer myRasterLayer = new RasterLayer(myRasterFile);

            // Load the layer
            await myRasterLayer.LoadAsync();

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(myRasterLayer);

            // Get the raster's extent in a viewpoint
            Viewpoint myFullRasterExtent = new Viewpoint(myRasterLayer.FullExtent);

            // Zoom to the extent
            MyMapView.Map.InitialViewpoint = myFullRasterExtent;
        }

        private async Task<string> GetRasterPath()
        {
            #region offlinedata

            // The desired raster is expected to be called Shasta.tif
            string filename = "Shasta.tif";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "RasterLayerFile", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData("7c4c679ab06a4df19dc497f577f111bd", "RasterLayerFile");
            }
            return filepath;

            #endregion offlinedata
        }
    }
}