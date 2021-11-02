// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.UI;
using System;
using System.Drawing;

namespace ArcGISRuntime.WPF.Samples.SimpleRenderers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Renderer - Simple",
        category: "Symbology",
        description: "Display common symbols for all graphics in a graphics overlay with a renderer.",
        instructions: "The sample loads with a predefined simple renderer, which displays a red cross simple marker symbol for the graphics in the graphics overlay.",
        tags: new[] { "graphics", "marker", "renderer", "symbol", "symbolize", "symbology" })]
    public partial class SimpleRenderers
    {
        public SimpleRenderers()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Create new map with basemap layer.
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create URL to the census feature service.
            Uri serviceUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

            // Create service feature table.
            ServiceFeatureTable statesFeatureTable = new ServiceFeatureTable(serviceUri);

            // Create a new feature layer using the service feature table.
            FeatureLayer statesLayer = new FeatureLayer(statesFeatureTable);

            // Create a new unique value renderer.
            SimpleRenderer stateRenderer = new SimpleRenderer();

            // Define a line symbol to use for the region fill symbols.
            SimpleLineSymbol stateOutlineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, Color.White, 0.7);

            // Define distinct fill symbols for a few regions (use the same outline symbol).
            SimpleFillSymbol stateFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Color.Blue, stateOutlineSymbol);


            stateRenderer.Symbol = stateFillSymbol;
            // Apply the unique value renderer to the states layer.
            statesLayer.Renderer = stateRenderer;

            // Add created layer to the map.
            myMap.OperationalLayers.Add(statesLayer);

            // Assign the map to the MapView.
            MyMapView.Map = myMap;
        }
    }
}