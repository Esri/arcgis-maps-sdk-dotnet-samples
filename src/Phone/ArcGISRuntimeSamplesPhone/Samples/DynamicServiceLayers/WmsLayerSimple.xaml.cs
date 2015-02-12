using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates displaying Open GIS Consortium (OGC) WMS layer on a map.
	/// </summary>
	/// <title>WMS Layer</title>
	/// <category>Dynamic Service Layers</category>
	public sealed partial class WmsLayerSimple : Page
	{
		public WmsLayerSimple()
		{
			this.InitializeComponent();
			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-15000000, 2000000, -7000000, 8000000));
		}
	}
}
