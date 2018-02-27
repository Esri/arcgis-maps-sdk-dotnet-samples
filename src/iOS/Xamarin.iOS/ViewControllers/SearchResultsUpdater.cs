using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace ArcGISRuntime
{
    public class SearchResultsUpdater : UISearchResultsUpdating
    {
        public event Action<string> UpdateSearchResults = delegate { };

        public override void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            this.UpdateSearchResults(searchController.SearchBar.Text);
        }
    }
}
