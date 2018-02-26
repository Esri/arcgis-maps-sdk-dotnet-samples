// Copyright 2016 Esri.
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
using Windows.UI.Popups;

namespace ArcGISRuntime.UWP.Samples.FeatureCollectionLayerFromQuery
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature collection layer from query result",
        "Layers",
        "This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.",
        "")]
    public partial class FeatureCollectionLayerFromQuery
    {
        private const string FeatureLayerUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire/FeatureServer/0";

        public FeatureCollectionLayerFromQuery()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Create a new map with the oceans basemap and add it to the map view
                var map = new Map(Basemap.CreateOceans());
                MyMapView.Map = map;

                // Call a function that will create a new feature collection layer from a service query
                GetFeaturesFromQuery();
            }
            catch (Exception ex)
            {
                var messageDlg = new MessageDialog("Unable to create feature collection layer: " + ex.Message, "Error");
                await messageDlg.ShowAsync();
            }
        }

        private async void GetFeaturesFromQuery()
        {
            // Create a service feature table to get features from
            var featTable = new ServiceFeatureTable(new Uri(FeatureLayerUrl));

            // Create a query to get all features in the table
            var queryParams = new QueryParameters();
            queryParams.WhereClause = "1=1";

            // Query the table to get all features
            var featureResult = await featTable.QueryFeaturesAsync(queryParams);

            // Create a new feature collection table from the result features
            var collectTable = new FeatureCollectionTable(featureResult);

            // Create a feature collection and add the table
            var featCollection = new FeatureCollection();
            featCollection.Tables.Add(collectTable);

            // Create a layer to display the feature collection, add it to the map's operational layers
            var featCollectionTable = new FeatureCollectionLayer(featCollection);
            MyMapView.Map.OperationalLayers.Add(featCollectionTable);
        }
    }
}
