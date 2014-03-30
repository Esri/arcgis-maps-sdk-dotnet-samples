using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Microsoft.Phone.Controls;
using System.Linq;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Mapping</category>
	public partial class OverviewMap : PhoneApplicationPage
    {
        public OverviewMap()
        {
            InitializeComponent();
        }

        private void map1_ExtentChanged(object sender, System.EventArgs e)
        {
            var graphicslayer = Overview.Layers.OfType<GraphicsLayer>().First();
            Graphic g = graphicslayer.Graphics.FirstOrDefault();
            if (g == null) //first time
            {
                g = new Graphic();
                graphicslayer.Graphics.Add(g);
            }
            g.Geometry = ((MapView)sender).Extent;
        }
    }
}