// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System.Collections.Generic;

namespace ArcGISMapsSDK.UWP.Samples.LoadWebTiledLayer
{
    [ArcGISMapsSDK.Samples.Shared.Attributes.Sample(
        name: "Web tiled layer",
        category: "Layers",
        description: "Display a tiled web layer.",
        instructions: "Run the sample and a map will appear. As you navigate the map, map tiles will be fetched automatically and displayed on the map.",
        tags: new[] { "OGC", "Open Street Map", "OpenStreetMap", "layer", "stamen.com", "tiled", "tiles" })]
    public partial class LoadWebTiledLayer
    {
        // Templated URL to the tile service
        private readonly string _templateUri = "https://stamen-tiles-{subdomain}.a.ssl.fastly.net/watercolor/{level}/{col}/{row}.jpg";

        // List of subdomains for use when constructing the web tiled layer
        private readonly List<string> _tiledLayerSubdomains = new List<string> { "a", "b", "c", "d" };

        // Attribution string for the Stamen service
        private readonly string _attribution = "Map tiles by Stamen Design, under CC BY 3.0. Data by OpenStreetMap, under CC BY SA.";

        public LoadWebTiledLayer()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create the layer from the URL and the subdomain list
            WebTiledLayer myBaseLayer = new WebTiledLayer(_templateUri, _tiledLayerSubdomains);

            // Create a basemap from the layer
            Basemap layerBasemap = new Basemap(myBaseLayer);

            // Apply the attribution for the layer
            myBaseLayer.Attribution = _attribution;

            // Create a map to hold the basemap
            Map myMap = new Map(layerBasemap);

            // Add the map to the map view
            MyMapView.Map = myMap;
        }
    }
}