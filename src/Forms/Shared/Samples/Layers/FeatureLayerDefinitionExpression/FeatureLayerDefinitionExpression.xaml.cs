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
using System;

using Xamarin.Forms;

namespace ArcGISRuntime.Samples.FeatureLayerDefinitionExpression
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Filter by definition expression or display filter",
        category: "Layers",
        description: "Filter features displayed on a map using a definition expression or a display filter.",
        instructions: "Press the apply expression button to limit the features requested from the feature layer to those specified by the SQL query definition expression. This option not only narrows down the results that are drawn, but also removes those features from the layer's attribute table. To filter the results being drawn without modifying the attribute table, hit the button to apply the filter instead. Tap the reset button to remove the definition expression or display filter on the feature layer, which returns all the records.",
        tags: new[] { "SQL", "definition expression", "display filter", "filter", "limit data", "query", "restrict data", "where clause" })]
    public partial class FeatureLayerDefinitionExpression : ContentPage
    {
        //Create and hold reference to the feature layer
        private FeatureLayer _featureLayer;

        public FeatureLayerDefinitionExpression()
        {
            InitializeComponent ();

            //setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create a MapPoint the map should zoom to
            MapPoint mapPoint = new MapPoint(
                -13630484, 4545415, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(mapPoint, 90000);

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Create the uri for the feature service
            Uri featureServiceUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0");

            // Initialize feature table using a url to feature server url
            ServiceFeatureTable featureTable = new ServiceFeatureTable(featureServiceUri);

            // Initialize a new feature layer based on the feature table
            _featureLayer = new FeatureLayer(featureTable);

            //Add the feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);
        }

        private void OnApplyExpressionClicked(object sender, EventArgs e)
        {
            // Adding definition expression to show specific features only
            _featureLayer.DefinitionExpression = "req_Type = 'Tree Maintenance or Damage'";
        }

        private void OnResetButtonClicked(object sender, EventArgs e)
        {
            // Reset the definition expression to see all features again
            _featureLayer.DefinitionExpression = "";
        }
    }
}
