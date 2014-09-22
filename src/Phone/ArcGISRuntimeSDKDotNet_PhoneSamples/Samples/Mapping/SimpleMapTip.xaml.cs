using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Mapping</category>
	public sealed partial class SimpleMapTip : Page
    {
        public SimpleMapTip()
        {
            this.InitializeComponent();
            ((Grid)mapView1.Overlays.Items[0]).DataContext = new MapPoint(-117.19568, 34.056601, SpatialReferences.Wgs84);			
        }
    }
}
