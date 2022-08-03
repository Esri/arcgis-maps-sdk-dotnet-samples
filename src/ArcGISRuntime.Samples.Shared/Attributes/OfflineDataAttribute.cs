// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.Shared.Attributes
{
    /// <summary>
    /// Attribute contains list of ArcGIS Online items (by GUID) used by a sample.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OfflineDataAttribute : Attribute
    {
        private readonly string[] _items;

        public OfflineDataAttribute(params string[] items)
        {
            _items = items;
        }

        public IReadOnlyList<string> Items { get { return _items; } }
    }
}
