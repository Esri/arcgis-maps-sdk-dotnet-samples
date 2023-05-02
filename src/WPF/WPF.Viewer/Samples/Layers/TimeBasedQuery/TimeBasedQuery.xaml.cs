// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Windows;

namespace ArcGIS.WPF.Samples.TimeBasedQuery
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Time-based query",
        category: "Layers",
        description: "Query data using a time extent.",
        instructions: "Run the sample, and a subset of records will be displayed on the map.",
        tags: new[] { "query", "time", "time extent" })]
    public partial class TimeBasedQuery
    {
        // Hold a URI pointing to the feature service
        private Uri _serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/0");

        // Hold a reference to the feature table used by the sample
        private ServiceFeatureTable _myFeatureTable;

        public TimeBasedQuery()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create a new map with oceans basemap
            Map myMap = new Map(BasemapStyle.ArcGISOceans);

            // Create feature table for the hurricane feature service
            _myFeatureTable = new ServiceFeatureTable(_serviceUri)
            {
                // Define the request mode
                FeatureRequestMode = FeatureRequestMode.ManualCache
            };

            // When feature table is loaded, populate data
            _myFeatureTable.LoadStatusChanged += OnLoadedPopulateData;

            // Create FeatureLayer that uses the created table
            FeatureLayer myFeatureLayer = new FeatureLayer(_myFeatureTable);

            // Add created layer to the map
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Assign the Map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnLoadedPopulateData(object sender, LoadStatusEventArgs e)
        {
            // If layer isn't loaded, do nothing
            if (e.Status != LoadStatus.Loaded) { return; }

            // Create new query object that contains a basic 'include everything' clause
            QueryParameters queryParameters = new QueryParameters()
            {
                WhereClause = "1=1"
            };

            // Create a new time extent that covers the desired interval (beginning of time to September 16th, 2000)
            TimeExtent myExtent = new TimeExtent(new DateTime(1, 1, 1), new DateTime(2000, 9, 16));

            // Apply the time extent to the query parameters
            queryParameters.TimeExtent = myExtent;

            // Create list of the fields that are returned from the service
            string[] outputFields = { "*" };

            try
            {
                // Populate feature table with the data based on query
                await _myFeatureTable.PopulateFromServiceAsync(queryParameters, true, outputFields);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}