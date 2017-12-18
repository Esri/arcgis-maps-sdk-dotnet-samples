// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Linq;
using System.IO;
using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using ArcGISRuntimeXamarin.Managers;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Rasters;

namespace ArcGISRuntimeXamarin.Samples.RasterLayerGeoPackage
{
    [Activity]
    public class RasterLayerGeoPackage : Activity
    {
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Raster layer (GeoPackage)";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }
        
        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado
            _myMapView.Map = new Map(BasemapType.LightGrayCanvas, 39.5517, -104.8589, 12);

            // Get the full path
            string geoPackagePath = await GetGeoPackagePath();

            // Open the GeoPackage
            GeoPackage myGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);

            // Read the raster images and get the first one
            Raster gpkgRaster = myGeoPackage.GeoPackageRasters.FirstOrDefault();

            // Make sure an image was found in the package
            if (gpkgRaster == null) { return; }

            // Create a layer to show the raster
            RasterLayer newLayer = new RasterLayer(gpkgRaster);
            await newLayer.LoadAsync();

            // Add the image as a raster layer to the map (with default symbology)
            _myMapView.Map.OperationalLayers.Add(newLayer);
        }

        private async Task<string> GetGeoPackagePath()
        {
            #region offlinedata

            // The GeoPackage will be downloaded from ArcGIS Online.
            // The data manager (a component of the sample viewer), *NOT* the runtime handles the offline data process

            // The desired GPKG is expected to be called "AuroraCO.shp"
            string filename = "AuroraCO.gpkg";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "RasterLayerGeoPackage", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // If it's missing, download the GeoPackage
                await DataManager.GetData("68ec42517cdd439e81b036210483e8e7", "RasterLayerGeoPackage");
            }

            // Return the path
            return filepath;

            #endregion offlinedata
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}