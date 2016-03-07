//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Models;
using System;
using System.Collections.Generic;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Navigation = Windows.UI.Xaml.Navigation;

namespace ArcGISRuntime.Windows.Viewer
{
    public sealed partial class MainPage
    {
        SystemNavigationManager _currentView = null;

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

        private async void Initialize()
        {
            // Initialize manager that handles all the samples, this will load all the items from samples assembly and related files
            await SampleManager.Current.InitializeAsync(ApplicationManager.Current.SelectedLanguage);

            // Create categories list. Also add Featured there as a single category.
            var categoriesList = SampleManager.Current.GetSamplesInCategories();

            var collectedFeaturedSamplesList = new List<object>();
            var featuredSampleList = SampleManager.Current.GetFeaturedSamples();

            // Collect all featured samples from the samples list and construct featured category
            foreach (var featured in featuredSampleList)
            {
                foreach (var category in categoriesList)
                {
                    foreach (var sample in category.Items)
                    {
                        var sampleModel = (sample as SampleModel);
                        if (sampleModel == null) continue;

                        if (sampleModel.SampleName == featured.SampleName)
                            collectedFeaturedSamplesList.Add(sampleModel);
                    }
                }
            }
            // Make sure that Featured is shown on top of the categories
            categoriesList.Insert(0, new TreeItem() { Name = "Featured", Items = collectedFeaturedSamplesList });

            categories.ItemsSource = categoriesList;
            categories.SelectedIndex = 0;

            Frame.Navigated += OnFrameNavigated;

            HideLoadingIndication();
        }

        private void OnCategoriesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var featuredItem = e.AddedItems[0] as TreeItem;
                if (RootSplitView.DisplayMode != SplitViewDisplayMode.Inline)
                    RootSplitView.IsPaneOpen = false;
            }
        }

        private void OnSampleItemTapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedSample = (sender as FrameworkElement).DataContext as SampleModel;
            if (selectedSample == null) return;

            SampleManager.Current.SelectedSample = selectedSample;

            // Navigate to the sample page that shows the sample and details
            Frame.Navigate(typeof(SamplePage));
        }

        private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (SearchToggleButton.IsChecked.HasValue && SearchToggleButton.IsChecked.Value)
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchToggleButton.Visibility = Visibility.Visible;
                SearchToggleButton.IsChecked = false;
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
    }
}
