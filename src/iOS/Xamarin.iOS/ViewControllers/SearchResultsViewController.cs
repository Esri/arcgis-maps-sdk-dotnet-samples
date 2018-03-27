// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;
using Foundation;
using UIKit;

namespace ArcGISRuntime
{
    [Register("SearchResultsViewController")]
    public class SearchResultsViewController : UITableViewController
    {
        private readonly UIViewController _parentViewController;
        private List<SampleInfo> _visibleSamples = new List<SampleInfo>();
        private readonly IList<SearchableTreeNode> _categories;
        private List<SampleInfo> _sampleItems = new List<SampleInfo>();
        private LoadingOverlay _loadPopup;

        public SearchResultsViewController(UIViewController controller, IList<SearchableTreeNode> categories)
        {
            _parentViewController = controller;
            _categories = categories;
            
            // Using the allsamples list avoids duplicate sample entries in the 'featured' category
            _sampleItems = SampleManager.Current.AllSamples.ToList();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Magic number is the row height for the view controller
            TableView.ContentInset = new UIEdgeInsets(44, 0, 0, 0);
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
            return "Search Results";
        }

        public override string TitleForFooter(UITableView tableView, nint section)
        {
            return $"Found {_visibleSamples.Count} matches";
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell searchCell = tableView.DequeueReusableCell("SearchCell") ?? new UITableViewCell(UITableViewCellStyle.Default, "SearchCell");

            searchCell.TextLabel.Text = _visibleSamples[indexPath.Row].SampleName;
            return searchCell;
        }

        public override async void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            try
            {
                // Clear credentials (if any) from previous sample runs
                foreach (Credential cred in AuthenticationManager.Current.Credentials)
                {
                    AuthenticationManager.Current.RemoveCredential(cred);
                }

                var sample = _visibleSamples[indexPath.Row];

                if (sample.OfflineDataItems != null)
                {
                    // Show progress overlay
                    var bounds = UIScreen.MainScreen.Bounds;

                    _loadPopup = new LoadingOverlay(bounds);
                    _parentViewController.ParentViewController.View.Add(_loadPopup);

                    // Ensure data present
                    await DataManager.EnsureSampleDataPresent(sample);

                    // Hide progress overlay
                    _loadPopup.Hide();
                }

                var control = (UIViewController)SampleManager.Current.SampleToControl(sample);
                _parentViewController.NavigationController.PushViewController(control, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}