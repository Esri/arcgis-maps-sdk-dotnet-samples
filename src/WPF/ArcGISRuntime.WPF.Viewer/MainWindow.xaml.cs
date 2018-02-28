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
using Esri.ArcGISRuntime.Security;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    public partial class MainWindow
    {
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
                Categories.DataContext = WPF.Viewer.Helpers.ToTreeViewItem(SampleManager.Current.FullTree);

                // Select a random sample
                Random rnd = new Random();
                SelectSample(SampleManager.Current.AllSamples[rnd.Next(0, SampleManager.Current.AllSamples.Count() - 1)]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Initialization exception occurred.");
            }
        }

        private void categoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var sample = e.AddedItems[0] as SampleInfo;
                SelectSample(sample);
                ((ListView)sender).SelectedItem = null;
            }
        }

        private void categories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sample = (((TreeViewItem)e.NewValue).DataContext as SampleInfo);
            SelectSample(sample);
        }

        private async void SelectSample(SampleInfo selectedSample)
        {
            if (selectedSample == null) return;

            SampleManager.Current.SelectedSample = selectedSample;
            DescriptionContainer.DataContext = selectedSample;

            try
            {
                if (selectedSample.OfflineDataItems != null)
                {
                    // Show waiting page
                    SampleContainer.Content = new WPF.Viewer.WaitPage();

                    // Wait for offline data to complete
                    await DataManager.EnsureSampleDataPresent(selectedSample);
                }

                // Show the sample
                SampleContainer.Content = SampleManager.Current.SampleToControl(selectedSample);

                // Call a function to clear any existing credentials from AuthenticationManager
                ClearCredentials();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception exception)
            {
                // failed to create new instance of the sample
                SampleContainer.Content = new WPF.Viewer.ErrorPage(exception);
            }
            CategoriesRegion.Visibility = Visibility.Collapsed;
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
        }

        private bool _openCategoryLeafs = true;

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            if (_openCategoryLeafs)
            {
                _openCategoryLeafs = false;
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
        }

        private void LiveSample_Click(object sender, RoutedEventArgs e)
        {
            SampleContainer.Visibility = Visibility.Visible;
            DescriptionContainer.Visibility = Visibility.Collapsed;
        }

        private void Description_Click(object sender, RoutedEventArgs e)
        {
            SampleContainer.Visibility = Visibility.Collapsed;
            DescriptionContainer.Visibility = Visibility.Visible;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseNavigation.Visibility = Visibility.Collapsed;
            OpenNavigation.Visibility = Visibility.Visible;
            Root.ColumnDefinitions[0].MaxWidth = 0;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenNavigation.Visibility = Visibility.Collapsed;
            CloseNavigation.Visibility = Visibility.Visible;
            Root.ColumnDefinitions[0].MaxWidth = 535;
        }
    }
}