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
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISRuntime.WinUI.Samples.OpenMapURL
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Open map URL",
        category: "Map",
        description: "Display a web map.",
        instructions: "A web map can be selected from the drop-down list. On selection the web map displays in the map view.",
        tags: new[] { "portal item", "web map" })]
    public partial class OpenMapURL
    {
        // String array to hold URLs to publicly available web maps.
        private readonly string[] _itemUrls =
        {
            "https://www.arcgis.com/home/item.html?id=392451c381ad4109bf04f7bd442bc038",
            "https://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3",
            "https://www.arcgis.com/home/item.html?id=92ad152b9da94dee89b9e387dfe21acd"
        };

        // String array to store titles for the webmaps specified above. These titles are in the same order as the URLs above.
        private readonly string[] _titles =
        {
            "Population Pressure",
            "USA Tapestry Segmentation",
            "Geology of United States"
        };

        public OpenMapURL()
        {
            InitializeComponent();

            // Select the first item.
            MapList.ItemsSource = _titles;
            MapList.SelectedIndex = 0;
        }

        private void OnMapsChooseSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedMapName = e.AddedItems[0].ToString();

            // Get index that is used to get the selected URL.
            int selectedIndex = _titles.ToList().IndexOf(selectedMapName);

            // Create a new Map instance with URL of the webmap that selected.
            MyMapView.Map = new Map(new Uri(_itemUrls[selectedIndex]));
        }
    }
}
