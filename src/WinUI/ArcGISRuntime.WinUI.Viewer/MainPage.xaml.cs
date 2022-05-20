﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Navigation = Microsoft.UI.Xaml.Navigation;

namespace ArcGISRuntime.WinUI.Viewer
{
    public sealed partial class MainPage
    {
        private bool _waitFlag;

        private List<string> _namedUserSamples = new List<string> {
            "AuthorMap",
            "SearchPortalMaps",
            "OAuth" };

        public MainPage()
        {
            InitializeComponent();

            this.Loaded += FirstLoaded;

            // Use required cache mode so we create only one page.
            NavigationCacheMode = Navigation.NavigationCacheMode.Required;

            Initialize();

            LoadTreeView(SampleManager.Current.FullTree);

            // Set the ItemsSource for the grid from the first category.
            SamplesGridView.ItemsSource = CategoriesTree.RootNodes[0].Children.ToList().Select(x => (SampleInfo)x.Content).ToList();
        }

        private void FirstLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= FirstLoaded;

            _ = CheckApiKey();
        }

        private async Task CheckApiKey()
        {
            // Attempt to load a locally stored API key.
            await ApiKeyManager.TrySetLocalKey();

            // Check that the current API key is valid.
            ApiKeyStatus status = await ApiKeyManager.CheckKeyValidity();
            if (status != ApiKeyStatus.Valid)
            {
                await ApiKeyDialog.ShowAsync();
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

            TreeViewNode rootNode;

            foreach (SearchableTreeNode category in fullTree.Items)
            {
                rootNode = new TreeViewNode() { Content = category };
                category.Items.ForEach(info => rootNode.Children.Add(new TreeViewNode() { Content = info }));

                CategoriesTree.RootNodes.Add(rootNode);
            }
        }

        private void Initialize()
        {
            // Initialize manager that handles all the samples, this will load all the items from samples assembly and related files
            SampleManager.Current.Initialize();

            SearchBoxBorder.Background = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
        }

        private async void OnSampleItemTapped(object sender, TappedRoutedEventArgs e)
        {
            SampleInfo selectedSample = (sender as FrameworkElement)?.DataContext as SampleInfo;
            await SelectSample(selectedSample);
        }

        private async Task SelectSample(SampleInfo selectedSample)
        {
            // Restore API key if leaving named user sample.
            if (_namedUserSamples.Contains(SampleManager.Current?.SelectedSample?.FormalName))
            {
                ApiKeyManager.EnableKey();
            }

            // Remove API key if opening named user sample.
            if (_namedUserSamples.Contains(selectedSample.FormalName))
            {
                ApiKeyManager.DisableKey();
            }

            // Call disposable on currently selected sample.
            if (SampleManager.Current?.SelectedSample is IDisposable disposableSample)
            {
                disposableSample.Dispose();
            }

            // Call a function to clear any existing credentials from AuthenticationManager
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
                // Failed to create new instance of the sample.
                SamplePageContainer.Visibility = Visibility.Collapsed;
                SampleSelectionGrid.Visibility = Visibility.Visible;
                CategoriesTree.SelectionMode = TreeViewSelectionMode.None;
                await new MessageDialog2(exception.Message).ShowAsync();
                CategoriesTree.SelectionMode = TreeViewSelectionMode.Single;
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

            // Clear the challenge handler.
            AuthenticationManager.Current.ChallengeHandler = null;
        }

        private async void OnSearchQuerySubmitted(AutoSuggestBox searchBox, AutoSuggestBoxTextChangedEventArgs searchBoxQueryChangedEventArgs)
        {
            // Dont search again until wait from previous search expires.
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
                SamplesGridView.ItemsSource = CategoriesTree.RootNodes[0].Children.ToList().Select(x => (SampleInfo)x.Content).ToList();

                if (!string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    foreach (TreeViewNode node in CategoriesTree.RootNodes)
                    {
                        node.IsExpanded = true;
                    }
                }
            }

            // Switch to the sample selection grid.
            SamplePageContainer.Visibility = Visibility.Collapsed;
            SamplePageContainer.Content = null;
            SampleSelectionGrid.Visibility = Visibility.Visible;
        }

        private bool SampleSearchFunc(SampleInfo sample)
        {
            return SampleManager.Current.SampleSearchFunc(sample, SearchBox.Text);
        }

        private async void CategoriesTree_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs e)
        {
            TreeViewNode selected = (TreeViewNode)e.InvokedItem;

            if (selected.Content.GetType() == typeof(SearchableTreeNode))
            {
                SamplePageContainer.Visibility = Visibility.Collapsed;
                SamplePageContainer.Content = null;
                SampleSelectionGrid.Visibility = Visibility.Visible;
                List<SampleInfo> samples = selected.Children.ToList().Select(x => (SampleInfo)x.Content).ToList();
                SamplesGridView.ItemsSource = samples;
            }
            else if (selected.Content.GetType() == typeof(SampleInfo))
            {
                await SelectSample((SampleInfo)selected.Content);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _ = settingsDialog.ShowAsync();
        }
    }

    internal class TreeViewItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate SampleTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            // Select the correct template to display the text for the node in the treeview.
            if (((TreeViewNode)item).Content.GetType() == typeof(SearchableTreeNode))
            {
                return CategoryTemplate;
            }
            else if (((TreeViewNode)item).Content.GetType() == typeof(SampleInfo))
            {
                return SampleTemplate;
            }
            else
            {
                // No text will be displayed if another type of content is in the treeview.
                return null;
            }
        }
    }
}