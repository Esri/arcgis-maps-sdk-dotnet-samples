using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Maui.Core.Extensions;
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
        List<SampleInfo> _allSamples;

        public SearchViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            SearchItems = new ObservableCollection<SearchResultViewModel>();
            SearchField = "Sample Name";
            SearchOrder = "Ascending";

            _allSamples = SampleManager.Current.AllSamples
                .OrderBy(sample => sample.SampleName)
                .ToList();
        }

        [ObservableProperty]
        ObservableCollection<SearchResultViewModel> _searchItems;

        [ObservableProperty]
        string _searchText;

        [ObservableProperty]
        string _searchField;

        [ObservableProperty]
        bool _hasSearchResults;

        partial void OnSearchItemsChanged(ObservableCollection<SearchResultViewModel> value)
        {
            HasSearchResults = value != null && value.Count > 0;
        }

        partial void OnSearchFieldChanged(string value)
        {
            SortSearchResults();
        }

        [ObservableProperty]
        string _searchOrder;

        partial void OnSearchOrderChanged(string value)
        {
            SortSearchResults();
        }

        [RelayCommand]
        void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchItems = new ObservableCollection<SearchResultViewModel>();
            }
            else
            {
                // Remove punctuation from the search text and any trailing white space at the end. 
                Regex regex = new Regex("[^a-zA-Z0-9 -]");
                var searchText = regex.Replace(SearchText, "");

                searchText = searchText.TrimEnd();

                _allSamples = SampleManager.Current.AllSamples.ToList();
                searchText = searchText.ToLower();

                var sampleResults = _allSamples.Where(sample => sample.SampleName.ToLower().Contains(searchText) ||
                   sample.Category.ToLower().Contains(searchText) ||
                   sample.Description.ToLower().Contains(searchText) ||
                   sample.Tags.Any(tag => tag.Contains(searchText))).ToList<SampleInfo>();

                try
                {
                    SearchItems = GetSortedSamples(sampleResults)
                        .Select(sample => new SearchResultViewModel(sample))
                        .ToObservableCollection();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        [RelayCommand]
        void SortSearchResults()
        {
            if (SearchItems != null && SearchItems.Count > 0)
            {
                var searchItems = SearchItems;
                if (SearchField == "Sample Name")
                {
                    if (SearchOrder == "Ascending")
                    {
                        searchItems = SearchItems.OrderBy(item => item.SampleName).ToObservableCollection();
                    }
                    else
                    {
                        searchItems = SearchItems.OrderByDescending(item => item.SampleName).ToObservableCollection();
                    }
                }
                else
                {
                    if (SearchOrder == "Ascending")
                    {
                        searchItems = SearchItems.OrderBy(item => item.SampleCategory).ToObservableCollection();
                    }
                    else
                    {
                        searchItems = SearchItems.OrderByDescending(item => item.SampleCategory).ToObservableCollection();
                    }
                }

                SearchItems = new ObservableCollection<SearchResultViewModel>(searchItems);
            }
        }

        private List<SampleInfo> GetSortedSamples(List<SampleInfo> samples)
        {
            if (SearchField == "Sample Name")
            {
                if (SearchOrder == "Ascending")
                {
                    samples = samples.OrderBy(sample => sample.SampleName).ToList();
                }
                else
                {
                    samples = samples.OrderByDescending(sample => sample.SampleName).ToList();
                }
            }
            else
            {
                if (SearchOrder == "Ascending")
                {
                    samples = samples.OrderBy(sample => sample.Category).ToList();
                }
                else
                {
                    samples = samples.OrderByDescending(sample => sample.Category).ToList();
                }
            }

            return samples;
        }
    }

    public partial class SearchResultViewModel : ObservableObject
    {
        public SearchResultViewModel(SampleInfo sampleResult)
        {
            SampleName = sampleResult.SampleName;
            SampleCategory = sampleResult.Category;
            SampleDescription = sampleResult.Description;
            SampleImage = new FileImageSource() { File = sampleResult.SampleImageName };
            SampleObject = sampleResult;
        }

        [ObservableProperty]
        string _sampleName;

        [ObservableProperty]
        string _sampleCategory;

        [ObservableProperty]
        string _sampleDescription;

        [ObservableProperty]
        ImageSource _sampleImage;

        [ObservableProperty]
        SampleInfo _sampleObject;
    }
}
