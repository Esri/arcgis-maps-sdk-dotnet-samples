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

namespace ArcGISRuntime.UWP.Samples.QueryFeatureCountAndExtent
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Query feature count and extent",
        "Analysis",
        "This sample demonstrates how to query a feature table, in this case returning a count, for features that are within the visible extent or that meet specified criteria.",
        "Use the button to zoom to the extent of the state specified (by abbreviation) in the textbox or use the button to count the features in the current extent.")]
    public partial class QueryFeatureCountAndExtent
    {
        // URL to the feature service
        private readonly Uri _usaCitiesSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0");

        // Feature table to query
        private ServiceFeatureTable _myFeatureTable;

        public QueryFeatureCountAndExtent()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
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

        private async void BtnZoomToFeaturesClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Create the query parameters
            QueryParameters queryStates = new QueryParameters() { WhereClause = $"upper(ST) LIKE '%{txtStateEntry.Text.ToUpper()}%'" };

            // Get the extent from the query
            Envelope resultExtent = await _myFeatureTable.QueryExtentAsync(queryStates);

            // Return if there is no result (might happen if query is invalid)
            if (resultExtent?.SpatialReference == null)
            {
                return;
            }

            // Create a viewpoint from the extent
            Viewpoint resultViewpoint = new Viewpoint(resultExtent);

            // Zoom to the viewpoint
            await MyMapView.SetViewpointAsync(resultViewpoint);

            // Update the UI
            txtResults.Text = $"Zoomed to features in {txtStateEntry.Text}";
        }

        private async void BtnCountFeaturesClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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
            txtResults.Text = $"{count} features in extent";
        }
    }
}