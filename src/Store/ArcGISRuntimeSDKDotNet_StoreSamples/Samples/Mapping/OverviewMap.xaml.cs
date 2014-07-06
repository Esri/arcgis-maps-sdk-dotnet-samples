using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// This sample demonstrates displaying an overview map to indicate the extent of the parent map.
    /// </summary>
    /// <title>Overview Map</title>
    /// <category>Mapping</category>
	public sealed partial class OverviewMap : Page
    {
        public OverviewMap()
        {
            this.InitializeComponent();

			mapView.Map.InitialViewpoint = new Esri.ArcGISRuntime.Controls.Viewpoint(new Envelope(-5, 20, 50, 65, SpatialReferences.Wgs84));
			mapView.Map.SpatialReference = SpatialReferences.WebMercator;
        }

        private async void mapView_ExtentChanged(object sender, System.EventArgs e)
        {
            var graphicslayer = overviewMap.Map.Layers.OfType<GraphicsLayer>().FirstOrDefault();
            Graphic g = graphicslayer.Graphics.FirstOrDefault();
            if (g == null) //first time
            {
                g = new Graphic();
                graphicslayer.Graphics.Add(g);
            }
            g.Geometry = mapView.Extent;

            // Adjust overview map scale
            await overviewMap.SetViewAsync(mapView.Extent.GetCenter(), mapView.Scale * 15);
        }
    }
}
