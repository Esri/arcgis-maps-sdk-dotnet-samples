// Copyright 2016 Esri.
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
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Windows.Samples.FeatureLayerQuery
{
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

            // Call a function that will create a basemap, the US states layer, and load a new map into the map view
            InitializeMap();
        }

        private void InitializeMap()
        {
            // Create a new Map with the Topographic basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Set an initial map location that shows the entire United States
            MapPoint initialLocation = new MapPoint(-11000000, 5000000, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 100000000);

            // Create a feature table using the US states service endpoint url
            _featureTable = new ServiceFeatureTable(new Uri(_statesUrl));
            
            // Create a feature layer using this feature table
            _featureLayer = new FeatureLayer(_featureTable);

            // Set the Opacity of the Feature Layer so it's semi-transparent
            _featureLayer.Opacity = 0.6;

            // Create an outline and fill symbol for rendering the States layer
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol
            {
                Style =SimpleLineSymbolStyle.Solid,
                Color = Colors.Black,
                Width = 1
            };
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol
            {
                Style = SimpleFillSymbolStyle.Solid,
                Color = Colors.Yellow,
                Outline = lineSymbol
            };
            
            // Apply the fill symbol to the States feature layer using a simple renderer
            _featureLayer.Renderer = new SimpleRenderer(fillSymbol);

            // Define selection symbol properties for the layer (color and line width)
            _featureLayer.SelectionColor = Colors.Red;
            _featureLayer.SelectionWidth = 10;

            // Add the feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;            
        }

        // Handler for the QueryStatesButton button click event
        private void OnQueryClicked(object sender, RoutedEventArgs e)
        {
            // Remove any previous feature selections and messages
            _featureLayer.ClearSelection();
            MessagesTextBlock.Text = string.Empty;

            // Call a function that will query states using the text entered 
            QueryStatesTable(StateNameTextBox.Text);
        }

        // Query features in the US States layer using STATE_NAME attribute values
        private async void QueryStatesTable(string stateName)
        {
            try
            {
                // Create a QueryParameters object to define the query  
                QueryParameters queryParams = new QueryParameters();
                
                // Assign a where clause that finds features with a matching STATE_NAME value
                queryParams.WhereClause = "upper(STATE_NAME) = '" + (stateName.ToUpper().Trim() + "'");

                // Restrict the query results to a single feature
                queryParams.MaxFeatures = 1;
                
                // Query the feature table using the QueryParameters
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Check for a valid feature in the results
                Feature stateResult = queryResult.FirstOrDefault();
                
                // If a result was found, select it in the map and zoom to its extent
                if (stateResult != null)
                {
                    // Select (highlight) the matching state in the layer
                    _featureLayer.SelectFeature(stateResult);

                    // Zoom to the extent of the feature (with 100 pixels of padding around the shape)
                    await MyMapView.SetViewpointGeometryAsync(stateResult.Geometry.Extent, 100);
                }
                else
                {
                    //Inform the user that the query was unsuccessful
                    MessagesTextBlock.Text = stateName + " was not found.";
                }
            }
            catch (Exception ex)
            {
                // Inform the user that an exception was encountered
                var message = new MessageDialog("An error occurred: " + ex.ToString(), "Sample error");
                await message.ShowAsync();
            }
        }          
    }
}