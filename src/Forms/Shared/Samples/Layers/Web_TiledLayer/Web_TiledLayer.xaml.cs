// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System.Collections.Generic;
using Esri.ArcGISRuntime.Mapping;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.Web_TiledLayer
{
    public partial class Web_TiledLayer : ContentPage
    {
        // Templated URL to the tile service
        private readonly string _templateUri = "http://{subDomain}.tile.stamen.com/terrain/{level}/{col}/{row}.png";

        // List of subdomains for use when constructing the web tiled layer
        private readonly List<string> _tiledLayerSubdomains = new List<string> { "a", "b", "c", "d" };

        // Attribution string for the Stamen service
        private readonly string _attribution = "Map tiles by <a href=\"http://stamen.com/\">Stamen Design</a>," + 
                                               "under <a href=\"http://creativecommons.org/licenses/by/3.0\">CC BY 3.0</a>." + 
                                               "Data by <a href=\"http://openstreetmap.org/\">OpenStreetMap</a>," +
                                               "under <a href=\"http://creativecommons.org/licenses/by-sa/3.0\">CC BY SA</a>.";

        public Web_TiledLayer()
        {
            InitializeComponent();

            Title = "Web TiledLayer";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create the layer from the URL and the subdomain list
            WebTiledLayer layer = new WebTiledLayer(_templateUri, _tiledLayerSubdomains);

            // Wait for the layer to load
            await layer.LoadAsync();

            // Create a basemap from the layer
            Basemap layerBasemap = new Basemap(layer);

            // Apply the attribution for the layer
            layer.Attribution = _attribution;

            // Create a map to hold the basemap
            Map myMap = new Map(layerBasemap);

            // Add the map to the map view
            MyMapView.Map = myMap;
        }
    }
}
