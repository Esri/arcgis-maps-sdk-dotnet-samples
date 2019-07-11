// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using ArcGISRuntime.UWP.Viewer.Dialogs;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using muxc = Microsoft.UI.Xaml.Controls;
using Navigation = Windows.UI.Xaml.Navigation;

namespace ArcGISRuntime.UWP.Viewer
{
    public sealed partial class MainPage
    {
        private readonly SystemNavigationManager _currentView;
        private bool _waitFlag;

        public MainPage()
        {
            InitializeComponent();

            // Use required cache mode so we create only one page
            NavigationCacheMode = Navigation.NavigationCacheMode.Required;

            // Get current view that provides access to the back button
            _currentView = SystemNavigationManager.GetForCurrentView();

            HideStatusBar();

            Initialize();

            LoadTreeView(SampleManager.Current.FullTree);

            // Acrylic backgrounds
            MainContentRegion.Background = new AcrylicBrush() { TintOpacity = 50, BackgroundSource = AcrylicBackgroundSource.HostBackdrop };
            CategoriesTree.Background = new SolidColorBrush() { Opacity = 0 };
            SamplePageContainer.Background = new SolidColorBrush() { Opacity = 0 };

            SetDarkMode();

            SamplesGridView.ItemsSource = SamplesListView.ItemsSource = CategoriesTree.RootNodes[0].Children.ToList().Select(x => (SampleInfo)x.Content).ToList();
        }

        private void SetDarkMode()
        {
            if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                MainContentRegion.Background = new AcrylicBrush() { TintColor = Windows.UI.Color.FromArgb(150, 0, 0, 0), TintOpacity = 25, BackgroundSource = AcrylicBackgroundSource.HostBackdrop };
            }
        }

        private void LoadTreeView(SearchableTreeNode fullTree)
        {
            // This happens when there are no search results.
            if (fullTree == null)
            {
                return;
            }
            CategoriesTree.RootNodes.Clear();

            muxc.TreeViewNode rootNode;

            foreach (SearchableTreeNode category in fullTree.Items)
            {
                rootNode = new muxc.TreeViewNode() { Content = category };
                category.Items.ForEach(info => rootNode.Children.Add(new muxc.TreeViewNode() { Content = info }));

                CategoriesTree.RootNodes.Add(rootNode);
            }
        }

