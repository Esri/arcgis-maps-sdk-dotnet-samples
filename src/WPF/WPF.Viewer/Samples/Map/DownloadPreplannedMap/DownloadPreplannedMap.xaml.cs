// Copyright 2019 Esri.
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.DownloadPreplannedMap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Download preplanned map area",
        category: "Map",
        description: "Take a map offline using a preplanned map area.",
        instructions: "Select a map area from the Preplanned Map Areas list. Click the button to download the selected area. The download progress will be shown in the Downloads list. When a download is complete, select it to display the offline map in the map view.",
        tags: new[] { "map area", "offline", "pre-planned", "preplanned" })]
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
                // Close the current mobile package when the sample closes.
                Unloaded += (s, e) => { _mobileMapPackage?.Close(); };

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

                // Load each item, then add it to the UI.
                foreach (PreplannedMapArea area in preplannedAreas)
                {
                    await area.LoadAsync();
                    AreasList.Items.Add(area);
                }

                // Hide the loading indicator.
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Something unexpected happened, show the error message.
                Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, "There was an error.");
            }
        }

        private void ShowOnlineButton_Click(object sender, RoutedEventArgs e)
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
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = 0;
            BusyText.Text = "Downloading map area...";
            BusyIndicator.Visibility = Visibility.Visible;

            // Create folder path where the map package will be downloaded.
            string path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If the area is already downloaded, open it.
            if (Directory.Exists(path))
            {
                try
                {
                    // Open the downloaded mobile map package.
                    _mobileMapPackage = await MobileMapPackage.OpenAsync(path);

                    // Show the first map.
                    MyMapView.Map = _mobileMapPackage.Maps.First();

                    // Update the UI.
                    BusyText.Text = string.Empty;
                    BusyIndicator.Visibility = Visibility.Collapsed;
                    MessageLabel.Text = "Opened offline area.";
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    MessageBox.Show(e.Message, "Couldn't open offline area. Proceeding to take area offline.");
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
                    MessageBox.Show(errors, "Warning!");
                }

                // Show the downloaded map.
                MyMapView.Map = results.OfflineMap;

                // Update the UI.
                ShowOnlineButton.IsEnabled = true;
                DownloadButton.Content = "View downloaded area";
                MessageLabel.Text = "Downloaded preplanned area.";
            }
            catch (Exception ex)
            {
                // Report any errors.
                Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, "Downloading map area failed.");
            }
            finally
            {
                BusyText.Text = string.Empty;
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void OnJobProgressChanged(object sender, EventArgs e)
        {
            // Because the event is raised on a background thread, the dispatcher must be used to
            // ensure that UI updates happen on the UI thread.
            Dispatcher.Invoke(() =>
            {
                // Update the UI with the progress.
                DownloadPreplannedOfflineMapJob downloadJob = sender as DownloadPreplannedOfflineMapJob;
                ProgressBar.Value = downloadJob.Progress;
                BusyPercentage.Text = $"{downloadJob.Progress}%";
            });
        }

        private void OnDownloadMapAreaClicked(object sender, RoutedEventArgs e)
        {
            if (AreasList.SelectedItem != null)
            {
                PreplannedMapArea selectedMapArea = AreasList.SelectedItem as PreplannedMapArea;
                _ = DownloadMapAreaAsync(selectedMapArea);
            }
        }

        private void OnDeleteAllMapAreasClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set up UI for downloading.
                ProgressBar.IsIndeterminate = true;
                BusyText.Text = "Deleting downloaded map area...";
                BusyIndicator.Visibility = Visibility.Visible;

                // Reset the map.
                MyMapView.Map = _originalMap;

                // Close the current mobile package.
                _mobileMapPackage?.Close();

                // Delete all data from the temporary data folder.
                Directory.Delete(_offlineDataFolder, true);
                Directory.CreateDirectory(_offlineDataFolder);

                // Update the UI.
                MessageLabel.Text = "Deleted downloaded areas.";
                DownloadButton.Content = "Download preplanned area";
                MessageLabel.Text = "Downloaded preplanned area.";
            }
            catch (Exception ex)
            {
                // Report the error.
                MessageBox.Show(ex.Message, "Deleting map area failed.");
            }
            finally
            {
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void AreasList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Update the download button to reflect if the map area has already been downloaded.
            if (Directory.Exists(Path.Combine(_offlineDataFolder, (AreasList.SelectedItem as PreplannedMapArea).PortalItem.Title)))
            {
                DownloadButton.Content = "View downloaded area";
            }
            else
            {
                DownloadButton.Content = "Download preplanned area";
            }
        }
    }
}