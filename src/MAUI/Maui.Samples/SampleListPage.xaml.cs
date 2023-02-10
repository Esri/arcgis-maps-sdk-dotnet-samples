// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using ArcGIS.Helpers;

#if WINDOWS || MACCATALYST
using ArcGIS.Controls;
#endif

namespace ArcGIS
{
    public partial class SampleListPage
    {
        private string _categoryName;
        private List<SampleInfo> _listSampleItems;

#if WINDOWS || MACCATALYST
        private TreeView _categoriesTree;
#endif

        public SampleListPage(string name)
        {
            _categoryName = name;

            InitializeComponent();

            Initialize();

#if WINDOWS || MACCATALYST
            LoadTreeView(SampleManager.Current.FullTree);
#endif
            Title = _categoryName;
        }

        private void Initialize()
        {
            SetBindingContext();
#if WINDOWS || MACCATALYST

            SampleListGrid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition() { Width = 280 }, new ColumnDefinition() { Width = GridLength.Star } };           

            _categoriesTree = new TreeView()
            {
                Margin = 4,
                BackgroundColor = Colors.Transparent
            };
            SampleListGrid.Add(_categoriesTree);
            SampleListGrid.SetColumn(_categoriesTree, 0);
            SampleListGrid.SetColumn(SampleScrollView, 1);
#else
            SampleListGrid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition() { Width = GridLength.Star } };
            SampleListGrid.SetColumn(SampleScrollView, 0);
#endif
        }

        private void SetBindingContext()
        {
            Title = _categoryName;

            // Get the list of sample categories.
            List<object> sampleCategories = SampleManager.Current.FullTree.Items;

            // Get the tree node for this category.
            var category = sampleCategories.FirstOrDefault(x => ((SearchableTreeNode)x).Name == _categoryName) as SearchableTreeNode;

            // Get the samples from the category.
            _listSampleItems = category?.Items.OfType<SampleInfo>().ToList();

            // Update the binding to show the samples.
            BindingContext = _listSampleItems;
        }

#if WINDOWS || MACCATALYST
        private void LoadTreeView(SearchableTreeNode fullTree)
        {
            var rootNodes = _categoriesTree.ProcessTreeItemGroups(fullTree);

            _categoriesTree.RootNodes = rootNodes;

            foreach (TreeViewNode rootNode in rootNodes)
            {
                var rootNodeTappedRecognizer = new TapGestureRecognizer()
                {
                    NumberOfTapsRequired = 1,
                    CommandParameter = rootNode,
                };

                rootNodeTappedRecognizer.Tapped += TapGestureRecognizer_RootNodeTapped;

                rootNode.GestureRecognizers.Add(rootNodeTappedRecognizer);

                foreach (TreeViewNode child in rootNode.ChildrenList)
                {
                    var childTappedRecognizer = new TapGestureRecognizer()
                    {
                        NumberOfTapsRequired = 1,
                        CommandParameter = child,
                    };

                    childTappedRecognizer.Tapped += TapGestureRecognizer_ChildTapped;

                    child.GestureRecognizers.Add(childTappedRecognizer);
                }
            }
        }

        private void TapGestureRecognizer_RootNodeTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is TreeViewNode rootNode && rootNode.BindingContext is SearchableTreeNode searchableTreeNode)
            {
                if (searchableTreeNode.Name != Title)
                {
                    _categoriesTree.SelectedItem = rootNode;
                    UpdateSelectedCategory(searchableTreeNode.Name);
                }
            }
        }

        private void TapGestureRecognizer_ChildTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is TreeViewNode childNode && childNode.BindingContext is SampleInfo)
            {
                _categoriesTree.SelectedItem = childNode;
            }
        }

        private void UpdateSelectedCategory(string categoryName)
        {
            _categoryName = categoryName;
            SetBindingContext();
        }
#endif

        private void TapGestureRecognizer_SampleTapped(object sender, TappedEventArgs e)
        {
            var sampleInfo = e.Parameter as SampleInfo;
            _ = SampleLoader.LoadSample(sampleInfo, this);
        }
    }
}