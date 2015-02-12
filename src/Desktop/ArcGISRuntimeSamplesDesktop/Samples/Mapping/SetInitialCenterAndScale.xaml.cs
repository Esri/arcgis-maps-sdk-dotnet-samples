using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Shows how to set the initial center and scale of the map (Map.InitialViewpoint).
	/// </summary>
	/// <title>Set Initial Center and Scale</title>
	/// <category>Mapping</category>
	public partial class SetInitialCenterAndScale : UserControl
    {
		public SetInitialCenterAndScale()
        {
            InitializeComponent();

			// Note: uncomment the following to set the initial center/scale of the map in code.
			//MyMapView.Map.InitialViewpoint = new Esri.ArcGISRuntime.Controls.Viewpoint(new MapPoint(-117.15,32.705,SpatialReferences.Wgs84), 50000); 
        }
    }
}
