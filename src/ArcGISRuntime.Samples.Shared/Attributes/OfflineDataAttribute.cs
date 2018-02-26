using ArcGISRuntime.Samples.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcGISRuntime.Samples.Shared.Attributes
{
    /// <summary>
    /// Attribute contains list of ArcGIS Online items (by GUID) used by a sample
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class OfflineDataAttribute : Attribute
    {
        private string[] items;

        public OfflineDataAttribute(params string[] items)
        {
            this.items = items;
        }

        // TODO - simplify syntax once C# 6 is available
        public IReadOnlyList<string> Items { get { return items; } }
    }
}
