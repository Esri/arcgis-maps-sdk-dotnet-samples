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

namespace ArcGISRuntimeXamarin.Samples.IndoorPositioning
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Show device location using indoor positioning",
        category: "Location",
        description: "Show your device's real-time location while inside a building by using signals from indoor positioning beacons.",
        instructions: "When the device is within range of an IPS beacon, toggle \"Show Location\" to change the visibility of the location indicator in the map view. The system will ask for permission to use the device's location if the user has not yet used location services in this app. It will then start the location display with auto-pan mode set to `navigation`.",
        tags: new[] { "BLE", "Bluetooth", "GPS", "IPS", "beacon", "blue dot", "building", "facility", "indoor", "location", "map", "mobile", "navigation", "site", "transmitter" })]
    public partial class IndoorPositioning : ContentPage
    {
        private IndoorsLocationDataSource _indoorsLocationDataSource;

        private int? _currentFloor = null;

        public IndoorPositioning()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            Uri uri = new Uri("https://viennardc.maps.arcgis.com");

            // Handle the login to the feature service.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleUser = "tester_viennardc";
                    string samplePass = "password.testing12345";
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
                // Create a oirtak uten for the web map.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(uri, true);
                PortalItem item = await PortalItem.CreateAsync(portal, "89f88764c29b48218366855d7717d266");

                // Load the map in the map view.
                MyMapView.Map = new Map(item);
                await MyMapView.Map.LoadAsync();

                // Get the positioning table from the map.
                FeatureTable positioningTable = null;
                foreach (FeatureTable table in MyMapView.Map.Tables)
                {
                    await table.LoadAsync();
                    if (table.TableName.Equals("ips_positioning"))
                    {
                        positioningTable = table;
                    }
                }
                if (positioningTable == null) return;

                // Get a table of all of the indoor pathways.
                FeatureLayer pathwaysFeatureLayer = MyMapView.Map.OperationalLayers.FirstOrDefault(l => l.Name.Equals("pathways", StringComparison.InvariantCultureIgnoreCase)) as FeatureLayer;
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

                // Create the indoor location data source using the tables nad Guid
                _indoorsLocationDataSource = new IndoorsLocationDataSource(positioningTable, pathwaysTable, globalID);

                MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                MyMapView.LocationDisplay.DataSource = _indoorsLocationDataSource;
                MyMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void LocationDisplay_LocationChanged(object sender, Location e)
        {
            IReadOnlyDictionary<string, object> locationProperties = e.AdditionalSourceProperties;

            string floor = locationProperties[LocationSourcePropertyKeys.Floor].ToString();
            string positionSource = locationProperties[LocationSourcePropertyKeys.PositionSource].ToString();
            string networkCount = locationProperties[LocationSourcePropertyKeys.SatelliteCount].ToString();

            int newFloor = int.Parse(floor);

            if (_currentFloor == null || _currentFloor != newFloor)
            {
                _currentFloor = newFloor;

                foreach (DimensionLayer layer in MyMapView.Map.OperationalLayers)
                {
                    if (layer?.Name == "Details" || layer?.Name == "Units" || layer?.Name == "Levels")
                    {
                        layer.DefinitionExpression = $"VERTICAL_ORDER = {_currentFloor}";
                    }
                }
            }

            PositioningLabel.Text = $"Floor: {floor}, Position-source: {positionSource}, Horizontal-accuracy: {e.HorizontalAccuracy}m, Satellite-count: {networkCount}";
        }
    }
}