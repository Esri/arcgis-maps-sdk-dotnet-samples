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
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ApplyScheduledUpdates
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Apply scheduled updates to preplanned map area",
        category: "Map",
        description: "Apply scheduled updates to a downloaded preplanned map area.",
        instructions: "Start the app. It will display an offline map, check for available updates, and show update availability and size. Select 'Apply Updates' to apply the updates to the local offline map and show the results.",
        tags: new[] { "offline", "pre-planned", "preplanned", "synchronize", "update", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class ApplyScheduledUpdates : ContentPage, IDisposable
    {
        // Mobile map package.
        private MobileMapPackage _mobileMapPackage;

        // Task used to sync the mobile map package.
        private OfflineMapSyncTask _offlineMapSyncTask;

        // ArcGIS online item id for the mobile map package.
        private const string _itemId = "740b663bff5e4198b9b6674af93f638a";

        // Path to the mobile map package.
        private string _mapPackagePath;

        public void Dispose()
        {
            // Close the mobile map package when the sample closes.
            _mobileMapPackage?.Close();
        }

        public ApplyScheduledUpdates()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Clear the exiting sample data.
                Directory.Delete(DataManager.GetDataFolder(_itemId, ""), true);
            }
            catch (IOException)
            {
                // Do nothing. Exception happens when sample hasn't been run before and data isn't already present.
            }

            try
            {
                // Download the mobile map package using the sample viewer's data manager.
                await DataManager.DownloadDataItem(_itemId);

                // Get the folder path to the mobile map package.
                _mapPackagePath = DataManager.GetDataFolder(_itemId, "");

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
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
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
                    double updateSize = info.ScheduledUpdatesDownloadSize / 1024;

                    // Update the UI.
                    InfoLabel.Text = $"Updates: {info.DownloadAvailability}\nUpdate size: {updateSize} kilobytes.";
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
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void ApplyButton_Clicked(object sender, EventArgs e)
        {
            ApplyUpdates();
        }

        private async void ApplyUpdates()
        {
            try
            {
                // Create default sync parameters.
                OfflineMapSyncParameters parameters = await _offlineMapSyncTask.CreateDefaultOfflineMapSyncParametersAsync();

                // Set the parameters to download all updates for the mobile map packages.
                parameters.PreplannedScheduledUpdatesOption = PreplannedScheduledUpdatesOption.DownloadAllUpdates;

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
                        _mobileMapPackage = new MobileMapPackage(_mapPackagePath);
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
                            await Application.Current.MainPage.DisplayAlert("Error", "Failed to load the mobile map package.", "OK");
                        }
                    }

                    // Verify that the map is up to date and change the UI to reflect the update availability status.
                    CheckForScheduledUpdates();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Error syncing the offline map.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}