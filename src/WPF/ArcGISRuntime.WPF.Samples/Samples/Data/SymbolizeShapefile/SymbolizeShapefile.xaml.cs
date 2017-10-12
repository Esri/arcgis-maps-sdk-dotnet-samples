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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.SymbolizeShapefile
{
    public partial class SymbolizeShapefile
    {
        public SymbolizeShapefile()
        {
            InitializeComponent();
            
            // Open a shapefile stored locally and add it to the map as a feature layer
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            MyMapView.Map = new Map(Basemap.CreateStreetsVector());

            // The shapefile will be downloaded from ArcGIS Online
            // The data manager (a component of the sample viewer, *NOT* the runtime
            //     handles the offline data process

            // The desired shapefile is expected to be called "Subdivisions.shp"
            string filename = "Subdivisions.shp";
            
            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "AddShapefile", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the shapefile
                await DataManager.GetData("d98b3e5293834c5f852f13c569930caa", "AddShapefile");
            }

            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

            // Add the feature layer to the map
            MyMapView.Map.OperationalLayers.Add(newFeatureLayer);

            // Zoom the map to the extent of the shapefile
            await MyMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);
        }

        private void UpdateSymbology(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get the shapefile feature table layer
            FeatureLayer shapefileLayer = MyMapView.Map.OperationalLayers.FirstOrDefault() as FeatureLayer;

            // Return if the layer isn't found (perhaps the shapefile wasn't loaded)
            if (shapefileLayer == null) { return; }

            // Create a new line symbol (for the fill outline)
            SimpleLineSymbol outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.DarkBlue, 1.6);
            
            // Create a new fill symbol
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.BackwardDiagonal, Colors.PowderBlue, outlineSymbol);

            // Create a renderer that uses the fill symbol
            SimpleRenderer renderer = new SimpleRenderer(fillSymbol);

            // Apply the renderer to the layer
            shapefileLayer.Renderer = renderer;
        }
    }
}