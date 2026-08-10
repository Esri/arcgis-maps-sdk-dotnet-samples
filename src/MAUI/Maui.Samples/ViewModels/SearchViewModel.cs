using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace ArcGIS.ViewModels
{
    public partial class SearchViewModel : ObservableObject
    {
        private IDispatcherTimer _delaySearchTimer;
        private const int _delayedTextChangedTimeout = 500;
        private const int _maxSearchResults = 15;

        private List<SearchResultViewModel> _results;

        public SearchViewModel()
        {
            SearchItems = new ObservableCollection<SearchResultViewModel>();
        }

        [ObservableProperty]
        ObservableCollection<SearchResultViewModel> _searchItems;

        [ObservableProperty]
        string _searchText;

        [ObservableProperty]
        bool _noSearchResults;

        partial void OnSearchTextChanged(string value)
        {
            SearchTextChanged();
        }

        partial void OnSearchItemsChanged(ObservableCollection<SearchResultViewModel> value)
        {
            NoSearchResults = value.Count == 0;
        }

        [RelayCommand]
        void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchItems = new ObservableCollection<SearchResultViewModel>();
                return;
            }

            var sampleMatches = SampleManager.Current.SearchEngine.Search(SearchText);

            _results = sampleMatches.Select(r =>
            {
                var sampleInfo = SampleManager.Current.GetSample(r.SampleFormalName);
                return new SearchResultViewModel(sampleInfo, r.Score);
            }).ToList();

            SearchItems = new ObservableCollection<SearchResultViewModel>(_results.Take(_maxSearchResults));
        }

        [RelayCommand]
        void BatchResults()
        {
            var startIndex = SearchItems.Count;
            var endIndex = Math.Min(startIndex + _maxSearchResults, _results.Count);

            if (endIndex >= _results.Count) return;

            foreach (var result in _results[startIndex..endIndex])
                SearchItems.Add(result);
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
            PerformSearch();

            if (_delaySearchTimer != null)
            {
                _delaySearchTimer.Stop();
            }
        }
    }
}
