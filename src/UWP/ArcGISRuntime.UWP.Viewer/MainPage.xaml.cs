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
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Navigation = Windows.UI.Xaml.Navigation;

namespace ArcGISRuntime.UWP.Viewer
{
    public sealed partial class MainPage
    {
        private readonly SystemNavigationManager _currentView;

        public MainPage()
        {
            InitializeComponent();

            // Use required cache mode so we create only one page
            NavigationCacheMode = Navigation.NavigationCacheMode.Required;
            // Get current view that provides access to the back button
            _currentView = SystemNavigationManager.GetForCurrentView();
            _currentView.BackRequested += OnFrameNavigationRequested;

            HideStatusBar();

            Initialize();
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

        private void OnFrameNavigationRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                Frame.GoBack();
            }
            _currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void OnFrameNavigated(object sender, Navigation.NavigationEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                _currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
        }

        private void Initialize()
        {
            // Initialize manager that handles all the samples, this will load all the items from samples assembly and related files
            SampleManager.Current.Initialize();

            // Create categories list. Also add Featured there as a single category.
            var categoriesList = SampleManager.Current.FullTree;

            Categories.ItemsSource = categoriesList.Items;
            Categories.SelectedIndex = 0;
            ((Frame)Window.Current.Content).Navigated += OnFrameNavigated;
        }

        private void OnCategoriesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (RootSplitView.DisplayMode != SplitViewDisplayMode.Inline)
                    RootSplitView.IsPaneOpen = false;
            }
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
                    // Show the waiting page
                    Frame.Navigate(typeof(WaitPage));
                    // Wait for offline data to complete
                    await DataManager.EnsureSampleDataPresent(selectedSample);
                }
                // Show the sample
                Frame.Navigate(typeof(SamplePage));

                // Only remove download page from navigation stack if it was shown
                if (selectedSample.OfflineDataItems != null)
                {
                    // Remove the wait page from the stack
                    Frame.BackStack.Remove(Frame.BackStack.First(m => m.SourcePageType == typeof(WaitPage)));
                }
            }
            catch (Exception exception)
            {
                // failed to create new instance of the sample
                Frame.Navigate(typeof(ErrorPage), exception);
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
            var sampleModel = (sender as Button)?.DataContext as SampleInfo;
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

        private void OnSearchQuerySubmitted(AutoSuggestBox searchBox, AutoSuggestBoxTextChangedEventArgs searchBoxQueryChangedEventArgs)
        {
            if (SearchToggleButton.IsChecked == true)
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchToggleButton.Visibility = Visibility.Visible;
                SearchToggleButton.IsChecked = false;
            }
            var categoriesList = SampleManager.Current.FullTree.Search(SampleSearchFunc);
            if (categoriesList == null)
            {
                categoriesList = new SearchableTreeNode("Search", new[]{new SearchableTreeNode("No results", new List<object>())});
            }
            Categories.ItemsSource = categoriesList.Items;

            if (categoriesList.Items.Any())
            {
                Categories.SelectedIndex = 0;
            }
        }

        private void OnSearchToggleChecked(object sender, RoutedEventArgs e)
        {
            if (SearchToggleButton.IsChecked.HasValue && SearchToggleButton.IsChecked.Value)
            {
                SearchBox.Visibility = Visibility.Visible;
                SearchToggleButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchBox.Visibility = Visibility.Collapsed;
            }
        }

        private bool SampleSearchFunc(SampleInfo sample)
        {
            return SampleManager.Current.SampleSearchFunc(sample, SearchBox.Text);
        }

    }
}