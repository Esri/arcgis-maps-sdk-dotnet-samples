using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;


namespace ArcGISRuntime.Samples.Phone.Samples
{

	/// <summary>
	/// This sample demonstrates setting the extent and spatial reference of a map via the Map.InitialExtent property.
	/// </summary>
	/// <title>Set Spatial Reference</title>
	/// <category>Mapping</category>
	public sealed partial class SetSpatialReference : Page
    {
        public SetSpatialReference()
        {
            this.InitializeComponent();
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(661140, -1420246, 3015668, 1594451, SpatialReference.Create(26777)));

			mapView1.Map.SpatialReference = SpatialReference.Create(26777); //Force map spatial reference to Wkid=26777
        }
    }
}
