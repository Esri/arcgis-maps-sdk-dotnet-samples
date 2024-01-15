using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ArcGIS.ViewModels
{
    public partial class SearchViewModel : ObservableObject
    {
        public SearchViewModel()
        {
            SearchItems = new ObservableCollection<SearchResultViewModel>();
        }

        [ObservableProperty]
        ObservableCollection<SearchResultViewModel> _searchItems;

        [ObservableProperty]
        string _searchText;

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
                var searchKeywords = GetKeywords(SearchText);

                List<SearchResultViewModel> sampleResults = new List<SearchResultViewModel>();

                foreach (var sample in SampleManager.Current.AllSamples.ToList())
                {
                    int score = 0;
                    var matchingKeyWords = new List<string>();

                    var sampleNameKeywords = GetKeywords(sample.SampleName);
                    var categoryKeywords = GetKeywords(sample.Category);
                    var descriptionKeywords = GetKeywords(sample.Description);
                    var tagsKeywords = sample.Tags.ToArray();

                    score += sampleNameKeywords.Intersect(searchKeywords).Count() * 6;
                    matchingKeyWords.AddRange(sampleNameKeywords.Intersect(searchKeywords).ToArray());

                    score += categoryKeywords.Intersect(searchKeywords).Count() * 3;
                    matchingKeyWords.AddRange(categoryKeywords.Intersect(searchKeywords).ToArray());

                    score += descriptionKeywords.Intersect(searchKeywords).Count() * 2;
                    matchingKeyWords.AddRange(descriptionKeywords.Intersect(searchKeywords).ToArray());

                    score += tagsKeywords.Intersect(searchKeywords).Count();
                    matchingKeyWords.AddRange(tagsKeywords.Intersect(searchKeywords).ToArray());

                    matchingKeyWords = matchingKeyWords.Distinct().Order().ToList();

                    if (score > 0)
                    {
                        sampleResults.Add(new SearchResultViewModel(sample, score, matchingKeyWords));
                    }
                }

                try
                {
                    if (sampleResults.Any())
                    {
                        sampleResults = sampleResults.OrderByDescending(sampleResults => sampleResults.Score).ToList();
                        SearchItems = new ObservableCollection<SearchResultViewModel>(sampleResults);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private string[] GetKeywords(string text)
        {
            // Remove punctuation from the search text and any trailing white space at the end.
            Regex regex = new Regex("[^a-zA-Z0-9 -]");
            text = regex.Replace(text, "");

            var cleanedTextWords = text.TrimEnd().ToLower().Split(" ").Distinct().ToList();
            var commonWords = new string[] { "in", "a", "of", "the", "by" };

            foreach (var word in commonWords)
            {
                if (cleanedTextWords.Contains(word))
                {
                    cleanedTextWords.Remove(word);
                }
            }

            return cleanedTextWords.ToArray();
        }
    }

    public partial class SearchResultViewModel : ObservableObject
    {
        public SearchResultViewModel(SampleInfo sampleResult, int score, List<string> matchingKeywords)
        {
            SampleName = sampleResult.SampleName;
            SampleCategory = sampleResult.Category;
            SampleDescription = sampleResult.Description;
            SampleImage = new FileImageSource() { File = sampleResult.SampleImageName };
            Score = score;
            SampleObject = sampleResult;

            if (matchingKeywords.Count > 1)
            {
                Keywords = $"[{string.Join(", ", matchingKeywords.ToArray())}]";
            }
            else
            {
                Keywords = $"[{matchingKeywords[0]}]";
            }
        }

        [ObservableProperty]
        string _keywords;

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
