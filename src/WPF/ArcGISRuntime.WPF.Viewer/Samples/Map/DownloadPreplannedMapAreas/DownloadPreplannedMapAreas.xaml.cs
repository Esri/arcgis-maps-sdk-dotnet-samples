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
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Download preplanned map areas",
        "Map",
        "This sample demonstrates how to download preplanned map areas from a webmap. In the preplanned offline workflow, the author of the online map defines map areas for offline use. When these areas are created, their offline packages are created and stored online for clients to download. This is the biggest difference between on-demand and preplanned workflows since on-demand workflow data packages are generated at request time.",
        "Select an area from the list to download. When you're done, select 'delete offline areas' to delete the downloaded copy of the map areas. ")]
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

            // Create the UI, setup the control references and execute initialization
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
                foreach (PreplannedMapArea area in preplannedAreas)
                {
                    await area.LoadAsync();
                }

                // Show the areas in the UI. The code to make this look nice in the UI is defined
                //    in the ListView's ItemTemplate in the XAML.
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
            string path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If the area is already downloaded, open it and don't download it again.
            if (Directory.Exists(path))
            {
                MobileMapPackage localMapArea = await MobileMapPackage.OpenAsync(path);
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
                    StringBuilder errorBuilder = new StringBuilder();

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
            DownloadPreplannedOfflineMapJob downloadJob = sender as DownloadPreplannedOfflineMapJob;
            if (downloadJob == null) return;

            // UI work needs to be done on the UI thread.
            Dispatcher.Invoke(() =>
            {
                // Update UI with the load progress.
                ProgressBar.Value = downloadJob.Progress;
                BusyPercentage.Text = downloadJob.Progress + "%";
            });
        }

        private async void OnDeleteAllMapAreasClicked(object sender, RoutedEventArgs e)
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


            try
            {
                // Find all downloaded offline areas from the sample folder.
                List<string> downloadedPackagePaths = Directory.GetDirectories(_offlineDataFolder).ToList();

                foreach (string packagePath in downloadedPackagePaths)
                {
                    MobileMapPackage downloadedAreaPackage = await MobileMapPackage.OpenAsync(packagePath);
                    if (!downloadedAreaPackage.Maps.Any())
                    {
                        // Delete temporary data folder from potential stray folders.
                        Directory.Delete(_offlineDataFolder, true);
                    }
                    else
                    {
                        // Unregister all geodatabases and delete the package.
                        await UnregisterAndRemoveMobileMapPackage(downloadedAreaPackage);
                    }
                }
            }
            catch (Exception ex)
            {
                // Report error accessing a resource.
                MessageBox.Show(ex.Message, "Deleting map areas failed");
            }
            finally
            {
                // Reset the UI.
                DownloadNotificationText.Visibility = Visibility.Visible;
                MyMapView.Visibility = Visibility.Collapsed;
                BusyIndicator.Visibility = Visibility.Collapsed;
                PreplannedAreasList.SelectedItem = null;
            }
        }

        private async Task UnregisterAndRemoveMobileMapPackage(MobileMapPackage mobileMapPackage)
        {
            // Unregister all geodatabases from all maps that are part of the mobile map package.
            // Offline areas that are downloaded by using OfflineMapTask will contain a single
            // map in them but it is a good practice to handle the case of multiple maps.
            foreach (Map map in mobileMapPackage.Maps)
            {
                // Find all geodatabases from the used map.
                List<Geodatabase> geodatabasesToUnregister = new List<Geodatabase>();

                // Add all geodatabases used in the feature layers.
                foreach (FeatureLayer featureLayer in map.OperationalLayers.OfType<FeatureLayer>())
                {
                    GeodatabaseFeatureTable geodatabaseFeatureTable = featureLayer.FeatureTable as GeodatabaseFeatureTable;
                    if (geodatabaseFeatureTable == null)
                        continue;
                    // Add the geodatabase feature table if it isn't already in the list.
                    if (geodatabasesToUnregister.All(x => x.Path != geodatabaseFeatureTable.Geodatabase.Path))
                    {
                        geodatabasesToUnregister.Add(geodatabaseFeatureTable.Geodatabase);
                    }
                }

                // Add all geodatabases used in a table.
                foreach (FeatureTable featureTable in map.Tables)
                {
                    GeodatabaseFeatureTable geodatabaseFeatureTable = featureTable as GeodatabaseFeatureTable;
                    if (geodatabaseFeatureTable == null)
                        continue;
                    // Add the geodatabase feature table if it isn't already in the list.
                    if (geodatabasesToUnregister.All(x => x.Path != geodatabaseFeatureTable.Geodatabase.Path))
                    {
                        geodatabasesToUnregister.Add(geodatabaseFeatureTable.Geodatabase);
                    }
                }

                // Unregister geodatabases that were used.
                foreach (Geodatabase geodatabaseToUnregister in geodatabasesToUnregister)
                {
                    GeodatabaseSyncTask geodatabaSyncTask = await GeodatabaseSyncTask.CreateAsync(geodatabaseToUnregister.Source);
                    await geodatabaSyncTask.UnregisterGeodatabaseAsync(geodatabaseToUnregister);
                }

                // Make sure that all geodatabases are closed and locks released.
                foreach (Geodatabase geodatabase in geodatabasesToUnregister)
                {
                    geodatabase.Close();
                }
            }

            // Remove package.
            Directory.Delete(mobileMapPackage.Path, true);
        }

        private async void OnDownloadMapAreaClicked(object sender, RoutedEventArgs e)
        {
            // Return if selection is being cleared.
            if (PreplannedAreasList.SelectedIndex == -1)
            {
                return;
            }

            // Download and show the map.
            PreplannedMapArea selectedMapArea = PreplannedAreasList.SelectedItem as PreplannedMapArea;
            await DownloadMapAreaAsync(selectedMapArea);
        }

        // Returns the platform-specific folder for storing offline data.
        private string GetDataFolder()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}