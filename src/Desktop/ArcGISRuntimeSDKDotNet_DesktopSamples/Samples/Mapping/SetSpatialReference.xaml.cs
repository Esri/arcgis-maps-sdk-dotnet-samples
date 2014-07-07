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

			// Create initial extend and set it
			var envelopeBuilder = new EnvelopeBuilder(SpatialReference.Create(26777));
			envelopeBuilder.XMin = 661140;
			envelopeBuilder.YMin = -1420246;
			envelopeBuilder.XMax = 3015668;
			envelopeBuilder.YMax = 1594451;

			mapView.Map.InitialViewpoint = new Esri.ArcGISRuntime.Controls.Viewpoint(envelopeBuilder.ToGeometry());
			
			mapView.Map.SpatialReference = SpatialReference.Create(26777); //Force map spatial reference to Wkid=26777
        }
    }
}
