// Copyright 2016 Esri.
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

#if NETFX_CORE
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel;
#endif
#if __WPF__
using System.Windows.Controls;
#endif

namespace ArcGISRuntime.Samples.Managers
{
    /// <summary>
    /// Single instance class to manage samples.
    /// </summary>
    public class SampleManager
    {
        private Assembly _samplesAssembly;

        // Private constructor
        private SampleManager() { }

        // Static initialization of the unique instance
        private static readonly SampleManager SingleInstance = new SampleManager();

        public static SampleManager Current
        {
            get { return SingleInstance; }
        }

        public IList<SampleInfo> AllSamples { get; set; }
        public SearchableTreeNode FullTree { get; set; }
        public SampleInfo SelectedSample { get; set; }

        public void Initialize()
        {
            _samplesAssembly = this.GetType().GetTypeInfo().Assembly;
            AllSamples = CreateSampleInfos(_samplesAssembly).OrderBy(info => info.Category)
                .ThenBy(info => info.SampleName.ToLowerInvariant())
                .ToList();

            FullTree = BuildFullTree(AllSamples);
        }

        private static IList<SampleInfo> CreateSampleInfos(Assembly assembly)
        {
            var sampleTypes = assembly.GetTypes()
                .Where(type => type.GetTypeInfo().GetCustomAttributes().OfType<SampleAttribute>().Any());

            var samples = new List<SampleInfo>();
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

        private static SearchableTreeNode BuildFullTree(IEnumerable<SampleInfo> allSamples)
        {
            return new SearchableTreeNode(
                "All Samples",
                allSamples.ToLookup(s => s.Category) // put samples into lookup by category
                .OrderBy(s => s.Key)
                .Select(BuildTreeForCategory) // create a tree for each category
                .ToList());
        }

        private static SearchableTreeNode BuildTreeForCategory(IGrouping<string, SampleInfo> byCategory)
        {
            // only supporting one-level hierarchies for now, no subcategories
            return new SearchableTreeNode(
                byCategory.Key.ToString(),
                byCategory.OrderBy(si => si.SampleName)
                .ToList());
        }

        /// <summary>
        /// Creates a new control from sample.
        /// </summary>
        /// <param name="sampleModel">Sample that is transformed into a control</param>
        /// <returns>Sample as a control.</returns>
        public object SampleToControl(SampleInfo sampleModel)
        {
            var item = Activator.CreateInstance(sampleModel.SampleType);
            return item;
        }
    }
}