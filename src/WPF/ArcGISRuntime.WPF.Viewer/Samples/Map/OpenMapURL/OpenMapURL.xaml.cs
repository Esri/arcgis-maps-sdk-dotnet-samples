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

namespace ArcGISRuntime.WPF.Samples.OpenMapURL
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open map (URL)",
        "Map",
        "This sample demonstrates loading a webmap in a map from a Uri.",
        "")]
    public partial class OpenMapURL
    {
        // String array to hold urls to publicly available web maps
        private string[] _itemURLs = new string[]
        {
            "http://www.arcgis.com/home/item.html?id=2d6fa24b357d427f9c737774e7b0f977",
            "http://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3",
            "http://www.arcgis.com/home/item.html?id=92ad152b9da94dee89b9e387dfe21acd"
        };

        // String array to store titles for the webmaps specified above. These titles are in the same order as the urls above
        private string[] _titles = new string[]
        {
            "Housing with Mortgages",
            "USA Tapestry Segmentation",
            "Geology of United States"
        };

        // Construct Load Map sample control.
        public OpenMapURL()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Set titles as a items source
            mapsChooser.ItemsSource = _titles;
            
            // Select the first option in the map titles. 
            mapsChooser.SelectedIndex = 0;

            // Create a new Map instance with url of the webmap that is displayed by default
            Map myMap = new Map(new Uri(_itemURLs[0]));

            // Provide used Map to the MapView
            MyMapView.Map = myMap;
        }

        private void OnMapsChooseSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedMap = e.AddedItems[0].ToString();

            // Get index that is used to get the selected url
            var selectedIndex = _titles.ToList().IndexOf(selectedMap);

            // Create a new Map instance with url of the webmap that selected
            MyMapView.Map = new Map(new Uri(_itemURLs[selectedIndex]));
        }
    }
}