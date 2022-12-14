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
using System.Drawing;
using System.Windows;

namespace ArcGIS.WPF.Samples.ChangeFeatureLayerRenderer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Change feature layer renderer",
        category: "Layers",
        description: "Change the appearance of a feature layer with a renderer.",
        instructions: "Use the button in the control panel to change the renderer on the feature layer. The original renderer displays orange circles, the diameters of which are proportional to carbon storage of each tree. When the blue renderer in this sample is applied, it displays the location of the trees simply as blue points.",
        tags: new[] { "feature layer", "renderer", "visualization" })]
    public partial class ChangeFeatureLayerRenderer
    {
        // Create and hold reference to the feature layer
        private FeatureLayer _featureLayer;

        public ChangeFeatureLayerRenderer()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create and set initial map area
            Envelope initialLocation = new Envelope(
                -9177811, 4247000, -9176791, 4247784,
                SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(initialLocation);

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Create uri to the used feature service
            Uri serviceUri = new Uri("https://services.arcgis.com/V6ZHFr6zdgNZuVG0/arcgis/rest/services/Landscape_Trees/FeatureServer/0");

            // Initialize feature table using a url to feature server url
            ServiceFeatureTable featureTable = new ServiceFeatureTable(serviceUri);

            // Initialize a new feature layer based on the feature table
            _featureLayer = new FeatureLayer(featureTable);
            myMap.OperationalLayers.Add(_featureLayer);
        }

        private void OnOverrideButtonClicked(object sender, RoutedEventArgs e)
        {
            // Create a symbol to be used in the renderer.
            SimpleMarkerSymbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Blue, 5);

            // Create and apply a new renderer using the symbol just created.
            _featureLayer.Renderer = new SimpleRenderer(symbol);
        }

        private void OnResetButtonClicked(object sender, RoutedEventArgs e)
        {
            // Reset the renderer to default
            _featureLayer.ResetRenderer();
        }
    }
}