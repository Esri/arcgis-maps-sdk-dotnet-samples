// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Linq;
using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Rasters;

namespace ArcGISRuntime.Samples.RasterLayerGeoPackage
{
    [Activity]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster layer (GeoPackage)",
        "Data",
        "This sample demonstrates how to open a GeoPackage and show a GeoPackage raster in a raster layer.",
        "The GeoPackage will be downloaded from an ArcGIS Online portal automatically.")]
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
            // Create a new map
            _myMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

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
            await _myMapView.SetViewpointAsync(new Viewpoint(newLayer.FullExtent));

            // Add the image as a raster layer to the map (with default symbology)
            _myMapView.Map.OperationalLayers.Add(newLayer);
        }

        private static string GetGeoPackagePath()
        {
            return DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");
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