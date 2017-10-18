// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Linq;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;
using System.IO;
using ArcGISRuntimeXamarin.Managers;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;

namespace ArcGISRuntimeXamarin.Samples.FeatureLayerGeoPackage
{
    [Register("FeatureLayerGeoPackage")]
    public class FeatureLayerGeoPackage : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public FeatureLayerGeoPackage()
        {
            Title = "Feature layer (GeoPackage)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Update the UI to account for new layout
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
        }

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado
            _myMapView.Map = new Map(BasemapType.LightGrayCanvasVector, 39.7294, -104.8319, 9);

            // Get the full path
            string geoPackagePath = await GetGeoPackagePath();

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
            _myMapView.Map.OperationalLayers.Add(newLayer);
        }

        private async Task<string> GetGeoPackagePath()
        {
            #region offline data

            // The GeoPackage will be downloaded from ArcGIS Online.
            // The data manager (a component of the sample viewer), *NOT* the runtime handles the offline data process

            // The desired GPKG is expected to be called "AuroraCO.shp"
            string filename = "AuroraCO.gpkg";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "ReadGeoPackage", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // If it's missing, download the GeoPackage
                await DataManager.GetData("68ec42517cdd439e81b036210483e8e7", "ReadGeoPackage");
            }

            // Return the path
            return filepath;

            #endregion offlinedata
        }

        private void CreateLayout()
        {
            // Add MapView to the page
            View.AddSubview(_myMapView);
        }
    }
}