﻿// Copyright 2018 Esri.
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
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.QueryFeatureCountAndExtent
{
    public partial class QueryFeatureCountAndExtent
    {
        // URL to the feature service
        private readonly Uri _usaCitiesSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0");

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
            _myFeatureTable = new ServiceFeatureTable(_usaCitiesSource);

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

        private async void BtnZoomToFeaturesClick(object sender, RoutedEventArgs e)
        {
            // Create the query parameters
            QueryParameters queryStates = new QueryParameters() { WhereClause = String.Format("upper(ST) LIKE '%{0}%'", txtStateEntry.Text.ToUpper()) };

            // Get the extent from the query
            Envelope resultExtent = await _myFeatureTable.QueryExtentAsync(queryStates);

            // Return if there is no result (might happen if query is invalid)
            if (resultExtent == null || resultExtent.SpatialReference == null)
            {
                return;
            }

            // Create a viewpoint from the extent
            Viewpoint resultViewpoint = new Viewpoint(resultExtent);

            // Zoom to the viewpoint
            await MyMapView.SetViewpointAsync(resultViewpoint);

            // Update the UI
            txtResults.Text = String.Format("Zoomed to features in {0}", txtStateEntry.Text);
        }

        private async void BtnCountFeaturesClick(object sender, RoutedEventArgs e)
        {
            // Create the query parameters
            QueryParameters queryCityCount = new QueryParameters
            {
                // Get the current view extent and use that as a query parameters
                Geometry = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry,
                // Specify the interpretation of the Geometry query parameters
                SpatialRelationship = SpatialRelationship.Intersects
            };

            // Get the count of matching features
            long count = await _myFeatureTable.QueryFeatureCountAsync(queryCityCount);

            // Update the UI
            txtResults.Text = String.Format("{0} features in extent", count);
        }
    }
}