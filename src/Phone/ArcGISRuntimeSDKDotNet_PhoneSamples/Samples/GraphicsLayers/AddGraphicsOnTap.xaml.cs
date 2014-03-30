using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using System.Linq;
using System.Windows.Input;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Graphics Layer</category>
	public partial class AddGraphicsOnTap : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        public AddGraphicsOnTap()
        {
            InitializeComponent();
        }

        private void mapView1_Tap(object sender, MapViewInputEventArgs e)
        {
          

            // Create graphic
            Graphic g = new Graphic() { Geometry = e.Location };

            // Get layer and add point to it
            mapview1.Map.Layers.OfType<GraphicsLayer>().First().Graphics.Add(g);
        }
    }
}