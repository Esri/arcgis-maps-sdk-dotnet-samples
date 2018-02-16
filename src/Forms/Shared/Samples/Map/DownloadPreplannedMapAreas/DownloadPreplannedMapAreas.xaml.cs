// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DownloadPreplannedMapAreas
{
    public partial class DownloadPreplannedMapAreas : ContentPage
    {
        // Webmap item that has preplanned areas defined
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
                _offlineDataFolder = Path.Combine(GetDataFolder(),
                    "SampleData", "DownloadPreplannedMapAreas");

                // If the temporary data folder doesn't exist, create it.
                if (!Directory.Exists(_offlineDataFolder))
                    Directory.CreateDirectory(_offlineDataFolder);

                // Create a portal that contains the portal item.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Create webmap based on the id.
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
                BusyIndicator.IsVisible = false;
            }
            catch (Exception ex)
            {
                // Something unexpected happened, show error message
                await DisplayAlert("An error occurred", ex.Message, "OK");
            }
        }

        private async Task DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
            // Setup UI for download.
            ProgressBar.Progress = 0;
            BusyText.Text = "Downloading map area...";
            BusyIndicator.IsVisible = true;
            MyMapView.IsVisible = true;

            // Get the path for the downloaded map area.
            var path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If area is already downloaded, open it and don't download again.
            if (Directory.Exists(path))
            {
                var localMapArea = await MobileMapPackage.OpenAsync(path);
                try
                {
                    MyMapView.Map = localMapArea.Maps.First();
                    BusyText.Text = string.Empty;
                    BusyIndicator.IsVisible = false;
                    return;
                }
                catch (Exception)
                {
                    // Do nothing, continue as if map wasn't downloaded.
                }
            }

            // Create job that is used to do the download.
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
                    foreach (var layerError in results.LayerErrors)
                    {
                        errorBuilder.AppendLine($"{layerError.Key.Name} {layerError.Value.Message}");
                    }

                    // Add table errors to the message.
                    foreach (var tableError in results.TableErrors)
                    {
                        errorBuilder.AppendLine($"{tableError.Key.TableName} {tableError.Value.Message}");
                    }

                    // Show the message.
                    DisplayAlert("Warning!", errorBuilder.ToString(), "OK");
                }

                // Show the Map in the MapView.
                MyMapView.Map = results.OfflineMap;
            }
            catch (Exception ex)
            {
                // Report exception.
                await DisplayAlert("Downloading map area failed.", ex.Message, "OK");
            }
            finally
            {
                // Clear the loading UI.
                BusyText.Text = string.Empty;
                BusyIndicator.IsVisible = false;
            }
        }

        private void OnJobProgressChanged(object sender, EventArgs e)
        {
            // Get the download job.
            var downloadJob = sender as DownloadPreplannedOfflineMapJob;
            if (downloadJob == null) return;

            // UI work needs to be done on the UI thread.
            Device.BeginInvokeOnMainThread(() => 
            {
                // Update UI with the load progress.
                ProgressBar.Progress = downloadJob.Progress / 100.0;
                BusyText.Text = "Downloading: " + downloadJob.Progress + "%";
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
        /// registered.</remarks>
        private async Task UnregisterAndDeleteAllAreas()
        {
            // Find all geodatabase files from the map areas, filtering by the .geodatabase extension.
            var geodatabasesToUnregister = Directory.GetFiles(
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

        private async void OnDownloadMapAreaClicked(object sender, EventArgs e)
        {
            // Return if selection is being cleared.
            if (PreplannedAreasList.SelectedItem == null) { return; }

            // Download and show the map.
            var selectedMapArea = PreplannedAreasList.SelectedItem as PreplannedMapArea;
            await DownloadMapAreaAsync(selectedMapArea);
        }

        private async void OnDeleteAllMapAreasClicked(object sender, EventArgs e)
        {
            try
            {
                // Show the UI for downloading.
                BusyText.Text = "Deleting downloaded map area...";
                BusyIndicator.IsVisible = true;

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

                await UnregisterAndDeleteAllAreas();

                // Update the UI
                PreplannedAreasList.SelectedItem = null;
            }
            catch (Exception ex)
            {
                // Report the error.
                await DisplayAlert("Deleting map areas failed.", ex.Message, "OK");
            }
            finally
            {
                MyMapView.IsVisible = false;
                BusyIndicator.IsVisible = false; 
            }
        }

        // Returns the platform-specific folder for storing offline data.
        private string GetDataFolder()
        {
            var appDataFolder =
#if NETFX_CORE
                Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif __ANDROID__ || __IOS__
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
#endif
            return appDataFolder;
        }
    }
}
