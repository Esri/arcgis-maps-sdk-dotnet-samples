// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.IO;
using System.Linq;
using Windows.UI.Popups;

namespace ArcGISRuntime.UWP.Samples.ApplyScheduledUpdates
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Apply scheduled updates to preplanned map area",
        "Map",
        "Apply scheduled updates to a downloaded preplanned map area.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class ApplyScheduledUpdates
    {
        // Mobile map package.
        private MobileMapPackage _mobileMapPackage;

        // Task used to sync the mobile map package.
        private OfflineMapSyncTask _offlineMapSyncTask;

        public ApplyScheduledUpdates()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // ArcGIS online item id for the mobile map package.
            string itemId = "740b663bff5e4198b9b6674af93f638a";

            try
            {
                // Clear the exiting sample data.
                Directory.Delete(DataManager.GetDataFolder(itemId, ""), true);
            }
            catch (IOException)
            {
                // Do nothing. Exception happens when sample hasn't been run before and data isn't already present.
            }

            try
            {
                // Download the mobile map package using the sample viewer's data manager.
                await DataManager.DownloadDataItem(itemId);

                // Add an event to close the mobile map package when the sample closes.
                Unloaded += (s, e) => { _mobileMapPackage?.Close(); };

                // Get the folder path to the mobile map package.
                string _mapPackagePath = DataManager.GetDataFolder(itemId, "");

                // Load the mobile map package.
                _mobileMapPackage = new MobileMapPackage(_mapPackagePath);
                await _mobileMapPackage.LoadAsync();

                // Set the mapview to the map from the package.
                Map offlineMap = _mobileMapPackage.Maps[0];
                MyMapView.Map = offlineMap;

                // Create an offline map sync task for the map.
                _offlineMapSyncTask = await OfflineMapSyncTask.CreateAsync(offlineMap);

                // Check if there are scheduled updates to the preplanned map area.
                CheckForScheduledUpdates();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "Error").ShowAsync();
            }
        }

        private async void CheckForScheduledUpdates()
        {
            try
            {
                // Get the information for offline updates.
                OfflineMapUpdatesInfo info = await _offlineMapSyncTask.CheckForUpdatesAsync();

                // Check if there are updates that can be downloaded.
                if (info.DownloadAvailability == OfflineUpdateAvailability.Available)
                {
                    // Get the size of the update.
                    long updateSize = info.ScheduledUpdatesDownloadSize;

                    // Update the UI.
                    InfoLabel.Text = $"Updates: {info.DownloadAvailability}\nUpdate size: {updateSize} bytes.";
                    ApplyButton.IsEnabled = true;
                }
                else
                {
                    // Update the UI.
                    InfoLabel.Text = $"Updates: {info.DownloadAvailability}\nThe preplanned map area is up to date.";
                    ApplyButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "Error").ShowAsync();
            }
        }

        private async void ApplyButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                // Create default sync parameters.
                OfflineMapSyncParameters parameters = await _offlineMapSyncTask.CreateDefaultOfflineMapSyncParametersAsync();

                // Set the parameters to download all updates for the mobile map packages.
                parameters.PreplannedScheduledUpdatesOption = PreplannedScheduledUpdatesOption.DownloadAllUpdates;

                // Set the map package to rollback to the old state should the sync job fail.
                parameters.RollbackOnFailure = true;

                // Create a sync job using the parameters.
                OfflineMapSyncJob offlineMapSyncJob = _offlineMapSyncTask.SyncOfflineMap(parameters);

                // Get the results of the job.
                offlineMapSyncJob.Start();
                OfflineMapSyncResult result = await offlineMapSyncJob.GetResultAsync();

                // Check if the job succeeded.
                if (offlineMapSyncJob.Status == JobStatus.Succeeded)
                {
                    // Check if the map package needs to be re-opened.
                    if (result.IsMobileMapPackageReopenRequired)
                    {
                        // Re-open the mobile map package.
                        _mobileMapPackage.Close();
                        await _mobileMapPackage.LoadAsync();

                        // Check that the mobile map package was loaded.
                        if (_mobileMapPackage.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded && _mobileMapPackage.Maps.Any())
                        {
                            // Set the mapview to the map from the package.
                            Map offlineMap = _mobileMapPackage.Maps[0];
                            MyMapView.Map = offlineMap;

                            // Create an offline map sync task for the map.
                            _offlineMapSyncTask = await OfflineMapSyncTask.CreateAsync(offlineMap);
                        }
                        else
                        {
                            await new MessageDialog("Failed to load the mobile map package.", "Error").ShowAsync();
                        }
                    }

                    // Perform another check for updates, to make sure that the newest update was applied.
                    CheckForScheduledUpdates();
                }
                else
                {
                    await new MessageDialog("Error syncing the offline map.", "Error").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "Error").ShowAsync();
            }
        }
    }
}