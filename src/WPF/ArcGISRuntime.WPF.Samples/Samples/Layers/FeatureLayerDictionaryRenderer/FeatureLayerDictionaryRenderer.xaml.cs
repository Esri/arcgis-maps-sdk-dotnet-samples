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
using System.IO;
using System.Threading.Tasks;

namespace ArcGISRuntime.WPF.Samples.FeatureLayerDictionaryRenderer
{
    public partial class FeatureLayerDictionaryRenderer
    {
        // Name and ID of the geodatabase file providing features
        private string _geodatabaseName = "militaryoverlay.geodatabase";

        private string _geodatabaseId = "e0d41b4b409a49a5a7ba11939d8535dc";

        // Name and ID of the symbol dictionary file
        private string _symbolDefName = "mil2525d.stylx";

        private string _symbolDefId = "e34835bf5ec5430da7cf16bb8c0b075c";

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
            await MyMapView.SetViewpointCenterAsync(centerGeometry);
            await MyMapView.SetViewpointScaleAsync(201555.279);

            // Get the path to the geodatabase
            string geodbFilePath = await GetPreparedFilePath(_geodatabaseName, _geodatabaseId);

            // Load the geodatabase from local storage
            Geodatabase baseGeodatabase = await Geodatabase.OpenAsync(geodbFilePath);

            // Get the path to the symbol dictionary
            string symbolFilepath = await GetPreparedFilePath(_symbolDefName, _symbolDefId);

            // Load the symbol dictionary from local storage
            //     Note that the type of the symbol definition must be explicitly provided along with the file name
            DictionarySymbolStyle symbolStyle = await DictionarySymbolStyle.OpenAsync("mil2525d", symbolFilepath);

            // Add geodatabase features to the map, using the defined symbology
            foreach (FeatureTable table in baseGeodatabase.GeodatabaseFeatureTables)
            {
                // Load the table
                await table.LoadAsync();

                // Create the feature layer from the table
                FeatureLayer layer = new FeatureLayer(table);

                // Load the layer
                await layer.LoadAsync();

                // Create a Dictionary Renderer using the DictionarySymbolStyle
                DictionaryRenderer dictRenderer = new DictionaryRenderer(symbolStyle);

                // Apply the dictionary renderer to the layer
                layer.Renderer = dictRenderer;

                // Add the layer to the map
                myMap.OperationalLayers.Add(layer);
            }
        }

        // Get the file path for the specified offline data
        private async Task<string> GetPreparedFilePath(string fileName, string fileId)
        {
            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "FeatureLayerDictionaryRenderer", fileName);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData(fileId, "FeatureLayerDictionaryRenderer");
            }

            return filepath;
        }
    }
}