// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

#if ENABLE_ANALYTICS
using ArcGIS.Helpers;
#endif

using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Managers;
using ArcGIS.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGIS.Samples.Desktop
{
    public partial class MainWindow
    {
        private List<TreeViewItem> _samples;
        private System.Windows.Forms.Timer _delaySearchTimer;
        private const int _delayedTextChangedTimeout = 500;
        private bool _settingsWindowOpen, _feedbackWindowOpen;

        private List<string> _namedUserSamples = new List<string> {
            "AuthorMap",
            "SearchPortalMaps",
            "OAuth" };

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                SampleManager.Current.Initialize();

                // Set category data context
                _samples = WPF.Viewer.Helpers.ToTreeViewItem(SampleManager.Current.FullTree);
                Categories.DataContext = _samples;

                // Select the first item
                _samples.First().IsSelected = true;

                Loaded += FirstLoaded;

#if ENABLE_ANALYTICS
                // Check analytics settings
                AnalyticsHelper.ReadSettingsFromFile();
                _ = AnalyticsHelper.TrackEvent("sample_viewer_opened");
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Initialization exception occurred.");
            }
        }

        private void FirstLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FirstLoaded;

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
                PromptForKey();
            }
        }

        private void PromptForKey()
        {
            var keyPrompt = new Window() { Width = 500, Height = 220, Title = "Edit API key" };
            keyPrompt.Content = new ApiKeyPrompt();
            keyPrompt.Show();
        }

        private async void categoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var sample = e.AddedItems[0] as SampleInfo;
                ((ListView)sender).SelectedItem = null;
                DetailsRegion.Visibility = Visibility.Visible;
                CategoriesRegion.Visibility = Visibility.Collapsed;
                await SelectSample(sample);

                // Deselect all categories
                ((List<TreeViewItem>)Categories.DataContext).ForEach(item => item.IsSelected = false);
            }
        }

        private async void categories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var context = e.NewValue as TreeViewItem;
            if (context == null) { return; }
            var sample = context.DataContext as SampleInfo;
            var category = context.DataContext as SearchableTreeNode;
            if (category != null)
            {
                CategoriesList.ItemsSource = category.Items;
                DetailsRegion.Visibility = Visibility.Collapsed;
                SampleContainer.Content = null;
                SampleManager.Current.SelectedSample = null;
                CategoriesRegion.Visibility = Visibility.Visible;
                CategoriesHeader.Text = category.Name;

                // Send analytics.
                var eventData = new Dictionary<string, string> 
                {
                    { "Category", category.Name },
                };

#if ENABLE_ANALYTICS
                _ = AnalyticsHelper.TrackEvent("category_selected", eventData);
#endif
            }
            else if (sample != null)
            {
                DetailsRegion.Visibility = Visibility.Visible;
                CategoriesRegion.Visibility = Visibility.Collapsed;
                CategoriesHeader.Text = string.Empty;
                await SelectSample(sample);
            }
        }

        private async Task SelectSample(SampleInfo selectedSample)
        {
            if (selectedSample == null) return;

            // Prevent re-opening current sample.
            if (SampleManager.Current.SelectedSample == selectedSample) return;

            // Send analytics.
            var eventData = new Dictionary<string, string> 
            {
                    { "Sample", selectedSample.SampleName },
            };

#if ENABLE_ANALYTICS
            _ = AnalyticsHelper.TrackEvent("sample_opened", eventData);
#endif

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

            // Call a function to clear any existing credentials from AuthenticationManager
            ClearCredentials();

            SampleTitleBlock.Text = selectedSample.SampleName;
            SampleManager.Current.SelectedSample = selectedSample;
            DescriptionContainer.SetSample(selectedSample);
            ShowSampleTab();

            try
            {
                if (selectedSample.OfflineDataItems != null)
                {
                    CancellationTokenSource cancellationSource = new CancellationTokenSource();

                    // Show waiting page
                    var waitPage = new WPF.Viewer.WaitPage(cancellationSource);
                    SampleContainer.Content = waitPage;

                    // Wait for offline data to complete
                    await DataManager.EnsureSampleDataPresent(selectedSample, cancellationSource.Token,
                    (info) =>
                    {
                        waitPage.SetProgress(info.Percentage, info.HasPercentage, info.TotalBytes);
                    });
                }

                // Show the sample
                SampleContainer.Content = SampleManager.Current.SampleToControl(selectedSample);
                (SampleContainer.Content as FrameworkElement).Unloaded += Sample_Unloaded;
                SourceCodeContainer.LoadSourceCode();

                // Set the color of the favorite button
                SetInSampleFavoriteButtonColor(selectedSample);
            }
            catch (OperationCanceledException)
            {
                CategoriesRegion.Visibility = Visibility.Visible;
                SampleContainer.Visibility = Visibility.Collapsed;
                SampleManager.Current.SelectedSample = null;
                return;
            }
            catch (Exception exception)
            {
                // failed to create new instance of the sample
                SampleContainer.Content = new WPF.Viewer.ErrorPage(exception);
            }

            SetScreenshotButttonVisibility();
            SetContainerDimensions();
        }

        private static void Sample_Unloaded(object sender, RoutedEventArgs e)
        {
            // Explicit cleanup of the Map and SceneView instead of waiting for garbage collector can help when
            // lots of geoviews are being opened and closed
            foreach (var geoView in TreeWalker<GeoView>(sender as UIElement))
            {
                if (geoView is MapView mapView)
                {
                    mapView.Map = null;
                    if (mapView.LocationDisplay != null) mapView.LocationDisplay.IsEnabled = false;
                }
                else if (geoView is SceneView sceneView) sceneView.Scene = null;
            }
        }

        private static IEnumerable<T> TreeWalker<T>(UIElement root)
        {
            if (root != null)
            {
                if (root is T t)
                    yield return t;
                else if (root is UIElement it)
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(it); i++)
                    {
                        var e = VisualTreeHelper.GetChild(it, i);
                        foreach (var obj in TreeWalker<T>(e as UIElement))
                            yield return obj;
                    }
                }
            }
        }

        private static void ClearCredentials()
        {
            // Clear credentials (if any) from previous sample runs
            foreach (var cred in AuthenticationManager.Current.Credentials)
            {
                if (cred != null)
                {
                    AuthenticationManager.Current.RemoveCredential(cred);
                }
            }

            // Clear the challenge handler.
            AuthenticationManager.Current.ChallengeHandler = null;
        }

        private void OpenCategoryLeaves()
        {
            if (Categories.Items.Count > 0)
            {
                var firstTreeViewItem = Categories.Items[0] as TreeViewItem;
                if (firstTreeViewItem != null) firstTreeViewItem.IsSelected = true;

                foreach (var item in Categories.Items)
                {
                    var treeViewItem = item as TreeViewItem;
                    if (treeViewItem != null) treeViewItem.IsExpanded = true;
                }
            }
        }

        private void CloseCategoryLeaves()
        {
            if (Categories.Items.Count > 0)
            {
                var firstTreeViewItem = Categories.Items[0] as TreeViewItem;
                if (firstTreeViewItem != null) firstTreeViewItem.IsSelected = true;

                foreach (var item in Categories.Items)
                {
                    var treeViewItem = item as TreeViewItem;
                    if (treeViewItem != null) treeViewItem.IsExpanded = false;
                }
            }
        }

        private void ShowSampleTab()
        {
            SampleRadTab.IsChecked = true;
            SampleContainer.Visibility = Visibility.Visible;
            DescriptionContainer.Visibility = Visibility.Collapsed;
            CategoriesRegion.Visibility = Visibility.Collapsed;
            SourceCodeContainer.Visibility = Visibility.Collapsed;
        }

        private void ShowDescriptionTab()
        {
            DetailsRadTab.IsChecked = true;
            SampleContainer.Visibility = Visibility.Collapsed;
            DescriptionContainer.Visibility = Visibility.Visible;
            CategoriesRegion.Visibility = Visibility.Collapsed;
            SourceCodeContainer.Visibility = Visibility.Collapsed;
        }

        private void ShowSourceTab()
        {
            SourceRadTab.IsChecked = true;
            SampleContainer.Visibility = Visibility.Collapsed;
            DescriptionContainer.Visibility = Visibility.Collapsed;
            CategoriesRegion.Visibility = Visibility.Collapsed;
            SourceCodeContainer.Visibility = Visibility.Visible;
        }

        private void LiveSample_Click(object sender, RoutedEventArgs e)
        {
            ShowSampleTab();

            var eventData = new Dictionary<string, string> {
                { "Tab", "Sample" },
                { "Sample", SampleManager.Current.SelectedSample?.SampleName },
            };

#if ENABLE_ANALYTICS
            _ = AnalyticsHelper.TrackEvent("tab_selected", eventData);
#endif
        }

        private void Description_Click(object sender, RoutedEventArgs e)
        {
            ShowDescriptionTab();

            var eventData = new Dictionary<string, string> {
                { "Tab", "Description" },
                { "Sample", SampleManager.Current.SelectedSample?.SampleName },
            };

#if ENABLE_ANALYTICS
            _ = AnalyticsHelper.TrackEvent("tab_selected", eventData);
#endif

        }

        private void SourceCode_Click(object sender, RoutedEventArgs e)
        {
            ShowSourceTab();

            var eventData = new Dictionary<string, string> {
                { "Tab", "Source code" },
                { "Sample", SampleManager.Current.SelectedSample?.SampleName },
            };

#if ENABLE_ANALYTICS
            _ = AnalyticsHelper.TrackEvent("tab_selected", eventData);
#endif
        }

        private void GitHub_Click(object sender, RoutedEventArgs e)
        {
#if NETFRAMEWORK
            System.Diagnostics.Process.Start(SampleManager.Current.SelectedSample.GetGitHubUrl());
#elif NETCOREAPP
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = SampleManager.Current.SelectedSample.GetGitHubUrl(),
                UseShellExecute = true
            });
