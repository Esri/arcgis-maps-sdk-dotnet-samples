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
using System.Drawing;

namespace ArcGISRuntime.WPF.Samples.RenderClassBreak
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Renderer - Class break",
        category: "Symbology",
        description: "Render features in a layer using a distinct symbol for each class range.",
        instructions: "The map with the symbolized feature layer will be shown automatically when the sample loads.",
        tags: new[] { "draw", "renderer", "symbol", "symbology", "classes", "classbreak" })]
    public partial class RenderClassBreak
    {
        public RenderClassBreak()
        {
            InitializeComponent();

            // Initialize the sample.
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create URL to the census feature service.
            Uri serviceUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

            // Create service feature table.
            ServiceFeatureTable statesFeatureTable = new ServiceFeatureTable(serviceUri);

            // Create a new feature layer using the service feature table.
            FeatureLayer statesLayer = new FeatureLayer(statesFeatureTable);

            // Create a new class break renderer.
            ClassBreaksRenderer pop2000Renderer = new ClassBreaksRenderer();

            // Add the "POP2000" field to the renderer.
            pop2000Renderer.FieldName = "POP2000";

            // Define a line symbol to use for the fill symbols.
            SimpleLineSymbol stateOutlineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, Color.White, 0.7);

            // Define distinct fill symbols for each class range (use the same outline symbol).
            SimpleFillSymbol clasbreak1 = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Color.Blue, stateOutlineSymbol);
            SimpleFillSymbol clasbreak2 = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Color.LawnGreen, stateOutlineSymbol);
            SimpleFillSymbol clasbreak3 = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Color.SandyBrown, stateOutlineSymbol);

            // Add values to the renderer: define the label, description, symbol, and attribute value for each.            
            pop2000Renderer.ClassBreaks.Add(
                new ClassBreak("500000 - 2000000", "500000 - 2000000", 500000, 2000000, clasbreak1));
            pop2000Renderer.ClassBreaks.Add(
                new ClassBreak("2000000 - 6000000", "2000000 - 6000000", 2000000, 6000000, clasbreak2));
            pop2000Renderer.ClassBreaks.Add(
               new ClassBreak("6000000 - 30000000", "6000000 - 30000000", 6000000, 30000000, clasbreak3));

            // Set the default region fill symbol for regions not explicitly defined in the renderer.
            SimpleFillSymbol defaultFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Cross, Color.Gray, null);
            pop2000Renderer.DefaultSymbol = defaultFillSymbol;
            pop2000Renderer.DefaultLabel = "Other";

            // Apply the unique value renderer to the states layer.
            statesLayer.Renderer = pop2000Renderer;

            // Add created layer to the map.
            myMap.OperationalLayers.Add(statesLayer);

            // Assign the map to the MapView.
            MyMapView.Map = myMap;
        }
    }
}
