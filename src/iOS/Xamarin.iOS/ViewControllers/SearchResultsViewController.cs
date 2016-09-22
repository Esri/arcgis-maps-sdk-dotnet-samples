using UIKit;
using Foundation;
using System.Collections.Generic;
using System.Linq;
using ArcGISRuntimeXamarin.Managers;
using System;
using ArcGISRuntimeXamarin.Models;

namespace ArcGISRuntimeXamarin
{
    [Register("SearchResultsViewController")]
    public class SearchResultsViewController : UITableViewController
    {
        private readonly UIViewController parentViewController;
        private readonly List<object> visibleSamples = new List<object>();
        private readonly IList<TreeItem> categories;
        private readonly List<SampleModel> sampleItems = new List<SampleModel>();

        public SearchResultsViewController(UIViewController controller, IList<TreeItem> categories)
        {
            this.parentViewController = controller;
            this.categories = categories;

            CreateLists();
        }

        public void CreateLists()
        {
            // Parse out the samples from the categories
            List<object> categoryItems = new List<object>();
            foreach (var category in categories)
            {
                for (int i = 0; i < category.Items.Count; i++)
                {
                    if ((category.Items[i] as TreeItem).Items.Count > 0)
                    {
                        categoryItems.Add((category.Items[i] as TreeItem).Items);
                    }
                }
            }

            // Create a flat list of samples
            foreach (List<object> item in categoryItems)
            {
                foreach (var item1 in item)
                {
                    sampleItems.Add(item1 as SampleModel);
                }
            }
        }
        public void Search(string searchText)
        {
            visibleSamples.Clear();
            foreach (var item in sampleItems.Where(c => c.Description.ToLower().Contains(searchText.ToLower()) ||
            c.Name.ToLower().Contains(searchText.ToLower()) ||
            c.Instructions.ToLower().Contains(searchText.ToLower())))
                visibleSamples.Add(item.Name);

            Console.WriteLine();

            this.TableView.ReloadData();
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return visibleSamples.Count;
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
            return string.Format("Found {0} matches", visibleSamples.Count);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell searchCell = tableView.DequeueReusableCell("SearchCell");
            if (searchCell == null)
                searchCell = new UITableViewCell(UITableViewCellStyle.Default, "SearchCell");

            searchCell.TextLabel.Text = visibleSamples[indexPath.Row].ToString();
            return searchCell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            try
            {
                var item = sampleItems[indexPath.Row];
                var sampleName = (item as SampleModel).SampleName;
                var sampleNamespace = (item as SampleModel).SampleNamespace;

                Type t = Type.GetType(sampleNamespace + "." + sampleName);
                UIViewController vc = Activator.CreateInstance(t) as UIViewController;
                parentViewController.NavigationController.PushViewController(vc, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view
        }
    }
}