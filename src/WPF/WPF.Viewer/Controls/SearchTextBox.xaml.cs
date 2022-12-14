using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.Controls
{
    /// <summary>
    /// Interaction logic for SearchTextBox.xaml
    /// </summary>
    public partial class SearchTextBox : UserControl
    {
        public SearchTextBox()
        {
            InitializeComponent();

            DataContext = this;

            SearchBox.TextChanged += SearchBoxOnTextChanged;
        }

        private void SearchBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = SearchBox.Text;
            TextChanged?.Invoke(this, e);
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(SearchTextBox), new PropertyMetadata(default(string)));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
            "SearchText", typeof(string), typeof(SearchTextBox), new PropertyMetadata(default(string)));

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public event TextChangedEventHandler TextChanged;

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(SearchBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Visible;
                ClearButton.Visibility = Visibility.Collapsed;
                SearchImage.Visibility = Visibility.Visible;
            }
            else
            {
                SearchPlaceholder.Visibility = Visibility.Collapsed;
                ClearButton.Visibility = Visibility.Visible;
                SearchImage.Visibility = Visibility.Collapsed;
            }
        }

        private void Clear_Clicked(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }
    }
}