// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;

namespace ArcGISRuntime.WPF.Samples.FeatureLayerDictionaryRenderer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer dictionary renderer",
        "Layers",
        "Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.",
        "")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e34835bf5ec5430da7cf16bb8c0b075c", "e0d41b4b409a49a5a7ba11939d8535dc")]
    public partial class FeatureLayerDictionaryRenderer
    {
        public FeatureLayerDictionaryRenderer()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Provide Map to the MapView
            MyMapView.Map = myMap;

            // Create geometry for the center of the map
            MapPoint centerGeometry = new MapPoint(-13549402.587055, 4397264.96879385, SpatialReference.Create(3857));

            // Set the map's viewpoint to highlight the desired content
            await MyMapView.SetViewpointAsync(new Viewpoint(centerGeometry, 201555));

            // Get the path to the geodatabase
            string geodbFilePath = GetGeodatabasePath();

            // Load the geodatabase from local storage
            Geodatabase baseGeodatabase = await Geodatabase.OpenAsync(geodbFilePath);

            // Get the path to the symbol dictionary
            string symbolFilepath = GetStyleDictionaryPath();

            // Load the symbol dictionary from local storage
            //     Note that the type of the symbol definition must be explicitly provided along with the file name
            DictionarySymbolStyle symbolStyle = await DictionarySymbolStyle.OpenAsync("mil2525d", symbolFilepath);

            // Add geodatabase features to the map, using the defined symbology
            foreach (FeatureTable table in baseGeodatabase.GeodatabaseFeatureTables)
            {
                // Load the table
                await table.LoadAsync();

                // Create the feature layer from the table
                FeatureLayer myLayer = new FeatureLayer(table);

                // Load the layer
                await myLayer.LoadAsync();

                // Create a Dictionary Renderer using the DictionarySymbolStyle
                DictionaryRenderer dictRenderer = new DictionaryRenderer(symbolStyle);

                // Apply the dictionary renderer to the layer
                myLayer.Renderer = dictRenderer;

                // Add the layer to the map
                myMap.OperationalLayers.Add(myLayer);
            }
        }

        // Get the file path for the style dictionary
        private static string GetStyleDictionaryPath()
        {
            return DataManager.GetDataFolder("e34835bf5ec5430da7cf16bb8c0b075c", "mil2525d.stylx");
        }

        // Get the file path for the geodatabase
        private static string GetGeodatabasePath()
        {
            return DataManager.GetDataFolder("e0d41b4b409a49a5a7ba11939d8535dc", "militaryoverlay.geodatabase");
        }
    }
}