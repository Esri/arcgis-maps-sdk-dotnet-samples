// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.UWP.Viewer.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Navigation = Windows.UI.Xaml.Navigation;
using ArcGISRuntime.Samples.Shared.Models;
using System.Threading.Tasks;

namespace ArcGISRuntime.UWP.Viewer
{
    public sealed partial class MainPage
    {
        private readonly SystemNavigationManager _currentView = null;

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
        private async void HideStatusBar()
        { 
            // If we have a phone contract, hide the status bar
            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
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
            _currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void HideLoadingIndication()
        {
            LoadingIndicatorArea.Visibility = Visibility.Collapsed;
            LoadingProgressRing.IsActive = false;
        }

        private void ShowLoadingIndication()
        {
            LoadingIndicatorArea.Visibility = Visibility.Visible;
            LoadingProgressRing.IsActive = true;
        }

        protected override void OnNavigatedTo(Navigation.NavigationEventArgs e)
        {
            // Force GC to get invoke full clean up when ever
            GC.Collect();
            GC.WaitForPendingFinalizers();
            base.OnNavigatedTo(e);
        }

        private void Initialize()
        {
            // Initialize manager that handles all the samples, this will load all the items from samples assembly and related files
            SampleManager.Current.Initialize();

            // Create categories list. Also add Featured there as a single category.
            var categoriesList = SampleManager.Current.FullTree;

            categories.ItemsSource = categoriesList.Items;
            categories.SelectedIndex = 0;
            (Window.Current.Content as Frame).Navigated += OnFrameNavigated;

            HideLoadingIndication();
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
            var selectedSample = (sender as FrameworkElement).DataContext as SampleInfo;
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
                    // Remove the waitpage from the stack
                    Frame.BackStack.Remove(Frame.BackStack.Where(m => m.SourcePageType == typeof(WaitPage)).First());
                }
                
                // Call a function to clear any existing credentials from AuthenticationManager
                ClearCredentials();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception exception)
            {
                // failed to create new instance of the sample
                Frame.Navigate(typeof(ErrorPage), exception);
            }
        }

        private void ClearCredentials()
        {
            // Clear credentials (if any) from previous sample runs
            var creds = Esri.ArcGISRuntime.Security.AuthenticationManager.Current.Credentials;
            for (var i = creds.Count() - 1; i >= 0; i--)
            {
                var c = creds.ElementAtOrDefault(i);
                if (c != null)
                {
                    Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveCredential(c);
                }
            }
        }

        private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (SearchToggleButton.IsChecked.HasValue && SearchToggleButton.IsChecked.Value)
            {
                //SearchBox.Visibility = Visibility.Collapsed;
                SearchToggleButton.Visibility = Visibility.Visible;
                SearchToggleButton.IsChecked = false;
            }
        }

        private void OnSearchToggleChecked(object sender, RoutedEventArgs e)
        {
            if (SearchToggleButton.IsChecked.HasValue && SearchToggleButton.IsChecked.Value)
            {
                //SearchBox.Visibility = Visibility.Visible;
                SearchToggleButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                //SearchBox.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnInfoClicked(object sender, RoutedEventArgs e)
        {
            var sampleModel = (sender as Button).DataContext as SampleInfo;
            if (sampleModel == null)
                return;

            // Create dialog that is used to show the picture
            var dialog = new ContentDialog()
            {
                Title = sampleModel.SampleName,
                //MaxWidth = ActualWidth,
                //MaxHeight = ActualHeight
            };

            dialog.PrimaryButtonText = "close";
            dialog.SecondaryButtonText = "show";
            dialog.PrimaryButtonClick += (s, args) =>
            {
               
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
    }
}
