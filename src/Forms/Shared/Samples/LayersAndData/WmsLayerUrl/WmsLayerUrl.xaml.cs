// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.WmsLayerUrl
{
    public partial class WmsLayerUrl : ContentPage
    {
        // Hold the URL to the WMS service showing the geology of Africa
        private Uri wmsUrl = new Uri("https://certmapper.cr.usgs.gov/arcgis/services/geology/africa/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of uniquely-identifying WMS layer names to display
        private List<String> wmsLayerNames = new List<string> { "0" };

        public WmsLayerUrl()
        {
            InitializeComponent();

            Title = "WMS layer (URL)";

            // Initialize the map
            Initialize();
        }

        private async void Initialize()
        {
            // Apply an imagery basemap to the map
            MyMapView.Map = new Map(Basemap.CreateImagery());

            // Create a new WMS layer displaying the specified layers from the service
            WmsLayer myWmsLayer = new WmsLayer(wmsUrl, wmsLayerNames);

            // Load the layer
            await myWmsLayer.LoadAsync();

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(myWmsLayer);
        }
    }
}