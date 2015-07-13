using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates displaying an WmsLayer on a map.
    /// The WmsLayer allows users to display Open GIS Consortium (OGC) WMS layers.
    /// </summary>
    /// <title>WMS Layer</title>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class WmsLayerSample : Page
    {
        public WmsLayerSample()
        {
            this.InitializeComponent();
            var wmsLayer = MyMapView.Map.Layers[1] as WmsLayer;
            wmsLayer.Layers = new string[] { "nexrad-n0r" };
        }
    }
}
