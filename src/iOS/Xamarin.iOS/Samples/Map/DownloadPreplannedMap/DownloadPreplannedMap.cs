// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DownloadPreplannedMap
{
    [Register("DownloadPreplannedMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Download preplanned map area",
        category: "Map",
        description: "Take a map offline using a preplanned map area.",
        instructions: "Select a map area from the Preplanned Map Areas list. Tap the button to download the selected area. The download progress will be shown in the Downloads list. When a download is complete, select it to display the offline map in the map view.",
        tags: new[] { "map area", "offline", "pre-planned", "preplanned" })]
    public class DownloadPreplannedMap : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITableViewController _tableController;
        private UILabel _helpLabel;
        private UIActivityIndicatorView _activityIndicator;
        private UINavigationController _tableDisplayController;
        private UIBarButtonItem _showOnlineButton;

        private MapAreaViewModel _mapAreaViewModel;

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

        public DownloadPreplannedMap()
        {
            Title = "Download a preplanned map area";
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

                // Configure the table view for showing map areas.
                _mapAreaViewModel = new MapAreaViewModel(_mapAreas);
                _mapAreaViewModel.MapSelected += _mapAreaViewModel_MapSelected;

                _tableController = new UITableViewController(UITableViewStyle.Plain);
                _tableController.TableView.Source = _mapAreaViewModel;

                // Hide the loading indicator.
                _activityIndicator.StopAnimating();
            }
            catch (Exception ex)
            {
                // Something unexpected happened, show the error message.
                Debug.WriteLine(ex);
                new UIAlertView("There was an error", ex.ToString(), (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private void ShowOnline_Click(object sender, EventArgs e)
        {
            // Show the online map.
            _myMapView.Map = _originalMap;

            // Disable the button.
            _showOnlineButton.Enabled = false;
        }

        private void _mapAreaViewModel_MapSelected(object sender, PreplannedMapArea area)
        {
            // Dismiss the table view.
            _tableDisplayController.DismissViewController(true, null);

            // Download the map area.
            DownloadMapAreaAsync(area);
        }

        private async void DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
            // Close the current mobile package.
            _mobileMapPackage?.Close();

            // Set up UI for downloading.
            _activityIndicator.StartAnimating();
            _helpLabel.Text = "Downloading map area...";

            // Create folder path where the map package will be downloaded.
            string path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If the area is already downloaded, open it.
            if (Directory.Exists(path))
            {
                try
                {
                    _mobileMapPackage = await MobileMapPackage.OpenAsync(path);
                    _myMapView.Map = _mobileMapPackage.Maps.First();

                    _helpLabel.Text = "Opened offline area.";
                    _showOnlineButton.Enabled = true;
                    _activityIndicator.StopAnimating();
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    new UIAlertView("Couldn't open offline map area. Proceeding to take area offline.", e.ToString(), (IUIAlertViewDelegate)null, "OK", null).Show();
                }
            }

            // Create download parameters.
            DownloadPreplannedOfflineMapParameters parameters = await _offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(mapArea);

            // Set the update mode to not receive updates.
            parameters.UpdateMode = PreplannedUpdateMode.NoUpdates;

            // Create the job.
            DownloadPreplannedOfflineMapJob job = _offlineMapTask.DownloadPreplannedOfflineMap(parameters, path);

            // Set up event to update the progress bar while the job is in progress.
            job.ProgressChanged += Job_ProgressChanged;

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
                    new UIAlertView("Warning!", errors, (IUIAlertViewDelegate)null, "OK", null).Show();
                }

                // Show the downloaded map.
                _myMapView.Map = results.OfflineMap;
            }
            catch (Exception ex)
            {
                // Report any errors.
                Debug.WriteLine(ex);
                new UIAlertView("Downloading map area failed", ex.ToString(), (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                _activityIndicator.StopAnimating();
                _helpLabel.Text = "Map area offline.";
                _showOnlineButton.Enabled = true;
            }
        }

        private void Job_ProgressChanged(object sender, EventArgs e)
        {
            // Because the event is raised on a background thread, the dispatcher must be used to
            // ensure that UI updates happen on the UI thread.
            InvokeOnMainThread(() =>
            {
                // Update the UI with the progress.
                DownloadPreplannedOfflineMapJob downloadJob = sender as DownloadPreplannedOfflineMapJob;
                _helpLabel.Text = $"Downloading map area... ({downloadJob.Progress}%).";
            });
        }

        private void ShowAreas_Click(object sender, EventArgs e)
        {
            // Show the layer list popover. Note: most behavior is managed by the table view & its source. See MapViewModel.
            _tableDisplayController = new UINavigationController(_tableController);
            UIBarButtonItem closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (o, ea) => _tableDisplayController.DismissViewController(true, null));
            _tableDisplayController.NavigationBar.Items[0].SetRightBarButtonItem(closeButton, false);
            _tableDisplayController.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            _tableDisplayController.PreferredContentSize = new CGSize(300, 250);
            UIPopoverPresentationController pc = _tableDisplayController.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem)sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new ppDelegate();
            }

            PresentViewController(_tableDisplayController, true, null);
        }

        private void DeleteAreas_Click(object sender, EventArgs e)
        {
            try
            {
                // Set up UI for downloading.
                _helpLabel.Text = "Deleting map areas...";
                _activityIndicator.StartAnimating();

                // Reset the map.
                _myMapView.Map = _originalMap;

                // Close the current mobile package.
                _mobileMapPackage?.Close();

                // Delete all data from the temporary data folder.
                Directory.Delete(_offlineDataFolder, true);
                Directory.CreateDirectory(_offlineDataFolder);
            }
            catch (Exception ex)
            {
                // Report the error.
                Debug.WriteLine(ex);
                new UIAlertView("Deleting map area failed", ex.ToString(), (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                _activityIndicator.StopAnimating();
                _helpLabel.Text = "Deleted offline areas.";
                _showOnlineButton.Enabled = false;
            }
        }

        // Force popover to display on iPhone.
        private class ppDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _showOnlineButton = new UIBarButtonItem("Show online", UIBarButtonItemStyle.Plain, ShowOnline_Click) { Enabled = false };

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem("Download", UIBarButtonItemStyle.Plain, ShowAreas_Click),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Delete all", UIBarButtonItemStyle.Plain, DeleteAreas_Click),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _showOnlineButton
            };

            _helpLabel = new UILabel
            {
                Text = "Choose a map area to take offline.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            _activityIndicator.TranslatesAutoresizingMaskIntoConstraints = false;
            _activityIndicator.HidesWhenStopped = true;
            _activityIndicator.BackgroundColor = UIColor.FromWhiteAlpha(0, .6f);
            _activityIndicator.StartAnimating();

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _helpLabel, _activityIndicator);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.HeightAnchor.ConstraintEqualTo(40),
                _activityIndicator.TopAnchor.ConstraintEqualTo(_helpLabel.BottomAnchor),
                _activityIndicator.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _activityIndicator.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _activityIndicator.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // Close the current mobile package.
            _mobileMapPackage?.Close();
        }
    }

    internal class MapAreaViewModel : UITableViewSource
    {
        private readonly List<PreplannedMapArea> _maps;
        private const string CellIdentifier = "LayerTableCell";

        public MapAreaViewModel(List<PreplannedMapArea> maps)
        {
            _maps = maps;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Gets a cell for the specified section and row.
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier);
            PreplannedMapArea selectedMap = _maps[indexPath.Row];

            cell.TextLabel.Text = selectedMap.PortalItem.Title;
            try
            {
                NSData imgData = NSData.FromUrl(selectedMap.PortalItem.ThumbnailUri);
                cell.ImageView.Image = new UIImage(imgData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            cell.ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;

            return cell;
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return false;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _maps.Count;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            PreplannedMapArea selectedMap = _maps[indexPath.Row];

            // Notify subscribers of the new selection.
            RaiseMapAreaSelected(selectedMap);
        }

        // Allow the app to detect when a row is selected.
        // This is an event that can be subscribed to with code like _viewModel.MapAreaSelected += (o, mapArea) => { };
        public delegate void MapAreaSelectedHandler(object sender, PreplannedMapArea area);

        public event MapAreaSelectedHandler MapSelected;

        private void RaiseMapAreaSelected(PreplannedMapArea selectedArea)
        {
            MapSelected?.Invoke(this, selectedArea);
        }
    }
}