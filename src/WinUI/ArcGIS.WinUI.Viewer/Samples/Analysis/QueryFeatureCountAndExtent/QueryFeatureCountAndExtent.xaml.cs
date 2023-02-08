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
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.QueryFeatureCountAndExtent
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Query feature count and extent",
        category: "Analysis",
        description: "Zoom to features matching a query and count the features in the current visible extent.",
        instructions: "Use the button to zoom to the extent of the state specified (by abbreviation) in the textbox or use the button to count the features in the current extent.",
        tags: new[] { "count", "feature layer", "feature table", "features", "filter", "number", "query" })]
    public partial class QueryFeatureCountAndExtent
    {
        // URL to the feature service.
        private readonly Uri _medicareHospitalSpendLayer =
            new Uri("https://services1.arcgis.com/4yjifSiIG17X0gW4/arcgis/rest/services/Medicare_Hospital_Spending_per_Patient/FeatureServer/0");

        // Key is for the ComboBox and value is for query.
        private readonly Dictionary<string, string> _states = new Dictionary<string, string>()
        {
            {"Alabama","AL"}, {"Alaska","AK"}, {"Arizona","AZ"}, {"Arkansas","AR"}, {"California","CA"}, {"Colorado","CO"},
            {"Connecticut","CT"}, {"DC","DC"}, {"Delaware","DE"}, {"Florida","FL"}, {"Georgia","GA"}, {"Hawaii","HI"}, {"Idaho","ID"},
            {"Illinois","IL"}, {"Indiana","IN"}, {"Iowa","IA"}, {"Kansas","KS"}, {"Kentucky","KY"}, {"Louisiana","LA"}, {"Maine","ME"},
            {"Maryland","MD"}, {"Massachusetts","MA"}, {"Michigan","MI"}, {"Minnesota","MN"}, {"Mississippi","MS"}, {"Missouri","MO"},
            {"Montana","MT"}, {"Nebraska","NE"}, {"Nevada","NV"}, {"New Hampshire","NH"}, {"New Jersey","NJ"}, {"New Mexico","NM"},
            {"New York","NY"}, {"North Carolina","NC"}, {"North Dakota","ND"}, {"Ohio","OH"}, {"Oklahoma","OK"}, {"Oregon","OR"},
            {"Pennsylvania","PA"}, {"Rhode Island","RI"}, {"South Carolina","SC"}, {"South Dakota","SD"}, {"Tennessee","TN"}, {"Texas","TX"},
            {"Utah","UT"}, {"Vermont","VT"}, {"Virginia","VA"}, {"Washington","WA"}, {"West Virginia","WV"}, {"Wisconsin","WI"},
            {"Wyoming","WY"}
        };

        // Feature table to query.
        private ServiceFeatureTable _featureTable;

        public QueryFeatureCountAndExtent()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the map with a basemap.
            Map myMap = new Map(BasemapStyle.ArcGISDarkGray);

            // Create the feature table from the service URL.
            _featureTable = new ServiceFeatureTable(_medicareHospitalSpendLayer);

            // Create the feature layer from the table.
            FeatureLayer myFeatureLayer = new FeatureLayer(_featureTable);

            // Add the feature layer to the map.
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Populate the ComboBox with states.
            foreach (var state in _states) { StatesComboBox.Items.Add(state.Key); }

            try
            {
                // Wait for the feature layer to load.
                await myFeatureLayer.LoadAsync();

                // Set the map initial extent to the extent of the feature layer.
                myMap.InitialViewpoint = new Viewpoint(myFeatureLayer.FullExtent);

                // Add the map to the MapView.
                MyMapView.Map = myMap;
            }
            catch (Exception e)
            {
                await new MessageDialog2(e.ToString(), "Error").ShowAsync();
            }
        }

        private async void ZoomToFeatures(object sender, RoutedEventArgs e)
        {
            // Create the query parameters.
            QueryParameters queryStates = new QueryParameters() { WhereClause = $"upper(State) LIKE '%{_states[StatesComboBox.SelectedItem.ToString()]}%'" };

            try
            {
                // Get the extent from the query.
                Envelope resultExtent = await _featureTable.QueryExtentAsync(queryStates);

                // Return if there is no result (might happen if query is invalid).
                if (resultExtent?.SpatialReference == null)
                {
                    return;
                }

                // Create a viewpoint from the extent.
                Viewpoint resultViewpoint = new Viewpoint(resultExtent);

                // Zoom to the viewpoint.
                await MyMapView.SetViewpointAsync(resultViewpoint);

                // Update the UI.
                Results.Text = $"Zoomed to features in {StatesComboBox.SelectedItem}";
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.ToString(), "Error").ShowAsync();
            }
        }

        private async void CountFeatures(object sender, RoutedEventArgs e)
        {
            // Get the current visible extent.
            Geometry currentExtent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry;

            // Create the query parameters.
            QueryParameters queryCityCount = new QueryParameters
            {
                Geometry = currentExtent,
                // Specify the interpretation of the Geometry query parameters.
                SpatialRelationship = SpatialRelationship.Intersects
            };

            try
            {
                // Get the count of matching features.
                long count = await _featureTable.QueryFeatureCountAsync(queryCityCount);

                // Update the UI.
                Results.Text = $"{count} features in extent";
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.ToString(), "Error").ShowAsync();
            }
        }
    }
}