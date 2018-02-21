// Copyright 2018 Esri.
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

namespace ArcGISRuntimeXamarin.Samples.DownloadPreplannedMapAreas
{
    [Activity]
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
            try
            {
                // Show the deletion UI.
                _downloadDeleteProgressDialog.SetMessage("Deleting downloaded map areas...");
                _downloadDeleteProgressDialog.SetTitle("Deleting");
                _downloadDeleteProgressDialog.Show();

                // If there is a map loaded, remove it.
                if (_myMapView.Map != null)
                {
                    _myMapView.Map = null;
                }

                // Delete the downloaded map areas.
                await UnregisterAndDeleteAllAreas();
            }
            catch (Exception ex)
            {
                // Report the error.
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(ex.Message).SetTitle("Deleting map areas failed.").Show();
            }
            finally
            {
                _downloadDeleteProgressDialog.Dismiss();
            }
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