#endif
        }

        // Code here is adapted from the following StackOverflow answer:
        // https://stackoverflow.com/a/34148858
        private void SearchFilterBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Don't update results immediately; makes search-as-you-type more comfortable
            if (_delaySearchTimer != null)
            {
                _delaySearchTimer.Stop();
            }

            if (_delaySearchTimer == null)
            {
                _delaySearchTimer = new System.Windows.Forms.Timer();
                _delaySearchTimer.Tick += DelaySearchTimer_Tick;
                _delaySearchTimer.Interval = _delayedTextChangedTimeout;
            }

            _delaySearchTimer.Start();
        }

        private void DelaySearchTimer_Tick(object sender, EventArgs e)
        {
            PopulateSearchedTree();

            if (_delaySearchTimer != null)
            {
                _delaySearchTimer.Stop();
            }
        }

        private void PopulateSearchedTree()
        {
            var results = SampleManager.Current.FullTree.Search(SampleSearchFunc);

            // Set category data context
            Categories.DataContext = WPF.Viewer.Helpers.ToTreeViewItem(results);

            // Open all if query isn't empty
            if (!string.IsNullOrWhiteSpace(SearchFilterBox.SearchText))
            {
                OpenCategoryLeaves();
                TrackSearchEvent();
            }
            else
            {
                CloseCategoryLeaves();
            }
        }

        private void TrackSearchEvent()
        {
            var eventData = new Dictionary<string, string>() { { "Search", SearchFilterBox.SearchText } };

            // If the search has been initiated in a sample or category track the sample or category name. 
            if (SampleManager.Current.SelectedSample != null)
            {
                eventData.Add("Sample", SampleManager.Current.SelectedSample.SampleName);
            }
            else if (!string.IsNullOrEmpty(CategoriesHeader.Text))
            {
                eventData.Add("Category", CategoriesHeader.Text);
            }

#if ENABLE_ANALYTICS
            _ = AnalyticsHelper.TrackEvent("search_text", eventData);
#endif
        }

        private bool SampleSearchFunc(SampleInfo sample)
        {
            return SampleManager.Current.SampleSearchFunc(sample, SearchFilterBox.SearchText);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindowOpen) return;
            _settingsWindowOpen = true;
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.Closing += (send, args) => 
            {
                _settingsWindowOpen = false;
                CloseWindow();
            };
            settingsWindow.Show();
        }

        private void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_feedbackWindowOpen) return;
            _feedbackWindowOpen = true;
            var feedbackWindow = new FeedbackWindow();
            feedbackWindow.Owner = this;
            feedbackWindow.Closing += (send, args) =>
            {
                _feedbackWindowOpen = false;
                CloseWindow();
            };
            feedbackWindow.Show();
        }

        private void CloseWindow()
        {
            SetScreenshotButttonVisibility();
            SetContainerDimensions();

            // Reactivate the parent window so that it does not minimize on close.
            Activate();
        }

