// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeMaui.Samples.ToggleBetweenFeatureRequestModes
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Toggle between feature request modes",
        category: "Data",
        description: "Use different feature request modes to populate the map from a service feature table.",
        instructions: "Run the sample and use the radio buttons to change what feature request modes you want to use (the default value is  **on interaction cache**). After you selected which feature request mode to use, tap the `Populate` button to apply the feature request mode. ",
        tags: new[] { "cache", "feature request mode", "performance" })]
    public partial class ToggleBetweenFeatureRequestModes : ContentPage
    {
        private ServiceFeatureTable _treeFeatureTable;
        private FeatureLayer _treeFeatureLayer;

        public ToggleBetweenFeatureRequestModes()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create new Map with basemap.
                Map myMap = new Map(BasemapStyle.ArcGISTopographic);
                MyMapView.Map = myMap;

                // Set intial map location.
                MyMapView.Map.InitialViewpoint = new Viewpoint(45.5266, -122.6219, 6000);

                // Create uri to the used feature service.
                Uri serviceUri = new Uri(
                   "https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/Trees_of_Portland/FeatureServer/0");

                // Create a feature table for the trees of Portland feature service.
                // The feature request mode for this service feature table is OnInteractionCache by default.
                _treeFeatureTable = new ServiceFeatureTable(serviceUri);

                // Create FeatureLayer that uses the created table.
                _treeFeatureLayer = new FeatureLayer(_treeFeatureTable);

                // Add created layer to the map
                MyMapView.Map.OperationalLayers.Add(_treeFeatureLayer);

                // Creates a list for the items in the pickers.
                List<string> cacheModeName = new List<string>();

                cacheModeName.Add("Cache");
                cacheModeName.Add("No Cache");
                cacheModeName.Add("Manual Cache");

                // Adds the list for the picker.
                CacheModes.ItemsSource = cacheModeName;

                CacheModes.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void CacheSelectionChanged(object sender, EventArgs e)
        {
            string currentPick = CacheModes.SelectedItem.ToString();
            switch (currentPick)
            {
                // Populates the map with server feature table request mode OnInteractionCache.
                // Features are requested automatically for the visible extent. If the area is visited again, the features won't be requested again.
                case "Cache":
                    _treeFeatureTable.FeatureRequestMode = FeatureRequestMode.OnInteractionCache;

                    // Disable populate map button used for manual cache.
                    PopulateMap.IsEnabled = false;
                    break;

                // Populates the map with server feature table request mode OnInteractionNoCache.
                // Features are downloaded for the visible extent. If the area is visited again, the cache will be populated with the latest data.
                case "No Cache":
                    _treeFeatureTable.FeatureRequestMode = FeatureRequestMode.OnInteractionNoCache;

                    // Disable populate map button used for manual cache.
                    PopulateMap.IsEnabled = false;
                    break;

                // Populates the map with server feature table request mode ManualCache.
                // Features are never automatically populated from the services. All features are loaded manually using PopulateFromServiceAsync.
                case "Manual Cache":
                    _treeFeatureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                    // Enable populate map button used for manual cache.
                    PopulateMap.IsEnabled = true;
                    break;

                default:
                    break;
            }
        }

        // Use this method for manual cache.
        private async Task FetchCacheManually()
        {
            // Create new query object that contains parameters to query specific request types.
            QueryParameters queryParameters = new QueryParameters()
            {
                Geometry = MyMapView.VisibleArea
            };

            // Create list of the fields that are returned from the service.
            // Using "*" will return all fields. This can be replaced to return certain fields.
            string[] outputFields = { "*" };

            try
            {
                // Populate feature table with the data based on query.
                await _treeFeatureTable.PopulateFromServiceAsync(queryParameters, true, outputFields);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void PopulateButtonClick(object sender, EventArgs e)
        {
            _ = FetchCacheManually();
        }
    }
}