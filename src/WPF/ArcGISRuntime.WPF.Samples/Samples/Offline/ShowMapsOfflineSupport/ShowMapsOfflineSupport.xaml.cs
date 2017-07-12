// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Windows;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

namespace ArcGISRuntime.WPF.Samples.ShowMapsOfflineSupport
{
    public partial class ShowMapsOfflineSupport
    {
        // String array to hold urls to publicly available web maps
        private string[] _itemIds = new string[]
        {
            "acc027394bc84c2fb04d1ed317aac674",
            "2d6fa24b357d427f9c737774e7b0f977",
            "01f052c8995e4b9e889d73c3e210ebe3",
            "92ad152b9da94dee89b9e387dfe21acd"
        };
        // String array to store titles for the webmaps specified above. These titles are in the same order as the urls above
        private string[] _titles = new string[]
        {
            "Naperville water network",
            "Housing with Mortgages",
            "USA Tapestry Segmentation",
            "Geology of United States"
        };

        // Portal instance that is used to access the webmaps.
        private ArcGISPortal _portal;

        public ShowMapsOfflineSupport()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Create portal and set values to the UI
                _portal = await ArcGISPortal.CreateAsync();
                mapsChooser.ItemsSource = _titles;
                mapsChooser.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
        }
        private async void OnMapsChooseSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Previous results
                noErrorsMessage.Visibility = Visibility.Collapsed;
                someErrorsMessage.Visibility = Visibility.Collapsed;
                allErrorsMessage.Visibility = Visibility.Collapsed;
                layerResultList.ItemsSource = null;
                tableResultList.ItemsSource = null;

                // Show analysing indicator
                analyzingIndicator.Visibility = Visibility.Visible;

                // Get selected map
                var selectedMap = e.AddedItems[0].ToString();
                // Get index that is used to get the selected url
                var selectedIndex = _titles.ToList().IndexOf(selectedMap);

                // Create a portal item for the map that is taken offline based on item id
                PortalItem webmapItem = await PortalItem.CreateAsync(
                    _portal, _itemIds[selectedIndex]);

                // Create task and parameters
                OfflineMapTask task = await OfflineMapTask.CreateAsync(webmapItem);
                GenerateOfflineMapParameters parameters =
                    await task.CreateDefaultGenerateOfflineMapParametersAsync(webmapItem.Extent);

                // Get offline capabilities for the layers
                OfflineMapCapabilities capabilities = await task.GetOfflineMapCapabilitiesAsync(parameters);

                // Show support message in the view
                if (capabilities.HasErrors)
                {
                    // Check if all the layers and tables has errors
                    var layersHasErrors = capabilities.LayerCapabilities.All(capability => !capability.Value.SupportsOffline);
                    var tablesHasErrors = capabilities.TableCapabilities.All(capability => !capability.Value.SupportsOffline);
                    if (layersHasErrors)// && tablesHasErrors)
                    {
                        allErrorsMessage.Visibility = Visibility.Visible;
                    }
                    else
                        someErrorsMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    noErrorsMessage.Visibility = Visibility.Visible;
                }

                // Construct visualization for the layers that supports and doesn't support offline use
                var layerResults = new List<string>();
                foreach (var layerCapability in capabilities.LayerCapabilities)
                {
                    if (layerCapability.Value.SupportsOffline)
                    {
                        var layerResult = string.Format("{0} : OK for offline use.",
                            layerCapability.Key.Name);
                        layerResults.Add(layerResult);
                    }
                    else
                    {
                        var layerResult = string.Format("{0} : {1}",
                            layerCapability.Key.Name,
                            layerCapability.Value.Error.Message);
                        layerResults.Add(layerResult);
                    }
                }
                // Set results to the view
                layerResultList.ItemsSource = layerResults;

                // Construct visualization for the tables that supports and doesn't support offline use
                var tableResults = new List<string>();
                foreach (var tableCapability in capabilities.TableCapabilities)
                {
                    if (tableCapability.Value.SupportsOffline)
                    {
                        var tableResult = string.Format("{0} : OK for offline use.",
                            tableCapability.Key.TableName);
                        tableResults.Add(tableResult);
                    }
                    else
                    {
                        var tableResult = string.Format("{0} : {1}",
                            tableCapability.Key.TableName,
                            tableCapability.Value.Error.Message);
                        tableResults.Add(tableResult);
                    }
                }

                if (!tableResults.Any())
                    tableResults.Add("Map doesn't contain tables.");

                // Set results to the view
                tableResultList.ItemsSource = tableResults;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
            finally
            {
                // Hide analysing indicator
                analyzingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}