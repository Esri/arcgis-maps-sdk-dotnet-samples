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

			esriOverlay.DataContext = new MapPointBuilder(-117.19568, 34.056601, SpatialReferences.Wgs84).ToGeometry();
        }
    }
}
