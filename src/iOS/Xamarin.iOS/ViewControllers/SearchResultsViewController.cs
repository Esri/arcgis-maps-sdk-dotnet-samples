// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UIKit;

namespace ArcGISRuntime
{
    [Register("SearchResultsViewController")]
    public class SearchResultsViewController : UITableViewController
    {
        private readonly UIViewController _parentViewController;
        private List<SampleInfo> _visibleSamples = new List<SampleInfo>();
        private readonly List<SampleInfo> _sampleItems;
        private LoadingOverlay _loadPopup;

        public SearchResultsViewController(UIViewController controller)
        {
            _parentViewController = controller;

            // Using the allsamples list avoids duplicate sample entries in the 'featured' category
            _sampleItems = SampleManager.Current.AllSamples.ToList();
        }

        public void Search(string searchText)
        {
            _visibleSamples = _sampleItems.Where(c => SampleManager.Current.SampleSearchFunc(c, searchText)).ToList();
            TableView.ReloadData();
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _visibleSamples.Count;
        }

        public override bool ShouldHighlightRow(UITableView tableView, NSIndexPath rowIndexPath)
        {
            return true;
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return $"Search results ({_visibleSamples.Count})";
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("sample") ?? new UITableViewCell(UITableViewCellStyle.Subtitle, "sample");
            SampleInfo item = _visibleSamples[indexPath.Row];
            cell.TextLabel.Text = item.SampleName;
            cell.DetailTextLabel.Text = item.Description;
            cell.DetailTextLabel.TextColor = UIColor.Gray;
            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            return cell;
        }

        public override async void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            try
            {
                // Call a function to clear existing credentials
                ClearCredentials();

                var sample = _visibleSamples[indexPath.Row];

                if (sample.OfflineDataItems != null)
                {
                    // Create a cancellation token source.
                    var cancellationTokenSource = new CancellationTokenSource();

                    // Show progress overlay
                    var bounds = UIScreen.MainScreen.Bounds;

                    _loadPopup = new LoadingOverlay(bounds, cancellationTokenSource);
                    _parentViewController.ParentViewController.View.Add(_loadPopup);

                    // Ensure data present
                    await DataManager.EnsureSampleDataPresent(sample, cancellationTokenSource.Token);

                    // Hide progress overlay
                    _loadPopup.Hide();
                }

                var control = (UIViewController)SampleManager.Current.SampleToControl(sample);
                control.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("InfoIcon"), UIBarButtonItemStyle.Plain, (s,e) => ViewSampleReadme(sample, control));
                _parentViewController.NavigationController.PushViewController(control, true);
            }
            catch (OperationCanceledException)
            {
                _loadPopup.Hide();
                TableView.DeselectRow(indexPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ViewSampleReadme(SampleInfo sample, UIViewController parentController)
        {
            var switcher = new UISegmentedControl(new string[] { "About", "Source code" }) { SelectedSegment = 0 };
            var control = new SampleInfoViewController(sample, switcher);
            control.NavigationItem.RightBarButtonItem = new UIBarButtonItem() { CustomView = switcher };
            parentController.NavigationController.PushViewController(control, true);
        }

        private static void ClearCredentials()
        {
            // Clear credentials (if any) from previous sample runs
            foreach (Credential cred in AuthenticationManager.Current.Credentials)
            {
                AuthenticationManager.Current.RemoveCredential(cred);
            }
        }
    }
}