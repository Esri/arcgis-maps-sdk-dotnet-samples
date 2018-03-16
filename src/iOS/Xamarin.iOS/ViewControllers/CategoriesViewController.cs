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
using Foundation;
using UIKit;

namespace ArcGISRuntime
{
    partial class CategoriesViewController : UITableViewController
	{
		public CategoriesViewController(IntPtr handle)
			: base(handle)
		{

		}

	    private UISearchController SearchController { get; set; }

        public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			SampleManager.Current.Initialize();
            List<SearchableTreeNode> data = SampleManager.Current.FullTree.Items.OfType<SearchableTreeNode>().ToList();
			TableView.Source = new CategoryDataSource(this, data);

			TableView.ReloadData();

            var searchResultsController = new SearchResultsViewController(this, data);

            // Create search updater and wire it up
            var searchUpdater = new SearchResultsUpdater();
            searchUpdater.UpdateSearchResults += searchResultsController.Search;

            // Create a new search controller
		    SearchController = new UISearchController(searchResultsController) {SearchResultsUpdater = searchUpdater};

		    // Display the search controller
            TableView.TableHeaderView = SearchController.SearchBar;
            DefinesPresentationContext = true;

        }

	    private class CategoryDataSource : UITableViewSource
		{
			private readonly UITableViewController _controller;
			private readonly List<SearchableTreeNode> _data;

		    private const string CellId = "cellid";

		    public CategoryDataSource(UITableViewController controller, List<SearchableTreeNode> data)
			{
				_data = data;
				_controller = controller;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell(CellId, indexPath);
				var item = _data[indexPath.Row];
				cell.TextLabel.Text = item.Name;
				return cell;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				return _data.Count;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				try
				{
					var selected = _data[indexPath.Row];
					_controller.NavigationController.PushViewController(new SamplesViewController(selected), true);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}
	}
}
