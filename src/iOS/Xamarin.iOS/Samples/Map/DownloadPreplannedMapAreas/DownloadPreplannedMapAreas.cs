// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DownloadPreplannedMapAreas
{
    [Register("DownloadPreplannedMapAreas")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Download preplanned map areas",
        "Map",
        "This sample demonstrates how to download preplanned map areas from a webmap. In the preplanned offline workflow, the author of the online map defines map areas for offline use. When these areas are created, their offline packages are created and stored online for clients to download. This is the biggest difference between on-demand and preplanned workflows since on-demand workflow data packages are generated at request time.",
        "Select an area from the list to download. When you're done, select 'delete offline areas' to delete the downloaded copy of the map areas. ")]
    public class DownloadPreplannedMapAreas : UIViewController
    {
        // Create and hold reference to the used MapView.
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
        private UIButton _downloadButton;

        private UIButton _deleteButton;
        private UIToolbar _toolbarTray;
        private LoadingOverlay _progressIndicator;

        private UILabel _initialPrompt = new UILabel()
        {
            Text = "Download a map area",
            TextColor = UIColor.White
        };

        public DownloadPreplannedMapAreas()
        {
            Title = "Download preplanned map areas";
        }

        private async void Initialize()
        {
            try
            {
                // Show a loading indicator.
                View.AddSubview(_progressIndicator);
                _progressIndicator.UpdateMessageAndProgress("Loading list of available map areas.", -1);

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

                // Create the offline map task and load it.
                _offlineMapTask = await OfflineMapTask.CreateAsync(webmapItem);

                // Query related preplanned areas.
                _preplannedMapAreas = await _offlineMapTask.GetPreplannedMapAreasAsync();

                // Load each preplanned map area.
                foreach (var area in _preplannedMapAreas)
                {
                    await area.LoadAsync();
                }

                // Show a popup menu of available areas when the download button is clicked.
                _downloadButton.TouchUpInside += (s, e) =>
                    {
                        // Create the alert controller.
                        UIAlertController mapAreaSelectionAlertController = UIAlertController.Create("Map Area Selection",
                            "Select a map area to download and show.", UIAlertControllerStyle.ActionSheet);

                        // Add one action per map area.
                        foreach (PreplannedMapArea area in _preplannedMapAreas)
                        {
                            mapAreaSelectionAlertController.AddAction(UIAlertAction.Create(area.PortalItem.Title, UIAlertActionStyle.Default,
                                action =>
                                {
                                    // Download and show the selected map area.
                                    OnDownloadMapAreaClicked(action.Title);
                                }));
                        }

                        // Needed to prevent a crash on iPad.
                        UIPopoverPresentationController presentationPopover = mapAreaSelectionAlertController.PopoverPresentationController;
                        if (presentationPopover != null)
                        {
                            presentationPopover.SourceView = View;
                            presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
                        }

                        // Display the alert.
                        PresentViewController(mapAreaSelectionAlertController, true, null);

                        // Remove the startup prompt if it hasn't been removed already.
                        if (_initialPrompt != null)
                        {
                            _initialPrompt.RemoveFromSuperview();
                            _initialPrompt = null;
                        }
                    };

                // Remove loading indicators from UI.
                _progressIndicator.RemoveFromSuperview();
            }
            catch (Exception ex)
            {
                // Something unexpected happened, show the error message.
                UIAlertController alertController = UIAlertController.Create("An error occurred.", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private async Task DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
            // Show the download UI.
            _progressIndicator.UpdateMessageAndProgress("Downloading map area", 0);
            View.AddSubview(_progressIndicator);

            // Get the path for the downloaded map area.
            var path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If the area is already downloaded, open it and don't download it again.
            if (Directory.Exists(path))
            {
                var localMapArea = await MobileMapPackage.OpenAsync(path);
                try
                {
                    // Load the map.
                    _myMapView.Map = localMapArea.Maps.First();

                    // Reset the UI.
                    _progressIndicator.RemoveFromSuperview();

                    // Return without proceeding to download.
                    return;
                }
                catch (Exception)
                {
                    // Do nothing, continue as if map wasn't downloaded.
                }
            }

            // Create the job that is used to do the download.
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

                    // Show the message.
                    UIAlertController alertController = UIAlertController.Create("Warning!", errorBuilder.ToString(), UIAlertControllerStyle.Alert);
                    alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alertController, true, null);
                }

                // Show the Map in the MapView.
                _myMapView.Map = results.OfflineMap;
            }
            catch (Exception ex)
            {
                // Report the exception.
                UIAlertController alertController = UIAlertController.Create("Downloading map area failed.", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
            finally
            {
                // Clear the loading UI.
                _progressIndicator.RemoveFromSuperview();
            }
        }

        private void OnJobProgressChanged(object sender, EventArgs e)
        {
            // Get the download job.
            var downloadJob = sender as DownloadPreplannedOfflineMapJob;
            if (downloadJob == null) return;

            // UI work needs to be done on the UI thread.
            InvokeOnMainThread(() =>
            {
                // Update UI with the load progress.
                _progressIndicator.UpdateMessageAndProgress("Downloading map area", downloadJob.Progress);
            });
        }

        private async void OnDownloadMapAreaClicked(string selectedArea)
        {
            try
            {
                // Get the selected map area.
                PreplannedMapArea area = _preplannedMapAreas.First(mapArea => mapArea.PortalItem.Title.ToString() == selectedArea);

                // Download the map area.
                await DownloadMapAreaAsync(area);
            }
            catch (Exception ex)
            {
                // No match found.
                UIAlertController alertController = UIAlertController.Create("Downloading map area failed.", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private async void OnDeleteAllMapAreasClicked(object sender, EventArgs e)
        {
            // Show the deletion UI.
            View.AddSubview(_progressIndicator);
            _progressIndicator.UpdateMessageAndProgress("Deleting downloaded map areas", -1);

            // If there is a map loaded, remove it.
            if (_myMapView.Map != null)
            {
                _myMapView.Map = null;
            }

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
                UIAlertController alertController = UIAlertController.Create("Deleting map areas failed.", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
            finally
            {
                // Reset the UI.
                _progressIndicator.RemoveFromSuperview();
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
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        private void CreateLayout()
        {
            // Create the progress indicator.
            _progressIndicator = new LoadingOverlay(View.Frame);

            // Create the download button.
            _downloadButton = new UIButton();
            _downloadButton.SetTitle("Download Area", UIControlState.Normal);
            _downloadButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Create the delete button.
            _deleteButton = new UIButton();
            _deleteButton.SetTitle("Delete all areas", UIControlState.Normal);
            _deleteButton.SetTitleColor(UIColor.Red, UIControlState.Normal);

            // Create the toolbar that will be used to contain the buttons.
            _toolbarTray = new UIToolbar();

            // Add the MapView to the page
            View.AddSubviews(_myMapView, _toolbarTray, _downloadButton, _deleteButton, _initialPrompt);

            // Subscribe to the delete button click event.
            //     Note: download button event is handled in Initialize.
            _deleteButton.TouchUpInside += OnDeleteAllMapAreasClicked;
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Get the top margin; this is used to adjust the MapView inset
            //    (because the MapView is shown under the navigation area).
            nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Set up the visual frame for the MapView.
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Update the map insets. This will keep the map content centered
            //     within the visible area of the MapView. Additionally, it will
            //     ensure that the attribution bar is not obscured by the toolbar.
            _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 50, 0);

            // Set up the visual frame for the Toolbar.
            _toolbarTray.Frame = new CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);

            // Set up the visual frames for the buttons.
            _downloadButton.Frame = new CGRect(10, _toolbarTray.Frame.Top + 10, View.Bounds.Width / 2 - 10, _toolbarTray.Frame.Height - 20);
            _deleteButton.Frame = new CGRect(View.Bounds.Width / 2 + 15, _toolbarTray.Frame.Top + 10, View.Bounds.Width / 2 - 10, _toolbarTray.Frame.Height - 20);

            // Set up the visual frame for the initial prompt, if it is still there.
            if (_initialPrompt != null)
            {
                _initialPrompt.Frame = new CGRect(10, View.Bounds.Height / 2, View.Bounds.Width, 20);
            }

            base.ViewDidLayoutSubviews();
        }
    }

    /// <summary>
    /// Custom view for showing the loading indicator.
    /// </summary>
    public sealed class LoadingOverlay : UIView
    {
        // Label for showing the loading message.
        private readonly UILabel _loadingMessageLabel;

        public LoadingOverlay(CGRect frame) : base(frame)
        {
            // Update the design.
            BackgroundColor = UIColor.Black;
            Alpha = 0.75f;
            AutoresizingMask = UIViewAutoresizing.All;

            // Calculate layout values.
            nfloat labelHeight = 22;
            nfloat labelWidth = Frame.Width - 20;
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // Create the spinner and show it.
            var activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);

            // Update the spinner's frame
            activityIndicator.Frame = new CGRect(centerX - (activityIndicator.Frame.Width / 2),
                    centerY - activityIndicator.Frame.Height - 20,
                    activityIndicator.Frame.Width,
                    activityIndicator.Frame.Height);
            activityIndicator.AutoresizingMask = UIViewAutoresizing.All;

            // Add the spinner to the view.
            AddSubview(activityIndicator);

            // Start the spinner animation.
            activityIndicator.StartAnimating();

            // Create the label.
            _loadingMessageLabel = new UILabel()
            {
                Frame = new CGRect(centerX - (labelWidth / 2), centerY + 20, labelWidth, labelHeight),
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.White,
                Text = "Loading...",
                TextAlignment = UITextAlignment.Center,
                AutoresizingMask = UIViewAutoresizing.All
            };

            // Add the label to the view.
            AddSubview(_loadingMessageLabel);
        }

        /// <summary>
        /// Method for updating the reported message and progress.
        /// </summary>
        public void UpdateMessageAndProgress(string message, int progress)
        {
            // Negative progress value is interpreted as indeterminate.
            if (progress >= 0)
            {
                _loadingMessageLabel.Text = $"{message}... ({progress})%";
            }
            else
            {
                _loadingMessageLabel.Text = message;
            }
        }
    }
}