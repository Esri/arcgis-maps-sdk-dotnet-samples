// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.FeatureCollectionLayerFromQuery
{
    [Activity(Label = "FeatureCollectionLayerFromQuery")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature collection layer from query result",
        "Layers",
        "This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.",
        "")]
    public class FeatureCollectionLayerFromQuery : Activity
    {
        // Store the map view displayed in the app
        private MapView _myMapView = new MapView();

        // Service endpoint to query for features
        private const string FeatureLayerUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire/FeatureServer/0";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Add a query result as a feature collection layer";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Create a new map with the oceans basemap and add it to the map view
                Map myMap = new Map(Basemap.CreateOceans());
                _myMapView.Map = myMap;

                // Call a function that will create a new feature collection layer from a service query
                GetFeaturesFromQuery();
            }
            catch (Exception ex)
            {
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage("Unable to create feature collection layer: " + ex.Message);
                alertBuilder.Show();
            }
        }

        private async void GetFeaturesFromQuery()
        {
            // Create a service feature table to get features from
            ServiceFeatureTable featTable = new ServiceFeatureTable(new Uri(FeatureLayerUrl));

            // Create a query to get all features in the table
            QueryParameters queryParams = new QueryParameters();
            queryParams.WhereClause = "1=1";

            // Query the table to get all features
            FeatureQueryResult featureResult = await featTable.QueryFeaturesAsync(queryParams);

            // Create a new feature collection table from the result features
            FeatureCollectionTable collectTable = new FeatureCollectionTable(featureResult);

            // Create a feature collection and add the table
            FeatureCollection featCollection = new FeatureCollection();
            featCollection.Tables.Add(collectTable);

            // Create a layer to display the feature collection, add it to the map's operational layers
            FeatureCollectionLayer featCollectionTable = new FeatureCollectionLayer(featCollection);
            _myMapView.Map.OperationalLayers.Add(featCollectionTable);
        }

        private void CreateLayout()
        {
            // Create a new layout
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}