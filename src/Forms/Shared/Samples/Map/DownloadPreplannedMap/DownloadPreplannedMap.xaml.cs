﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DownloadPreplannedMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Download a preplanned map area",
        "Map",
        "Take a map offline using a preplanned map area",
        "Select a map area to take offline, then use the button to take it offline. Click 'Delete offline areas' to remove any downloaded map areas.")]
    public partial class DownloadPreplannedMap
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

        public DownloadPreplannedMap()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
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
                await ((Page) Parent).DisplayAlert("There was an error.", ex.Message, "OK");
            }
        }

        private async Task DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
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
                    MobileMapPackage localMapArea = await MobileMapPackage.OpenAsync(path);

                    // Open the first map in the package.
                    MyMapView.Map = localMapArea.Maps.First();

                    // Update the UI.
                    BusyText.Text = string.Empty;
                    BusyIndicator.IsVisible = false;
                    MessageLabel.Text = "Opened offline area.";
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    await ((Page) Parent).DisplayAlert("Couldn't open offline area. Proceeding to take area offline.", e.Message, "OK");
                }
            }

            // Create download parameters.
            DownloadPreplannedOfflineMapParameters parameters = await _offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(mapArea);

            // Create the job.
            DownloadPreplannedOfflineMapJob job = _offlineMapTask.DownloadPreplannedOfflineMap(parameters, path);

            // Set up event to update the progress bar while the job is in progress.
            job.ProgressChanged += OnJobProgressChanged;

            try
            {
                // Download the area.
                DownloadPreplannedOfflineMapResult results = await job.GetResultAsync();

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
                    await ((Page) Parent).DisplayAlert("Warning!", errors, "OK");
                }

                // Show the downloaded map.
                MyMapView.Map = results.OfflineMap;

                // Update the UI.
                MessageLabel.Text = "Downloaded preplanned area.";
            }
            catch (Exception ex)
            {
                // Report any errors.
                Debug.WriteLine(ex);
                await ((Page) Parent).DisplayAlert("Downloading map area failed.", ex.Message, "OK");
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
            Device.BeginInvokeOnMainThread(() =>
            {
                // Update the UI with the progress.
                DownloadPreplannedOfflineMapJob downloadJob = sender as DownloadPreplannedOfflineMapJob;
                ProgressView.Progress = downloadJob.Progress / 100.0;
                BusyText.Text = $"Downloading map... {downloadJob.Progress}%";
            });
        }

        private void DeleteAllAreas()
        {
            // Delete all data from the temporary data folder.
            Directory.Delete(_offlineDataFolder, true);
            Directory.CreateDirectory(_offlineDataFolder);

            // Update the UI.
            MessageLabel.Text = "Deleted offline areas.";
        }

        private async void OnDownloadMapAreaClicked(object sender, EventArgs e)
        {
            PreplannedMapArea selectedMapArea = AreasList.SelectedItem as PreplannedMapArea;
            await DownloadMapAreaAsync(selectedMapArea);
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

                // Wait for the garbage collector to get the hint.
                // Areas can't be deleted until handles to geodatabase tables are released.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Delete everything.
                DeleteAllAreas();
            }
            catch (Exception ex)
            {
                // Report the error.
                await ((Page) Parent).DisplayAlert("Deleting map areas failed.", ex.Message, "OK");
            }
            finally
            {
                BusyIndicator.IsVisible = false;
            }
        }
    }
}