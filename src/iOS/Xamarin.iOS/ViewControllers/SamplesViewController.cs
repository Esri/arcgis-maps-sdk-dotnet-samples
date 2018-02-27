using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using ArcGISRuntime.Samples.Shared.Models;
using ArcGISRuntime.Samples.Managers;
using Foundation;

namespace ArcGISRuntime
{
    public class SamplesViewController : UITableViewController
    {
        SearchableTreeNode category;

        public SamplesViewController(SearchableTreeNode category)
        {
            this.category = category;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.Title = "Samples";

            var listSampleItems = category.Items;

            this.TableView.Source = new SamplesDataSource(this, listSampleItems);

            this.TableView.ReloadData();
        }

        public class SamplesDataSource : UITableViewSource
        {
            private UITableViewController controller;
            private LoadingOverlay loadPopup;
            private List<SampleInfo> data;

            public SamplesDataSource(UITableViewController controller, List<Object> data)
            {
                this.data = data.OfType<SampleInfo>().ToList();
                this.controller = controller;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = new UITableViewCell();
                var item = data[indexPath.Row];
                cell.TextLabel.Text = (item as SampleInfo).SampleName;
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return data.Count;
            }

            public override async void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                try
                {
                    var sample = data[indexPath.Row];
                    
                    if(sample.OfflineDataItems != null)
                    {
                        // Show progress overlay
                        var bounds = UIScreen.MainScreen.Bounds;

                        loadPopup = new LoadingOverlay(bounds); 
                        controller.ParentViewController.View.Add(loadPopup);

                        // Ensure data present
                        await DataManager.EnsureSampleDataPresent(sample);

                        // Hide progress overlay
                        loadPopup.Hide();
                    }
                    // Call a function to clear existing credentials
                    ClearCredentials();

                    var control = (UIViewController)SampleManager.Current.SampleToControl(sample);
                    controller.NavigationController.PushViewController(control, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            private void ClearCredentials()
            {
                // Clear credentials (if any) from previous sample runs
                var creds = Esri.ArcGISRuntime.Security.AuthenticationManager.Current.Credentials;
                for (var i = creds.Count() - 1; i >= 0; i--)
                {
                    var c = creds.ElementAtOrDefault(i);
                    if (c != null)
                    {
                        Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveCredential(c);
                    }
                }
            }
        }
    }
}
