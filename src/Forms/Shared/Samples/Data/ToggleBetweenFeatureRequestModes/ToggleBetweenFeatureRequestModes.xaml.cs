﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System.Threading.Tasks;
using Xamarin.Forms;
using System;
using System.Collections.Generic;

namespace ArcGISRuntimeXamarin.Samples.ToggleBetweenFeatureRequestModes
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
            Initialize();
        }

        private async void Initialize()
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
                _treeFeatureTable = new ServiceFeatureTable(serviceUri)
                {
                    FeatureRequestMode = FeatureRequestMode.OnInteractionCache
                };

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
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        // Use this method for manual cache.
        private async void FetchCacheManually()
        {
            // Create new query object that contains parameters to query specific request types.
            QueryParameters queryParameters = new QueryParameters()
            {
                WhereClause = "Condition < '4'",
                Geometry = MyMapView.VisibleArea
            };

            // Create list of the fields that are returned from the service.
            string[] outputFields = { "*" };

            try
            {
                // Populate feature table with the data based on query.
                await _treeFeatureTable.PopulateFromServiceAsync(queryParameters, true, outputFields);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void PopulateButtonClick(object sender, EventArgs e)
        {
            string currentPick = CacheModes.SelectedItem.ToString();
            switch (currentPick)
            {
                //Populates the map with server feature table request mode cache.
                case "Cache":
                    _treeFeatureTable.FeatureRequestMode = FeatureRequestMode.OnInteractionCache;
                    break;

                // Populates the map with server feature table request mode no cache.
                case "No Cache":
                    _treeFeatureTable.FeatureRequestMode = FeatureRequestMode.OnInteractionNoCache;
                    break;

                // Populates the map with server feature table request mode manual cache.
                case "Manual Cache":
                    _treeFeatureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;
                    FetchCacheManually();
                    break;

                default:
                    break;
            }
        }
    }
}