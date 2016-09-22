// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;

namespace ArcGISRuntimeXamarin.Samples.RenderUniqueValues
{
    [Activity]
    public class RenderUniqueValues : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Render unique values";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
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
                SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 0.7);

            // Define distinct fill symbols for a few regions (use the same outline symbol)
            SimpleFillSymbol pacificFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, System.Drawing.Color.Blue, stateOutlineSymbol);
            SimpleFillSymbol mountainFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, System.Drawing.Color.LawnGreen, stateOutlineSymbol);
            SimpleFillSymbol westSouthCentralFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, System.Drawing.Color.SandyBrown, stateOutlineSymbol);

            // Add values to the renderer: define the label, description, symbol, and attribute value for each
            regionRenderer.UniqueValues.Add(
                new UniqueValue("Pacific", "Pacific Region", pacificFillSymbol, "Pacific"));
            regionRenderer.UniqueValues.Add(
                new UniqueValue("Mountain", "Rocky Mountain Region", mountainFillSymbol, "Mountain"));
            regionRenderer.UniqueValues.Add(
                new UniqueValue("West South Central", "West South Central Region", westSouthCentralFillSymbol, "West South Central"));

            // Set the default region fill symbol (transparent with no outline) for regions not explicitly defined in the renderer
            SimpleFillSymbol defaultFillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Null, System.Drawing.Color.Transparent, null);
            regionRenderer.DefaultSymbol = defaultFillSymbol;
            regionRenderer.DefaultLabel = "Other";

            // Apply the unique value renderer to the states layer
            statesLayer.Renderer = regionRenderer;

            // Add created layer to the map
            myMap.OperationalLayers.Add(statesLayer);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}