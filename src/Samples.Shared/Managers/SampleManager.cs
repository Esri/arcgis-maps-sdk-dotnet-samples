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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ArcGIS.Samples.Managers
{
    /// <summary>
    /// Single instance class to manage samples.
    /// </summary>
    public partial class SampleManager
    {
        private const string _favoritedSampleFileName = "favoritedSamples";

        // Static initialization of the unique instance
        private static readonly SampleManager SingleInstance = new SampleManager();

        private readonly Dictionary<string, SampleInfo> _sampleCache = new Dictionary<string, SampleInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _catalogSampleNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _catalogCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private ISampleCatalogProvider _catalogProvider;
        private IList<SampleInfo> _allSamples;
        private bool _initialized;

        // Private constructor
        private SampleManager()
        { }

        public static SampleManager Current
        {
            get { return SingleInstance; }
        }

        /// <summary>
        /// The catalog provider used to discover sample metadata and create sample controls.
        /// </summary>
        public ISampleCatalogProvider CatalogProvider
        {
            get
            {
                if (_catalogProvider == null)
                {
                    TryCreateGeneratedCatalogProvider(ref _catalogProvider);
                    _catalogProvider ??= new ReflectionSampleCatalogProvider(GetType().GetTypeInfo().Assembly);
                }

                return _catalogProvider;
            }
        }

        /// <summary>
        /// A list of all samples.
        /// </summary>
        /// <remarks>This is public on purpose. Other solutions that consume
        /// this project reference it directly.</remarks>
        public IList<SampleInfo> AllSamples
        {
            get
            {
                EnsureInitialized();
                return _allSamples ??= CreateAllSamples();
            }
            set
            {
                _allSamples = value;
                _sampleCache.Clear();

                if (_allSamples != null)
                {
                    foreach (SampleInfo sample in _allSamples)
                    {
                        _sampleCache[sample.FormalName] = sample;
                    }
                }
            }
        }

        /// <summary>
        /// A collection of all samples organized by category.
        /// </summary>
        public SearchableTreeNode FullTree { get; private set; }

        /// <summary>
        /// The sample that is currently being shown to the user.
        /// </summary>
        public SampleInfo SelectedSample { get; set; }

        private SearchEngine _searchEngine;
        /// <summary>
        /// A search engine for searching samples.
        /// </summary>
        public SearchEngine SearchEngine => _searchEngine ??= new SearchEngine(AllSamples);

        /// <summary>
        /// Initializes the sample manager by preparing the generated sample catalog when available.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            ISampleCatalogProvider catalogProvider = CatalogProvider;
            _initialized = true;

            foreach (string sampleName in catalogProvider.GetSampleNames())
            {
                _catalogSampleNames[sampleName] = sampleName;
            }

            foreach (string category in catalogProvider.GetCategories())
            {
                _catalogCategories[category] = category;
            }

            BuildSampleCategories();
        }

        /// <summary>
        /// Gets the metadata for a sample by formal name, creating it only when first requested.
        /// </summary>
        public SampleInfo GetSample(string formalName)
        {
            EnsureInitialized();

            if (string.IsNullOrEmpty(formalName))
            {
                return null;
            }

            if (_sampleCache.TryGetValue(formalName, out SampleInfo cachedSample))
            {
                return cachedSample;
            }

            if (!TryGetCatalogSampleName(formalName, out string catalogFormalName))
            {
                return null;
            }

            SampleInfo sampleInfo = CatalogProvider.CreateSampleInfo(catalogFormalName);

#if !(WinUI)
            sampleInfo.IsFavorite = IsSampleFavorited(sampleInfo.FormalName);
#endif

            _sampleCache[sampleInfo.FormalName] = sampleInfo;
            return sampleInfo;
        }

        /// <summary>
        /// Gets all samples for the requested category, materializing only that category.
        /// </summary>
        public IList<SampleInfo> GetSamplesForCategory(string category)
        {
            EnsureInitialized();

            if (TryGetCatalogCategory(category, out string catalogCategory))
            {
                return CatalogProvider.GetSampleNamesForCategory(catalogCategory)
                    .Select(GetSample)
                    .Where(sample => sample != null)
                    .OrderBy(sample => sample.SampleName.ToLowerInvariant())
                    .ToList();
            }

            SearchableTreeNode categoryNode = FullTree?.Items.OfType<SearchableTreeNode>()
                .FirstOrDefault(node => node.Name.Equals(category, StringComparison.OrdinalIgnoreCase));

            return categoryNode?.Items.OfType<SampleInfo>().ToList() ?? new List<SampleInfo>();
        }

        private bool TryGetCatalogSampleName(string formalName, out string catalogFormalName)
        {
            catalogFormalName = null;
            return !string.IsNullOrEmpty(formalName) && _catalogSampleNames.TryGetValue(formalName, out catalogFormalName);
        }

        private bool TryGetCatalogCategory(string category, out string catalogCategory)
        {
            catalogCategory = null;
            return !string.IsNullOrEmpty(category) && _catalogCategories.TryGetValue(category, out catalogCategory);
        }

        private void EnsureInitialized()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }

        static partial void TryCreateGeneratedCatalogProvider(ref ISampleCatalogProvider provider);

        private IList<SampleInfo> CreateAllSamples()
        {
            return CatalogProvider.GetSampleNames()
                .Select(GetSample)
                .Where(sample => sample != null)
                .OrderBy(info => info.Category)
                .ThenBy(info => info.SampleName.ToLowerInvariant())
                .ToList();
        }

        private void BuildSampleCategories()
        {
            FullTree = BuildFullTreeFromCatalog();

#if INCLUDE_SAMPLES_SUBSET
            // Add a category for the samples subset.
            FullTree.Items.Insert(0, GetSearchableTreeNodeFromFile("SubsetSamples.xml", "Subset", false));
            FullTree.Items.Insert(1, GetSearchableTreeNodeFromFile("FeaturedSamples.xml", "Featured"));
#else
            // Add a category for featured samples.
            FullTree.Items.Insert(0, GetSearchableTreeNodeFromFile("FeaturedSamples.xml", "Featured"));
#endif

#if !(WinUI)
            // Get favorite samples if they exist. This feature is only available on WPF.
            AddFavoritesCategory();
#endif
        }

        private SearchableTreeNode BuildFullTreeFromCatalog()
        {
            return new SearchableTreeNode(
                "All Samples",
                () => CatalogProvider.GetCategories()
                    .OrderBy(category => category)
                    .Select(category => new SearchableTreeNode(category, () => GetSamplesForCategory(category).Cast<object>()))
                    .Cast<object>());
        }

        /// <summary>
        /// Get a list of sample names from a resource file.
        /// </summary>
        /// <returns>An searchable tree node containing the samples found in the resource file.</returns>
        private SearchableTreeNode GetSearchableTreeNodeFromFile(string fileName, string searchableTreeNodeTitle, bool orderByName = true)
        {
            XElement sampleElement = null;
            List<string> samples = new List<string>();

            string resourceStreamName = GetType().Assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

            using (Stream stream = GetType().Assembly.GetManifestResourceStream(resourceStreamName))
            {
                sampleElement = XElement.Load(stream);
            }

            if (sampleElement != null)
            {
                samples = sampleElement.Descendants("Sample").Select(x => x.Value).ToList();
            }

            return new SearchableTreeNode(searchableTreeNodeTitle, () =>
            {
                IEnumerable<SampleInfo> searchableTreeNodeItems = samples
                    .Select(GetSample)
                    .Where(sample => sample != null);

                if (orderByName)
                {
                    searchableTreeNodeItems = searchableTreeNodeItems.OrderBy(sample => sample.SampleName);
                }

                return searchableTreeNodeItems.Cast<object>();
            });
        }

        /// <summary>
        /// Creates a <c>SearchableTreeNode</c> representing the entire
        /// collection of samples, organized by category.
        /// </summary>
        /// <remarks>This is public on purpose. Other solutions that
        /// consume this project reference it directly.</remarks>
        /// <param name="allSamples">A list of all samples.</param>
        /// <returns>A <c>SearchableTreeNode</c> with all samples organized by category.</returns>
        public static SearchableTreeNode BuildFullTree(IEnumerable<SampleInfo> allSamples)
        {
            // This code only supports one level of nesting.
            return new SearchableTreeNode(
                "All Samples",
                allSamples.ToLookup(s => s.Category) // put samples into lookup by category
                .OrderBy(s => s.Key)
                .Select(BuildTreeForCategory) // create a tree for each category
                .ToList());
        }

        /// <summary>
        /// Creates a <c>SearchableTreeNode</c> representing a category of samples.
        /// </summary>
        /// <param name="byCategory">A grouping that associates one category title with many samples.</param>
        /// <returns>A <c>SearchableTreeNode</c> representing a category of samples.</returns>
        private static SearchableTreeNode BuildTreeForCategory(IGrouping<string, SampleInfo> byCategory)
        {
            // This code only supports one level of nesting.
            return new SearchableTreeNode(
                name: byCategory.Key,
                items: byCategory.OrderBy(si => si.SampleName.ToLower()).ToList()
            );
        }

        /// <summary>
        /// Constructs the sample control from the provided <paramref name="sampleModel"/>.
        /// </summary>
        /// <param name="sampleModel">Sample for which to create the sample control.</param>
        /// <returns>Sample as a control.</returns>
        public object SampleToControl(SampleInfo sampleModel)
        {
            EnsureInitialized();

            if (TryGetCatalogSampleName(sampleModel.FormalName, out string catalogFormalName))
            {
                return CatalogProvider.CreateSample(catalogFormalName);
            }

            if (sampleModel.SampleType != null)
            {
                return Activator.CreateInstance(sampleModel.SampleType);
            }

            throw new InvalidOperationException($"Sample '{sampleModel.FormalName}' cannot be created because no factory or sample type is available.");
        }

