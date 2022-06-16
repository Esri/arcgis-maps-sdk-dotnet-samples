// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.ReadGeoPackage
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Read GeoPackage",
        category: "Data",
        description: "Add rasters and feature tables from a GeoPackage to a map.",
        instructions: "When the sample loads, the feature tables and rasters from the GeoPackage will be shown on the map.",
        tags: new[] { "GeoPackage", "OGC", "container", "layer", "map", "package", "raster", "table" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    public class ReadGeoPackage : Activity
    {
        // Member MapView UI control used in the sample.
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Read a GeoPackage";
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado.
            _myMapView.Map = new Map(BasemapStyle.ArcGISStreets);
            _myMapView.Map.InitialViewpoint = new Viewpoint(39.7294, -104.70, 11);

            // Get the full path to the GeoPackage on the device.
            string myGeoPackagePath = GetGeoPackagePath();

            try
            {
                // Open the GeoPackage.
                GeoPackage myGeoPackage = await GeoPackage.OpenAsync(myGeoPackagePath);

                // Get the read only list of GeoPackageRasters from the GeoPackage.
                IReadOnlyList<GeoPackageRaster> myReadOnlyListOfGeoPackageRasters = myGeoPackage.GeoPackageRasters;

                // Loop through each GeoPackageRaster.
                foreach (GeoPackageRaster oneGeoPackageRaster in myReadOnlyListOfGeoPackageRasters)
                {
                    // Create a RasterLayer from the GeoPackageRaster.
                    RasterLayer myRasterLayer = new RasterLayer(oneGeoPackageRaster)
                    {
                        // Set the opacity on the RasterLayer to partially visible.
                        Opacity = 0.55
                    };

                    // Add the layer to the map.
                    _myMapView.Map.OperationalLayers.Add(myRasterLayer);
                }

                // Get the read only list of GeoPackageFeatureTabless from the GeoPackage.
                IReadOnlyList<GeoPackageFeatureTable> myReadOnlyListOfGeoPackageFeatureTables = myGeoPackage.GeoPackageFeatureTables;

                // Loop through each GeoPackageFeatureTable.
                foreach (GeoPackageFeatureTable oneGeoPackageFeatureTable in myReadOnlyListOfGeoPackageFeatureTables)
                {
                    // Create a FeatureLayer from the GeoPackageFeatureLayer.
                    FeatureLayer myFeatureLayer = new FeatureLayer(oneGeoPackageFeatureTable);

                    // Add the layer to the map.
                    _myMapView.Map.OperationalLayers.Add(myFeatureLayer);
                }
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Add a map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private static string GetGeoPackagePath() => DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");
    }
}