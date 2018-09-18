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
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.FeatureLayerQuery
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer query",
        "Data",
        "This sample demonstrates how to return features from a feature layer using an attribute query on the underlying feature table.",
        "The sample provides a panel with two controls: a text box where you can input the name of a US State, and a button that executes an attribute query using that value. A successful query will select (highlight) the state and zoom the map to its extent.")]
    public partial class FeatureLayerQuery
    {
        // Create reference to service of US States
        private string _statesUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2";

        // Create globally available feature table for easy referencing
        private ServiceFeatureTable _featureTable;

        // Create globally available feature layer for easy referencing
        private FeatureLayer _featureLayer;

        public FeatureLayerQuery()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map location
            MapPoint initialLocation = new MapPoint(
                -11000000, 5000000, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 100000000);

            // Create feature table using a url
            _featureTable = new ServiceFeatureTable(new Uri(_statesUrl));

            // Create feature layer using this feature table
            _featureLayer = new FeatureLayer(_featureTable)
            {

                // Set the Opacity of the Feature Layer
                Opacity = 0.6
            };

            // Create a new renderer for the States Feature Layer.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Transparent, lineSymbol);

            // Set States feature layer renderer
            _featureLayer.Renderer = new SimpleRenderer(fillSymbol);

            // Set the selection color
            MyMapView.SelectionProperties.Color = Color.Cyan;

            // Add feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async Task QueryStateFeature(string stateName)
        {
            try
            {
                // Create a query parameters that will be used to Query the feature table
                QueryParameters queryParams = new QueryParameters();

                // Trim whitespace on the state name to prevent broken queries
                string formattedStateName = stateName.Trim().ToUpper();

                // Construct and assign the where clause that will be used to query the feature table
                queryParams.WhereClause = "upper(STATE_NAME) LIKE '%" + formattedStateName + "%'";

                // Query the feature table
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Cast the QueryResult to a List so the results can be interrogated
                List<Feature> features = queryResult.ToList();

                if (features.Any())
                {
                    // Create an envelope.
                    EnvelopeBuilder envBuilder = new EnvelopeBuilder(SpatialReferences.WebMercator);

                    foreach (Feature feature in features)
                    {
                        // Add the extent of each matching feature to the envelope.
                        envBuilder.UnionOf(feature.Geometry.Extent);

                        // Select each feature.
                        _featureLayer.SelectFeature(feature);
                    }

                    // Zoom to the extent of the selected feature(s).
                    await MyMapView.SetViewpointGeometryAsync(envBuilder.ToGeometry(), 50);
                }
                else
                {
                    MessageDialog message = new MessageDialog("State Not Found!", "Add a valid state name.");
                    await message.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog("Sample error: " + ex, "An error occurred");
                await message.ShowAsync();
            }
        }

        private async void QueryEntry_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                // Remove any previous feature selections that may have been made
                _featureLayer.ClearSelection();

                // Begin query process
                await QueryStateFeature(args.QueryText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}