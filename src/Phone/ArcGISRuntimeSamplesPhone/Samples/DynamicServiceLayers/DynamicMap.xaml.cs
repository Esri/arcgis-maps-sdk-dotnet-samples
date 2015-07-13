using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class DynamicMap : Page
    {
        public DynamicMap()
        {
            this.InitializeComponent();
			mapView.Map.InitialViewpoint = new Viewpoint(new Envelope(
				-12387666.9930794, 
				3775019.32005654, 
				-12309395.4761154, 
				3818219.62318802, 
				SpatialReferences.WebMercator));
        }
    }
}
