// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Shared.Models;
using System.Collections.Generic;

namespace ArcGIS.Samples.Managers
{
    /// <summary>
    /// Provides generated sample metadata and sample factories without requiring runtime assembly scans.
    /// </summary>
    public interface ISampleCatalogProvider
    {
        IReadOnlyList<string> GetCategories();

        IReadOnlyList<string> GetSampleNames();

        IReadOnlyList<string> GetSampleNamesForCategory(string category);

        SampleInfo CreateSampleInfo(string formalName);

        object CreateSample(string formalName);
    }
}
