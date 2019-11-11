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
using UIKit;

namespace ArcGISRuntime
{
    public class SamplesViewController : UITableViewController
    {
        private readonly SearchableTreeNode _category;

        public SamplesViewController(SearchableTreeNode category)
        {
            _category = category;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = _category.Name;
            NavigationController.NavigationBar.PrefersLargeTitles = false;

            List<object> listSampleItems = _category.Items;

            TableView.Source = new SamplesDataSource(this, listSampleItems);

            TableView.ReloadData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.NavigationBar.PrefersLargeTitles = false;
        }

        private class SamplesDataSource : UITableViewSource
        {
            private readonly UITableViewController _controller;
            private LoadingOverlay _loadPopup;
            private readonly List<SampleInfo> _data;
            private SampleInfo _sample;

            public SamplesDataSource(UITableViewController controller, IEnumerable<object> data)
            {
                _data = data.OfType<SampleInfo>().ToList();
                _controller = controller;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell("sample") ?? new UITableViewCell(UITableViewCellStyle.Subtitle, "sample");
                SampleInfo item = _data[indexPath.Row];
                cell.TextLabel.Text = item.SampleName;
                cell.DetailTextLabel.Text = item.Description;
                cell.DetailTextLabel.TextColor = UIColor.Gray;
                cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _data.Count;
            }

            public override async void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                try
                {
                    // Call a function to clear existing credentials
                    ClearCredentials();

                    _sample = _data[indexPath.Row];

                    if (_sample.OfflineDataItems != null)
                    {
                        // Show progress overlay
                        var bounds = UIScreen.MainScreen.Bounds;

                        _loadPopup = new LoadingOverlay(bounds);
                        _controller.ParentViewController.View.Add(_loadPopup);

                        // Ensure data present
                        await DataManager.EnsureSampleDataPresent(_sample);

                        // Hide progress overlay
                        _loadPopup.Hide();
                    }

                    var control = (UIViewController)SampleManager.Current.SampleToControl(_sample);
                    control.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("InfoIcon"), UIBarButtonItemStyle.Plain, ViewSampleReadme);
                    _controller.NavigationController.PushViewController(control, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            private void ViewSampleReadme(object sender, EventArgs e)
            {
                var switcher = new UISegmentedControl(new string[] { "About", "Source code" }) { SelectedSegment = 0 };
                var control = new SampleInfoViewController(_sample, switcher);
                control.NavigationItem.RightBarButtonItem = new UIBarButtonItem() { CustomView = switcher };
                _controller.NavigationController.PushViewController(control, true);
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
}