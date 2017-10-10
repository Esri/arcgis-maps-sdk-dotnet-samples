// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.IO;

namespace ArcGISRuntime.WPF.Samples.ReadGeoPackage
{
    public partial class ReadGeoPackage
    {
        public ReadGeoPackage()
        {
            InitializeComponent();

            // Read data from the GeoPackage
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado
            MyMapView.Map = new Map(BasemapType.LightGrayCanvasVector, 39.7294, -104.8319, 10);

            // The GeoPackage will be downloaded from ArcGIS Online
            // The data manager (a component of the sample viewer), *NOT* the runtime handles the offline data process

            // The desired GPKG is expected to be called Yellowstone.gpkg
            string filename = "AuroraCO.gpkg";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string geoPackagePath = Path.Combine(folder, "SampleData", "ReadGeoPackage", filename);

            // Check if the file exists
            if (!File.Exists(geoPackagePath))
            {
                // If it's missing, download the GeoPackage file
                await DataManager.GetData("e1f3a7254cb845b09450f54937c16061", "ReadGeoPackage");
            }

            // Open the GeoPackage
            GeoPackage myGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);

            // Show the lists of GeoPackageFeatureTables and GeoPackageRasters in the list boxes
            GeoPackageDatasetListBox.ItemsSource = myGeoPackage.GeoPackageFeatureTables; 
            GeoPackageImageListBox.ItemsSource = myGeoPackage.GeoPackageRasters; 
        }

        private async void AddGeoPackageData(object sender, System.Windows.RoutedEventArgs e)
        {
            // Iterate each selected GeoPackageRaster and add it to the map as a new raster layer
            foreach (GeoPackageRaster packageRaster in GeoPackageImageListBox.SelectedItems)
            {
                try
                {
                    // Create a new RasterLayer to show the image and add it to the map
                    var layer = new RasterLayer(packageRaster);
                    MyMapView.Map.OperationalLayers.Add(layer);
                }
                catch (Exception ex)
                {
                    // Show exception in the console (layer might already exist in the map, e.g.)
                    Console.WriteLine("Could not add layer: " + ex.Message);
                }
            }

            // Iterate each selected GeoPackageFeatureTable and add it to the map as a new feature layer
            foreach (GeoPackageFeatureTable packageTable in GeoPackageDatasetListBox.SelectedItems)
            {
                try
                {
                    // Create a new FeatureLayer to show the table and add it to the map
                    var layer = new FeatureLayer(packageTable);
                    await layer.LoadAsync();
                    MyMapView.Map.OperationalLayers.Add(layer);
                }
                catch (Exception ex)
                {
                    // Show exception in the console (layer might already exist in the map, e.g.)
                    Console.WriteLine("Could not add layer: " + ex.Message);
                }
            }
        }

        private void ClearGeoPackageData(object sender, System.Windows.RoutedEventArgs e)
        {
            // Clear all GeoPackage layers from the map
            MyMapView.Map.OperationalLayers.Clear();
        }
    }
}