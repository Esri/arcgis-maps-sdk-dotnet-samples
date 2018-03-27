// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Security;
using Xamarin.Forms;

namespace ArcGISRuntime
{
    public partial class CategoryListPage
    {
        public CategoryListPage()
        {
            Initialize();
            InitializeComponent();
        }

        private void Initialize()
        {
            // Initialize the sample manager.
            SampleManager.Current.Initialize();

            ViewModel = new SamplesSearchViewModel();

            // Update the binding.
            BindingContext = ViewModel;
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // Get the selected category.
            var category = e.Item as SearchableTreeNode;
            var sample = e.Item as SampleInfo;
            if (category != null)
            {
                // Navigate to the listing page for the category.
                await Navigation.PushAsync(new SampleListPage(category.Name));
            }
            else if (sample != null)
            {
                // Navigate to the sample page
                ShowSample(sample);
            }
        }

        private async void ShowSample(SampleInfo item)
        {
            // Clear Credentials
            foreach (Credential cred in AuthenticationManager.Current.Credentials)
            {
                AuthenticationManager.Current.RemoveCredential(cred);
            }

            try
            {
                // Load offline data before showing the sample.
                if (item.OfflineDataItems != null)
                {
                    // Show the wait page.
                    await Navigation.PushModalAsync(new WaitPage { Title = item.SampleName }, false);

                    // Wait for the sample data download.
                    await DataManager.EnsureSampleDataPresent(item);

                    // Remove the waiting page.
                    await Navigation.PopModalAsync(false);
                }

                // Get the sample control from the selected sample.
                var sampleControl = (ContentPage)SampleManager.Current.SampleToControl(item);

                // Create the sample display page to show the sample and the metadata.
                SamplePage page = new SamplePage(sampleControl, item);

                // Show the sample.
                await Navigation.PushAsync(page, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception occurred on OnItemTapped. Exception = " + ex);
            }
        }
        public SamplesSearchViewModel ViewModel { get; set; }
    }

    public class SamplesSearchViewModel : INotifyPropertyChanged
    {
        public List<SearchableTreeNode> SampleCategories { get; }
        public List<SampleInfo> SearchResults => _allSamples.Where(SearchFunction).ToList();
        private readonly List<SampleInfo> _allSamples;
        private string _query = "";

        public SamplesSearchViewModel()
        {
            SampleCategories = SampleManager.Current.FullTree.Items.OfType<SearchableTreeNode>().ToList();
            _allSamples = SampleManager.Current.AllSamples.ToList();
        }

        public string SearchQuery
        {
            get { return _query; }
            set
            {
                if (value != _query)
                {
                    _query = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SearchResults));
                    OnPropertyChanged(nameof(IsSearchOpen));
                    OnPropertyChanged(nameof(IsCategoriesOpen));
                }
            }
        }

        public bool IsSearchOpen => !string.IsNullOrWhiteSpace(_query);
        public bool IsCategoriesOpen => string.IsNullOrWhiteSpace(_query);

        private bool SearchFunction(SampleInfo sample)
        {
            return SampleManager.Current.SampleSearchFunc(sample, SearchQuery);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}