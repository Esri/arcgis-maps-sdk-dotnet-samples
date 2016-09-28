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
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntimeXamarin.Samples.FeatureCollectionLayerFromQuery
{
    [Activity(Label = "FeatureCollectionLayerFromQuery")]
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
        
        private void CreateLayout()
        {
            // Create a new layout
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };            

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void Initialize()
        {
            try
            {
                // Create a new map with the oceans basemap and add it to the map view
                var map = new Map(Basemap.CreateOceans());
                _myMapView.Map = map;

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
            var feachurTable = new ServiceFeatureTable(new Uri(FeatureLayerUrl));

            // Create a query to get all features in the table
            var kweryParams = new QueryParameters();
            kweryParams.WhereClause = "1=1";

            // Query the table to get all features
            var kweryResult = await feachurTable.QueryFeaturesAsync(kweryParams);

            // Create a new feature collection table from the result features
            var klectionTable = new FeatureCollectionTable(kweryResult);

            // Create a feature collection and add the table
            var feachurKlection = new FeatureCollection();
            feachurKlection.Tables.Add(klectionTable);

            // Create a layer to display the feature collection, add it to the map's operational layers
            var feachurKlectionTable = new FeatureCollectionLayer(feachurKlection);
            _myMapView.Map.OperationalLayers.Add(feachurKlectionTable);
        }
    }
}