// Copyright 2022 Esri.
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
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.DisplayFeatureLayers
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display feature layers",
        category: "Layers",
        description: "Display feature layers from various data sources.",
        instructions: "Click the button on the toolbar to add feature layers, from different sources, to the map. Pan and zoom the map to view the feature layers.",
        tags: new[] { "feature", "geodatabase", "geopackage", "layers", "service", "shapefile", "table" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("1759fd3e8a324358a0c58d9a687a8578", "cb1b20748a9f4d128dad8a87244e3e37", "68ec42517cdd439e81b036210483e8e7", "15a7cbd3af1e47cfa5d2c6b93dc44fc2")]
    public partial class DisplayFeatureLayers
    {
        public enum FeatureLayerSource
        {
            ServiceFeatureTable,
            PortalItem,
            Geodatabase,
            Geopackage,
            Shapefile
        }

        public DisplayFeatureLayers()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            // Configure the feature layer selection box.
            FeatureLayerCombo.ItemsSource = Enum.GetValues(typeof(FeatureLayerSource));
            FeatureLayerCombo.SelectedItem = FeatureLayerSource.ServiceFeatureTable;
        }

        private void FeatureLayerCombo_Selected(object sender, RoutedEventArgs e)
        {
            _ = SetFeatureLayerSource();
        }

        private async Task SetFeatureLayerSource()
        {
            try
            {
                // Clear the existing FeatureLayer when a new FeatureLayer is selected.
                MyMapView.Map.OperationalLayers.Clear();

                switch (FeatureLayerCombo.SelectedItem)
                {
                    case FeatureLayerSource.ServiceFeatureTable:
                        await SetServiceFeatureTableFeatureLayer();
                        break;

                    case FeatureLayerSource.PortalItem:
                        await SetPortalItemFeatureLayer();
                        break;

                    case FeatureLayerSource.Geodatabase:
                        await SetGeodatabaseFeatureLayerSource();
                        break;

                    case FeatureLayerSource.Geopackage:
                        await SetGeopackagingFeatureLayer();
                        break;

                    case FeatureLayerSource.Shapefile:
                        await SetShapefileFeatureLayer();
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        #region ServiceFeatureTable

        private async Task SetServiceFeatureTableFeatureLayer()
        {
            // Handle the login to the feature service.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleServer7User = "viewer01";
                    string sampleServer7Pass = "I68VGU^nMurF";
                    return await AccessTokenCredential.CreateAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            // Set the viewpoint.
            await MyMapView.SetViewpointAsync(new Viewpoint(41.773519, -88.143104, 4e3));

            // Create uri for a given feature service.
            Uri serviceUri = new Uri(
                "https://sampleserver7.arcgisonline.com/server/rest/services/DamageAssessment/FeatureServer/0");

            // Create a new FeatureTable from the service uri.
            FeatureTable featureTable = new ServiceFeatureTable(serviceUri);

            // Create a FeatureLayer with the FeatureTable.
            FeatureLayer featureLayer = new FeatureLayer(featureTable);

            // Add the FeatureLayer to the operations layers collection of the map.
            MyMapView.Map.OperationalLayers.Add(featureLayer);

            // Wait for the FeatureLayer to load.
            await featureLayer.LoadAsync();
        }

        #endregion ServiceFeatureTable

        #region Geodatabase

        private async Task SetGeodatabaseFeatureLayerSource()
        {
            // Get the path to the downloaded mobile geodatabase (.geodatabase file).
            string mobileGeodatabaseFilePath = DataManager.GetDataFolder("cb1b20748a9f4d128dad8a87244e3e37", "LA_Trails.geodatabase");

            // Open the mobile geodatabase.
            Geodatabase mobileGeodatabase = await Geodatabase.OpenAsync(mobileGeodatabaseFilePath);

            // Get the 'Trailheads' geodatabase feature table from the mobile geodatabase.
            GeodatabaseFeatureTable trailheadsGeodatabaseFeatureTable = mobileGeodatabase.GetGeodatabaseFeatureTable("Trailheads");

            // Asynchronously load the 'Trailheads' geodatabase feature table.
            await trailheadsGeodatabaseFeatureTable.LoadAsync();

            // Create a FeatureLayer based on the geodatabase feature table.
            FeatureLayer trailheadsFeatureLayer = new FeatureLayer(trailheadsGeodatabaseFeatureTable);

            // Add the FeatureLayer to the operations layers collection of the map.
            MyMapView.Map.OperationalLayers.Add(trailheadsFeatureLayer);

            // Zoom the map to the extent of the FeatureLayer.
            await MyMapView.SetViewpointGeometryAsync(trailheadsFeatureLayer.FullExtent, 50);
        }

        #endregion Geodatabase

        #region Geopackage

        private async Task SetGeopackagingFeatureLayer()
        {
            // Set the viewpoint.
            await MyMapView.SetViewpointAsync(new Viewpoint(39.7294, -104.8319, 5e5));

            // Get the full path.
            string geoPackagePath = DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");

            // Open the GeoPackage.
            GeoPackage myGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);

            // Read the feature tables and get the first one.
            FeatureTable geoPackageTable = myGeoPackage.GeoPackageFeatureTables.FirstOrDefault();

            // Make sure a feature table was found in the package.
            if (geoPackageTable == null) { return; }

            // Create a FeatureLayer to show the FeatureTable.
            FeatureLayer featureLayer = new FeatureLayer(geoPackageTable);
            await featureLayer.LoadAsync();

            // Add the FeatureLayer to the operations layers collection of the map.
            MyMapView.Map.OperationalLayers.Add(featureLayer);
        }

        #endregion Geopackage

        #region PortalItem

        private async Task SetPortalItemFeatureLayer()
        {
            // Set the viewpoint.
            await MyMapView.SetViewpointAsync(new Viewpoint(45.5266, -122.6219, 6000));

            // Create a portal instance.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();

            // Instantiate a PortalItem for a given portal item ID.
            PortalItem portalItem = await PortalItem.CreateAsync(portal, "1759fd3e8a324358a0c58d9a687a8578");

            // Create a FeatureLayer using the PortalItem.
            FeatureLayer featureLayer = new FeatureLayer(portalItem, 0);

            // Add the FeatureLayer to the operations layers collection of the map.
            MyMapView.Map.OperationalLayers.Add(featureLayer);
        }

        #endregion PortalItem

        #region Shapefile

        private async Task SetShapefileFeatureLayer()
        {
            // Get the path to the downloaded shapefile.
            string filepath = DataManager.GetDataFolder("15a7cbd3af1e47cfa5d2c6b93dc44fc2", "ScottishWildlifeTrust_ReserveBoundaries_20201102.shp");

            // Open the shapefile.
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

            // Create a FeatureLayer to display the shapefile.
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

            // Add the FeatureLayer to the operations layers collection of the map.
            MyMapView.Map.OperationalLayers.Add(newFeatureLayer);

            // Set the viewpoint.
            await MyMapView.SetViewpointAsync(new Viewpoint(56.641344, -3.889066, 6e6));
        }

        #endregion Shapefile
    }
}