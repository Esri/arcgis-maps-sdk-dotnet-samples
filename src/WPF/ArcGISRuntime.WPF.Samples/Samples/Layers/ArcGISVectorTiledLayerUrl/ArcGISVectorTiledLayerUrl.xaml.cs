﻿// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.ArcGISVectorTiledLayerUrl
{
    public partial class ArcGISVectorTiledLayerUrl
    {
        private string _navigationUrl = "http://www.arcgis.com/home/item.html?id=dcbbba0edf094eaa81af19298b9c6247";
        private string _streetUrl = "http://www.arcgis.com/home/item.html?id=4e1133c28ac04cca97693cf336cd49ad";
        private string _nightUrl = "http://www.arcgis.com/home/item.html?id=bf79e422e9454565ae0cbe9553cf6471";
        private string _darkGrayUrl = "http://www.arcgis.com/home/item.html?id=850db44b9eb845d3bd42b19e8aa7a024";

        private string _vectorTiledLayerUrl;
        private ArcGISVectorTiledLayer _vectorTiledLayer;

        // String array to store some vector layer choices
        private string[] _vectorLayerNames = new string[]
        {
            "Dark gray",
            "Streets",
            "Night",
            "Navigation"
        };

        public ArcGISVectorTiledLayerUrl()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create a new ArcGISVectorTiledLayer with the navigation service Url
            _vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(_navigationUrl));

            // Create new Map with basemap
            Map myMap = new Map(new Basemap(_vectorTiledLayer));

            // Set titles as a items source
            vectorLayersChooser.ItemsSource = _vectorLayerNames;

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private void OnVectorLayersChooserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedVectorLayer = e.AddedItems[0].ToString();

            switch (selectedVectorLayer)
            {
                case "Dark gray":
                    _vectorTiledLayerUrl = _darkGrayUrl;
                    break;

                case "Streets":
                    _vectorTiledLayerUrl = _streetUrl;
                    break;

                case "Night":
                    _vectorTiledLayerUrl = _nightUrl;
                    break;

                case "Navigation":
                    _vectorTiledLayerUrl = _navigationUrl;
                    break;

                default:
                    break;
            }

            // Create a new ArcGISVectorTiledLayer with the Url Selected by the user
            _vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(_vectorTiledLayerUrl));

            // // Create new Map with basemap and assigning to the Mapviews Map
            MyMapView.Map = new Map(new Basemap(_vectorTiledLayer));
        }
    }
}
