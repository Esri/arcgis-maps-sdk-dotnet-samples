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
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.AxHost;

namespace ArcGIS.WPF.Samples.QueryFeatureCountAndExtent
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

        // Feature table to query.
        private ServiceFeatureTable _featureTable;

        string[] states = { "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "DC", "Delaware",
                "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland",
                "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
                "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania",
                "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington",
                "West Virginia", "Wisconsin", "Wyoming" };

        private string[] states_abbreviated = { "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "HI", "ID", "IL",
                "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC",
                "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY" };

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
                MessageBox.Show(e.ToString(), "Error");
            }

            // Populate the ComboBox with states.
            foreach (var state in states) { StateComboBox.Items.Add(state); }
        }

        private async void ZoomToFeatures(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create the query parameters.
                QueryParameters queryStates = new QueryParameters { WhereClause = $"upper(State) LIKE '%{states_abbreviated[StateComboBox.SelectedIndex]}%'" };

                // Get the extent from the query.
                Envelope resultExtent = await _featureTable.QueryExtentAsync(queryStates);

                // Create a viewpoint from the extent.
                Viewpoint resultViewpoint = new Viewpoint(resultExtent);

                // Zoom to the viewpoint.
                await MyMapView.SetViewpointAsync(resultViewpoint);

                // Update the UI.
                ResultsTextbox.Text = $"Zoomed to features in {StateComboBox.SelectedItem.ToString()}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private async void BtnCountFeaturesClick(object sender, RoutedEventArgs e)
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
                ResultsTextbox.Text = $"{count} features in extent";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}