using System;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class SampleAttribute : Attribute
    {
        private string name;
        private string category;
        private string description;
        private string instructions;
        private string[] tags;

        public SampleAttribute(string name, string category, string description, string instructions, params string[] tags)
        {
            this.name = name;
            this.category = category;
            this.description = description;
            this.instructions = instructions;
            this.tags = tags;
        }

        public string Name { get { return name; } }
        public string Category { get { return category; } }
        public string Description { get { return description; } }
        public string Instructions { get { return instructions; } }
        public IReadOnlyList<string> Tags { get { return tags; } }
    }
}