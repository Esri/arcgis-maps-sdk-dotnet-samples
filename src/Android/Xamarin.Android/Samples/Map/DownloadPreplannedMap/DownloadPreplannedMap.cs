// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using Path = System.IO.Path;

namespace ArcGISRuntimeXamarin.Samples.DownloadPreplannedMap
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Download preplanned map area",
        "Map",
        "Take a map offline using a preplanned map area.",
        "Downloading tiles for offline use requires authentication with the web map's server. An [ArcGIS Online](www.arcgis.com) account is required to use this sample.",
        "map area", "offline", "pre-planned", "preplanned")]
    public class DownloadPreplannedMap : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private LinearLayout _mapListView;
        private AlertDialog _progressView;
        private ProgressBar _progressBar;
        private TextView _helpLabel;
        private Button _showOnlineButton;

        // ID of a web map with preplanned map areas.
        private const string PortalItemId = "acc027394bc84c2fb04d1ed317aac674";

        // Folder to store the downloaded mobile map packages.
        private string _offlineDataFolder;

        // Task for taking map areas offline.
        private OfflineMapTask _offlineMapTask;

        // Hold onto the original map.
        private Map _originalMap;

        // Hold a list of available map areas.
        private readonly List<PreplannedMapArea> _mapAreas = new List<PreplannedMapArea>();

        // Most recently opened map package.
        private MobileMapPackage _mobileMapPackage;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Download a preplanned map area";

            CreateLayout();
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
                _myMapView.Map = _originalMap;

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
                ConfigureMapsButtons();

                // Hide the loading indicator.
                _progressView.Dismiss();
            }
            catch (Exception ex)
            {
                // Something unexpected happened, show the error message.
                Debug.WriteLine(ex);
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("There was an error.").Show();
            }
        }

        private void ShowOnlineClick(object sender, EventArgs e)
        {
            // Show the map.
            _myMapView.Map = _originalMap;

            // Disable the button.
            _showOnlineButton.Enabled = false;
        }

        private async Task DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
            // Close the current mobile package.
            _mobileMapPackage?.Close();

            // Set up UI for downloading.
            _progressBar.SetProgress(0, false);
            _progressView.SetMessage("Downloading map area...");
            _progressView.Show();

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
                    _myMapView.Map = _mobileMapPackage.Maps.First();

                    // Update the UI.
                    _progressView.SetMessage("");
                    _progressView.Dismiss();
                    _helpLabel.Text = "Opened offline area.";
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    new AlertDialog.Builder(this).SetMessage(e.Message).SetTitle("Couldn't open offline area. Proceeding to take area offline.").Show();
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

                // Set the update mode to not receive updates.
                parameters.UpdateMode = PreplannedUpdateMode.NoUpdates;

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
                    new AlertDialog.Builder(this).SetMessage(errors).SetTitle("Warning!").Show();
                }

                // Show the downloaded map.
                _myMapView.Map = results.OfflineMap;

                // Update the UI.
                _helpLabel.Text = "Downloaded offline area.";
                _showOnlineButton.Enabled = true;
            }
            catch (Exception ex)
            {
                // Report any errors.
                Debug.WriteLine(ex);
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle("Downloading map area failed.").Show();
            }
            finally
            {
                _progressBar.SetProgress(0, false);
                _progressView.SetMessage("");
                _progressView.Dismiss();
            }
        }

        private void OnJobProgressChanged(object sender, EventArgs e)
        {
            // Because the event is raised on a background thread, the dispatcher must be used to
            // ensure that UI updates happen on the UI thread.
            RunOnUiThread(() =>
            {
                // Update the UI with the progress.
                DownloadPreplannedOfflineMapJob downloadJob = sender as DownloadPreplannedOfflineMapJob;
                _progressBar.SetProgress(downloadJob.Progress, true);
                _progressView.SetMessage($"{downloadJob.Progress}%");
            });
        }

        private void DeleteButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                // Set up UI for downloading.
                _progressView.SetMessage("Deleting map areas...");
                _progressView.Show();

                // Reset the map.
                _myMapView.Map = _originalMap;

                // Close the current mobile package.
                _mobileMapPackage?.Close();

                // Delete all data from the temporary data folder.
                Directory.Delete(_offlineDataFolder, true);
                Directory.CreateDirectory(_offlineDataFolder);

                // Update the UI.
                _helpLabel.Text = "Deleted offline areas.";
                _showOnlineButton.Enabled = false;
            }
            catch (Exception ex)
            {
                // Report the error.
                Debug.WriteLine(ex);
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Deleting map areas failed.").Show();
            }
            finally
            {
                _progressView.Dismiss();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a help label.
            _helpLabel = new TextView(this);
            _helpLabel.Text = "Select a map area to take offline.";
            layout.AddView(_helpLabel);

            // Add space for adding options for each map.
            _mapListView = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            layout.AddView(_mapListView);

            // Create a new layout for the buttons.
            LinearLayout buttonLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Add button for deleting map areas.
            Button deleteButton = new Button(this) { Text = "Delete offline areas" };
            deleteButton.Click += DeleteButtonOnClick;
            buttonLayout.AddView(deleteButton);

            // Add button for showing the original map.
            _showOnlineButton = new Button(this) { Text = "Show online map", Enabled = false };
            _showOnlineButton.Click += ShowOnlineClick;
            buttonLayout.AddView(_showOnlineButton);

            // Add the buttons to the layout.
            layout.AddView(buttonLayout);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);

            // Create the progress dialog display.
            _progressBar = new ProgressBar(this);
            _progressBar.SetProgress(0, true);
            AlertDialog.Builder builder = new AlertDialog.Builder(this).SetView(_progressBar);
            builder.SetCancelable(true);
            _progressView = builder.Create();
        }

        private async void ConfigureMapsButtons()
        {
            foreach (PreplannedMapArea mapArea in _mapAreas)
            {
                Button mapButton = new Button(this);

                mapButton.SetTextColor(Color.Black);
                mapButton.Background = Drawable.CreateFromStream(await mapArea.PortalItem.Thumbnail.GetEncodedBufferAsync(), "");
                mapButton.Text = mapArea.PortalItem.Title.Substring(0, 6);
                mapButton.Click += async (o, e) => { await DownloadMapAreaAsync(mapArea); };
                _mapListView.AddView(mapButton);

                LinearLayout.LayoutParams lparams = (LinearLayout.LayoutParams)mapButton.LayoutParameters;
                lparams.SetMargins(5, 5, 0, 5);
                mapButton.LayoutParameters = lparams;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            // Close the current mobile package.
            _mobileMapPackage?.Close();
        }
    }
}