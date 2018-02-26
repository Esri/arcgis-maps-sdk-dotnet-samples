using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ArcGISRuntime.Samples.Shared.Models;

namespace ArcGISRuntime.WPF.Viewer
{
    class Helpers
    {
        public static List<TreeViewItem> ToTreeViewItem(SearchableTreeNode FullTree)
        {
            var categories = new List<TreeViewItem>();

            foreach (var category in FullTree.Items)
            {
                var categoryItem = new TreeViewItem();
                categoryItem.Header = (category as SearchableTreeNode).Name;
                categoryItem.DataContext = category;

                foreach (SampleInfo sampleInfo in ((SearchableTreeNode)category).Items)
                {
                    categoryItem.Items.Add(new TreeViewItem { Header = sampleInfo.SampleName, DataContext = sampleInfo });
                }

                categories.Add(categoryItem);
            }
            return categories;
        }
    }
}
