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
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.RasterLayerGeoPackage
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster layer (GeoPackage)",
        "Data",
        "This sample demonstrates how to open a GeoPackage and show a GeoPackage raster in a raster layer.",
        "The GeoPackage will be downloaded from an ArcGIS Online portal automatically.")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    public partial class RasterLayerGeoPackage : ContentPage
    {
        public RasterLayerGeoPackage()
        {
            InitializeComponent();

            Title = "Raster layer (GeoPackage)";

            // Read data from the GeoPackage
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            // Get the full path
            string geoPackagePath = GetGeoPackagePath();

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

        private static string GetGeoPackagePath()

        {
            return DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");
        }
    }
}