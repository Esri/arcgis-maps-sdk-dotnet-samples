// Copyright 2018 Esri.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.QueryFeatureCountAndExtent
{
    public partial class QueryFeatureCountAndExtent
    {
        // URL to the feature service
        private Uri _UsaCitiesSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0");
        
        // Statistic definition that returns a count of AREANAME (city names)
        private StatisticDefinition countStatistic = new StatisticDefinition("AREANAME", StatisticType.Count, "pop");

        // Feature table to query
        private ServiceFeatureTable _myFeatureTable;

        public QueryFeatureCountAndExtent()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with a vector street basemap
            Map myMap = new Map(Basemap.CreateStreetsVector());

            // Create the feature table from the service URL
            _myFeatureTable = new ServiceFeatureTable(_UsaCitiesSource);

            // Create the feature layer from the table
            FeatureLayer myFeatureLayer = new FeatureLayer(_myFeatureTable);

            // Add the feature layer to the map
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Wait for the feature layer to load
            await myFeatureLayer.LoadAsync();

            // Set the map initial extent to the extent of the feature layer
            myMap.InitialViewpoint = new Viewpoint(myFeatureLayer.FullExtent);

            // Add the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void btnStateCount_Click(object sender, RoutedEventArgs e)
        {
            // Create the statistics query parameters to control the query
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(new List<StatisticDefinition>() { countStatistic });

            // Limit results to matching states
            statQueryParams.WhereClause = String.Format("upper(ST) LIKE '%{0}%'", txtStateEntry.Text.ToUpper());

            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _myFeatureTable.QueryStatisticsAsync(statQueryParams);

            // Display the results in the UI
            txtResults.Text = statQueryResult.First().Statistics["pop"].ToString();
        }

        private async void btnExtentCount_Click(object sender, RoutedEventArgs e)
        {
            // Create the statistics query parameters, pass in the list of definitions
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(new List<StatisticDefinition>() { countStatistic });

            // Get the current extent (envelope) from the map view
            Envelope currentExtent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

            // Set the statistics query parameters geometry with the envelope
            statQueryParams.Geometry = currentExtent;

            // Set the spatial relationship to Intersects (which is the default)
            statQueryParams.SpatialRelationship = SpatialRelationship.Intersects;

            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _myFeatureTable.QueryStatisticsAsync(statQueryParams);

            // Display the results in the UI
            txtResults.Text = statQueryResult.First().Statistics["pop"].ToString();
        }
    }
}