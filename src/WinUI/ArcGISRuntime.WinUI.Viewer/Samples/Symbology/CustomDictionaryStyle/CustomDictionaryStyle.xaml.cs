// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using System;

namespace ArcGISRuntime.WinUI.Samples.CustomDictionaryStyle
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Custom dictionary style",
        category: "Symbology",
        description: "Use a custom dictionary style (.stylx) to symbolize features using a variety of attribute values.",
        instructions: "Pan and zoom the map to see the symbology from the custom dictionary style.",
        tags: new[] { "ArcGIS Online", "dictionary", "military", "renderer", "style", "stylx", "unique value", "visualization", "web styles", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("751138a2e0844e06853522d54103222a")]
    public partial class CustomDictionaryStyle
    {
        // Path for the restaurants style file.
        private readonly string _stylxPath = DataManager.GetDataFolder("751138a2e0844e06853522d54103222a", "Restaurant.stylx");

        // Uri for the restaurants feature service.
        private readonly Uri _restaurantUri = new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/Redlands_Restaurants/FeatureServer/0");

        // Hold a reference to the custom dictionary styles for use in the event handlers. 
        private DictionarySymbolStyle _localRestaurantStyle;
        private DictionarySymbolStyle _restaurantStyleFromPortal;

        // Hold a reference to the feature layer for use in the event handlers.
        private FeatureLayer _restaurantLayer;

        public CustomDictionaryStyle()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Create a new map with a topographic basemap.
                Map map = new Map(BasemapStyle.ArcGISTopographic);

                // Create the restaurants layer and add it to the map.
                _restaurantLayer = new FeatureLayer(_restaurantUri);
                map.OperationalLayers.Add(_restaurantLayer);

                // Load the feature table for the restaurants layer.
                await _restaurantLayer.FeatureTable.LoadAsync();

                // Open the local custom style file.
                _localRestaurantStyle = await DictionarySymbolStyle.CreateFromFileAsync(_stylxPath);

                // Open a portal item containing a dictionary symbol style.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();
                PortalItem portalItem = await PortalItem.CreateAsync(portal, "adee951477014ec68d7cf0ea0579c800");

                // Open the portal custom style file.
                _restaurantStyleFromPortal = await DictionarySymbolStyle.OpenAsync(portalItem);

                // Set the map's initial extent to that of the restaurants.
                map.InitialViewpoint = new Viewpoint(_restaurantLayer.FullExtent);

                // Set the map to the map view.
                MyMapView.Map = map;

                // Add event handlers to the radio buttons. 
                LocalStyleButton.Checked += LocalStyleButton_Checked;
                WebStyleButton.Checked += WebStyleButton_Checked; 

                // Set the local style radio button to be checked.  
                LocalStyleButton.IsChecked = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void LocalStyleButton_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Create the dictionary renderer with the style file and the field overrides.
            DictionaryRenderer dictionaryRenderer = new DictionaryRenderer(_localRestaurantStyle);

            // Set the restaurant layer renderer to the dictionary renderer.
            _restaurantLayer.Renderer = dictionaryRenderer;
        }

        private void WebStyleButton_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Create the dictionary renderer with the style file and the field overrides.
            DictionaryRenderer dictionaryRenderer = new DictionaryRenderer(_restaurantStyleFromPortal);

            // Set the restaurant layer renderer to the dictionary renderer.
            _restaurantLayer.Renderer = dictionaryRenderer;
        }
    }
}