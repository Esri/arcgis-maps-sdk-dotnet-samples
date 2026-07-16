// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Shared.Attributes;
using ArcGIS.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ArcGIS.Samples.Managers
{
    /// <summary>
    /// Reflection-backed sample catalog used when a generated catalog is not available.
    /// </summary>
    public sealed class ReflectionSampleCatalogProvider : ISampleCatalogProvider
    {
        private readonly Dictionary<string, SampleInfo> _samplesByName;
        private readonly Dictionary<string, string[]> _sampleNamesByCategory;
        private readonly string[] _categories;
        private readonly string[] _sampleNames;

        public ReflectionSampleCatalogProvider(Assembly samplesAssembly)
        {
            if (samplesAssembly == null)
            {
                throw new ArgumentNullException(nameof(samplesAssembly));
            }

            List<SampleInfo> samples = CreateSampleInfos(samplesAssembly)
                .OrderBy(info => info.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(info => info.SampleName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            List<SampleInfo> uniqueSamples = samples
                .GroupBy(sample => sample.FormalName, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            _samplesByName = uniqueSamples.ToDictionary(sample => sample.FormalName, sample => sample, StringComparer.OrdinalIgnoreCase);
            _sampleNames = uniqueSamples.Select(sample => sample.FormalName).ToArray();
            _categories = uniqueSamples.Select(sample => sample.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(category => category, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            _sampleNamesByCategory = _categories.ToDictionary(
                category => category,
                category => uniqueSamples
                    .Where(sample => sample.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .Select(sample => sample.FormalName)
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<string> GetCategories() => _categories;

        public IReadOnlyList<string> GetSampleNames() => _sampleNames;

        public IReadOnlyList<string> GetSampleNamesForCategory(string category)
        {
            return !string.IsNullOrEmpty(category) && _sampleNamesByCategory.TryGetValue(category, out string[] sampleNames)
                ? sampleNames
                : Array.Empty<string>();
        }

        public SampleInfo CreateSampleInfo(string formalName)
        {
            return GetRequiredSampleInfo(formalName);
        }

        public object CreateSample(string formalName)
        {
            SampleInfo sampleInfo = GetRequiredSampleInfo(formalName);
            return Activator.CreateInstance(sampleInfo.SampleType);
        }

        private SampleInfo GetRequiredSampleInfo(string formalName)
        {
            if (!string.IsNullOrEmpty(formalName) && _samplesByName.TryGetValue(formalName, out SampleInfo sampleInfo))
            {
                return sampleInfo;
            }

            throw new ArgumentException($"Unknown sample '{formalName}'.", nameof(formalName));
        }

        private static IList<SampleInfo> CreateSampleInfos(Assembly assembly)
        {
            IEnumerable<Type> sampleTypes = assembly.GetTypes()
                .Where(type => type.GetTypeInfo().GetCustomAttributes().OfType<SampleAttribute>().Any());

            List<SampleInfo> samples = new List<SampleInfo>();

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
    }
}
