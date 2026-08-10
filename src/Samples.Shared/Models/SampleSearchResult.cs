// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

namespace ArcGIS.Samples.Shared.Models;

/// <summary>
/// Represents the result of a search operation, containing the score of the match and the identifier of the matched item.
/// </summary>
public class SampleSearchResult
{
    /// <summary>
    /// Gets or sets the identifier of the matched item. This identifier is used to reference the specific item that was matched in the search operation.
    /// </summary>
	public string SampleFormalName { get; set; }

    /// <summary>
    /// Gets or sets the score of the search result. The score indicates the relevance of the match, with higher scores representing more relevant matches.
    /// </summary>
    public int Score { get; set; }
}