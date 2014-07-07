using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Linq;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates displaying an overview map to indicate the extent of the parent map.
    /// </summary>
    /// <title>Overview Map</title>
    /// <category>Mapping</category>
    public partial class OverviewMap : UserControl
    {
        /// <summary>Construct Overview Map sample control</summary>
        public OverviewMap()
        {
            InitializeComponent();

			mapView.Map.InitialViewpoint = new Envelope(-5, 20, 50, 65, SpatialReferences.Wgs84);

            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        private async void mapView_ExtentChanged(object sender, System.EventArgs e)
        {
            // Update overview map graphic
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
