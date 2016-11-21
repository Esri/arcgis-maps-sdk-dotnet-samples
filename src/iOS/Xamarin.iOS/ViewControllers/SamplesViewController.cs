using System;
using System.Collections.Generic;
using UIKit;
using ArcGISRuntimeXamarin.Managers;
using ArcGISRuntimeXamarin.Models;
using Foundation;

namespace ArcGISRuntimeXamarin
{
    public class SamplesViewController : UITableViewController
    {
        TreeItem category;

        public SamplesViewController(TreeItem category)
        {
            this.category = category;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.Title = "Samples";

            List<Object> listSubCategoryItems = new List<Object>();
            for (int i = 0; i < category.Items.Count; i++)
            {
                listSubCategoryItems.Add((category.Items[i] as TreeItem).Items);
            }

            List<Object> listSampleItems = new List<Object>();
            foreach (List<Object> subCategoryItem in listSubCategoryItems)
            {
                foreach (var sample in subCategoryItem)
                {
                    listSampleItems.Add(sample);
                }
            }

            this.TableView.Source = new SamplesDataSource(this, listSampleItems);

            this.TableView.ReloadData();
        }

        public class SamplesDataSource : UITableViewSource
        {
            private UITableViewController controller;
            private List<object> data;

            public SamplesDataSource(UITableViewController controller, List<Object> data)
            {
                this.data = data;
                this.controller = controller;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = new UITableViewCell();
                var item = data[indexPath.Row];
                cell.TextLabel.Text = (item as SampleModel).Name;
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return data.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                try
                {
                    var item = data[indexPath.Row];
                    var sampleName = (item as SampleModel).SampleName;
                    var sampleNamespace = (item as SampleModel).SampleNamespace;

                    Type t = Type.GetType(sampleNamespace + "." + sampleName);
                    UIViewController vc = Activator.CreateInstance(t) as UIViewController;

                    controller.NavigationController.PushViewController(vc, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
