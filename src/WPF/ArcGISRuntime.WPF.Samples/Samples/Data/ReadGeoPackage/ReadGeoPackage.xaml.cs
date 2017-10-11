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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;

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
            MyMapView.Map = new Map(BasemapType.LightGrayCanvasVector, 39.7294, -104.8310, 14);

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
                await DataManager.GetData("68ec42517cdd439e81b036210483e8e7", "ReadGeoPackage");
            }

            // Open the GeoPackage
            GeoPackage myGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);

            // Create a collection of FeatureLayers that reference each GeoPackage feature table
            var featureLayers = new ObservableCollection<Layer>();

            // Iterate all feature tables in the GeoPackage
            foreach (var table in myGeoPackage.GeoPackageFeatureTables)
            {
                // Create a feature layer to display the table
                var lyr = new FeatureLayer(table);

                // Use the GeoPackageFeatureTable description for the layer name
                lyr.Name = table.Description;

                // Add it to the feature layer collection
                featureLayers.Add(lyr);
            }

            // Create a collection of RasterLayers that reference each GeoPackage raster image
            var rasterLayers = new ObservableCollection<Layer>();

            // Iterate all raster images in the GeoPackage
            foreach(var raster in myGeoPackage.GeoPackageRasters)
            {
                // Create a raster layer to display the image
                var lyr = new RasterLayer(raster);

                // Use the GeoPackageRaster description as the layer name
                lyr.Name = raster.Description;

                // Add it to the raster layer collection
                rasterLayers.Add(lyr);
            }

            // Show the lists of feature and raster layers in the list boxes
            GeoPackageFeatureTablesListBox.ItemsSource = featureLayers;
            GeoPackageImagesListBox.ItemsSource = rasterLayers;
        }

        private async void AddGeoPackageLayers(object sender, System.Windows.RoutedEventArgs e)
        {
            // Iterate each selected feature layer and add it to the map
            foreach (FeatureLayer featureLyr in GeoPackageFeatureTablesListBox.SelectedItems)
            {
                try
                {
                    // Load the layer and render it according to the geometry type
                    await featureLyr.LoadAsync();
                    featureLyr.Renderer = CreateRenderer(featureLyr.FeatureTable.GeometryType);

                    // Add feature layers to the operational layer collection
                    MyMapView.Map.OperationalLayers.Add(featureLyr);
                }
                catch (Exception ex)
                {
                    // Show exception in the console (layer might already exist in the map, e.g.)
                    Console.WriteLine("Could not add layer: " + ex.Message);
                }
            }

            // Iterate each selected raster layer and add it to the map
            foreach (Layer rasterLyr in GeoPackageImagesListBox.SelectedItems)
            {
                try
                {
                    // Insert raster layers at the bottom of the operational layer stack
                    MyMapView.Map.OperationalLayers.Insert(0, rasterLyr);
                }
                catch (Exception ex)
                {
                    // Show exception in the console (layer might already exist in the map, e.g.)
                    Console.WriteLine("Could not add layer: " + ex.Message);
                }
            }
        }

        // Create the right type of renderer for the specified geometry type
        private SimpleRenderer CreateRenderer(GeometryType layerGeometryType)
        {
            // Create the appropriate symbol for the layer geometry type
            Symbol layerSymbol = null;
            switch (layerGeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    // Display point geometry with a simple marker symbol
                    layerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Colors.LightBlue, 16);
                    break;
                case GeometryType.Polyline:
                    // Display line geometry with a simple line symbol
                    layerSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Red, 2);
                    break;
                case GeometryType.Polygon:
                    // Display polygon geometry with a simple fill symbol
                    SimpleLineSymbol outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.DarkGray, 1);
                    layerSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Cross, Colors.Yellow, outlineSymbol);
                    break;
            }

            // Create and return a new renderer with the symbol
            SimpleRenderer layerRenderer = new SimpleRenderer(layerSymbol);
            return layerRenderer;
        }

        private void ClearGeoPackageLayers(object sender, System.Windows.RoutedEventArgs e)
        {
            // Clear all GeoPackage layers from the map
            MyMapView.Map.OperationalLayers.Clear();
        }
    }
}