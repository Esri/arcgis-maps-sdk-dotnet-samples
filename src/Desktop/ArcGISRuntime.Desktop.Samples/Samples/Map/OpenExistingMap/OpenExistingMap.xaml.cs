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
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;


namespace ArcGISRuntime.Desktop.Samples.OpenExistingMap
{
    public partial class OpenExistingMap
    {
        // Below are three URLs corresponding to Portal Items. In this example the items are Maps. 
        private string itemURL1 = "http://www.arcgis.com/home/item.html?id=2d6fa24b357d427f9c737774e7b0f977";
        private string itemURL2 = "http://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3";
        private string itemURL3 = "http://www.arcgis.com/home/item.html?id=74a8f6645ab44c4f82d537f1aa0e375d";

        // Corresponding Title strings for the itemUrls.  
        private string title1 = "Housing with Mortgages";
        private string title2 = "USA Tapestry Segmentation";
        private string title3 = "Geology of United States";

        // Construct Load Map sample control.
        public OpenExistingMap()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        // Loads UI elements and an initial Map.
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Adding items' Titles and URLs to a collection that will be used to populate the combobox's drop down. 
            ICollection<KeyValuePair<String, String>> comboBoxContent =
                new Dictionary<String, String>()
                {
                    { title1, itemURL1 },
                    { title2, itemURL2 },
                    { title3, itemURL3 }
                };

            try
            {
                comboMap.ItemsSource = comboBoxContent;
                comboMap.SelectedIndex = 0;
                await LoadMapAsync(comboBoxContent.FirstOrDefault().Value);
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
            string url = string.Empty;
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