// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.RasterLayerGeoPackage
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Raster layer (GeoPackage)",
        category: "Data",
        description: "Display a raster contained in a GeoPackage.",
        instructions: "When the sample starts, a raster will be loaded from a GeoPackage and displayed in the map view.",
        tags: new[] { "OGC", "container", "data", "image", "import", "layer", "package", "raster", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    public partial class RasterLayerGeoPackage
    {
        public RasterLayerGeoPackage()
        {
            InitializeComponent();

            // Read data from the GeoPackage
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a new map
            MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);

            // Get the full path
            string geoPackagePath = GetGeoPackagePath();

            try
            {
                // Open the GeoPackage
                GeoPackage myGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);

                // Read the raster images and get the first one
                Raster gpkgRaster = myGeoPackage.GeoPackageRasters.FirstOrDefault();

                // Make sure an image was found in the package
                if (gpkgRaster == null) { return; }

                // Create a layer to show the raster
                RasterLayer newLayer = new RasterLayer(gpkgRaster);
                await newLayer.LoadAsync();

                // Set the viewpoint
                await MyMapView.SetViewpointAsync(new Viewpoint(newLayer.FullExtent));

                // Add the image as a raster layer to the map (with default symbology)
                MyMapView.Map.OperationalLayers.Add(newLayer);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private static string GetGeoPackagePath()

        {
            return DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");
        }
    }
}