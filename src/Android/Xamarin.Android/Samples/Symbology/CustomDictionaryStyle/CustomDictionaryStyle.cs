// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.CustomDictionaryStyle
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Custom dictionary style",
        "Symbology",
        "Use a custom dictionary style (.stylx) to symbolize features using a variety of attribute values.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("751138a2e0844e06853522d54103222a")]
    public class CustomDictionaryStyle : Activity
    {
        // Hold a reference to the MapView.
        private MapView _myMapView;

        // Path for the restaurants style file.
        private readonly string _stylxPath = DataManager.GetDataFolder("751138a2e0844e06853522d54103222a", "Restaurant.stylx");

        // The custom dictionary style for symbolizing restaurants.
        private DictionarySymbolStyle _restaurantStyle;

        // Uri for the restaurants feature service.
        private readonly Uri _restaurantUri = new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/Redlands_Restaurants/FeatureServer/0");

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Custom dictionary style";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Open the custom style file.
                _restaurantStyle = await DictionarySymbolStyle.CreateFromFileAsync(_stylxPath);

                // Create a new map with a streets basemap.
                Map map = new Map(Basemap.CreateStreets());

                // Create the restaurants layer and add it to the map.
                FeatureLayer restaurantLayer = new FeatureLayer(_restaurantUri);
                map.OperationalLayers.Add(restaurantLayer);

                // Load the feature table for the restaurants layer.
                FeatureTable restaurantTable = restaurantLayer.FeatureTable;
                await restaurantTable.LoadAsync();

                // Set the map's initial extent to that of the restaurants.
                map.InitialViewpoint = new Viewpoint(restaurantLayer.FullExtent);

                // Set the map to the map view.
                _myMapView.Map = map;

                // Apply the custom dictionary to the restaurant feature layer.
                ApplyCustomDictionary();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ApplyCustomDictionary()
        {
            // Create overrides for expected field names that are different in this dataset.
            Dictionary<string, string> styleToFieldMappingOverrides = new Dictionary<string, string>();
            styleToFieldMappingOverrides.Add("style", "Style");
            styleToFieldMappingOverrides.Add("price", "Price");
            styleToFieldMappingOverrides.Add("healthgrade", "Inspection");
            styleToFieldMappingOverrides.Add("rating", "Rating");

            // Create overrides for expected text field names (if any).
            Dictionary<string, string> textFieldOverrides = new Dictionary<string, string>();
            textFieldOverrides.Add("name", "Name");

            // Set the text visibility configuration setting.
            _restaurantStyle.Configurations.ToList().Find(c => c.Name == "text").Value = "ON";

            // Create the dictionary renderer with the style file and the field overrides.
            DictionaryRenderer dictRenderer = new DictionaryRenderer(_restaurantStyle, styleToFieldMappingOverrides, textFieldOverrides);

            // Apply the dictionary renderer to the layer.
            FeatureLayer restaurantLayer = _myMapView.Map.OperationalLayers.First() as FeatureLayer;
            restaurantLayer.Renderer = dictRenderer;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}