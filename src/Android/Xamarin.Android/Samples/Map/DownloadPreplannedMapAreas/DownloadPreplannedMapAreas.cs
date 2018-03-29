// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.DisplayMap
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Download preplanned map areas",
        "Map",
        "This sample demonstrates how to download preplanned map areas from a webmap. In the preplanned offline workflow, the author of the online map defines map areas for offline use. When these areas are created, their offline packages are created and stored online for clients to download. This is the biggest difference between on-demand and preplanned workflows since on-demand workflow data packages are generated at request time.",
        "Select an area from the list to download. When you're done, select 'delete offline areas' to delete the downloaded copy of the map areas. ")]
    public class DownloadPreplannedMapAreas : Activity
    {
        // Create and hold a reference to the used MapView.
        private readonly MapView _myMapView = new MapView();

        // ID of webmap item that has preplanned areas defined
        private const string PortalItemId = "acc027394bc84c2fb04d1ed317aac674";

        // Folder where the areas are downloaded
        private string _offlineDataFolder;

        // Task that is used to work with preplanned map areas
        private OfflineMapTask _offlineMapTask;

        // Reference to the list of available map areas
        private IReadOnlyList<PreplannedMapArea> _preplannedMapAreas;

        // UI controls
        private Button _downloadButton;

        private Button _deleteButton;
        private ProgressDialog _downloadDeleteProgressDialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Download preplanned map areas";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Show a loading indicator.
                ProgressDialog progressIndicator = new ProgressDialog(this);
                progressIndicator.SetTitle("Loading");
                progressIndicator.SetMessage("Loading the available map areas.");
                progressIndicator.SetCancelable(false);
                progressIndicator.Show();

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

                // Create a webmap based on the id.
                PortalItem webmapItem = await PortalItem.CreateAsync(portal, PortalItemId);

                // Create the offline task and load it.
                _offlineMapTask = await OfflineMapTask.CreateAsync(webmapItem);

                // Query related preplanned areas.
                _preplannedMapAreas = await _offlineMapTask.GetPreplannedMapAreasAsync();

                // Load each preplanned map area.
                foreach (var area in _preplannedMapAreas)
                {
                    await area.LoadAsync();
                }

                // Show a popup menu of available areas when the download button is clicked.
                _downloadButton.Click += (s, e) =>
                    {
                        // Create a menu to show the available map areas.
                        PopupMenu areaMenu = new PopupMenu(this, _downloadButton);
                        areaMenu.MenuItemClick += (sndr, evt) =>
                        {
                            // Get the name of the selected area.
                            string selectedArea = evt.Item.TitleCondensedFormatted.ToString();

                            // Download and show the map.
                            OnDownloadMapAreaClicked(selectedArea);
                        };

                        // Create the menu options.
                        foreach (PreplannedMapArea area in _preplannedMapAreas)
                        {
                            areaMenu.Menu.Add(area.PortalItem.Title.ToString());
                        }

                        // Show the menu in the view.
                        areaMenu.Show();
                    };

                // Remove loading indicators from the UI.
                progressIndicator.Dismiss();
            }
            catch (Exception ex)
            {
                // Something unexpected happened, show error message.
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(ex.Message).SetTitle("An error occurred").Show();
            }
        }

        private async Task DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
            // Set up UI for download.
            _downloadDeleteProgressDialog.Progress = 0;
            _downloadDeleteProgressDialog.SetMessage("Downloading map area...");
            _downloadDeleteProgressDialog.SetTitle("Downloading");
            _downloadDeleteProgressDialog.Show();

            // Get the path for the downloaded map area.
            var path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If the map area is already downloaded, open it and don't download it again.
            if (Directory.Exists(path))
            {
                var localMapArea = await MobileMapPackage.OpenAsync(path);
                try
                {
                    // Load the map area.
                    _myMapView.Map = localMapArea.Maps.First();

                    // Update the UI.
                    _downloadDeleteProgressDialog.Dismiss();

                    // Return without downloading the item again.
                    return;
                }
                catch (Exception)
                {
                    // Do nothing, continue as if the map wasn't downloaded.
                }
            }

            // Create the job that is used to download the map area.
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
                        errorBuilder.AppendLine($"{layerError.Key.Name} {layerError.Value.Message}");
                    }

                    // Add table errors to the message.
                    foreach (KeyValuePair<FeatureTable, Exception> tableError in results.TableErrors)
                    {
                        errorBuilder.AppendLine($"{tableError.Key.TableName} {tableError.Value.Message}");
                    }

                    // Show the error message.
                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage(errorBuilder.ToString()).SetTitle("Warning!").Show();
                }

                // Show the Map in the MapView.
                _myMapView.Map = results.OfflineMap;
            }
            catch (Exception ex)
            {
                // Report exception.
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(ex.Message).SetTitle("Downloading map area failed.").Show();
            }
            finally
            {
                // Clear the loading UI.
                _downloadDeleteProgressDialog.Dismiss();
            }
        }

        private void OnJobProgressChanged(object sender, EventArgs e)
        {
            // Get the download job.
            var downloadJob = sender as DownloadPreplannedOfflineMapJob;
            if (downloadJob == null) return;

            // UI work needs to be done on the UI thread.
            RunOnUiThread(() =>
            {
                // Update the UI with the load progress.
                _downloadDeleteProgressDialog.Progress = downloadJob.Progress;
                _downloadDeleteProgressDialog.SetMessage($"Downloading map area... ({downloadJob.Progress}%)");
            });
        }

        private async void OnDownloadMapAreaClicked(string selectedArea)
        {
            try
            {
                // Get the selected map area.
                PreplannedMapArea area = _preplannedMapAreas.First(mapArea => mapArea.PortalItem.Title.ToString() == selectedArea);

                // Download and show the map.
                await DownloadMapAreaAsync(area);
            }
            catch (Exception ex)
            {
                // No match found.
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(ex.Message).SetTitle("Downloading map area failed.").Show();
            }
        }

        private async void OnDeleteAllMapAreasClicked(object sender, EventArgs e)
        {
            // Show the deletion UI.
            _downloadDeleteProgressDialog.SetMessage("Deleting downloaded map areas...");
            _downloadDeleteProgressDialog.SetTitle("Deleting");
            _downloadDeleteProgressDialog.Show();

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
                // Report the error.
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(ex.Message).SetTitle("Deleting map areas failed.").Show();
            }
            finally
            {
                // Reset the UI.
                _downloadDeleteProgressDialog.Dismiss();
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

        // Returns the platform-specific folder for storing offline data.
        private string GetDataFolder()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the progress dialog.
            _downloadDeleteProgressDialog = new ProgressDialog(this);

            // Create the download button. Note: click handler is set up in Initialize.
            _downloadButton = new Button(this) { Text = "Download Area" };

            // Create the delete button.
            _deleteButton = new Button(this) { Text = "Delete offline areas" };
            _deleteButton.Click += OnDeleteAllMapAreasClicked;

            // Add the buttons to the layout.
            layout.AddView(_downloadButton);
            layout.AddView(_deleteButton);

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}