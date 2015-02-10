using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates the use of GroupLayers to contain related layers in a map.  In this sample there are two GroupLayers, one containing two basemap layers and the other containing four dynamic or feature layers.  The legend shows how Layer properties (like IsVisible and Opacity) of layers within a GroupLayer can be managed either individually or as a part of the group.
    /// </summary>
    /// <title>Group Layers</title>
	/// <category>Mapping</category>
    public partial class GroupLayers : UserControl
    {
        public GroupLayers()
        {
            InitializeComponent();
        }
    }
}
