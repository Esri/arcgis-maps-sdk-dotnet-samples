// Copyright 2018 Esri.
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.WMTSLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "WMTS layer",
        category: "Layers",
        description: "Display a layer from a Web Map Tile Service.",
        instructions: "The layer will be displayed automatically. Use the buttons to choose a different method of loading the layer.",
        tags: new[] { "OGC", "layer", "raster", "tiled", "web map tile service" })]
    public partial class WMTSLayer
    {
        public WMTSLayer()
        {
            InitializeComponent();
            _ = LoadWMTSLayer(true);
        }

        private void UriButton_Click(object sender, RoutedEventArgs e)
        {
            //Load the WMTS layer using Uri method.
            _ = LoadWMTSLayer(true);

            // Disable and enable the appropriate buttons.
            UriButton.IsEnabled = false;
            InfoButton.IsEnabled = true;
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            //Load the WMTS layer using layer info.
            _ = LoadWMTSLayer(false);

            // Disable and enable the appropriate buttons.
            UriButton.IsEnabled = true;
            InfoButton.IsEnabled = false;
        }

        private async Task LoadWMTSLayer(bool uriMode)
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
                Uri wmtsUri = new Uri("https://gibs.earthdata.nasa.gov/wmts/epsg4326/best");

                if (uriMode)
                {
                    // Create a WMTS layer using a Uri and provide an Id value.
                    myWmtsLayer = new WmtsLayer(wmtsUri, "SRTM_Color_Index");
                }
                else
                {
                    // Define a new instance of the WMTS service.
                    WmtsService myWmtsService = new WmtsService(wmtsUri);

                    // Load the WMTS service.
                    await myWmtsService.LoadAsync();

                    // Get the service information (i.e. metadata) about the WMTS service.
                    WmtsServiceInfo myWmtsServiceInfo = myWmtsService.ServiceInfo;

                    // Obtain the read only list of WMTS layer info objects, and select the one with the desired Id value.
                    WmtsLayerInfo info = myWmtsServiceInfo.LayerInfos.Single(l => l.Id == "SRTM_Color_Index");

                    // Create a WMTS layer using WMTS layer info.
                    myWmtsLayer = new WmtsLayer(info);
                }

                // Add the WMTS layer to the layer collection of the map.
                myLayerCollection.Add(myWmtsLayer);

                // Assign the map to the MapView.
                MyMapView.Map = myMap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}