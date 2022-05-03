// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

#if XAMARIN_ANDROID
using ArcGISRuntime.Droid;
#endif

namespace ArcGISRuntimeXamarin.Samples.IndoorPositioning
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Show device location using indoor positioning",
        category: "Location",
        description: "Show your device's real-time location while inside a building by using signals from indoor positioning beacons.",
        instructions: "When the device is within range of an IPS beacon, toggle \"Show Location\" to change the visibility of the location indicator in the map view. The system will ask for permission to use the device's location if the user has not yet used location services in this app. It will then start the location display with auto-pan mode set to `navigation`.",
        tags: new[] { "BLE", "Bluetooth", "GPS", "IPS", "beacon", "blue dot", "building", "facility", "indoor", "location", "map", "mobile", "navigation", "site", "transmitter" })]
    public partial class IndoorPositioning : ContentPage, IDisposable
    {
        private IndoorsLocationDataSource _indoorsLocationDataSource;

        private int? _currentFloor = null;

        // This data is specific to a building on the Esri campus. Substitute your own data in order to use this sample. Code in the sample may need to be modified to work with other maps.

        #region EsriBuildingData

        private Uri _portalUri = new Uri("https://viennardc.maps.arcgis.com");

        private const string sampleUser = "tester_viennardc";
        private const string samplePass = "password.testing12345";

        private const string ItemId = "89f88764c29b48218366855d7717d266";

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
            // Handle the login to the feature service.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    return await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri, sampleUser, samplePass, info.GenerateTokenOptions);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            try
            {
                // Get bluetooth permission for Android devices.
#if XAMARIN_ANDROID
                bool locationGranted = await MainActivity.Instance.AskForLocationPermission();
                bool bluetoothGranted = await MainActivity.Instance.AskForBluetoothPermission();
                if (!locationGranted || !bluetoothGranted)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Bluetooth and location permissions required for use of indoor positioning.", "OK");
                    return;
                }
#endif

                // Create a portal item for the web map.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(_portalUri, true);
                PortalItem item = await PortalItem.CreateAsync(portal, ItemId);

                // Load the map in the map view.
                MyMapView.Map = new Map(item);
                await MyMapView.Map.LoadAsync();

                // Get the positioning table from the map.
                await Task.WhenAll(MyMapView.Map.Tables.Select(table => table.LoadAsync()));
                FeatureTable positioningTable = MyMapView.Map.Tables.FirstOrDefault(table => table.TableName.Equals(PositioningTableName));
                if (positioningTable == null) return;

                // Get a table of all of the indoor pathways.
                FeatureLayer pathwaysFeatureLayer = MyMapView.Map.OperationalLayers.FirstOrDefault(l => l.Name.Equals(PathwaysLayerName, StringComparison.InvariantCultureIgnoreCase)) as FeatureLayer;
                ArcGISFeatureTable pathwaysTable = pathwaysFeatureLayer.FeatureTable as ArcGISFeatureTable;

                // Get the global id for positioning.
                Field dateCreatedFieldName = positioningTable.Fields.FirstOrDefault(f => f.Name.Equals("DateCreated", StringComparison.InvariantCultureIgnoreCase) || f.Name.Equals("DATE_CREATED", StringComparison.InvariantCultureIgnoreCase));

                QueryParameters queryParameters = new QueryParameters
                {
                    MaxFeatures = 1,
                    // "1=1" will give all the features from the table.
                    WhereClause = "1=1",
                };
                queryParameters.OrderByFields.Add(new OrderBy(dateCreatedFieldName.Name, SortOrder.Descending));

                FeatureQueryResult queryResult = await positioningTable.QueryFeaturesAsync(queryParameters);
                Guid globalID = (Guid)queryResult.First().Attributes["GlobalID"];

                // Create the indoor location data source using the tables and Guid.
                _indoorsLocationDataSource = new IndoorsLocationDataSource(positioningTable, pathwaysTable, globalID);

                MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                MyMapView.LocationDisplay.DataSource = _indoorsLocationDataSource;
                MyMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

                await MyMapView.LocationDisplay.DataSource.StartAsync();
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

            int newFloor = int.Parse(floor.ToString());

            // Check if the new location is on a different floor.
            if (_currentFloor == null || _currentFloor != newFloor)
            {
                _currentFloor = newFloor;

                foreach (Layer layer in MyMapView.Map.OperationalLayers)
                {
                    if (_layerNames.Contains(layer?.Name) && layer is DimensionLayer dimLayer)
                    {
                        // Set the layer definition expression to only show data for the current floor.
                        dimLayer.DefinitionExpression = $"VERTICAL_ORDER = {_currentFloor}";
                    }
                }
            }

            // Set text for satellites or beacons.
            string countText = string.Empty;
            if (positionSource.Equals("GNSS"))
            {
                countText = $"Satellite count: {satCount}";
            }
            else if (positionSource.Equals("BLE"))
            {
                countText = $"Beacon count: {transmitterCount}";
            }

            // Update UI on the main thread.
            Device.BeginInvokeOnMainThread(() =>
            {
                PositioningLabel.Text = $"Floor: {floor}\nPosition-source: {positionSource}\nHorizontal-accuracy: {string.Format("{0:0.##}", loc.HorizontalAccuracy)}m\n{countText}";
            });
        }

        public void Dispose()
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}