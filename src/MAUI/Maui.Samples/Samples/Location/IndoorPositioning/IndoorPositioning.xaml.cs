// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISMapsSDKMaui.Samples.Maui;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Location = Esri.ArcGISRuntime.Location.Location;
using Map = Esri.ArcGISRuntime.Mapping.Map;


#if __ANDROID__
//TODO
//using ArcGISRuntime.Droid;
#endif

namespace ArcGISMapsSDK.Samples.IndoorPositioning
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Show device location using indoor positioning",
        category: "Location",
        description: "Show your device's real-time location while inside a building by using signals from indoor positioning beacons.",
        instructions: "Bring the device within range of an IPS beacon. The system will ask for permission to use the device's location if the user has not yet used location services in this app. It will then start the location display with auto-pan mode set to `Navigation`.",
        tags: new[] { "BLE", "Bluetooth", "GPS", "IPS", "beacon", "blue dot", "building", "facility", "indoor", "location", "map", "mobile", "navigation", "site", "transmitter" })]
    public partial class IndoorPositioning : ContentPage, IDisposable
    {
        private IndoorsLocationDataSource _indoorsLocationDataSource;

        private int? _currentFloor = null;

        // This data is specific to a building on the Esri campus. Substitute your own data in order to use this sample. Code in the sample may need to be modified to work with other maps.

        #region EsriBuildingData

        private Uri _portalUri = new Uri("https://www.arcgis.com/");

        private const string ItemId = "8fa941613b4b4b2b8a34ad4cdc3e4bba";

        private const string PositioningTableName = "ips_positioning";
        private const string PathwaysLayerName = "pathways";

        private string[] _layerNames = new string[] { "Details", "Units", "Levels" };

        #endregion EsriBuildingData

        public IndoorPositioning()
        {
            InitializeComponent();
            _ = Initialize();
        }
        private async Task Initialize()
        {
            try
            {
                PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    throw new Exception("Location permission required for use of indoor positioning.");
                }

#if __ANDROID__
                // Get bluetooth permission for Android devices. Devices running Android 12 or higher need the `BluetoothScan` permission. Android 11 and below require the `Bluetooth` and `BluetoothAdmin` permissions.
                //TODO
                //bool bluetoothScanGranted = await MainActivity.Instance.AskForBluetoothPermission();
                //if (!bluetoothScanGranted)
                //{
                //    throw new Exception("Bluetooth permission is required for use of indoor positioning.");
                //}
#endif
                PositioningLabel.Text = "Loading map";

                // Create a portal item for the web map.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(_portalUri, false);
                PortalItem item = await PortalItem.CreateAsync(portal, ItemId);

                // Load the map in the map view.
                MyMapView.Map = new Map(item);
                await MyMapView.Map.LoadAsync();

                PositioningLabel.Text = "Creating indoors location data source";

                // Get the positioning table from the map.
                await Task.WhenAll(MyMapView.Map.Tables.Select(table => table.LoadAsync()));
                FeatureTable positioningTable = MyMapView.Map.Tables.Single(table => table.TableName.Equals(PositioningTableName));

                // Get a table of all of the indoor pathways.
                FeatureLayer pathwaysFeatureLayer = MyMapView.Map.OperationalLayers.OfType<FeatureLayer>().Single(l => l.Name.Equals(PathwaysLayerName, StringComparison.InvariantCultureIgnoreCase));
                ArcGISFeatureTable pathwaysTable = pathwaysFeatureLayer.FeatureTable as ArcGISFeatureTable;

                // Get the global ID for positioning. The ID identifies a specific row in the feature table that should be used for setting up IPS. We use this ID to construct the data source.
                Field dateCreatedFieldName = positioningTable.Fields.Single(f => f.Name.Equals("DateCreated", StringComparison.InvariantCultureIgnoreCase) || f.Name.Equals("DATE_CREATED", StringComparison.InvariantCultureIgnoreCase));

                // Create a query for the latest created feature. This is needed for correctly constructing the IPS used in this sample. There is a constructor without an ID parameter that will attempt to automatically find the latest row.
                QueryParameters queryParameters = new QueryParameters
                {
                    MaxFeatures = 1,
                    // "1=1" will give all the features from the table.
                    WhereClause = "1=1",
                };
                queryParameters.OrderByFields.Add(new OrderBy(dateCreatedFieldName.Name, SortOrder.Descending));

                FeatureQueryResult queryResult = await positioningTable.QueryFeaturesAsync(queryParameters);
                Guid globalID = (Guid)queryResult.Single().Attributes["GlobalID"];

                // Create the indoor location data source using the tables and Guid.
                _indoorsLocationDataSource = new IndoorsLocationDataSource(positioningTable, pathwaysTable, globalID);

                PositioningLabel.Text = "Starting IPS";

                MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                MyMapView.LocationDisplay.DataSource = _indoorsLocationDataSource;
                _indoorsLocationDataSource.LocationChanged += LocationDisplay_LocationChanged;

                await MyMapView.LocationDisplay.DataSource.StartAsync();

                PositioningLabel.Text = "Waiting for location";
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }

        private void LocationDisplay_LocationChanged(object sender, Location loc)
        {
            // Get the properties from the new location.
            IReadOnlyDictionary<string, object> locationProperties = loc.AdditionalSourceProperties;
            locationProperties.TryGetValue(LocationSourcePropertyKeys.Floor, out object floor);
            locationProperties.TryGetValue(LocationSourcePropertyKeys.PositionSource, out object positionSource);
            locationProperties.TryGetValue(LocationSourcePropertyKeys.SatelliteCount, out object satCount);
            locationProperties.TryGetValue("transmitterCount", out object transmitterCount);

            // Handle any floor changes.
            if (floor != null)
            {
                int newFloor = int.Parse(floor.ToString());

                // Check if the new location is on a different floor.
                if (_currentFloor == null || _currentFloor != newFloor)
                {
                    _currentFloor = newFloor;

                    // Loop through dimension layers specified for this data set.
                    foreach (DimensionLayer dimLayer in MyMapView.Map.OperationalLayers.OfType<DimensionLayer>().Where(layer => _layerNames.Contains(layer.Name)))
                    {
                        // Set the layer definition expression to only show data for the current floor.
                        dimLayer.DefinitionExpression = $"VERTICAL_ORDER = {_currentFloor}";
                    }
                }
            }

            // Create the UI label with information about the updated location.
            string labelText = string.Empty;
            if (_currentFloor != null) labelText += $"Floor: {_currentFloor}\n";
            if (positionSource != null) labelText += $"Position-source: {positionSource}\n";
            if (satCount != null) labelText += $"Satellite count: {satCount}\n";
            if (transmitterCount != null) labelText += $"Beacon count: {transmitterCount}\n";
            labelText += $"Horizontal accuracy: {string.Format("{0:0.##}", loc.HorizontalAccuracy)}m";

            // Update UI on the main thread.
            Device.BeginInvokeOnMainThread(() =>
            {
                PositioningLabel.Text = labelText;
            });
        }

        public void Dispose()
        {
            // Stop the location data source.
            _ = MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}