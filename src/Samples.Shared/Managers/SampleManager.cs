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

using ArcGIS.Samples.Shared.Attributes;
using ArcGIS.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ArcGIS.Samples.Managers
{
    /// <summary>
    /// Single instance class to manage samples.
    /// </summary>
    public class SampleManager
    {
        // Private constructor
        private SampleManager()
        { }

        // Static initialization of the unique instance
        private static readonly SampleManager SingleInstance = new SampleManager();

        public static SampleManager Current
        {
            get { return SingleInstance; }
        }

        /// <summary>
        /// A list of all samples.
        /// </summary>
        /// <remarks>This is public on purpose. Other solutions that consume
        /// this project reference it directly.</remarks>
        public IList<SampleInfo> AllSamples { get; set; }

        /// <summary>
        /// A collection of all samples organized by category.
        /// </summary>
        public SearchableTreeNode FullTree { get; private set; }

        /// <summary>
        /// The sample that is currently being shown to the user.
        /// </summary>
        public SampleInfo SelectedSample { get; set; }

        private const string _favoritedSampleFileName = "favoritedSamples";

        /// <summary>
        /// Initializes the sample manager by loading all of the samples in the app.
        /// </summary>
        public void Initialize()
        {
            // Get the currently-executing assembly.
            Assembly samplesAssembly = GetType().GetTypeInfo().Assembly;

            // Get the list of all samples in the assembly.
            AllSamples = CreateSampleInfos(samplesAssembly).OrderBy(info => info.Category)
                .ThenBy(info => info.SampleName.ToLowerInvariant())
                .ToList();

            BuildSampleCategories();
        }

        private void BuildSampleCategories()
        {
            // Create a tree from the list of all samples.
            FullTree = BuildFullTree(AllSamples);

#if INCLUDE_SAMPLES_SUBSET
            // Add a category for the samples subset.
            FullTree.Items.Insert(0, GetSearchableTreeNodeFromFile("SubsetSamples.xml", "Subset", false));
            FullTree.Items.Insert(1, GetSearchableTreeNodeFromFile("FeaturedSamples.xml", "Featured"));
#else
            // Add a category for featured samples.
            FullTree.Items.Insert(0, GetSearchableTreeNodeFromFile("FeaturedSamples.xml", "Featured"));
#endif

#if !(WinUI || WINDOWS_UWP)
            // Get favorite samples if they exist. This feature is only available on WPF.
            AddFavoritesCategory();
#endif
        }

        /// <summary>
        /// Get a list of sample names from a resource file.
        /// </summary>
        /// <returns>An searchable tree node containing the samples found in the resource file.</returns>
        private SearchableTreeNode GetSearchableTreeNodeFromFile(string fileName, string searchableTreeNodeTitle, bool orderByName = true)
        {
            // Instantiate a null XElement to be populated by the resource file.
            XElement sampleElement = null;

            // Create a list to hold the names of the samples.
            List<string> samples = new List<string>();

            string resourceStreamName = this.GetType().Assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

            // Load the FeaturedSamples resource file.
            using (Stream stream = this.GetType().Assembly.
                       GetManifestResourceStream(resourceStreamName))
            {
                sampleElement = XElement.Load(stream);
            }

            // If the resource file has been successfully loaded populate the list of samples.
            if (sampleElement != null)
            {
                samples = sampleElement.Descendants("Sample").Select(x => x.Value).ToList();
            }

            IEnumerable<SampleInfo> searchableTreeNodeItems = AllSamples.Where(sample => samples.Contains(sample.FormalName, StringComparer.OrdinalIgnoreCase));

            if (orderByName)
            {
                searchableTreeNodeItems = searchableTreeNodeItems.OrderBy(sample => sample.SampleName);
            }

            return new SearchableTreeNode(searchableTreeNodeTitle, searchableTreeNodeItems);
        }

        /// <summary>
        /// Creates a list of sample metadata objects for each sample in the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to search for samples.</param>
        /// <returns>List of sample metadata objects.</returns>
        private static IList<SampleInfo> CreateSampleInfos(Assembly assembly)
        {
            // Get all the types in the assembly that are decorated with a SampleAttribute.
            IEnumerable<Type> sampleTypes = assembly.GetTypes()
                .Where(type => type.GetTypeInfo().GetCustomAttributes().OfType<SampleAttribute>().Any());

            // Create a list to hold all constructed sample metadata objects.
            List<SampleInfo> samples = new List<SampleInfo>();

            // Create the sample metadata for each sample.
            foreach (Type type in sampleTypes)
            {
                try
                {
                    SampleInfo sampleInfo = new SampleInfo(type);

                    samples.Add(sampleInfo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Could not create sample from " + type + ": " + ex);
                }
            }
            return samples;
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
            return Activator.CreateInstance(sampleModel.SampleType);
        }

        /// <summary>
        /// Common sample search predicate implementation
        /// </summary>
        /// <param name="sample">Sample to evaluate</param>
        /// <param name="searchText">Query</param>
        /// <returns><c>true</c> if the sample matches the query.</returns>
        public bool SampleSearchFunc(SampleInfo sample, string searchText)
        {
            searchText = searchText.ToLower();
            return sample.SampleName.ToLower().Contains(searchText) ||
                   sample.Category.ToLower().Contains(searchText) ||
                   sample.Description.ToLower().Contains(searchText) ||
                   sample.Tags.Any(tag => tag.Contains(searchText));
        }

#if !(WinUI || WINDOWS_UWP)
        public bool IsSampleFavorited(string sampleFormalName)
        {
            return GetFavoriteSampleNames().Contains(sampleFormalName);
        }

        private static List<string> GetFavoriteSampleNames()
        {
            // Get the names of the favorite samples from the saved file if it exists.
            // If the file does not exist, create it.
            if (File.Exists(Path.Combine(GetFavoritesFolder(), _favoritedSampleFileName)))
            {
                return File.ReadAllLines(Path.Combine(GetFavoritesFolder(), _favoritedSampleFileName)).ToList();
            }
            else
            {
                File.Create(Path.Combine(GetFavoritesFolder(), _favoritedSampleFileName));
            }

            return new List<string>();
        }

        public void AddRemoveFavorite(string sampleName)
        {
            // Get the list of favorites from the saved file.
            List<string> favorites = File.ReadAllLines(Path.Combine(GetFavoritesFolder(), _favoritedSampleFileName)).ToList();

            // If the sample currently being added/removed is present or not present remove or add it to the list accordingly.
            if (favorites.Contains(sampleName))
            {
                favorites.Remove(sampleName);
            }
            else
            {
                favorites.Add(sampleName);

#if ENABLE_ANALYTICS
                var eventData = new Dictionary<string, string> {
                    { "Sample", AllSamples.FirstOrDefault(s => s.FormalName.Equals(sampleName)).SampleName }
                };

                _ = Helpers.AnalyticsHelper.TrackEvent("favorite_added", eventData);
#endif
            }

            // Save the new list of favorites.
            File.WriteAllLines(Path.Combine(GetFavoritesFolder(), _favoritedSampleFileName), favorites);

            // Build the categories tree again with the updated favorite category.
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
            IEnumerable<string> favoriteSamples = GetFavoriteSampleNames();

            // Set favorited samples.
            foreach (var sample in AllSamples)
            {
                sample.IsFavorite = favoriteSamples.Contains(sample.FormalName, StringComparer.OrdinalIgnoreCase);
            }

            // Create a new SearchableTreeNode for the updated favorites.
            return new SearchableTreeNode("Favorites", AllSamples.Where(sample => favoriteSamples.Contains(sample.FormalName, StringComparer.OrdinalIgnoreCase)).OrderBy(sample => sample.SampleName));
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
