#if WINDOWS || MACCATALYST
using ArcGIS.Helpers;
using ArcGIS.Samples.Shared.Models;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ArcGIS.Controls
{
    public class TreeView : ScrollView
    {
        private readonly StackLayout _stackLayout = new StackLayout { Orientation = StackOrientation.Vertical };

        private IList<TreeViewNode> _rootNodes = new ObservableCollection<TreeViewNode>();
        private TreeViewNode _selectedItem;

        /// <summary>
        /// The item that is selected in the tree
        /// </summary>
        public TreeViewNode SelectedItem
        {
            get => _selectedItem;

            set
            {
                if (_selectedItem == value)
                {
                    return;
                }

                if (_selectedItem != null)
                {
                    _selectedItem.IsSelected = false;
                }

                _selectedItem = value;

                SelectedItemChanged?.Invoke(this, new EventArgs());
            }
        }


        public IList<TreeViewNode> RootNodes
        {
            get => _rootNodes;
            set
            {
                _rootNodes = value;

                if (value is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged += (s, e) =>
                    {
                        RenderNodes(_rootNodes, _stackLayout, e, null);
                    };
                }

                RenderNodes(_rootNodes, _stackLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), null);
            }
        }

        /// <summary>
        /// Occurs when the user selects a TreeViewItem
        /// </summary>
        public event EventHandler SelectedItemChanged;

        public TreeView()
        {
            Content = _stackLayout;
            SelectedItemChanged += TreeView_SelectedItemChanged;
        }

        private void TreeView_SelectedItemChanged(object sender, EventArgs e)
        {
            ChildSelected(_selectedItem);
        }

        private void RemoveSelectionRecursive(IEnumerable<TreeViewNode> nodes)
        {
            foreach (var treeViewItem in nodes)
            {
                if (treeViewItem != SelectedItem)
                {
                    treeViewItem.IsSelected = false;
                }

                RemoveSelectionRecursive(treeViewItem.ChildrenList);
            }
        }

        private static void AddItems(IEnumerable<TreeViewNode> childTreeViewItems, StackLayout parent, TreeViewNode parentTreeViewItem)
        {
            foreach (var childTreeNode in childTreeViewItems)
            {
                if (!parent.Children.Contains(childTreeNode))
                {
                    parent.Children.Add(childTreeNode);
                }

                childTreeNode.ParentTreeViewItem = parentTreeViewItem;
            }
        }

        internal void ChildSelected(TreeViewNode child)
        {
            SelectedItem = child;
            child.IsSelected = true;
            child.SelectionBoxView.Color = child.SelectedBackgroundColor;
            child.SelectionBoxView.Opacity = child.SelectedBackgroundOpacity;
            RemoveSelectionRecursive(RootNodes);

            if (child.BindingContext is SampleInfo sampleInfo)
            {
                _ = SampleLoader.LoadSample(sampleInfo, this);
            }
        }

        internal static void RenderNodes(IEnumerable<TreeViewNode> childTreeViewItems, StackLayout parent, NotifyCollectionChangedEventArgs e, TreeViewNode parentTreeViewItem)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                //TODO: Reintate this...
                //parent.Children.Clear();
                AddItems(childTreeViewItems, parent, parentTreeViewItem);
            }
            else
            {
                AddItems(e.NewItems.Cast<TreeViewNode>(), parent, parentTreeViewItem);
            }
        }

        private TreeViewNode CreateChildTreeViewNode(object bindingContext, Label label)
        {
            AppTheme currentTheme = Application.Current.RequestedTheme;

            string imageResource = currentTheme == AppTheme.Light ? "bulletpoint.png" : "bulletpointdark.png";

            var node = new TreeViewNode
            {
                BindingContext = bindingContext,
                Content = new StackLayout
                {
                    Children =
                    {
                        new ResourceImage
                        {
                            Resource = imageResource,
                            HeightRequest = 16,
                            WidthRequest = 16
                        },
                        label
                    },
                    Orientation = StackOrientation.Horizontal
                }
            };

            //set DataTemplate for expand button content
            node.ExpandButtonTemplate = new DataTemplate(() => new ExpandButtonContent { BindingContext = node });

            return node;
        }

        private TreeViewNode CreateParentTreeViewNode(object bindingContext, Label label)
        {
            var node = new TreeViewNode
            {
                BindingContext = bindingContext,
                Content = new StackLayout
                {
                    Children =
                    {
                        label
                    },
                    Orientation = StackOrientation.Horizontal
                }
            };

            //set DataTemplate for expand button content
            node.ExpandButtonTemplate = new DataTemplate(() => new ExpandButtonContent { BindingContext = node });

            return node;
        }

        private void CreateTreeItem(IList<TreeViewNode> children, SampleInfo sampleInfo)
        {
            var label = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap,
                WidthRequest = 200
            };

            label.SetBinding(Label.TextProperty, "SampleName");
            label.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

            var xamlItemTreeViewNode = CreateChildTreeViewNode(sampleInfo, label);
            children.Add(xamlItemTreeViewNode);
        }

        public ObservableCollection<TreeViewNode> ProcessTreeItemGroups(SearchableTreeNode treeNode)
        {
            var rootNodes = new ObservableCollection<TreeViewNode>();

            foreach (SearchableTreeNode category in treeNode.Items)
            {
                var label = new Label
                {
                    VerticalOptions = LayoutOptions.Center,
                };

                label.SetBinding(Label.TextProperty, "Name");
                label.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);
    

                var groupTreeViewNode = CreateParentTreeViewNode(category, label);

                rootNodes.Add(groupTreeViewNode);

                foreach (SampleInfo sampleInfo in category.Items)
                {
                    CreateTreeItem(groupTreeViewNode.ChildrenList, sampleInfo);
                }
            }

            return rootNodes;
        }
    }
}
#endif