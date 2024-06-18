﻿using ArcGIS.Samples.Managers;
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

        private string[] _previousSearchKeywords = [];

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
            }
            else
            {
                // Remove punctuation from the search text and any trailing white space at the end.
                var searchKeywords = SearchViewModel.GetKeywords(SearchText);

                // Check if the keywords are the same as the previous search.
                if (Enumerable.SequenceEqual(searchKeywords, _previousSearchKeywords))
                {
                    return;
                }
                else
                {
                    _previousSearchKeywords = searchKeywords;
                }

                List<SearchResultViewModel> sampleResults = new List<SearchResultViewModel>();

                foreach (var sample in SampleManager.Current.AllSamples.ToList())
                {
                    int score = 0;

                    var sampleNameKeywords = GetKeywords(sample.SampleName);
                    var categoryKeywords = GetKeywords(sample.Category);
                    var descriptionKeywords = GetKeywords(sample.Description);
                    var tagsKeywords = sample.Tags.ToArray();

                    score += GetMatches(sampleNameKeywords, searchKeywords) * 6;
                    score += GetMatches(categoryKeywords, searchKeywords) * 3;
                    score += GetMatches(descriptionKeywords, searchKeywords) * 2;
                    score += GetMatches(tagsKeywords, searchKeywords);

                    if (score > 0)
                    {
                        sampleResults.Add(new SearchResultViewModel(sample, score));
                    }
                }

                try
                {
                    if (sampleResults.Count != 0)
                    {
                        sampleResults = sampleResults.OrderByDescending(sampleResults => sampleResults.Score).ThenBy(sampleResults => sampleResults.SampleName).ToList();
                        SearchItems = new ObservableCollection<SearchResultViewModel>(sampleResults);
                    }
                    else
                    {
                        SearchItems = new ObservableCollection<SearchResultViewModel>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private static int GetMatches(string[] contentKeywords, string[] searchKeywords)
        {
            int matches = 0;

            foreach (var searchKeyword in searchKeywords)
            {
                foreach (var contentKeyword in contentKeywords)
                {
                    if (contentKeyword == searchKeyword)
                    {
                        matches += 2;
                    }
                    else if (contentKeyword.Contains(searchKeyword))
                    {
                        matches++;
                    }
                }
            }

            return matches;
        }

        private static string[] GetKeywords(string text)
        {
            // Remove punctuation from the search text and any trailing white space at the end.
            Regex regex = new Regex("[^a-zA-Z0-9 -]");
            text = regex.Replace(text, "");

            var cleanedTextWords = text.TrimEnd().ToLower().Split(" ").Distinct().ToList();
            var commonWords = new string[] { "in", "a", "of", "the", "by", "an", "and" };

            foreach (var word in commonWords)
            {
                cleanedTextWords.Remove(word);
            }

            return cleanedTextWords.ToArray();
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

    public partial class SearchResultViewModel : ObservableObject
    {
        public SearchResultViewModel(SampleInfo sampleResult, int score)
        {
            SampleName = sampleResult.SampleName;
            SampleCategory = sampleResult.Category;
            SampleDescription = sampleResult.Description;
            SampleImage = new FileImageSource() { File = sampleResult.SampleImageName };
            Score = score;
            SampleObject = sampleResult;
        }

        [ObservableProperty]
        int _score;

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
