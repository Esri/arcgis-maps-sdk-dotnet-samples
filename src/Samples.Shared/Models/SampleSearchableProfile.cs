// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Collections.Generic;

namespace ArcGIS.Samples.Shared.Models;

/// <summary>
/// Represents a searchable profile for a sample, containing its formal name and associated keyword scores.
/// </summary>
public class SampleSearchableProfile
{
    /// <summary>
    /// Gets or sets the formal name of the sample. This is used as an identifier for the sample in search operations.
    /// </summary>
    public string SampleFormalName { get; set; }

    /// <summary>
    /// Gets the collection of keyword scores associated with the sample. Each keyword score contains keywords and a score factor that indicates the relevance of those keywords for search operations.
    /// </summary>
    public List<SampleKeywordScore> KeywordScoreCollection { get; private set; } = new List<SampleKeywordScore>();
}