        // Check if the phone contract is available (mobile) and hide status bar if it is there
        private static async void HideStatusBar()
        {
            // If we have a phone contract, hide the status bar
            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                await StatusBar.GetForCurrentView().HideAsync();
            }
        }

        private void Initialize()
        {
            // Initialize manager that handles all the samples, this will load all the items from samples assembly and related files
            SampleManager.Current.Initialize();

            // Create categories list. Also add Featured there as a single category.
            var categoriesList = SampleManager.Current.FullTree;
        }

        private void OnCategoriesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SamplePageContainer.Visibility = Visibility.Collapsed;
            SampleSelectionGrid.Visibility = Visibility.Visible;
        }

        private async void OnSampleItemTapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedSample = (sender as FrameworkElement)?.DataContext as SampleInfo;
            await SelectSample(selectedSample);
        }

        private async Task SelectSample(SampleInfo selectedSample)
        {
            // Call a function to clear existing credentials
            ClearCredentials();

            SampleManager.Current.SelectedSample = selectedSample;

            try
            {
                if (selectedSample.OfflineDataItems != null)
                {
                    CancellationTokenSource cancellationSource = new CancellationTokenSource();

                    // Show the waiting page
                    SamplePageContainer.Content = new WaitPage(cancellationSource);
                    SamplePageContainer.Visibility = Visibility.Visible;
                    SampleSelectionGrid.Visibility = Visibility.Collapsed;

                    // Wait for offline data to complete
                    await DataManager.EnsureSampleDataPresent(selectedSample, cancellationSource.Token);
                }
                // Show the sample
                SamplePageContainer.Content = new SamplePage();
                SamplePageContainer.Visibility = Visibility.Visible;
                SampleSelectionGrid.Visibility = Visibility.Collapsed;
            }
            catch (Exception exception)
            {
                // failed to create new instance of the sample
                SamplePageContainer.Visibility = Visibility.Collapsed;
                SampleSelectionGrid.Visibility = Visibility.Visible;
                await new MessageDialog(exception.Message).ShowAsync();
            }
        }

        private static void ClearCredentials()
        {
            // Clear credentials (if any) from previous sample runs
            foreach (Credential cred in AuthenticationManager.Current.Credentials)
            {
                if (cred != null)
                {
                    AuthenticationManager.Current.RemoveCredential(cred);
                }
            }
        }

        private async void OnInfoClicked(object sender, RoutedEventArgs e)
        {
            var sampleModel = ((Button)sender)?.DataContext as SampleInfo;
            if (sampleModel == null)
                return;

            // Create dialog that is used to show the picture
            var dialog = new ContentDialog
            {
                Title = sampleModel.SampleName,
                PrimaryButtonText = "close",
                SecondaryButtonText = "show",
            };

            dialog.SecondaryButtonClick += (s, args) =>
            {
                OnSampleItemTapped(sender, new TappedRoutedEventArgs());
            };

            dialog.Content = new SampleInfoDialog() { DataContext = sampleModel };

            // Show dialog as a full screen overlay.
            await dialog.ShowAsync();
        }

        private void OnInfoTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private async void OnSearchQuerySubmitted(AutoSuggestBox searchBox, AutoSuggestBoxTextChangedEventArgs searchBoxQueryChangedEventArgs)
        {
            if (_waitFlag) { return; }
            _waitFlag = true;
            await Task.Delay(200);
            _waitFlag = false;

            // Search using the sample manager
            var categoriesList = SampleManager.Current.FullTree.Search(SampleSearchFunc);
            if (categoriesList == null)
            {
                categoriesList = new SearchableTreeNode("Search", new[] { new SearchableTreeNode("No results", new List<object>()) });
            }

            // Load the tree of the current categories.
            LoadTreeView(categoriesList);

            // Check if there are search results.
            if (CategoriesTree.RootNodes.Any())
            {
                // Set the items source of the grid to the first category from the search.
                SamplesGridView.ItemsSource = SamplesListView.ItemsSource = CategoriesTree.RootNodes[0].Children.ToList().Select(x => (SampleInfo)x.Content).ToList();
                foreach (muxc.TreeViewNode node in CategoriesTree.RootNodes)
                {
                    node.IsExpanded = true;
                }
            }

            // Switch to the sample selection grid.
            SamplePageContainer.Visibility = Visibility.Collapsed;
            SampleSelectionGrid.Visibility = Visibility.Visible;
        }

        private bool SampleSearchFunc(SampleInfo sample)
        {
            return SampleManager.Current.SampleSearchFunc(sample, SearchBox.Text);
        }

        private async void CategoriesTree_ItemInvoked(muxc.TreeView sender, muxc.TreeViewItemInvokedEventArgs e)
        {
            muxc.TreeViewNode selected = (muxc.TreeViewNode)e.InvokedItem;

            if (selected.Content.GetType() == typeof(SearchableTreeNode))
            {
                SamplePageContainer.Visibility = Visibility.Collapsed;
                SampleSelectionGrid.Visibility = Visibility.Visible;
                List<SampleInfo> samples = selected.Children.ToList().Select(x => (SampleInfo)x.Content).ToList();
                SamplesGridView.ItemsSource = samples;
                SamplesListView.ItemsSource = samples;
            }
            else if (selected.Content.GetType() == typeof(SampleInfo))
            {
                await SelectSample((SampleInfo)selected.Content);
            }
        }

        // https://stackoverflow.com/questions/32692792/open-a-new-frame-window-from-mainpage-in-windows-10-universal-app
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsWindow));
        }
    }

    internal class TreeViewItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate SampleTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (((muxc.TreeViewNode)item).Content.GetType() == typeof(SearchableTreeNode))
            {
                return CategoryTemplate;
            }
            else if (((muxc.TreeViewNode)item).Content.GetType() == typeof(SampleInfo))
            {
                return SampleTemplate;
            }
            else
            {
                return null;
            }
        }
    }
}