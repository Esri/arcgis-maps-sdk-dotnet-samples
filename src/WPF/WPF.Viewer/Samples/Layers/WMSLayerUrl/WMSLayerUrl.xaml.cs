// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;

namespace ArcGIS.WPF.Samples.WMSLayerUrl
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "WMS layer (URL)",
        category: "Layers",
        description: "Display a WMS layer using a WMS service URL.",
        instructions: "The map will load automatically when the sample starts.",
        tags: new[] { "OGC", "WMS", "layer", "web map service" })]
    public partial class WMSLayerUrl
    {
        // Hold the URL to the WMS service showing U.S. weather radar.
        private readonly Uri _wmsUrl = new Uri(
            "https://nowcoast.noaa.gov/geoserver/observations/weather_radar/wms?SERVICE=WMS&REQUEST=GetCapabilities");

        // Hold a list of uniquely-identifying WMS layer names to display.
        private readonly List<string> _wmsLayerNames = new List<string> { "conus_base_reflectivity_mosaic" };

        public WMSLayerUrl()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Create a map with basemap and initial viewpoint.
            Map myMap = new Map(BasemapStyle.ArcGISLightGray)
            {
                // Set the initial viewpoint.
                InitialViewpoint = new Viewpoint(
                    new Envelope(-19195297.778679, 512343.939994, -3620418.579987, 8658913.035426, 0.0, 0.0, SpatialReferences.WebMercator))
            };

            // Add the map to the mapview.
            MyMapView.Map = myMap;

            // Create a new WMS layer displaying the specified layers from the service.
            WmsLayer myWmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

            // Add the layer to the map.
            myMap.OperationalLayers.Add(myWmsLayer);
        }
    }
}