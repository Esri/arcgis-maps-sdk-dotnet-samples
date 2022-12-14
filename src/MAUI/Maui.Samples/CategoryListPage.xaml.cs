// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Helpers;
using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Managers;
using ArcGIS.Samples.Shared.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGIS
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
            this.Appearing += FirstLoaded;

            // Initialize the sample manager.
            SampleManager.Current.Initialize();

            ViewModel = new SamplesSearchViewModel();

            // Update the binding.
            BindingContext = ViewModel;

#if MACCATALYST || IOS
            // Workaround visibility binding bug on Mac Catalyst. Binding for visibility that starts as false does not behave correctly on iOS and Mac. 
            ViewModel.PropertyChanged += MacVisibilityHandler;
#endif
        }

        private void MacVisibilityHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSearchOpen")
            {
                SampleSearchResultList.IsVisible = ((SamplesSearchViewModel)sender).IsSearchOpen;
            }
        }

        private void FirstLoaded(object sender, EventArgs e)
        {
            this.Appearing -= FirstLoaded;

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
                await Navigation.PushAsync(new ApiKeyPrompt(), true);
            }
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
                _ = SampleLoader.LoadSample(sample, this);
            }
        }

        public SamplesSearchViewModel ViewModel { get; set; }

        private async void SettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage(), true);
        }
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
                    _query = value ?? ""; // Need to guard against null on X.F ios - happens when canceling search.
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