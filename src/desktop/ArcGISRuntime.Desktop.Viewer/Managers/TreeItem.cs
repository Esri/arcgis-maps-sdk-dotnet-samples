using System.Collections.Generic;

namespace ArcGISRuntime.Desktop.Viewer.Managers
{
    /// <summary>
    /// Item that is used to map samples to tree that supports sub nodes
    /// </summary>
    public class TreeItem
    {
        public TreeItem()
        {
            Items = new List<object>();
        }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sub items of the node.
        /// </summary>
        public List<object> Items { get; set; }
    }
}
