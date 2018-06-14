// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;

namespace ArcGISRuntime.Samples.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SampleAttribute : System.Attribute
    {
        private readonly string _name;
        private readonly string _category;
        private readonly string _description;
        private readonly string _instructions;
        private readonly string[] _tags;

        public SampleAttribute(string name, string category, string description, string instructions, params string[] tags)
        {
            _name = name;
            _category = category;
            _description = description;
            _instructions = instructions;
            _tags = tags;
        }

        public string Name { get { return _name; } }
        public string Category { get { return _category; } }
        public string Description { get { return _description; } }
        public string Instructions { get { return _instructions; } }
        public System.Collections.Generic.IReadOnlyList<string> Tags { get { return _tags; } }
    }
}