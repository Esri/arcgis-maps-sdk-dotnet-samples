using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates adding an ArcGIS image service layer to a map
    /// </summary>
    /// <title>ArcGIS Image Service Layer</title>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class ArcGISImageServiceLayerSample : Page
    {
        public ArcGISImageServiceLayerSample()
        {
            this.InitializeComponent();
            
            mapView.Map.InitialExtent = new Envelope(-13486609, 5713307, -13263258, 5823117) { SpatialReference = new SpatialReference(102100) };
        }
    }
}