#region Update Favorites

        private void SampleGridFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected category name and expanded categories before updating the category tree.
            string selectedCategoryName = GetSelectedCategoryName();
            List<string> expandedCategoryNames = GetExpandedCategoryNames();

            string sampleFormalName = (sender as Button).CommandParameter.ToString();
            SampleManager.Current.AddRemoveFavorite(sampleFormalName);

            UpdateTreeViewItems();

            if (!string.IsNullOrEmpty(SearchFilterBox.SearchText))
            {
                PopulateSearchedTree();
            }

            // Set the selected category.
            SetSelectedCategory(selectedCategoryName);

            // Set the expanded categories.
            SetExpandedCategories(expandedCategoryNames);
        }

        private void InSampleFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            Categories.SelectedItemChanged -= categories_SelectedItemChanged;
            CategoriesList.SelectionChanged -= categoriesList_SelectionChanged;

            SampleManager.Current.AddRemoveFavorite(SampleManager.Current.SelectedSample.FormalName);
            SetInSampleFavoriteButtonColor(SampleManager.Current.SelectedSample);
            ResetCategories();

            Categories.SelectedItemChanged += categories_SelectedItemChanged;
            CategoriesList.SelectionChanged += categoriesList_SelectionChanged;
        }

        private void SetInSampleFavoriteButtonColor(SampleInfo selectedSample)
        {
            SampleFavoriteButton.Foreground = selectedSample.IsFavorite ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.White);
        }

