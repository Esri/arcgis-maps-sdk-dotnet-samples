﻿using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ArcGIS.ViewModels
{
    public partial class SearchViewModel : ObservableObject
    {
        private IDispatcherTimer _delaySearchTimer;
        private const int _delayedTextChangedTimeout = 500;

        public SearchViewModel()
        {
            _searchItems = new ObservableCollection<SearchResultViewModel>();
        }

        [ObservableProperty]
        ObservableCollection<SearchResultViewModel> _searchItems;

        [ObservableProperty]
        string _searchText;

        [RelayCommand]
        void PerformSearch(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                SearchItems = new ObservableCollection<SearchResultViewModel>();
            }
            else
            {
                // Remove punctuation from the search text and any trailing white space at the end. 
                Regex regex = new Regex("[^a-zA-Z0-9 -]");
                searchTerm = regex.Replace(searchTerm, "");
                
                searchTerm = searchTerm.TrimEnd();

                var allSamples = SampleManager.Current.AllSamples.ToList();
                string searchText = searchTerm.ToLower();

                var sampleResults = allSamples.Where(sample => sample.SampleName.ToLower().Contains(searchText) ||
                   sample.Category.ToLower().Contains(searchText) ||
                   sample.Description.ToLower().Contains(searchText) ||
                   sample.Tags.Any(tag => tag.Contains(searchText))).ToList<SampleInfo>();

                try
                {
                    var searchResults = new ObservableCollection<SearchResultViewModel>();

                    foreach (var sampleResult in sampleResults)
                    {
                        searchResults.Add(new SearchResultViewModel(sampleResult));
                    }

                    SearchItems = searchResults;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(SearchText))
            {
                SearchTextChanged();
            }
        }

        private void SearchTextChanged()
        {
            // Don't update results immediately; makes search-as-you-type more comfortable
            if (_delaySearchTimer != null)
            {
                _delaySearchTimer.Stop();
            }

            if (_delaySearchTimer == null)
            {
                _delaySearchTimer = Application.Current.Dispatcher.CreateTimer();
                _delaySearchTimer.Tick += DelaySearchTimer_Tick;
                _delaySearchTimer.Interval = TimeSpan.FromMilliseconds(_delayedTextChangedTimeout);
            }

            _delaySearchTimer.Start();
        }

        private void DelaySearchTimer_Tick(object sender, EventArgs e)
        {
            PerformSearch(SearchText);

            if (_delaySearchTimer != null)
            {
                _delaySearchTimer.Stop();
            }
        }
    }

    public partial class SearchResultViewModel : ObservableObject
    {
        public SearchResultViewModel(SampleInfo sampleResult)
        {
            SampleName = sampleResult.SampleName;
            SampleImage = new FileImageSource() { File = sampleResult.SampleImageName };
            SampleObject = sampleResult;
        }

        [ObservableProperty]
        string _sampleName;

        [ObservableProperty]
        ImageSource _sampleImage;

        [ObservableProperty]
        SampleInfo _sampleObject;
    }
}
