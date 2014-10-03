using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Controls;
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

			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-5, 20, 50, 65, SpatialReferences.Wgs84));
            
        }

        private async void mapView1_ExtentChanged(object sender, System.EventArgs e)
        {
            var graphicslayer = overviewMap.Map.Layers.OfType<GraphicsLayer>().FirstOrDefault();
            Graphic g = graphicslayer.Graphics.FirstOrDefault();
            if (g == null) //first time
            {
                g = new Graphic();
                graphicslayer.Graphics.Add(g);
            }
            g.Geometry = mapView1.Extent;

            // Adjust overview map scale
            await overviewMap.SetViewAsync(mapView1.Extent.GetCenter(), mapView1.Scale * 15);
        }
    }
}
