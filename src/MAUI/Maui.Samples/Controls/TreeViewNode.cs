using ArcGIS.Helpers;
using ArcGIS.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.Controls
{
    public class TreeViewNode : StackLayout
    {
        private DataTemplate _expandButtonTemplate = null;

        private TreeViewNode _parentTreeViewItem;

        private DateTime _expandButtonClickedTime;

        private readonly BoxView _spacerBoxView = new BoxView() { Color = Colors.Transparent };
        private readonly BoxView _emptyBox = new BoxView { BackgroundColor = Colors.Orange, Opacity = 0.5 };

        private const int ExpandButtonWidth = 32;
        private ContentView _expandButtonContent = new();

        private readonly Grid _mainGrid = new Grid
        {
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Fill,
            RowSpacing = 2
        };

        private readonly StackLayout _contentStackLayout = new StackLayout {Orientation = StackOrientation.Horizontal };

        private readonly ContentView _contentView = new ContentView
        {
            HorizontalOptions = LayoutOptions.Fill,
        };

        private readonly StackLayout _childrenStackLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            Spacing = 0,
            IsVisible = false
        };

        private IList<TreeViewNode> _children = new ObservableCollection<TreeViewNode>();
        private readonly TapGestureRecognizer _expandButtonGestureRecognizer = new TapGestureRecognizer();

        internal readonly BoxView SelectionBoxView = new BoxView { Color = Colors.Orange, Opacity = .5, IsVisible = false };

        // The size of indentation in the nested samples within a category.
        private double _indentWidth => Depth * SpacerWidth;
        private int SpacerWidth { get; } = 10;
        private int Depth => ParentTreeViewItem?.Depth + 1 ?? 0;

        private Color _selectedBackgroundColor = Colors.Orange;
        private double _selectedBackgroundOpacity = .3;

        public event EventHandler Expanded;

        protected override void OnParentSet()
        {
            base.OnParentSet();
            Render();
        }

        public bool IsSelected
        {
            get => SelectionBoxView.IsVisible;
            set => SelectionBoxView.IsVisible = value;
        }

        public bool IsExpanded
        {
            get => _childrenStackLayout.IsVisible;
            set
            {
                _childrenStackLayout.IsVisible = value;

                Render();
                if (value)
                {
                    Expanded?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Set BackgroundColor when node is tapped/selected
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get { return _selectedBackgroundColor; }
            set { _selectedBackgroundColor = value; }
        }

        /// <summary>
        /// SelectedBackgroundOpacity when node is tapped/selected
        /// </summary>
        public double SelectedBackgroundOpacity
        {
            get { return _selectedBackgroundOpacity; }
            set { _selectedBackgroundOpacity = value; }
        }

        public DataTemplate ExpandButtonTemplate
        {
            get { return _expandButtonTemplate; }
            set { _expandButtonTemplate = value; }
        }

        public View Content
        {
            get => _contentView.Content;
            set => _contentView.Content = value;
        }

        public IList<TreeViewNode> ChildrenList
        {
            get => _children;
            set
            {
                if (_children is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged -= ItemsSource_CollectionChanged;
                }

                _children = value;

                if (_children is INotifyCollectionChanged notifyCollectionChanged2)
                {
                    notifyCollectionChanged2.CollectionChanged += ItemsSource_CollectionChanged;
                }

                TreeView.RenderNodes(_children, _childrenStackLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), this);

                Render();
            }
        }

        public TreeViewNode ParentTreeViewItem
        {
            get => _parentTreeViewItem;
            set
            {
                _parentTreeViewItem = value;
                Render();
            }
        }

        /// <summary>
        /// Constructs a new TreeViewItem
        /// </summary>
        public TreeViewNode()
        {
            var itemsSource = (ObservableCollection<TreeViewNode>)_children;
            itemsSource.CollectionChanged += ItemsSource_CollectionChanged;

            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _mainGrid.Children.Add(SelectionBoxView);

            _contentStackLayout.Children.Add(_spacerBoxView);
            _contentStackLayout.Children.Add(_expandButtonContent);
            _contentStackLayout.Children.Add(_contentView);

            SetExpandButtonContent(_expandButtonTemplate);

            _expandButtonGestureRecognizer.Tapped += ExpandButton_Tapped;
            _expandButtonContent.GestureRecognizers.Add(_expandButtonGestureRecognizer);

            _mainGrid.SetRow((IView)_childrenStackLayout, 1);
            _mainGrid.SetColumn((IView)_childrenStackLayout, 0);

            _mainGrid.Children.Add(_contentStackLayout);
            _mainGrid.Children.Add(_childrenStackLayout);

            base.Children.Add(_mainGrid);

            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Start;

            Render();
        }

        public void ChildSelected(TreeViewNode child)
        {
            if (child.BindingContext is SampleInfo sampleInfo)
            {
                _ = SampleLoader.LoadSample(sampleInfo, this);
            }
        }

        private void Render()
        {
            _spacerBoxView.WidthRequest = _indentWidth;

            if ((ChildrenList == null || ChildrenList.Count == 0))
            {
                SetExpandButtonContent(_expandButtonTemplate);
                return;
            }

            SetExpandButtonContent(_expandButtonTemplate);

            foreach (var item in ChildrenList)
            {
                item.Render();
            }
        }

        /// <summary>
        /// Use DataTemplate 
        /// </summary>
        private void SetExpandButtonContent(DataTemplate expandButtonTemplate)
        {
            if (expandButtonTemplate != null)
            {
                _expandButtonContent.Content = (View)expandButtonTemplate.CreateContent();
            }
            else
            {
                _expandButtonContent.Content = (View)new ContentView { Content = _emptyBox };
            }
        }

        private void ExpandButton_Tapped(object sender, EventArgs e)
        {
            _expandButtonClickedTime = DateTime.Now;
            IsExpanded = !IsExpanded;
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TreeView.RenderNodes(_children, _childrenStackLayout, e, this);
            Render();
        }
    }
}
