//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Windows.Media;

namespace ArcGISRuntime.Desktop.Samples.UniqueValueRenderer
{
    public partial class UniqueValueRenderer
    {
        // Url for the USA States (polygon) layer
        private const string StatesServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3";

        public UniqueValueRenderer()
        {
            InitializeComponent();

            // Handle the spatial reference changed event to add a new layer with a unique value renderer
            MyMapView.SpatialReferenceChanged += ShowRegionsWithUniqueValues;
        }        

        // Add a new layer with some US States symbolized by region
        private void ShowRegionsWithUniqueValues(object sender, System.EventArgs e)
        {
            // Create a new service feature table using the USA States url
            var statesFeatureTable = new ServiceFeatureTable(new System.Uri(StatesServiceUrl));
            // Add the "SUB_REGION" field to the outfields, will be used to render polygons in the layer
            statesFeatureTable.OutFields.Add("SUB_REGION");
            // Create a new feature layer using the service feature table
            var statesLayer = new FeatureLayer(statesFeatureTable);

            // Create a new unique value renderer
            var regionRenderer = new Esri.ArcGISRuntime.Symbology.UniqueValueRenderer();
            // Add the "SUB_REGION" field to the renderer
            regionRenderer.FieldNames.Add("SUB_REGION");

            // Define a line symbol to use for the region fill symbols
            var stateOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.White, 2, 0.7);
            // Define distinct fill symbols for a few regions (use the same outline symbol)
            var pacificFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Blue, 1, stateOutlineSymbol);
            var mountainFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.LawnGreen, 1, stateOutlineSymbol);
            var westSouthCentralFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.SandyBrown, 1, stateOutlineSymbol);

            // Add values to the renderer: define the label, description, symbol, and attribute value for each
            regionRenderer.UniqueValues.Add(new UniqueValue("Pacific", "Pacific Region", pacificFillSymbol, "Pacific"));
            regionRenderer.UniqueValues.Add(new UniqueValue("Mountain", "Rocky Mountain Region", mountainFillSymbol, "Mountain"));
            regionRenderer.UniqueValues.Add(new UniqueValue("West South Central", "West South Central Region", westSouthCentralFillSymbol, "West South Central"));

            // Set the default region fill symbol (transparent with no outline) for regions not explicitly defined in the renderer
            var defaultFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, Colors.Transparent, 1, null);
            regionRenderer.DefaultSymbol = defaultFillSymbol;
            regionRenderer.DefaultLabel = "Other";

            // Apply the unique value renderer to the states layer
            statesLayer.Renderer = regionRenderer;
            // Add the new layer to the map
            MyMapView.Map.OperationalLayers.Add(statesLayer);
        }
    }
}