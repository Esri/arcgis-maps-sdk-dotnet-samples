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
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Windows.Samples.ArcGISVectorTiledLayerUrl
{
    public partial class ArcGISVectorTiledLayerUrl
    {
        private string _navigationUrl = "http://www.arcgis.com/sharing/rest/content/items/00cd8e843bae49b3a040423e5d65416b/resources/styles/root.json";
        private string _streetUrl = "http://www.arcgis.com/sharing/rest/content/items/3b8814f6ddbd485cae67e8018992246e/resources/styles/root.json";
        private string _nightUrl = "http://www.arcgis.com/sharing/rest/content/items/f96366254a564adda1dc468b447ed956/resources/styles/root.json";
        private string _topographicUrl = "http://www.arcgis.com/sharing/rest/content/items/be44936bcdd24db588a1ae5076e36f34/resources/styles/root.json";

        private string _vectorTiledLayerUrl;
        private ArcGISVectorTiledLayer _vectorTiledLayer;

        // String array to store some vector layer choices
        private string[] _vectorLayerNames = new string[]
        {
            "Topo",
            "Streets",
            "Night",
            "Navigation"
        };

        public ArcGISVectorTiledLayerUrl()
        {
            this.InitializeComponent();

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
                case "Topo":
                    _vectorTiledLayerUrl = _topographicUrl;
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
