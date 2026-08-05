// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

namespace ArcGIS.Samples.Shared.Models;

public class SampleKeywordScore
{
    /// <summary>
    /// Gets or sets the keywords associated with this level. Keywords are used to match search terms to samples.
    /// </summary>
    public string[] Keywords { get; set; }

    /// <summary>
    /// Gets or sets the score factor for this level of keywords. The score factor is used to weight the relevance of matches.
    /// </summary>
    public int ScoreFactor { get; set; } = 1;
}
