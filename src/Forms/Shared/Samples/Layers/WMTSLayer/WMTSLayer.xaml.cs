// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.WMTSLayer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "WMTS layer",
        "Layers",
        "Display a layer from a Web Map Tile Service.",
        "The layer will be displayed automatically. Use the buttons to choose a different method of loading the layer.",
        "OGC", "layer", "raster", "tiled", "web map tile service")]
    public partial class WMTSLayer : ContentPage
    {
        public WMTSLayer()
        {
            InitializeComponent();
            LoadWMTSLayer(true);
        }

        private void UriButton_Click(object sender, EventArgs e)
        {
            //Load the WMTS layer using Uri method.
            LoadWMTSLayer(true);

            // Disable and enable the appropriate buttons.
            UriButton.IsEnabled = false;
            InfoButton.IsEnabled = true;
        }

        private void InfoButton_Click(object sender, EventArgs e)
        {
            //Load the WMTS layer using layer info.
            LoadWMTSLayer(false);

            // Disable and enable the appropriate buttons.
            UriButton.IsEnabled = true;
            InfoButton.IsEnabled = false;
        }

        private async void LoadWMTSLayer(bool uriMode)
        {
            try
            {
                // Create a new map.
                Map myMap = new Map();

                // Get the basemap from the map.
                Basemap myBasemap = myMap.Basemap;

                // Get the layer collection for the base layers.
                LayerCollection myLayerCollection = myBasemap.BaseLayers;

                // Create an instance for the WMTS layer.
                WmtsLayer myWmtsLayer;

                // Define the Uri to the WMTS service.
                Uri wmtsUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS");

                if (uriMode)
                {
                    // Create a WMTS layer using a Uri and provide an Id value.
                    myWmtsLayer = new WmtsLayer(wmtsUri, "WorldTimeZones");

                }
                else
                {
                    // Define a new instance of the WMTS service.
                    WmtsService myWmtsService = new WmtsService(wmtsUri);

                    // Load the WMTS service.
                    await myWmtsService.LoadAsync();

                    // Get the service information (i.e. metadata) about the WMTS service.
                    WmtsServiceInfo myWmtsServiceInfo = myWmtsService.ServiceInfo;

                    // Obtain the read only list of WMTS layer info objects.
                    IReadOnlyList<WmtsLayerInfo> myWmtsLayerInfos = myWmtsServiceInfo.LayerInfos;

                    // Create a WMTS layer using the first item in the read only list of WMTS layer info objects.
                    myWmtsLayer = new WmtsLayer(myWmtsLayerInfos[0]);
                }

                // Add the WMTS layer to the layer collection of the map.
                myLayerCollection.Add(myWmtsLayer);

                // Assign the map to the MapView.
                MyMapView.Map = myMap;

                // Zoom to appropriate level for iOS.
                await MyMapView.SetViewpointScaleAsync(300000000);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Sample error", ex.ToString(), "OK");
            }
        }
    }
}