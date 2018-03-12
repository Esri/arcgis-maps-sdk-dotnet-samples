// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System;
using Windows.UI;

namespace ArcGISRuntime.UWP.Samples.RenderUniqueValues
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Render unique values",
        "Symbology",
        "This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.",
        "")]
    public sealed partial class RenderUniqueValues
    {
        public RenderUniqueValues()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create uri to the used feature service
            var serviceUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

            // Create service feature table
            ServiceFeatureTable statesFeatureTable = new ServiceFeatureTable(serviceUri);

            // Create a new feature layer using the service feature table
            FeatureLayer statesLayer = new FeatureLayer(statesFeatureTable);

            // Create a new unique value renderer
            UniqueValueRenderer regionRenderer = new UniqueValueRenderer();

            // Add the "SUB_REGION" field to the renderer
            regionRenderer.FieldNames.Add("SUB_REGION");

            // Define a line symbol to use for the region fill symbols
            SimpleLineSymbol stateOutlineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, Colors.White, 0.7);

            // Define distinct fill symbols for a few regions (use the same outline symbol)
            SimpleFillSymbol pacificFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Colors.Blue, stateOutlineSymbol);
            SimpleFillSymbol mountainFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Colors.LawnGreen, stateOutlineSymbol);
            SimpleFillSymbol westSouthCentralFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Colors.SandyBrown, stateOutlineSymbol);

            // Add values to the renderer: define the label, description, symbol, and attribute value for each
            regionRenderer.UniqueValues.Add(
                new UniqueValue("Pacific", "Pacific Region", pacificFillSymbol, "Pacific"));
            regionRenderer.UniqueValues.Add(
                new UniqueValue("Mountain", "Rocky Mountain Region", mountainFillSymbol, "Mountain"));
            regionRenderer.UniqueValues.Add(
                new UniqueValue("West South Central", "West South Central Region", westSouthCentralFillSymbol, "West South Central"));

            // Set the default region fill symbol (transparent with no outline) for regions not explicitly defined in the renderer
            var defaultFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Null, Colors.Transparent, null);
            regionRenderer.DefaultSymbol = defaultFillSymbol;
            regionRenderer.DefaultLabel = "Other";

            // Apply the unique value renderer to the states layer
            statesLayer.Renderer = regionRenderer;

            // Add created layer to the map
            myMap.OperationalLayers.Add(statesLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}
