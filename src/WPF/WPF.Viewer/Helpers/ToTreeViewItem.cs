// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Shared.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ArcGIS.WPF.Viewer
{
    internal static class Helpers
    {
        /// <summary>
        /// Creates a usable list of <c>TreeViewItem</c>.
        /// from the tree of samples and categories.
        /// This function assumes that there is only one level of categories.
        /// </summary>
        public static List<TreeViewItem> ToTreeViewItem(SearchableTreeNode fullTree)
        {
            // Create the list of tree view items.
            List<TreeViewItem> categories = new List<TreeViewItem>();

            // This happens when there are no search results.
            if (fullTree == null)
            {
                return categories;
            }

            // For each category in the tree, create a category item.
            foreach (SearchableTreeNode category in fullTree.Items)
            {
                // Create the category item.
                var categoryItem = new TreeViewItem
                {
                    Header = category.Name,
                    DataContext = category
                };

                // Add items for each sample.
                foreach (SampleInfo sampleInfo in category.Items)
                {
                    categoryItem.Items.Add(new TreeViewItem { Header = sampleInfo.SampleName, DataContext = sampleInfo });
                }

                categories.Add(categoryItem);
            }
            return categories;
        }
    }
}