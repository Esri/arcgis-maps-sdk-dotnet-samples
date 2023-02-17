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
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.QueryFeatureCountAndExtent
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Query feature count and extent",
        category: "Analysis",
        description: "Zoom to features matching a query and count the features in the current visible extent.",
        instructions: "Use the picker to zoom to the extent of the state specified. Use the button to count the features in the current extent.",
        tags: new[] { "count", "feature layer", "feature table", "features", "filter", "number", "query" })]
    public partial class QueryFeatureCountAndExtent
    {
        // URL to the feature service.
        private readonly Uri _medicareHospitalSpendLayer =
            new Uri("https://services1.arcgis.com/4yjifSiIG17X0gW4/arcgis/rest/services/Medicare_Hospital_Spending_per_Patient/FeatureServer/0");

        // Key is for ComboBox and value is for query.
        private readonly Dictionary<string, string> _states = new Dictionary<string, string>()
        {
            {"Alabama","AL"}, {"Alaska","AK"}, {"Arizona","AZ"}, {"Arkansas","AR"}, {"California","CA"}, {"Colorado","CO"},
            {"Connecticut","CT"}, {"Delaware","DE"}, {"District of Columbia","DC"}, {"Florida","FL"}, {"Georgia","GA"}, {"Hawaii","HI"}, {"Idaho","ID"},
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
            // Populate the ComboBox with states.
            StatesComboBox.ItemsSource = _states.Keys;

            // Create the map with a basemap.
            Map myMap = new Map(BasemapStyle.ArcGISDarkGray);

            // Create the feature table from the service URL.
            _featureTable = new ServiceFeatureTable(_medicareHospitalSpendLayer);

            // Create the feature layer from the table.
            FeatureLayer myFeatureLayer = new FeatureLayer(_featureTable);

            // Add the feature layer to the map.
            myMap.OperationalLayers.Add(myFeatureLayer);

            try
            {
                // Wait for the feature layer and spatial reference to load.
                await myFeatureLayer.LoadAsync();
                await myMap.LoadAsync();

                // Set the map initial extent to the US.
                myMap.InitialViewpoint = new Viewpoint(new MapPoint(-10900000, 4900000, SpatialReferences.WebMercator), 25000000);

                // Add the map to the MapView.
                MyMapView.Map = myMap;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private async void StatesComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the abbreviation of the selected state to match the dataset's State field.
                string stateAbbreviation = _states[StatesComboBox.SelectedItem.ToString()];

                // Create the query parameters.
                QueryParameters queryStates = new QueryParameters { WhereClause = $"State LIKE '%{stateAbbreviation}%'" };

                // Get the extent from the query.
                Envelope resultExtent = await _featureTable.QueryExtentAsync(queryStates);

                // Create a viewpoint from the extent.
                Viewpoint resultViewpoint = new Viewpoint(resultExtent);

                // Zoom to the viewpoint.
                await MyMapView.SetViewpointAsync(resultViewpoint);

                // Update the UI.
                Results.Text = $"Zoomed to features in {StatesComboBox.SelectedItem}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private async void CountFeaturesButton_Click(object sender, RoutedEventArgs e)
        {
            try
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

                // Get the count of matching features.
                long count = await _featureTable.QueryFeatureCountAsync(queryCityCount);

                // Update the UI.
                Results.Text = $"{count} features in extent";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}