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
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature collection layer (query)",
        category: "Layers",
        description: "Create a feature collection layer to show a query result from a service feature table.",
        instructions: "When launched, this sample displays a map with point features as a feature collection layer. Pan and zoom to explore the map.",
        tags: new[] { "layer", "query", "search", "table" })]
    public class FeatureCollectionLayerFromQuery : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        // Service endpoint to query for features
        private const string FeatureLayerUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire/FeatureServer/0";

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
            // Create a new map with the oceans basemap and add it to the map view
            Map myMap = new Map(Basemap.CreateOceans());
            _myMapView.Map = myMap;

            // Call a function that will create a new feature collection layer from a service query
            GetFeaturesFromQuery();
        }

        private async void GetFeaturesFromQuery()
        {
            try
            {
                // Create a service feature table to get features from
                ServiceFeatureTable featTable = new ServiceFeatureTable(new Uri(FeatureLayerUrl));

                // Create a query to get all features in the table
                QueryParameters queryParams = new QueryParameters
                {
                    WhereClause = "1=1"
                };

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
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new layout
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}