// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.DownloadPreplannedMapAreas
{
    public partial class DownloadPreplannedMapAreas
    {
        // ID of webmap item that has preplanned areas defined
        private const string PortalItemId = "acc027394bc84c2fb04d1ed317aac674";

        // Folder where the areas are downloaded
        private string _offlineDataFolder;

        // Task that is used to work with preplanned map areas
        private OfflineMapTask _offlineMapTask;

        public DownloadPreplannedMapAreas()
        {
            InitializeComponent();

            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Get the offline data folder.
                _offlineDataFolder = Path.Combine(GetDataFolder(),
                    "SampleData", "DownloadPreplannedMapAreas");

                // If the temporary data folder doesn't exist, create it.
                if (!Directory.Exists(_offlineDataFolder))
                {
                    Directory.CreateDirectory(_offlineDataFolder);
                }

                // Create a portal that contains the portal item.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Create the webmap based on the id.
                PortalItem webmapItem = await PortalItem.CreateAsync(portal, PortalItemId);

                // Create the offline task and load it.
                _offlineMapTask = await OfflineMapTask.CreateAsync(webmapItem);

                // Query related preplanned areas.
                IReadOnlyList<PreplannedMapArea> preplannedAreas = await _offlineMapTask.GetPreplannedMapAreasAsync();

                // Load each preplanned map area.
                foreach (var area in preplannedAreas)
                {
                    await area.LoadAsync();
                }

                // Show the areas in the UI.
                PreplannedAreasList.ItemsSource = preplannedAreas;

                // Remove loading indicators from UI.
                DownloadNotificationText.Visibility = Visibility.Visible;
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Something unexpected happened, show the error message.
                MessageBox.Show(ex.Message, "An error occurred.");
            }
        }

        private async Task DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
            // Configure UI for the download.
            DownloadNotificationText.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = 0;
            BusyText.Text = "Downloading map area...";
            BusyIndicator.Visibility = Visibility.Visible;
            MyMapView.Visibility = Visibility.Visible;

            // Get the path for the downloaded map area.
            var path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If the area is already downloaded, open it and don't download it again.
            if (Directory.Exists(path))
            {
                var localMapArea = await MobileMapPackage.OpenAsync(path);
                try
                {
                    // Load the map.
                    MyMapView.Map = localMapArea.Maps.First();

                    // Update the UI.
                    BusyText.Text = string.Empty;
                    BusyIndicator.Visibility = Visibility.Collapsed;

                    // Return and don't proceed to download.
                    return;
                }
                catch (Exception)
                {
                    // Do nothing, continue as if map wasn't downloaded.
                }
            }

            // Create job that is used to download the map area.
            DownloadPreplannedOfflineMapJob job = _offlineMapTask.DownloadPreplannedOfflineMap(mapArea, path);

            // Subscribe to progress change events to support showing a progress bar.
            job.ProgressChanged += OnJobProgressChanged;

            try
            {
                // Download the map area.
                DownloadPreplannedOfflineMapResult results = await job.GetResultAsync();

                // Handle possible errors and show them to the user.
                if (results.HasErrors)
                {
                    var errorBuilder = new StringBuilder();

                    // Add layer errors to the message.
                    foreach (KeyValuePair<Layer, Exception> layerError in results.LayerErrors)
                    {
                        errorBuilder.AppendLine(string.Format("{0} {1}", layerError.Key.Name,
                            layerError.Value.Message));
                    }

                    // Add table errors to the message.
                    foreach (KeyValuePair<FeatureTable, Exception> tableError in results.TableErrors)
                    {
                        errorBuilder.AppendLine(string.Format("{0} {1}", tableError.Key.TableName,
                            tableError.Value.Message));
                    }

                    // Show the message.
                    MessageBox.Show(errorBuilder.ToString(), "Warning!");
                }

                // Show the Map in the MapView.
                MyMapView.Map = results.OfflineMap;
            }
            catch (Exception ex)
            {
                // Report exception.
                MessageBox.Show(ex.Message, "Downloading map area failed.");
            }
            finally
            {
                // Clear the loading UI.
                BusyText.Text = string.Empty;
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void OnJobProgressChanged(object sender, EventArgs e)
        {
            // Get the download job.
            var downloadJob = sender as DownloadPreplannedOfflineMapJob;
            if (downloadJob == null) return;

            // UI work needs to be done on the UI thread.
            Dispatcher.Invoke(() =>
            {
                // Update UI with the load progress.
                ProgressBar.Value = downloadJob.Progress;
                BusyPercentage.Text = downloadJob.Progress + "%";
            });
        }

        /// <summary>
        /// Find all geodatabases from all the downloaded map areas, unregister them then re-create
        /// the temporary data folder.
        /// </summary>
        /// <remarks>
        /// When an area is downloaded, geodatabases are registered with the original service to support
        /// synchronization. When the area is deleted from the device, it is important first to unregister
        /// all geodatabases that are used in the map so the service doesn't have stray geodatabases
        /// registered.
        /// </remarks>
        private async Task UnregisterAndDeleteAllAreas()
        {
            // Find all geodatabase files from the map areas, filtering by the .geodatabase extension.
            string[] geodatabasesToUnregister = Directory.GetFiles(
                _offlineDataFolder, "*.geodatabase", SearchOption.AllDirectories);

            // Unregister all geodatabases.
            foreach (var geodatabasePath in geodatabasesToUnregister)
            {
                // First open the geodatabase.
                var geodatabase = await Geodatabase.OpenAsync(geodatabasePath);
                try
                {
                    // Create the sync task from the geodatabase source.
                    var geodatabaSyncTask = await GeodatabaseSyncTask.CreateAsync(geodatabase.Source);

                    // Unregister the geodatabase.
                    await geodatabaSyncTask.UnregisterGeodatabaseAsync(geodatabase);
                }
                catch (Exception)
                {
                    // Unable to unregister the geodatabase, proceed to delete.
                }
                finally
                {
                    // Ensure the geodatabase is closed before deleting it.
                    geodatabase.Close();
                }
            }

            // Delete all data from the temporary data folder.
            Directory.Delete(_offlineDataFolder, true);
            Directory.CreateDirectory(_offlineDataFolder);
        }

        private async void OnDownloadMapAreaClicked(object sender, RoutedEventArgs e)
        {
            // Return if selection is being cleared.
            if (PreplannedAreasList.SelectedIndex == -1)
            {
                return;
            }

            // Download and show the map.
            var selectedMapArea = PreplannedAreasList.SelectedItem as PreplannedMapArea;
            await DownloadMapAreaAsync(selectedMapArea);
        }

        private async void OnDeleteAllMapAreasClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show the map area deletion UI.
                DownloadNotificationText.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = true;
                BusyText.Text = "Deleting downloaded map areas...";
                BusyIndicator.Visibility = Visibility.Visible;

                // If there is a map loaded, remove it.
                if (MyMapView.Map != null)
                {
                    // Clear the layers. This will ensure that their resources are freed.
                    MyMapView.Map.OperationalLayers.Clear();
                    MyMapView.Map = null;
                }

                // Ensure the map and related resources (for example, handles to geodatabases) are cleared.
                //    This is important on Windows where open files can't be deleted.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Delete the map areas.
                await UnregisterAndDeleteAllAreas();

                // Update the UI.
                PreplannedAreasList.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                // Report the error.
                MessageBox.Show(ex.Message, "Deleting map areas failed");
            }
            finally
            {
                // Reset the UI.
                DownloadNotificationText.Visibility = Visibility.Visible;
                MyMapView.Visibility = Visibility.Collapsed;
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // Returns the platform-specific folder for storing offline data.
        private string GetDataFolder()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}