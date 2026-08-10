// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.


// Uncomment the following line to include the samples subset in the app.
//#define INCLUDE_SAMPLES_SUBSET

using ArcGIS.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArcGIS.Samples.Managers
{
    /// <summary>
    /// A class that manages the search functionality for the samples.
    /// </summary>
    public partial class SearchEngine
    {
        private string[] _previousSearchKeywords;
        private List<SampleSearchResult> _cachedResults = new();
        private static readonly HashSet<string> _commonWords = ["a", "an", "and", "at", "but", "by", "for", "from", "in", "is", "it", "of", "on", "or", "the", "to", "with"];
        
        public IList<SampleSearchableProfile> ItemKeywords { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchManager"/> class with the provided list of samples.
        /// </summary>
        /// <param name="samples">The list of samples to be managed by the search engine.</param>
        public SearchEngine(IList<SampleInfo> samples)
        {
            this.ItemKeywords = samples.Select(sample =>
            {
                var item = new SampleSearchableProfile() {  SampleFormalName = sample.FormalName };
                item.KeywordScoreCollection.Add(new SampleKeywordScore()
                {
                    Keywords = GetKeywords(sample.SampleName).Append(sample.FormalName.ToLower()).ToArray(),
                    ScoreFactor = 6
                });
                item.KeywordScoreCollection.Add(new SampleKeywordScore()
                {
                    Keywords = GetKeywords(sample.Category),
                    ScoreFactor = 3
                });
                item.KeywordScoreCollection.Add(new SampleKeywordScore()
                {
                    Keywords = GetKeywords(sample.Description),
                    ScoreFactor = 2
                });
                item.KeywordScoreCollection.Add(new SampleKeywordScore()
                {
                    Keywords = sample.Tags.ToArray(),
                    ScoreFactor = 1
                });
                return item;
            }).ToList();
        }

        /// <summary>
        /// Searches for samples based on the provided search text and returns a list of search results.
        /// </summary>
        /// <param name="searchText">The text to search for within the samples.</param>
        /// <returns>A list of search results matching the search text sorted by score and then by name.</returns>
        public List<SampleSearchResult> Search(string searchText)
        {
            var searchKeywords = GetKeywords(searchText);
            // If the keywords are the same as the previous search we exit
            if (_previousSearchKeywords != null && _previousSearchKeywords.SequenceEqual(searchKeywords))
                return _cachedResults;

            _previousSearchKeywords = searchKeywords;

            List<SampleSearchResult> sampleResults = new List<SampleSearchResult>();

            foreach (var sample in ItemKeywords)
            {
                int score = 0;

                foreach (var kws in sample.KeywordScoreCollection.OrderBy(s => s.ScoreFactor))
                    score += GetMatches(kws.Keywords, searchKeywords) * kws.ScoreFactor;

                if (score > 0)
                    sampleResults.Add(new SampleSearchResult() { SampleFormalName = sample.SampleFormalName, Score = score });
            }

            _cachedResults = sampleResults.OrderByDescending(sampleResults => sampleResults.Score).ThenBy(sampleResults => sampleResults.SampleFormalName).ToList();
            return _cachedResults;
        }

        /// <summary>
        /// Searches <paramref name="fullTree"/> for samples matching <paramref name="searchText"/> and
        /// returns every match flattened into a single category, sorted by score (desc) then title.
        /// </summary>
        /// <param name="searchText">The text to search for within the samples.</param>
        /// <param name="fullTree">The full tree of categories and samples to search and filter.</param>
        /// <returns>
        /// <paramref name="fullTree"/> filtered but unflattened when <paramref name="searchText"/> is empty;
        /// a single-category tree containing every match when there are results; otherwise <c>null</c>.
        /// </returns>
        public SearchableTreeNode Search(string searchText, SearchableTreeNode fullTree)
        {
            bool isSearching = !string.IsNullOrWhiteSpace(searchText);

            var scoresByFormalName = Search(searchText).ToDictionary(r => r.SampleFormalName, r => r.Score);

            var categoriesList = fullTree.Search((s) => scoresByFormalName.ContainsKey(s.FormalName) || !isSearching);

            if (!isSearching || categoriesList == null) return categoriesList;

            // Flatten every matching sample across all categories into a single list, then sort by
            // score (desc) then title, so callers can show one flat, relevance-ordered result set.
            var flatMatches = categoriesList.Items.OfType<SearchableTreeNode>()
                .SelectMany(category => category.Items.OfType<SampleInfo>())
                .DistinctBy(s => s.FormalName)
                .OrderByDescending(s => scoresByFormalName.TryGetValue(s.FormalName, out var score) ? score : 0)
                .ThenBy(s => s.SampleName, StringComparer.OrdinalIgnoreCase);

            return new SearchableTreeNode("Search Results", new[] { new SearchableTreeNode("Search Results", flatMatches) });
        }

        [GeneratedRegex("[^a-zA-Z0-9 -]")]
        private static partial Regex NonWordCharRegex();

        private string[] GetKeywords(string text)
        {
            // Remove punctuation from the search text and any trailing white space at the end.
            text = NonWordCharRegex().Replace(text, "").ToLower();

            // Split the text into words, remove duplicates, and filter out common words in one go.
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       .Distinct()
                       .Where(static word => !_commonWords.Contains(word))
                       .ToArray();
        }

        private int GetMatches(string[] contentKeywords, string[] searchKeywords)
        {
            int matches = 0;

            foreach (var searchKeyword in searchKeywords)
            {
                foreach (var contentKeyword in contentKeywords)
                {
                    if (contentKeyword == searchKeyword)
                        matches += 2;
                    else if (contentKeyword.Contains(searchKeyword))
                        matches++;
                }
            }

            return matches;
        }
    }
}