#endregion Update Favorites

#region Category Visibility Properties

        private void SetSelectedCategory(string selectedCategoryName)
        {
            if (!string.IsNullOrEmpty(selectedCategoryName))
            {
                var selectedTreeViewItem = Categories.Items.Cast<TreeViewItem>().First(t => t.Header.Equals(selectedCategoryName));
                if (selectedTreeViewItem != null) selectedTreeViewItem.IsSelected = true;
            }
            else
            {
                var firstTreeViewItem = Categories.Items[0] as TreeViewItem;
                if (firstTreeViewItem != null) firstTreeViewItem.IsSelected = true;
            }
        }

        private void SetExpandedCategories(List<string> expandedCategoryNames)
        {
            if (expandedCategoryNames.Any())
            {
                foreach (var category in Categories.Items.Cast<TreeViewItem>())
                {
                    category.IsExpanded = expandedCategoryNames.Contains((string)category.Header);
                }
            }
        }

        private string GetSelectedCategoryName()
        {
            List<TreeViewItem> categories = (List<TreeViewItem>)Categories.DataContext;

            // Get the first selected category.
            if (categories.Any(c => c.IsSelected))
            {
                return categories.First(c => c.IsSelected).Header as string;
            }

            return string.Empty;
        }

        private List<string> GetExpandedCategoryNames()
        {
            List<TreeViewItem> categories = (List<TreeViewItem>)Categories.DataContext;

            List<string> expandedCategories = new List<string>();

            if (categories.Any(c => c.IsExpanded))
            {
                expandedCategories = categories.Where(c => c.IsExpanded).Select(c => (string)c.Header).ToList();
            }

            return expandedCategories;
        }

