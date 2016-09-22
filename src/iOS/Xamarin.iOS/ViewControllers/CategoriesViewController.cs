using ArcGISRuntimeXamarin.Managers;
using CoreGraphics;
using Foundation;
using System;
using UIKit;
using System.Collections.Generic;

namespace ArcGISRuntimeXamarin
{
    partial class CategoriesViewController : UITableViewController
	{
		public CategoriesViewController(IntPtr handle)
			: base(handle)
		{

		}

        public UISearchController SearchController { get; set; }

        public async override void ViewDidLoad()
		{
			base.ViewDidLoad();

			await SampleManager.Current.InitializeAsync();
			var data = SampleManager.Current.GetSamplesAsTree();
			this.TableView.Source = new CategoryDataSource(this, data);

			this.TableView.ReloadData();

            var searchResultsController = new SearchResultsViewController(this, data);

            // Create search updater and wire it up
            var searchUpdater = new SearchResultsUpdater();
            searchUpdater.UpdateSearchResults += searchResultsController.Search;

            // Create a new search controller
            SearchController = new UISearchController(searchResultsController);
            SearchController.SearchResultsUpdater = searchUpdater;

            // Display the search controller
            SearchController.SearchBar.Frame = new CGRect(SearchController.SearchBar.Frame.X, SearchController.SearchBar.Frame.Y, SearchController.SearchBar.Frame.Width, 44f);
            TableView.TableHeaderView = SearchController.SearchBar;
            DefinesPresentationContext = true;

        }

        public class CategoryDataSource : UITableViewSource
		{
			private UITableViewController controller;
			private List<TreeItem> data;

			static string CELL_ID = "cellid";

			public CategoryDataSource(UITableViewController controller, List<TreeItem> data)
			{
				this.data = data;
				this.controller = controller;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell(CELL_ID, indexPath);
				var item = data[indexPath.Row];
				cell.TextLabel.Text = item.Name;
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
					var selected = data[indexPath.Row];
					controller.NavigationController.PushViewController(new SamplesViewController(selected), true);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}
	}
}
