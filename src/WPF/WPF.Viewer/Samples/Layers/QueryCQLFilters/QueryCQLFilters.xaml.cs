// Copyright 2021 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Color = System.Drawing.Color;

namespace ArcGIS.WPF.Samples.QueryCQLFilters
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Query with CQL filters",
        category: "Layers",
        description: "Query data from an OGC API feature service using CQL filters.",
        instructions: "Enter a CQL query. Press the \"Apply query\" button to see the query applied to the OGC API features shown on the map.",
        tags: new[] { "CQL", "OGC", "OGC API", "browse", "catalog", "common query language", "feature table", "filter", "query", "service", "web" })]
    public partial class QueryCQLFilters
    {
        private IReadOnlyList<string> DefaultWhereClause = new[]
        {
            // Sample Query 1: Query for features with an F_CODE attribute property of "AP010".
            "{ \"op\": \"=\", \"args\": [ { \"property\": \"F_CODE\" }, \"AP010\" ] }", // cql2-json query

            // Sample Query 2: Query for features with an F_CODE attribute property similar to "AQ".
            "F_CODE LIKE 'AQ%'", // cql-text query

            // Sample Query 3: use cql2-json to combine "before" and "eq" operators with the logical "and" operator.
           "{\"op\": \"and\", \"args\":[{ \"op\": \"=\", \"args\":[{ \"property\" : \"F_CODE\" }, \"AP010\"]}, { \"op\": \"t_before\", \"args\":[{ \"property\" : \"ZI001_SDV\"},\"2013-01-01\"]}]}"
        };

        // Hold a reference to the OGC feature collection table.
        private OgcFeatureCollectionTable _featureTable;

        // Constants for the service URL and collection id.
        private const string ServiceUrl = "https://demo.ldproxy.net/daraa";

        // Note that the service defines the collection id which can be accessed via OgcFeatureCollectionInfo.CollectionId.
        private const string CollectionId = "TransportationGroundCrv";

        public QueryCQLFilters()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Populate the UI.
            WhereClauseBox.ItemsSource = DefaultWhereClause;
            MaxFeaturesBox.Text = "3000";
            StartDatePicker.SelectedDate = new DateTime(2011, 6, 13);
            EndDatePicker.SelectedDate = new DateTime(2012, 1, 7);

            // Create the map with topographic basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            try
            {
                LoadingProgressBar.Visibility = Visibility.Visible;

                // Create the feature table from URI and collection id.
                _featureTable = new OgcFeatureCollectionTable(new Uri(ServiceUrl), CollectionId);

                // Set the feature request mode to manual (only manual is currently supported).
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
                _featureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Load the table.
                await _featureTable.LoadAsync();

                // Populate the OGC feature collection table.
                QueryParameters queryParamaters = new QueryParameters();
                queryParamaters.MaxFeatures = 3000;
                await _featureTable.PopulateFromServiceAsync(queryParamaters, false, null);

                // Create a feature layer from the OGC feature collection
                // table to visualize the OAFeat features.
                FeatureLayer ogcFeatureLayer = new FeatureLayer(_featureTable);

                // Set a renderer for the layer.
                ogcFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 1.5));

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(ogcFeatureLayer);

                // Zoom to the extent of the feature layer/table.
                Envelope tableExtent = _featureTable.Extent;
                if (tableExtent != null && !tableExtent.IsEmpty)
                {
                    await MyMapView.SetViewpointGeometryAsync(tableExtent, 20);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading/populating service");
            }
            finally
            {
                // Update the UI.
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        public QueryParameters CreateQueryParameters()
        {
            // Create new query parameters.
            var queryParameters = new QueryParameters();

            // Assign the where clause if one is provided.
            if (!string.IsNullOrEmpty(WhereClauseBox.Text)) queryParameters.WhereClause = WhereClauseBox.Text;

            // Set the MaxFeatures property to MaxFeaturesBox content.
            if (int.TryParse(MaxFeaturesBox.Text, out int parsedMaxFeatures))
                queryParameters.MaxFeatures = parsedMaxFeatures;

            // Set user date times if provided.
            if (DateBox.IsChecked == true)
            {
                DateTime startDate = (StartDatePicker.SelectedDate is DateTime userStart) ? userStart : DateTime.MinValue;
                DateTime endDate = (EndDatePicker.SelectedDate is DateTime userEnd) ? userEnd : new DateTime(9999, 12, 31);

                // Use the newly created startDate and endDate to create the TimeExtent.
                queryParameters.TimeExtent = new Esri.ArcGISRuntime.TimeExtent(startDate, endDate);
            }

            return queryParameters;
        }

        private async void ApplyQuery_Click(object sender, RoutedEventArgs e)
        {
            if (_featureTable.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                return;
            }

            try
            {
                LoadingProgressBar.Visibility = Visibility.Visible;

                // Set queryParameters to the user's input
                var queryParameters = CreateQueryParameters();

                // Populate the table with the query, clearing the exist content of the table.
                // Setting outFields to null requests all fields.
                var result = await _featureTable.PopulateFromServiceAsync(queryParameters, true, outFields: null);

                // Report the number of returned features by the query
                NumberOfReturnedFeatures.Content = $"Query returned {result.Count()} features.";

                // Zoom to the extent of the returned features.
                Envelope tableExtent = GeometryEngine.CombineExtents(result.Select(feature => feature.Geometry));
                if (tableExtent != null && !tableExtent.IsEmpty)
                {
                    await MyMapView.SetViewpointGeometryAsync(tableExtent, 20);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error querying/populating service");
            }
            finally
            {
                // Update the UI.
                LoadingProgressBar.Visibility = Visibility.Hidden;
            }
        }

        private void DateBox_Checked(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker != null && EndDatePicker != null)
            {
                StartDatePicker.IsEnabled = EndDatePicker.IsEnabled = DateBox.IsChecked == true;
            }
        }
    }
}