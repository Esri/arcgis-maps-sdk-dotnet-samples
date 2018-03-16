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
using System.Linq;

namespace ArcGISRuntime.UWP.Samples.FeatureLayerGeoPackage
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer (GeoPackage)",
        "Data",
        "This sample demonstrates how to open a GeoPackage and show a GeoPackage feature table in a feature layer.",
        "The GeoPackage will be downloaded from an ArcGIS Online portal automatically.",
        "Featured")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    public partial class FeatureLayerGeoPackage
    {
        public FeatureLayerGeoPackage()
        {
            InitializeComponent();

            // Read data from the GeoPackage
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado
            MyMapView.Map = new Map(BasemapType.LightGrayCanvasVector, 39.7294, -104.8319, 9);

            // Get the full path
            string geoPackagePath = GetGeoPackagePath();

            // Open the GeoPackage
            GeoPackage myGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);

            // Read the feature tables and get the first one
            FeatureTable geoPackageTable = myGeoPackage.GeoPackageFeatureTables.FirstOrDefault();

            // Make sure a feature table was found in the package
            if (geoPackageTable == null) { return; }

            // Create a layer to show the feature table
            FeatureLayer newLayer = new FeatureLayer(geoPackageTable);
            await newLayer.LoadAsync();

            // Add the feature table as a layer to the map (with default symbology)
            MyMapView.Map.OperationalLayers.Add(newLayer);
        }

        private static string GetGeoPackagePath()

        {
            return DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");
        }
    }
}