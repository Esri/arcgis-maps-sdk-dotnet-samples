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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.ChangeSublayerVisibility
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change sublayer visibility",
        "Layers",
        "This sample demonstrates how to show or hide sublayers of a map image layer.",
        "")]
    public partial class ChangeSublayerVisibility
    {
        private ArcGISMapImageLayer _imageLayer;

        public ChangeSublayerVisibility()
        {
            InitializeComponent();

            // Setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create uri to the map image layer
            var serviceUri = new Uri(
               "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer");

            // Create new image layer from the url
            _imageLayer = new ArcGISMapImageLayer(serviceUri)
            {
                Name = "World Cities Population"
            };

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(_imageLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnSublayersButtonClicked(object sender, RoutedEventArgs e)
        {
            // Make sure that layer and it's sublayers are loaded
            // If layer is already loaded, this returns directly
            await _imageLayer.LoadAsync();

            var dialog = new ContentDialog()
            {
                Title = "Sublayers",
                FullSizeDesired = true
            };

            // Create list for layers
            var sublayersListView = new ListView();

            // Create cells for each of the sublayers
            foreach (ArcGISSublayer sublayer in _imageLayer.Sublayers)
            {
                // Using a toggle that provides on/off functionality
                var toggle = new ToggleSwitch()
                {
                    Header = sublayer.Name,
                    IsOn = sublayer.IsVisible,
                    Margin = new Thickness(5)
                };

                // Hook into the On/Off changed event
                toggle.Toggled += OnSublayerToggled;
                     
                // Add cell into the table view
                sublayersListView.Items.Add(toggle);
            }

            // Set listview to the dialog
            dialog.Content = sublayersListView;

            // Show dialog as a full screen overlay. 
            await dialog.ShowAsync();
        }

        private void OnSublayerToggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;

            // Find the layer from the image layer
            ArcGISSublayer sublayer = _imageLayer.Sublayers.First(x => x.Name == toggle.Header.ToString());

            // Change sublayers visibility
            sublayer.IsVisible = toggle.IsOn;
        }
    }
}
