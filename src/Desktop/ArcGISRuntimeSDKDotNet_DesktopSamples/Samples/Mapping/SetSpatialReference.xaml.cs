using Esri.ArcGISRuntime.Geometry;
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

            // NOTE: uncomment the following to set the initial extent and spatial reference via code.

            //mapView.Map.InitialExtent = new Envelope()
            //{
            //    XMin = 661140,
            //    YMin = -1420246,
            //    XMax = 3015668,
            //    YMax = 1594451,
            //    SpatialReference = new SpatialReference(26777)
            //};
        }
    }
}
