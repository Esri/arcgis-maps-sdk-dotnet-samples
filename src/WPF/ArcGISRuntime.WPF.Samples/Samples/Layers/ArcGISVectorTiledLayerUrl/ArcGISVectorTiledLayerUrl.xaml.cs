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
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.ArcGISVectorTiledLayerUrl
{
    public partial class ArcGISVectorTiledLayerUrl
    {
        private string _navigationUrl = "https://www.arcgis.com/home/item.html?id=63c47b7177f946b49902c24129b87252";
        private string _streetUrl = "https://www.arcgis.com/home/item.html?id=de26a3cf4cc9451298ea173c4b324736";
        private string _nightUrl = "https://www.arcgis.com/home/item.html?id=86f556a2d1fd468181855a35e344567f";
        private string _darkGrayUrl = "https://www.arcgis.com/home/item.html?id=5e9b3685f4c24d8781073dd928ebda50";

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

            // Update UI to match selection
            vectorLayersChooser.SelectedIndex = 3;
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

            // Create new Map with basemap and assigning to the MapView's Map
            MyMapView.Map = new Map(new Basemap(_vectorTiledLayer));
        }
    }
}
