using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
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

            // Note: uncomment the following to set the initial extent and spatial reference) 
            //  of the map in code.
            
            //map1.InitialExtent = new Esri.ArcGISRuntime.Geometry.Envelope()
            //{               
            //    XMin = -3170138,
            //    YMin = -1823795,
            //    XMax = 2850785,
            //    YMax = 1766663,
            //    SpatialReference = new Esri.ArcGISRuntime.Geometry.SpatialReference(102009)
            //};
        }
    }
}
