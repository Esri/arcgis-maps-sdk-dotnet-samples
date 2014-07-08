using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Mapping</category>
	public sealed partial class OverviewMap : Page
    {
        public OverviewMap()
        {
            this.InitializeComponent();

			mapView1.Map.InitialViewpoint = GeometryEngine.Project(new Envelope(-5, 20, 50, 65, SpatialReferences.Wgs84), SpatialReferences.WebMercator) as Envelope;
            
        }

        private void mapView1_ExtentChanged(object sender, System.EventArgs e)
        {
            var graphicslayer = overviewMap.Map.Layers.OfType<GraphicsLayer>().FirstOrDefault();
            Graphic g = graphicslayer.Graphics.FirstOrDefault();
            if (g == null) //first time
            {
                g = new Graphic();
                graphicslayer.Graphics.Add(g);
            }
            g.Geometry = mapView1.Extent;
        }
    }
}
