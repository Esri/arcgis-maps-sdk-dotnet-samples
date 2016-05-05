//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;


namespace ArcGISRuntime.Desktop.Samples.OpenExistingMap
{
    public partial class OpenExistingMap
    {
        // String array to hold urls to publicly available web maps
        private string[] _itemURLs = new string[]
        {
            "http://www.arcgis.com/home/item.html?id=2d6fa24b357d427f9c737774e7b0f977",
            "http://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3",
            "http://www.arcgis.com/home/item.html?id=74a8f6645ab44c4f82d537f1aa0e375d"
        };

        // String array to store titles for the webmaps specified above. These titles are in the same order as the urls above
        private string[] _titles = new string[]
        {
            "Housing with Mortgages",
            "USA Tapestry Segmentation",
            "Geology of United States"
        };

        // Construct Load Map sample control.
        public OpenExistingMap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Adding items' Titles and URLs to a collection that will be used to populate the combobox's drop down. 
            ICollection<KeyValuePair<string, string>> comboBoxContent =
                new Dictionary<string, string>
                {
                    { _titles[0], _itemURLs[0] },
                    { _titles[1], _itemURLs[1] },
                    { _titles[2], _itemURLs[2] }
                };

            try
            {
                // Set items to combobox
                comboMap.ItemsSource = comboBoxContent;
                comboMap.SelectedIndex = 0;

                // Create a new Map instance with url of the webmap that is displayed by default
                Map myMap = new Map(new Uri(_itemURLs[0]));

                // Provide used Map to the MapView
                MyMapView.Map = myMap;
            }
            catch (Exception ex)
            {
                var errorMessage = "Map cannot be loaded. " + ex.Message;
                MessageBox.Show(errorMessage, "Sample error");
            }
        }

        // Loads a webmap on load button click.
        private async void OnLoadButtonClicked(object sender, RoutedEventArgs e)
        {
            var url = string.Empty;
            if (comboMap.SelectedIndex >= 0)
            {
                try
                {
                    url = comboMap.SelectedValue as string;
                    await LoadMapAsync(url);
                }
                catch (Exception ex)
                {
                    var errorMessage = "Map cannot be loaded." + ex.Message;
                    MessageBox.Show(errorMessage, "Sample error");
                }
            }
        }

        // Loads the given map.
        private async Task LoadMapAsync(string mapUrl)
        {
            progress.Visibility = Visibility.Visible;

            // Initialize map from a portal item URI and load map into MapView. 
            var map = new Map(new Uri(mapUrl));
            // Await LoadAsync so all properties of map will definitely be loaded when interrogated later. i.e Map.PortalItem. 
            await map.LoadAsync();
            MyMapView.Map = map;

            // Get map's info to populate "Map Details" UI element.          
            var item = MyMapView.Map.PortalItem;
            detailsPanel.DataContext = item;

            detailsPanel.Visibility = Visibility.Visible;
            progress.Visibility = Visibility.Hidden;
        }
    }
}