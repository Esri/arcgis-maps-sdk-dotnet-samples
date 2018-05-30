// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntime.Samples.LoadWebTiledLayer
{
    [Register("LoadWebTiledLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Web TiledLayer",
        "Layers",
        "This sample demonstrates how to load a web tiled layer from a non-ArcGIS service, including how to include proper attribution.",
        "")]
    public class LoadWebTiledLayer : UIViewController
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

        public LoadWebTiledLayer()
        {
            Title = "Web TiledLayer";
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
            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }

        public override async void ViewDidLoad()
        {
            CreateLayout();
            await Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }
    }
}