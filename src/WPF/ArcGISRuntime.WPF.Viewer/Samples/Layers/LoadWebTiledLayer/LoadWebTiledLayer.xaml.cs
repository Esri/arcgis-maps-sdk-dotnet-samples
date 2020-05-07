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

namespace ArcGISRuntime.WPF.Samples.LoadWebTiledLayer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Web tiled layer",
        "Layers",
        "Display a tiled web layer.",
        "Run the sample and a map will appear. As you navigate the map, map tiles will be fetched automatically and displayed on the map.",
        "OGC", "Open Street Map", "OpenStreetMap", "layer", "stamen.com", "tiled", "tiles")]
    public partial class LoadWebTiledLayer
    {
        // Templated URL to the tile service
        private readonly string _templateUri = "https://stamen-tiles-{subdomain}.a.ssl.fastly.net/watercolor/{level}/{col}/{row}.jpg";

        // List of subdomains for use when constructing the web tiled layer
        private readonly List<string> _tiledLayerSubdomains = new List<string> { "a", "b", "c", "d" };

        // Attribution string for the Stamen service
        private readonly string _attribution = "Map tiles by <a href=\"http://stamen.com/\">Stamen Design</a>," +
                                               "under <a href=\"http://creativecommons.org/licenses/by/3.0\">CC BY 3.0</a>." +
                                               "Data by <a href=\"http://openstreetmap.org/\">OpenStreetMap</a>," +
                                               "under <a href=\"http://creativecommons.org/licenses/by-sa/3.0\">CC BY SA</a>.";

        public LoadWebTiledLayer()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
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