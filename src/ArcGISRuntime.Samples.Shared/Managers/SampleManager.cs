// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Attributes;
using ArcGISRuntime.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ArcGISRuntime.Samples.Managers
{
    /// <summary>
    /// Single instance class to manage samples.
    /// </summary>
    public class SampleManager
    {
        // Private constructor
        private SampleManager() { }

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

            // Create a tree from the list of all samples.
            FullTree = BuildFullTree(AllSamples);

            // Add a special category for featured samples.
            SearchableTreeNode featured = new SearchableTreeNode("Featured", AllSamples.Where(sample => sample.Tags.Contains("Featured")));
            FullTree.Items.Insert(0, featured);
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
                    samples.Add(new SampleInfo(type));
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
    }
}