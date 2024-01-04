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

namespace ArcGIS.WPF.Samples.LoadWebTiledLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Web tiled layer",
        category: "Layers",
        description: "Display a tiled web layer.",
        instructions: "Run the sample and a map will appear. As you navigate the map, map tiles will be fetched automatically and displayed on the map.",
        tags: new[] { "OGC", "layer", "tiled", "tiles" })]
    public partial class LoadWebTiledLayer
    {
        // Templated URL to the tile service.
        private readonly string _templateUri = "https://server.arcgisonline.com/arcgis/rest/services/Ocean/World_Ocean_Base/MapServer/tile/{level}/{row}/{col}.jpg";

        // Attribution string.
        private readonly string _attribution = "Map tiles by <a href=\"https://livingatlas.arcgis.com\">ArcGIS Living Atlas of the World</a>, " +
                                            "under <a href=\"https://www.esri.com/en-us/legal/terms/full-master-agreement\">Esri Master License Agreement</a>. " +
                                            "Data by Esri, Garmin, GEBCO, NOAA NGDC, and other contributors.";

        public LoadWebTiledLayer()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Create the layer from the URL and the subdomain list.
            WebTiledLayer myBaseLayer = new WebTiledLayer(_templateUri);

            // Create a basemap from the layer.
            Basemap layerBasemap = new Basemap(myBaseLayer);

            // Apply the attribution for the layer.
            myBaseLayer.Attribution = _attribution;

            // Create a map to hold the basemap.
            Map myMap = new Map(layerBasemap);

            // Add the map to the map view.
            MyMapView.Map = myMap;
        }
    }
}