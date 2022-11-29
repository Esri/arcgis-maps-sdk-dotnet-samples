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

namespace ArcGIS.WPF.Samples.ArcGISMapImageLayerUrl
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "ArcGIS map image layer",
        category: "Layers",
        description: "Add an ArcGIS Map Image Layer from a URL to a map.",
        instructions: "Run the sample and view the map image layer. As you navigate the map, the layer issues image requests for the new map extent. The resulting images are rendered on the screen.",
        tags: new[] { "display", "image", "layer", "map" })]
    public partial class ArcGISMapImageLayerUrl
    {
        public ArcGISMapImageLayerUrl()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create uri to the map image layer
            Uri serviceUri = new Uri(
               "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer");

            // Create new image layer from the url
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(serviceUri);

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(imageLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}