#if !(WinUI)
        public bool IsSampleFavorited(string sampleFormalName)
        {
            return GetFavoriteSampleNames().Contains(sampleFormalName, StringComparer.OrdinalIgnoreCase);
        }

        private static List<string> GetFavoriteSampleNames()
        {
            string favoritesFile = Path.Combine(GetFavoritesFolder(), _favoritedSampleFileName);

            if (File.Exists(favoritesFile))
            {
                return File.ReadAllLines(favoritesFile).ToList();
            }

            using FileStream _ = File.Create(favoritesFile);

            return new List<string>();
        }

        public void AddRemoveFavorite(string sampleName)
        {
            string favoritesFile = Path.Combine(GetFavoritesFolder(), _favoritedSampleFileName);
            List<string> favorites = File.ReadAllLines(favoritesFile).ToList();
            string existingFavorite = favorites.FirstOrDefault(name => name.Equals(sampleName, StringComparison.OrdinalIgnoreCase));

            if (existingFavorite != null)
            {
                favorites.Remove(existingFavorite);
            }
            else
            {
                favorites.Add(sampleName);

#if ENABLE_ANALYTICS
                SampleInfo favoritedSample = GetSample(sampleName);
                var eventData = new Dictionary<string, string> {
                    { "Sample", favoritedSample?.SampleName ?? sampleName }
                };

                _ = Helpers.AnalyticsHelper.TrackEvent("favorite_added", eventData);
#endif
            }

            File.WriteAllLines(favoritesFile, favorites);

            HashSet<string> favoriteNames = new HashSet<string>(favorites, StringComparer.OrdinalIgnoreCase);
            foreach (SampleInfo sample in _sampleCache.Values)
            {
                sample.IsFavorite = favoriteNames.Contains(sample.FormalName);
            }

            if (_allSamples != null)
            {
                foreach (SampleInfo sample in _allSamples)
                {
                    sample.IsFavorite = favoriteNames.Contains(sample.FormalName);
                }
            }

            BuildSampleCategories();
        }

        private void AddFavoritesCategory()
        {
            SearchableTreeNode favorites = GetFavoritesCategory();

            // Get the existing favorites to check if they are already present in the category tree.
            SearchableTreeNode existingFavorites = FullTree.Items.FirstOrDefault(i => i is SearchableTreeNode t && t.Name == "Favorites") as SearchableTreeNode;

            if (existingFavorites == null)
            {
                FullTree.Items.Insert(1, favorites);
            }
            else
            {
                FullTree.Items[1] = favorites;
            }
        }

        public SearchableTreeNode GetFavoritesCategory()
        {
            List<string> favoriteSampleNames = GetFavoriteSampleNames();
            HashSet<string> favoriteNames = new HashSet<string>(favoriteSampleNames, StringComparer.OrdinalIgnoreCase);

            foreach (SampleInfo sample in _sampleCache.Values)
            {
                sample.IsFavorite = favoriteNames.Contains(sample.FormalName);
            }

            if (_allSamples != null)
            {
                foreach (SampleInfo sample in _allSamples)
                {
                    sample.IsFavorite = favoriteNames.Contains(sample.FormalName);
                }
            }

            return new SearchableTreeNode("Favorites", () => favoriteSampleNames
                .Select(GetSample)
                .Where(sample => sample != null)
                .OrderBy(sample => sample.SampleName)
                .Cast<object>());
        }

        internal static string GetFavoritesFolder()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string sampleDataFolder = Path.Combine(appDataFolder, "ESRI", "dotnetSamples", "Favorites");

            if (!Directory.Exists(sampleDataFolder)) { Directory.CreateDirectory(sampleDataFolder); }

            return sampleDataFolder;
        }

#endif
    }
}
