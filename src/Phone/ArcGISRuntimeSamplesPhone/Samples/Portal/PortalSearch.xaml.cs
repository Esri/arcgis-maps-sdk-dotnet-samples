using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// This sample demonstrates how to use the Portal API to search for web maps on ArcGIS.com that match the search criteria.
    /// </summary>
    /// <title>Search</title>
    /// <category>Portal</category>
    public partial class PortalSearch : Page
    {
        private const string DEFAULT_SERVER_URL = "https://www.arcgis.com/sharing/rest";
        private const string DEFAULT_TOKEN_URL = "https://www.arcgis.com/sharing/generateToken";

        private ArcGISPortal _portal;

        /// <summary>Construct Portal Search sample control</summary>
        public PortalSearch()
        {
            InitializeComponent();

            IdentityManager.Current.RegisterServer(
                new ServerInfo()
                {
                    ServerUri = DEFAULT_SERVER_URL,
                    TokenServiceUri = DEFAULT_TOKEN_URL,
                });
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(Challenge);

            Loaded += control_Loaded;
        }

        // Activate IdentityManager but don't accept any challenge.
        // User must use the 'SignIn' button for getting their own maps.
        private Task<Credential> Challenge(CredentialRequestInfo arg)
        {
            return Task.FromResult<Credential>(null);
        }

        // Initialize the display with a web map and search portal for basemaps
        private async void control_Loaded(object sender, RoutedEventArgs e)
        {
            _portal = await ArcGISPortal.CreateAsync();

            // Initial search on load
            DoSearch();
        }

        private void BackToResults_Click(object sender, RoutedEventArgs e)
        {
            ResetVisibility();
        }

		private void QueryText_KeyDown(object sender, KeyRoutedEventArgs e)
		{
            if (e.Key == VirtualKey.Enter)
            {
                DoSearch();
                e.Handled = true;
            }
        }

        private void DoSearch_Click(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        private void ResetVisibility()
        {
            WebmapContent.Visibility = Visibility.Collapsed;
            BackToResults.IsEnabled = false;
            LoginGrid.Visibility = Visibility.Collapsed;
            ShadowGrid.Visibility = Visibility.Collapsed;
        }

        // Search arcgis.com for web maps matching the query text
        private async void DoSearch()
        {
            try
            {
                ResultsListBox.ItemsSource = null;
                ResetVisibility();
                if (QueryText == null || string.IsNullOrEmpty(QueryText.Text.Trim()))
                    return;

                var queryString = string.Format("{0} type:(\"web map\" NOT \"web mapping application\")", QueryText.Text.Trim());
                if (_portal.CurrentUser != null && _portal.ArcGISPortalInfo != null && !string.IsNullOrEmpty(_portal.ArcGISPortalInfo.Id))
                    queryString = string.Format("{0} orgid:(\"{1}\")", queryString, _portal.ArcGISPortalInfo.Id);

                var searchParameters = new SearchParameters()
                {
                    QueryString = queryString,
                    SortField = "avgrating",
                    SortOrder = QuerySortOrder.Descending,
                    Limit = 20
                };

                var result = await _portal.SearchItemsAsync(searchParameters);
                ResultsListBox.ItemsSource = result.Results;

                if (result.Results != null && result.Results.Count() > 0)
                    ResultsListBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
        }

        private async void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BackToResults.IsEnabled = true;

                var item = ((Button)sender).DataContext as ArcGISPortalItem;
                var webmap = await WebMap.FromPortalItemAsync(item);
                var vm = await WebMapViewModel.LoadAsync(webmap, _portal);
                MyMapView.Map = vm.Map;

                WebmapContent.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
        }

        #region Sign in/out
        private async void ShowSignIn_Click(object sender, RoutedEventArgs e)
        {
            if (SignInButton.Content.ToString() == "Sign In")
            {
                ShadowGrid.Visibility = Visibility.Visible;
                LoginGrid.Visibility = Visibility.Visible;
            }
            else //Sign Out
            {
                ResultsListBox.ItemsSource = null;

                var crd = IdentityManager.Current.FindCredential(DEFAULT_SERVER_URL);
                IdentityManager.Current.RemoveCredential(crd);

                _portal = await ArcGISPortal.CreateAsync(new Uri(DEFAULT_SERVER_URL));

                ResetVisibility();
                SignInButton.Content = "Sign In";

                DoSearch();
            }
        }

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            ResultsListBox.ItemsSource = null;

            try
            {
                var crd = await IdentityManager.Current.GenerateCredentialAsync(
                    DEFAULT_SERVER_URL, UserTextBox.Text, PasswordTextBox.Password);
                IdentityManager.Current.AddCredential(crd);

                _portal = await ArcGISPortal.CreateAsync(new Uri(DEFAULT_SERVER_URL));

                ResetVisibility();
                SignInButton.Content = "Sign Out";

                DoSearch();
            }
            catch
            {
				var _x = new MessageDialog("Could not log in. Please check credentials.").ShowAsync();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            ShadowGrid.Visibility = Visibility.Collapsed;
        }
        #endregion
    }

    // Helper class to get a description string from HTML content
    internal class HtmlToTextConverter : IValueConverter
    {
        private static string htmlLineBreakRegex = @"(<br */>)|(\[br */\])"; //@"<br(.)*?>";	// Regular expression to strip HTML line break tag
        private static string htmlStripperRegex = @"<(.|\n)*?>";	// Regular expression to strip HTML tags

        public static string GetHtmlToInlines(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlToInlinesProperty);
        }

        public static void SetHtmlToInlines(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlToInlinesProperty, value);
        }

        // Using a DependencyProperty as the backing store for HtmlToInlinesProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlToInlinesProperty =
          DependencyProperty.RegisterAttached("HtmlToInlines", typeof(string), typeof(HtmlToTextConverter), new PropertyMetadata(null, OnHtmlToInlinesPropertyChanged));

        private static void OnHtmlToInlinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Paragraph)
            {
                if (e.NewValue == null)
                    (d as Paragraph).Inlines.Clear();
                else
                {
                    var splits = Regex.Split(e.NewValue as string, htmlLineBreakRegex, RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
                    foreach (var line in splits)
                    {
                        string text = Regex.Replace(line, htmlStripperRegex, string.Empty);
                        Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            text = regex.Replace(text, @" "); //Remove multiple spaces
                            text = text.Replace("&quot;", "\""); //Unencode quotes
                            text = text.Replace("&nbsp;", " "); //Unencode spaces
                            (d as Paragraph).Inlines.Add(new Run() { Text = text });
                            (d as Paragraph).Inlines.Add(new LineBreak());
                        }
                    }
                }
            }
        }

        private static string ToStrippedHtmlText(object input)
        {
            string retVal = string.Empty;

            if (input != null)
            {
                // Replace HTML line break tags with $LINEBREAK$:
                retVal = Regex.Replace(input as string, htmlLineBreakRegex, "", RegexOptions.IgnoreCase);
                // Remove the rest of HTML tags:
                retVal = Regex.Replace(retVal, htmlStripperRegex, string.Empty);
                //retVal.Replace("$LINEBREAK$", "\n");
                retVal = retVal.Trim();
            }

            return retVal;
        }

		public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
                return ToStrippedHtmlText(value);
            return value;
        }

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	internal class ConcatenateStringsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var strings = value as IEnumerable<string>;
			if (strings == null)
				return value;

			return string.Join(", ", strings.ToArray());
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
