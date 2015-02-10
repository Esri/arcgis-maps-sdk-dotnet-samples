using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// Shows how to set the initial extent of the map (Map.InitialViewpoint).
	/// </summary>
	/// <title>Set Initial Map Extent</title>
	/// <category>Mapping</category>
	public sealed partial class SetInitialMapExtent : Page
	{
		public SetInitialMapExtent()
		{
			this.InitializeComponent();

			// Note: uncomment the following to set the initial extent of the map in code.
			//MyMapView.Map.InitialViewpoint = new ViewpointExtent(
			//	new Envelope(-117.182686,32.695853,-117.133872,32.718530, SpatialReferences.Wgs84)); 
		}
	}
}
