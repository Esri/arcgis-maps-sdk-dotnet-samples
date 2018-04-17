// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;

namespace ArcGISRuntime.WPF.Samples.ChangeSublayerVisibility
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change sublayer visibility",
        "Layers",
        "This sample demonstrates how to show or hide sublayers of a map image layer.",
        "")]
    public partial class ChangeSublayerVisibility
    {
        public ChangeSublayerVisibility()
        {
            InitializeComponent();

            // Setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create uri to the map image layer
            var serviceUri = new Uri(
               "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer");

            // Create new image layer from the url
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(serviceUri)
            {
                Name = "World Cities Population"
            };

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(imageLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Wait that the image layer is loaded and sublayer information is fetched
            await imageLayer.LoadAsync();

            // Assign sublayers to the listview
            sublayerListView.ItemsSource = imageLayer.Sublayers;
        }
    }
}
