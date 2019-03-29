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
        "Download a preplanned map area",
        "Map",
        "Take a map offline using a preplanned map area",
        "Select a map area to take offline, then use the button to take it offline. Click 'Delete offline areas' to remove any downloaded map areas.")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class DownloadPreplannedMap : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITableViewController _tableController;
        private UILabel _helpLabel;
        private UIActivityIndicatorView _activityIndicator;
        private UINavigationController _tableDisplayController;

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
                foreach (var area in preplannedAreas)
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
                new UIAlertView("There was an error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        void _mapAreaViewModel_MapSelected(object sender, PreplannedMapArea area)
        {
            // Dismiss the table view.
            _tableDisplayController.DismissViewController(true, null);

            // Download the map area.
            DownloadMapAreaAsync(area);
        }

        private async void DownloadMapAreaAsync(PreplannedMapArea mapArea)
        {
            // Set up UI for downloading.
            _activityIndicator.StartAnimating();
            _helpLabel.Text = "Downloading map area...";

            // Create folder path where the map package will be downloaded.
            var path = Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // If the area is already downloaded, open it.
            if (Directory.Exists(path))
            {
                try
                {
                    var localMapArea = await MobileMapPackage.OpenAsync(path);
                    _myMapView.Map = localMapArea.Maps.First();

                    _helpLabel.Text = "Opened offline area.";
                    _activityIndicator.StopAnimating();
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    new UIAlertView("Couldn't open offline map area. Proceeding to take area offline.", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
                }
            }

            // Create download parameters.
            DownloadPreplannedOfflineMapParameters parameters = await _offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(mapArea);

            // Create the job.
            DownloadPreplannedOfflineMapJob job = _offlineMapTask.DownloadPreplannedOfflineMap(parameters, path);

            // Set up event to update the progress bar while the job is in progress.
            job.ProgressChanged += Job_ProgressChanged;

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
                    new UIAlertView("Warning!", errors, (IUIAlertViewDelegate) null, "OK", null).Show();
                }

                // Show the downloaded map.
                _myMapView.Map = results.OfflineMap;
            }
            catch (Exception ex)
            {
                // Report any errors.
                Debug.WriteLine(ex);
                new UIAlertView("Downloading map area failed", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
            finally
            {
                _activityIndicator.StopAnimating();
                _helpLabel.Text = "Map area offline.";
            }
        }

        void Job_ProgressChanged(object sender, EventArgs e)
        {
            // Because the event is raised on a background thread, the dispatcher must be used to
            // ensure that UI updates happen on the UI thread.
            InvokeOnMainThread(() =>
            {
                // Update the UI with the progress.
                var downloadJob = sender as DownloadPreplannedOfflineMapJob;
                _helpLabel.Text = $"Downloading map area... ({downloadJob.Progress}%).";
            });
        }

        private void ShowAreas_Click(object sender, EventArgs e)
        {
            // Show the layer list popover. Note: most behavior is managed by the table view & its source. See MapViewModel.
            _tableDisplayController = new UINavigationController(_tableController);
            var closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (o, ea) => _tableDisplayController.DismissViewController(true, null));
            _tableDisplayController.NavigationBar.Items[0].SetRightBarButtonItem(closeButton, false);
            _tableDisplayController.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            _tableDisplayController.PreferredContentSize = new CGSize(300, 250);
            UIPopoverPresentationController pc = _tableDisplayController.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem) sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new ppDelegate();
            }

            PresentViewController(_tableDisplayController, true, null);
        }

        private void DeleteAllAreas()
        {
            // Delete all data from the temporary data folder.
            Directory.Delete(_offlineDataFolder, true);
            Directory.CreateDirectory(_offlineDataFolder);
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
                Debug.WriteLine(ex);
                new UIAlertView("Deleting map area failed", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
            finally
            {
                _activityIndicator.StopAnimating();
                _helpLabel.Text = "Choose a map area to take offline.";
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
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem("Delete offline areas", UIBarButtonItemStyle.Plain, DeleteAreas_Click),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Download area", UIBarButtonItemStyle.Plain, ShowAreas_Click)
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
    }

    class MapAreaViewModel : UITableViewSource
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
            var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier);
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