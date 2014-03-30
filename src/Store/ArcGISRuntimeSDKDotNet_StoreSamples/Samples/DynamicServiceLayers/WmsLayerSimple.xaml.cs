using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class WmsLayerSimple : Page
    {
        public WmsLayerSimple()
        {
            this.InitializeComponent();
            mapView1.Map.InitialExtent = new Envelope(-15000000, 2000000, -7000000, 8000000);
        }
    }
}
