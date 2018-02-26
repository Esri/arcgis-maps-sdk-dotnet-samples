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
using System.ComponentModel;
using System.Linq;

namespace ArcGISRuntime.Samples.Shared.Models
{
    public class SearchableTreeNode : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public List<object> Items { get; set; }

        private bool m_IsExpanded;

        /// <summary>
        /// Supports collapsing in a tree view context
        /// </summary>
        public bool IsExpanded
        {
            get { return m_IsExpanded; }
            set
            {
                m_IsExpanded = value;
                PropertyChangedEventHandler pc = PropertyChanged;
                if (pc != null) pc.Invoke(this, new PropertyChangedEventArgs("IsExpanded"));
            }
        }

        public SearchableTreeNode(string name, IEnumerable<object> items)
        {
            Name = name;
            Items = items.ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SearchableTreeNode Search(Func<SampleInfo, bool> predicate)
        {
            // Search recursively if node contains sub-trees
            var subTrees = Items.OfType<SearchableTreeNode>()
                .Select(cn => cn.Search(predicate))
                .Where(cn => cn != null)
                .ToArray();
            if (subTrees.Any()) return new SearchableTreeNode(Name, subTrees);

            // If the node contains samples, search those
            var matchingSamples = Items
                .OfType<SampleInfo>()
                .Where(predicate)
                .ToArray();
            if (matchingSamples.Any()) return new SearchableTreeNode(Name, matchingSamples);

            // No matches
            return null;
        }
    }
}