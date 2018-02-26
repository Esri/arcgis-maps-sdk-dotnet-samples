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
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.WMTSLayer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "WMTS layer",
        "Layers",
        "This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.",
        "")]
    public partial class WMTSLayer
    {
        public WMTSLayer()
        {
            InitializeComponent();
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Define the Uri to the WMTS service
                var myUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS");

                // Create a new instance of a WMTS layer using a Uri and provide an Id value
                WmtsLayer myWmtsLayer = new WmtsLayer(myUri, "WorldTimeZones");

                // Create a new map
                Map myMap = new Map();

                // Get the basemap from the map
                Basemap myBasemap = myMap.Basemap;

                // Get the layer collection for the base layers
                LayerCollection myLayerCollection = myBasemap.BaseLayers;

                // Add the WMTS layer to the layer collection of the map
                myLayerCollection.Add(myWmtsLayer);

                // Assign the map to the MapView
                MyMapView.Map = myMap;
            }
            catch (Exception ex)
            {
                MessageDialog messageDlg = new MessageDialog(ex.ToString(), "Error");
                await messageDlg.ShowAsync();
            }
        }

        private async void Button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Define the Uri to the WMTS service
                var myUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS");

                // Define a new instance of the WMTS service
                WmtsService myWmtsService = new WmtsService(myUri);

                // Load the WMTS service 
                await myWmtsService.LoadAsync();

                // Get the service information (i.e. metadata) about the WMTS service
                WmtsServiceInfo myWMTSServiceInfo = myWmtsService.ServiceInfo;

                // Obtain the read only list of WMTS layer info objects
                IReadOnlyList<WmtsLayerInfo> myWmtsLayerInfos = myWMTSServiceInfo.LayerInfos;

                // Create a new instance of a WMTS layer using the first item in the read only list of WMTS layer info objects
                WmtsLayer myWmtsLayer = new WmtsLayer(myWmtsLayerInfos[0]);

                // Create a new map
                Map myMap = new Map();

                // Get the basemap from the map
                Basemap myBasemap = myMap.Basemap;

                // Get the layer collection for the base layers
                LayerCollection myLayerCollection = myBasemap.BaseLayers;

                // Add the WMTS layer to the layer collection of the map
                myLayerCollection.Add(myWmtsLayer);

                // Assign the map to the MapView
                MyMapView.Map = myMap;
            }
            catch (Exception ex)
            {
                MessageDialog messageDlg = new MessageDialog(ex.ToString(), "Error");
                await messageDlg.ShowAsync();
            }
        }
    }
}
