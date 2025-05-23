﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using System.Diagnostics;

namespace ArcGIS.Samples.DownloadPreplannedMap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Download preplanned map area",
        category: "Map",
        description: "Take a map offline using a preplanned map area.",
        instructions: "Select a map area from the Preplanned Map Areas list. Tap the button to download the selected area. The download progress will be shown in the Downloads list. When a download is complete, select it to display the offline map in the map view.",
        tags: new[] { "map area", "offline", "pre-planned", "preplanned" })]
    public partial class DownloadPreplannedMap : IDisposable
    {
        // ID of a web map with preplanned map areas.
        private const string PortalItemId = "acc027394bc84c2fb04d1ed317aac674";

        // Folder to store the downloaded mobile map packages.
        private string _offlineDataFolder;

        // Task for taking map areas offline.
        private OfflineMapTask _offlineMapTask;

        // Hold onto the original map.
        private Map _originalMap;

        // Hold list of map areas for use in the UI.
        private readonly List<PreplannedMapArea> _mapAreas = new List<PreplannedMapArea>();

        // Most recently opened map package.
        private MobileMapPackage _mobileMapPackage;

        public DownloadPreplannedMap()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // The data manager provides a method to get a suitable offline data folder.
                _offlineDataFolder = Path.Combine(DataManager.GetDataFolder(), "SampleData", "DownloadPreplannedMapAreas");

                // If temporary data folder doesn't exists, create it.
                if (!Directory.Exists(_offlineDataFolder))
                {
                    Directory.CreateDirectory(_offlineDataFolder);
                }

                // Create a portal to enable access to the portal item.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Create the portal item from the portal item ID.
                PortalItem webMapItem = await PortalItem.CreateAsync(portal, PortalItemId);

                // Show the map.
                _originalMap = new Map(webMapItem);
                MyMapView.Map = _originalMap;

                // Create an offline map task for the web map item.
                _offlineMapTask = await OfflineMapTask.CreateAsync(webMapItem);

                // Find the available preplanned map areas.
                IReadOnlyList<PreplannedMapArea> preplannedAreas = await _offlineMapTask.GetPreplannedMapAreasAsync();

                // Load each item, then add it to the list of areas.
                foreach (PreplannedMapArea area in preplannedAreas)
                {
                    await area.LoadAsync();
                    _mapAreas.Add(area);
                }

                // Show the map areas in the UI.
                AreasList.ItemsSource = _mapAreas;

                // Hide the loading indicator.
                BusyIndicator.IsVisible = false;
            }
            catch (Exception ex)
            {
                // Something unexpected happened, show the error message.
                Debug.WriteLine(ex);
                await Application.Current.Windows[0].Page.DisplayAlert("There was an error.", ex.Message, "OK");
            }
        }

        private void ShowOnlineButton_Clicked(object sender, EventArgs e)
        {
            // Show the online map.
            MyMapView.Map = _originalMap;

            // Disable the button.
            ShowOnlineButton.IsEnabled = false;
        }

        private async Task DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
            // Close the current mobile package.
            _mobileMapPackage?.Close();

            // Set up UI for downloading.
            ProgressView.Progress = 0;
            BusyText.Text = "Downloading map area...";
            BusyIndicator.IsVisible = true;

            // Create folder path where the map package will be downloaded.
            string path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If the area is already downloaded, open it.
            if (Directory.Exists(path))
            {
                try
                {
                    // Open the offline map package.
                    _mobileMapPackage = await MobileMapPackage.OpenAsync(path);

                    // Open the first map in the package.
                    MyMapView.Map = _mobileMapPackage.Maps.First();

                    // Update the UI.
                    BusyText.Text = string.Empty;
                    BusyIndicator.IsVisible = false;
                    MessageLabel.Text = "Opened offline area.";
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    await Application.Current.Windows[0].Page.DisplayAlert("Couldn't open offline area. Proceeding to take area offline.", e.Message, "OK");
                }
            }

            // Create download parameters.
            DownloadPreplannedOfflineMapParameters parameters = await _offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(mapArea);

            // Set the update mode to not receive updates.
            parameters.UpdateMode = PreplannedUpdateMode.NoUpdates;

            // Create the job.
            DownloadPreplannedOfflineMapJob job = _offlineMapTask.DownloadPreplannedOfflineMap(parameters, path);

            // Set up event to update the progress bar while the job is in progress.
            job.ProgressChanged += OnJobProgressChanged;

            try
            {
                // Download the area.
                DownloadPreplannedOfflineMapResult results = await job.GetResultAsync();

                // Set the current mobile map package.
                _mobileMapPackage = results.MobileMapPackage;

                // Handle possible errors and show them to the user.
                if (results.HasErrors)
                {
                    // Accumulate all layer and table errors into a single message.
                    string errors = "";

                    foreach (KeyValuePair<Layer, Exception> layerError in results.LayerErrors)
                    {
                        errors = $"{errors}\n{layerError.Key.Name} {layerError.Value.Message}";
                    }

                    foreach (KeyValuePair<FeatureTable, Exception> tableError in results.TableErrors)
                    {
                        errors = $"{errors}\n{tableError.Key.TableName} {tableError.Value.Message}";
                    }

                    // Show the message.
                    await Application.Current.Windows[0].Page.DisplayAlert("Warning!", errors, "OK");
                }

                // Show the downloaded map.
                MyMapView.Map = results.OfflineMap;

                // Update the UI.
                ShowOnlineButton.IsEnabled = true;
                MessageLabel.Text = "Downloaded preplanned area.";
                DownloadButton.Text = "Display";
            }
            catch (Exception ex)
            {
                // Report any errors.
                Debug.WriteLine(ex);
                await Application.Current.Windows[0].Page.DisplayAlert("Downloading map area failed.", ex.Message, "OK");
            }
            finally
            {
                BusyText.Text = string.Empty;
                BusyIndicator.IsVisible = false;
            }
        }

        private void OnJobProgressChanged(object sender, EventArgs e)
        {
            // Because the event is raised on a background thread, the dispatcher must be used to
            // ensure that UI updates happen on the UI thread.
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                // Update the UI with the progress.
                DownloadPreplannedOfflineMapJob downloadJob = sender as DownloadPreplannedOfflineMapJob;
                ProgressView.Progress = downloadJob.Progress / 100.0;
                BusyText.Text = $"Downloading map... {downloadJob.Progress}%";
            });
        }

        private void OnDownloadMapAreaClicked(object sender, EventArgs e)
        {
            if (AreasList.SelectedItem != null)
            {
                PreplannedMapArea selectedMapArea = AreasList.SelectedItem as PreplannedMapArea;
                _ = DownloadMapAreaAsync(selectedMapArea);
            }
        }

        private async void OnDeleteAllMapAreasClicked(object sender, EventArgs e)
        {
            try
            {
                // Set up UI for downloading.
                BusyText.Text = "Deleting downloaded map area...";
                BusyIndicator.IsVisible = true;

                // Reset the map.
                MyMapView.Map = _originalMap;

                // Close the current mobile package.
                _mobileMapPackage?.Close();

                // Delete all data from the temporary data folder.
                Directory.Delete(_offlineDataFolder, true);
                Directory.CreateDirectory(_offlineDataFolder);

                // Update the UI.
                MessageLabel.Text = "Deleted downloaded areas.";
                DownloadButton.Text = "Download";
                MessageLabel.Text = "Downloaded preplanned area.";
            }
            catch (Exception ex)
            {
                // Report the error.
                await Application.Current.Windows[0].Page.DisplayAlert("Deleting map areas failed.", ex.Message, "OK");
            }
            finally
            {
                BusyIndicator.IsVisible = false;
            }
        }

        private void AreaSelected(object sender, SelectedItemChangedEventArgs e)
        {
            PreplannedMapArea selectedMapArea = e.SelectedItem as PreplannedMapArea;
            string path = Path.Combine(_offlineDataFolder, selectedMapArea.PortalItem.Title);
            if (Directory.Exists(path))
            {
                DownloadButton.Text = "Display";
            }
            else
            {
                DownloadButton.Text = "Download";
            }
        }

        public void Dispose()
        {
            // Close the current mobile package.
            _mobileMapPackage?.Close();
        }
    }
}