#endregion Category Visibility Properties

        private void UpdateTreeViewItems()
        {
            List<TreeViewItem> samples = WPF.Viewer.Helpers.ToTreeViewItem(SampleManager.Current.FullTree);
            Categories.DataContext = samples;
        }

        private void ResetCategories()
        {
            // Get the selected category name and expanded categories before updating the category tree.
            string selectedCategoryName = GetSelectedCategoryName();
            List<string> expandedCategoryNames = GetExpandedCategoryNames();

            UpdateTreeViewItems();

            // Set the selected category.
            SetSelectedCategory(selectedCategoryName);

            // Set the expanded categories.
            SetExpandedCategories(expandedCategoryNames);
        }

#region Screenshot Tool

        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            SaveScreenshot(SampleContainer);
        }

        private void SetScreenshotButttonVisibility()
        {
            ScreenshotButton.Visibility = ScreenshotManager.ScreenshotSettings.ScreenshotEnabled ? Visibility.Visible : Visibility.Hidden;
        }

        private void SetContainerDimensions()
        {
            if (ScreenshotManager.ScreenshotSettings.ScreenshotEnabled)
            {
                SampleContainer.Width = ScreenshotManager.ScreenshotSettings.Width.HasValue ? ScreenshotManager.ScreenshotSettings.Width.Value : double.NaN;
                SampleContainer.Height = ScreenshotManager.ScreenshotSettings.Height.HasValue ? ScreenshotManager.ScreenshotSettings.Height.Value : double.NaN;
            }
            else
            {
                SampleContainer.Width = double.NaN;
                SampleContainer.Height = double.NaN;
            }
        }

        // Code here is adapted from the following Stack Overflow answers:
        // https://stackoverflow.com/q/24466482
        // https://stackoverflow.com/a/15537372
        private void SaveScreenshot(UIElement source)
        {
            PresentationSource presentationSource = PresentationSource.FromVisual(source);

            double scaleX = presentationSource.CompositionTarget.TransformToDevice.M11;
            double scaleY = presentationSource.CompositionTarget.TransformToDevice.M22;

            int height = (int)(source.DesiredSize.Height * scaleY);
            int width = (int)(source.DesiredSize.Width * scaleX);
            System.Windows.Point screenPoint = source.PointToScreen(new System.Windows.Point(0d, 0d));
            Bitmap screenshot = new Bitmap(width, height);
            Graphics G = Graphics.FromImage(screenshot);
            G.CopyFromScreen((int)screenPoint.X, (int)screenPoint.Y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);

            // If scaling has occurred due to screen scaling we need to resize the image.
            Bitmap resizedScreenshot = new Bitmap(screenshot, new System.Drawing.Size((int)(screenshot.Width / scaleX), (int)(screenshot.Height / scaleY)));

            string filePath = $"{ScreenshotManager.ScreenshotSettings.SourcePath}\\WPF\\WPF.Viewer\\Samples\\" +
                $"{SampleManager.Current.SelectedSample.Category}\\" +
                $"{SampleManager.Current.SelectedSample.FormalName}\\" +
                $"{SampleManager.Current.SelectedSample.FormalName}.jpg";

            // Remove white space.
            filePath = Regex.Replace(filePath, @"\s+", "");

            try
            {
                System.IO.FileStream fs = System.IO.File.Open(filePath, System.IO.FileMode.OpenOrCreate);
                resizedScreenshot.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving screenshot: {ex.Message}");
            }
        }

        #endregion Screenshot Tool
    }
}
