using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates setting the extent and spatial reference of a map via the Map.InitialExtent property.
    /// </summary>
    /// <title>Set Spatial Reference</title>
    /// <category>Mapping</category>
	public sealed partial class SetSpatialReference : Page
    {
        public SetSpatialReference()
        {
            this.InitializeComponent();

            // NOTE: uncomment the following to set the initial extent and spatial reference via code.
            
            //mapView.Map.InitialExtent = new Esri.ArcGISRuntime.Geometry.Envelope()
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
