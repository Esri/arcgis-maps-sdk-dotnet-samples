// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ShowMapsOfflineSupport
{
    public partial class ShowMapsOfflineSupport : ContentPage
    {
        // publicly available webmaps (item id, title)
        private KeyValuePair<string, string>[] _portalItems = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("acc027394bc84c2fb04d1ed317aac674", "Naperville water network"),
            new KeyValuePair<string, string>("2d6fa24b357d427f9c737774e7b0f977", "Housing with Mortgages"),
            new KeyValuePair<string, string>("01f052c8995e4b9e889d73c3e210ebe3", "USA Tapestry Segmentation"),
            new KeyValuePair<string, string>("92ad152b9da94dee89b9e387dfe21acd", "Geology of United States")
        };

        // Cache default portal to access webmaps
        private ArcGISPortal _portal;

        public ShowMapsOfflineSupport()
        {
            InitializeComponent();

            Title = "Show maps' offline support";

            // Display only webmap titles
            foreach (var kvp in _portalItems)
                MapPicker.Items.Add(kvp.Value);
        }

        private async void OnMapPickerSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (_portal == null)
                    _portal = await ArcGISPortal.CreateAsync();

                var item = _portalItems[MapPicker.SelectedIndex];

                // Create portal item with specified item id
                var portalItem = await PortalItem.CreateAsync(_portal, item.Key);

                // Create task and parameters based on portal item extent metadata
                var task = await OfflineMapTask.CreateAsync(portalItem);
                var parameters = await task.CreateDefaultGenerateOfflineMapParametersAsync(portalItem.Extent);

                // Get offline capabilities for layers and table
                var capabilities = await task.GetOfflineMapCapabilitiesAsync(parameters);

                // Interrogate results for errors
                var noOfflineSupport = capabilities.LayerCapabilities.All(c => !c.Value.SupportsOffline)
                    && capabilities.TableCapabilities.All(c => !c.Value.SupportsOffline);

                // Quick summary for map content offline capability
                if (noOfflineSupport)
                {
                    OfflineMapCapabilityResult.BackgroundColor = Color.Red;
                    OfflineMapCapabilityResult.Text = "Map content does not support offline.";
                }
                else if (capabilities.HasErrors)
                {
                    OfflineMapCapabilityResult.BackgroundColor = Color.Yellow;
                    OfflineMapCapabilityResult.Text = "Map content partially supports offline.";
                }
                else
                {
                    OfflineMapCapabilityResult.BackgroundColor = Color.Green;
                    OfflineMapCapabilityResult.Text = "All map content supports offline.";
                }

                // Displays offline capability per layer and table with their associated error if any
                var layerResults = new List<string>();
                if (capabilities.LayerCapabilities.Count == 0)
                    layerResults.Add("Map does not contain layers.");
                else
                {
                    foreach (var layerCapability in capabilities.LayerCapabilities)
                    {
                        var layer = layerCapability.Key;
                        var offlineCapability = layerCapability.Value;
                        var status = offlineCapability.SupportsOffline ? "OK for offline use" : offlineCapability.Error?.Message;
                        layerResults.Add($"{layer.Name} : {status}");
                    }
                }
                LayerOfflineCapabilities.ItemsSource = layerResults;
                var tableResults = new List<string>();
                if (capabilities.TableCapabilities.Count == 0)
                    tableResults.Add("Map does not contain tables.");
                else
                {
                    foreach (var tableCapability in capabilities.TableCapabilities)
                    {
                        var table = tableCapability.Key;
                        var offlineCapability = tableCapability.Value;
                        var status = offlineCapability.SupportsOffline ? "OK for offline use" : offlineCapability.Error?.Message;
                        tableResults.Add($"{table.TableName} : {status}");
                    }
                }
                TableOfflineCapabilities.ItemsSource = tableResults;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}