using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISRuntime.Controls
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
            get { return (string) GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
            "SearchText", typeof(string), typeof(SearchTextBox), new PropertyMetadata(default(string)));

        public string SearchText
        {
            get { return (string) GetValue(SearchTextProperty); }
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
