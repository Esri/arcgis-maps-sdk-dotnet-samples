// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;

namespace ArcGIS.WinUI.Samples.ArcGISTiledLayerUrl
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "ArcGIS tiled layer",
        category: "Layers",
        description: "Load an ArcGIS tiled layer from a URL.",
        instructions: "Launch the app to view the \"World Topographic Map\" tile layer as the basemap.",
        tags: new[] { "basemap", "layers", "raster tiles", "tiled layer", "visualization" })]
    public partial class ArcGISTiledLayerUrl
    {
        public ArcGISTiledLayerUrl()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create uri to the tiled service
            Uri serviceUri = new Uri(
               "https://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");

            // Create new tiled layer from the url
            ArcGISTiledLayer imageLayer = new ArcGISTiledLayer(serviceUri);

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(imageLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}