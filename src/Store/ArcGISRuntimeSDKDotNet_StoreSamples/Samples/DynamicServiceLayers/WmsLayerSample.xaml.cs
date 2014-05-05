using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
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

            mapView.Map.InitialExtent = new Envelope(-15000000, 2000000, -7000000, 8000000);

            var wmsLayer = mapView.Map.Layers[1] as WmsLayer;
            wmsLayer.Layers = new string[] { "nexrad-n0r" };
        }
    }
}
