using Esri.ArcGISRuntime.Geometry;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates setting the extent and spatial reference of a map via the Map.InitialExtent property.
    /// </summary>
    /// <title>Set Spatial Reference</title>
	/// <category>Mapping</category>
	public partial class SetSpatialReference : UserControl
    {
        public SetSpatialReference()
        {
            InitializeComponent();
		
			MyMapView.Map.SpatialReference = SpatialReference.Create(26777); //Force map spatial reference to Wkid=26777
        }
    }
}
