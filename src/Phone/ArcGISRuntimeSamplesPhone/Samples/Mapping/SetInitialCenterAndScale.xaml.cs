using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// Shows how to set the initial center and scale of the map (Map.InitialViewpoint).
	/// </summary>
	/// <title>Set Initial Center and Scale</title>
	/// <category>Mapping</category>
	public sealed partial class SetInitialCenterAndScale : Page
	{
		public SetInitialCenterAndScale()
		{
			InitializeComponent();

			// Note: uncomment the following to set the initial center/scale of the map in code.
			// MyMapView.Map.InitialViewpoint = new Esri.ArcGISRuntime.Controls.Viewpoint(new MapPoint(-117.15,32.705,SpatialReferences.Wgs84), 50000); 
		}
	}
}
