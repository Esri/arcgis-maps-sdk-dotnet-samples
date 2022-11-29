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

namespace ArcGIS.Samples.Shared.Models
{
    public class SearchableTreeNode
    {
        // Name of this node in the tree.
        public string Name { get; private set; }

        // List of child items. These are expected to be other SearchableTreeNodes or SampleInfos.
        public List<object> Items { get; private set; }

        /// <summary>
        /// Creates a new SearchableTreeNode from a list of items.
        /// </summary>
        /// <param name="name">The name for this node in the tree.</param>
        /// <param name="items">A list of containing <c>SampleInfo</c>s and <c>SearchableTreeNode</c>s.</param>
        public SearchableTreeNode(string name, IEnumerable<object> items)
        {
            Name = name;
            Items = items.ToList();
        }

        /// <summary>
        /// Searches the node and any sub-nodes for samples matching the predicate.
        /// </summary>
        /// <param name="predicate">Function that should return true for any matching samples.</param>
        /// <returns><c>null</c> if there are no matches, a <c>SearchableTreeNode</c> if there are.</returns>
        public SearchableTreeNode Search(Func<SampleInfo, bool> predicate)
        {
            // Search recursively if this node contains sub-trees.
            SearchableTreeNode[] subTrees = Items.OfType<SearchableTreeNode>()
                .Select(cn => cn.Search(predicate))
                .Where(cn => cn != null)
                .ToArray();
            if (subTrees.Any()) return new SearchableTreeNode(Name, subTrees);

            // If the node contains samples, search those.
            SampleInfo[] matchingSamples = Items
                .OfType<SampleInfo>()
                .Where(predicate)
                .ToArray();

            // Return null if there are no results.
            return matchingSamples.Any() ? new SearchableTreeNode(Name, matchingSamples) : null;
        }
    }
}