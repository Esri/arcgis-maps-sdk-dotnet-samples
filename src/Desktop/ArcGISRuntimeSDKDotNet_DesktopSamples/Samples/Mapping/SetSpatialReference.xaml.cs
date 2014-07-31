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

			// Create initial extent and set it
			var envelope = new Envelope(661140, -1420246, 3015668, 1594451, SpatialReference.Create(26777));
			MyMapView.Map.InitialViewpoint = new Esri.ArcGISRuntime.Controls.Viewpoint(envelope);
			
			MyMapView.Map.SpatialReference = SpatialReference.Create(26777); //Force map spatial reference to Wkid=26777
        }
    }
}
