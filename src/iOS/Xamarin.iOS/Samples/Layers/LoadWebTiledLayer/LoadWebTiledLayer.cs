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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.LoadWebTiledLayer
{
    [Register("LoadWebTiledLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Web tiled layer",
        "Layers",
        "This sample demonstrates how to load a web tiled layer from a non-ArcGIS service, including how to include proper attribution.",
        "")]
    public class LoadWebTiledLayer : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView;

        // Templated URL to the tile service.
        private const string TemplateUri = "https://stamen-tiles-{subdomain}.a.ssl.fastly.net/watercolor/{level}/{col}/{row}.jpg";

        // List of subdomains for use when constructing the web tiled layer.
        private readonly List<string> _tiledLayerSubdomains = new List<string> {"a", "b", "c", "d"};

        // Attribution string for the Stamen service.
        private const string Attribution = "Map tiles by <a href=\"http://stamen.com/\">Stamen Design</a>," +
                                           "under <a href=\"http://creativecommons.org/licenses/by/3.0\">CC BY 3.0</a>." +
                                           "Data by <a href=\"http://openstreetmap.org/\">OpenStreetMap</a>," +
                                           "under <a href=\"http://creativecommons.org/licenses/by-sa/3.0\">CC BY SA</a>.";

        public LoadWebTiledLayer()
        {
            Title = "Web tiled layer";
        }

        private void Initialize()
        {
            // Create the layer from the URL and the subdomain list.
            WebTiledLayer baseLayer = new WebTiledLayer(TemplateUri, _tiledLayerSubdomains);

            // Create a basemap from the layer.
            Basemap layerBasemap = new Basemap(baseLayer);

            // Apply the attribution for the layer.
            baseLayer.Attribution = Attribution;

            // Show the tiled layer basemap.
            _myMapView.Map = new Map(layerBasemap);
        }

        public override void LoadView()
        {
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView();
            View.AddSubviews(_myMapView);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }
    }
}