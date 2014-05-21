using System.Linq;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Graphics Layers</category>
	public sealed partial class AddPointOnTap : Page
    {
        public AddPointOnTap()
        {
            this.InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var layer = mapView1.Map.Layers.OfType<GraphicsLayer>().First();
            layer.Graphics.Clear();
        }

        private void mapView1_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            // Convert screen point to map point
            var mapPoint = mapView1.ScreenToLocation(e.Position);

            // Create graphic
            Graphic g = new Graphic() { Geometry = mapPoint };

            // Get layer and add point to it
            var graphicsLayer = mapView1.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.Graphics.Add(g);
        }
    }
}
