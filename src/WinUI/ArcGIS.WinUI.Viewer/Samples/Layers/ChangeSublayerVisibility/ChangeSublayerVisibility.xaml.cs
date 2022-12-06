// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace ArcGIS.WinUI.Samples.ChangeSublayerVisibility
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Map image layer sublayer visibility",
        category: "Layers",
        description: "Change the visibility of sublayers.",
        instructions: "Each sublayer has a check box which can be used to toggle the visibility of the sublayer.",
        tags: new[] { "layers", "sublayers", "visibility" })]
    public partial class ChangeSublayerVisibility
    {
        private ArcGISMapImageLayer _imageLayer;

        public ChangeSublayerVisibility()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map.
            Map myMap = new Map();

            // Create uri to the map image layer.
            Uri serviceUri = new Uri(
               "https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer");

            // Create new image layer from the url.
            _imageLayer = new ArcGISMapImageLayer(serviceUri)
            {
                Name = "World Cities Population"
            };

            // Add created layer to the basemaps collection.
            myMap.Basemap.BaseLayers.Add(_imageLayer);

            // Assign the map to the MapView.
            MyMapView.Map = myMap;
        }

        private async void OnSublayersButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Make sure that layer and it's sublayers are loaded.
                // If layer is already loaded, this returns directly.
                await _imageLayer.LoadAsync();

                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Sublayers",
                    FullSizeDesired = true
                };

                // Create list for layers.
                ListView sublayersListView = new ListView();

                // Create cells for each of the sublayers.
                foreach (ArcGISSublayer sublayer in _imageLayer.Sublayers)
                {
                    // Generate a toggle that provides on/off functionality.
                    ToggleSwitch toggle = new ToggleSwitch()
                    {
                        Header = sublayer.Name,
                        IsOn = sublayer.IsVisible,
                        Margin = new Thickness(5)
                    };

                    // Hook into the On/Off changed event.
                    toggle.Toggled += OnSublayerToggled;

                    // Add cell into the table view.
                    sublayersListView.Items.Add(toggle);
                }

                // Set listview to the dialog.
                dialog.Content = sublayersListView;

                // Add a close button for the dialog.
                dialog.PrimaryButtonText = "Close";
                dialog.PrimaryButtonClick += (s, a) => dialog.Hide();

                // Show dialog as a full screen overlay.
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.ToString(), "Error").ShowAsync();
            }
        }

        private void OnSublayerToggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = (ToggleSwitch)sender;

            // Find the layer from the image layer.
            ArcGISSublayer sublayer = _imageLayer.Sublayers.First(x => x.Name == toggle.Header.ToString());

            // Change sublayers visibility.
            sublayer.IsVisible = toggle.IsOn;
        }
    }
}