// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.LoadWebTiledLayer
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Web TiledLayer",
        "Layers",
        "This sample demonstrates how to load a web tiled layer from a non-ArcGIS service, including how to include proper attribution.",
        "")]
    public class LoadWebTiledLayer : Activity
    {
        // Create and hold reference to the used MapView
        private readonly MapView _myMapView = new MapView();

        // Templated URL to the tile service
        private readonly string _templateUri = "http://{subDomain}.tile.stamen.com/terrain/{level}/{col}/{row}.png";

        // List of subdomains for use when constructing the web tiled layer
        private readonly List<string> _tiledLayerSubdomains = new List<string> { "a", "b", "c", "d" };

        // Attribution string for the Stamen service
        private readonly string _attribution = "Map tiles by <a href=\"http://stamen.com/\">Stamen Design</a>," +
                                               "under <a href=\"http://creativecommons.org/licenses/by/3.0\">CC BY 3.0</a>." +
                                               "Data by <a href=\"http://openstreetmap.org/\">OpenStreetMap</a>," +
                                               "under <a href=\"http://creativecommons.org/licenses/by-sa/3.0\">CC BY SA</a>.";

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Web TiledLayer";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            await Initialize();
        }

        private async Task Initialize()
        {
            // Create the layer from the URL and the subdomain list
            WebTiledLayer myBaseLayer = new WebTiledLayer(_templateUri, _tiledLayerSubdomains);

            // Wait for the layer to load
            await myBaseLayer.LoadAsync();

            // Create a basemap from the layer
            Basemap layerBasemap = new Basemap(myBaseLayer);

            // Apply the attribution for the layer
            myBaseLayer.Attribution = _attribution;

            // Create a map to hold the basemap
            Map myMap = new Map(layerBasemap);

            // Add the map to the map view
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