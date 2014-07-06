using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Shows how to access layers in the map.
    /// </summary>
    /// <title>Layer List</title>
    /// <category>Mapping</category>
    public sealed partial class LayerList : Page
    {
        public LayerList()
        {
            this.InitializeComponent();

			mapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-13279586, 4010136, -12786147, 4280850, SpatialReferences.WebMercator));
        }

        private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement).DataContext as Layer;
            mapView.Map.Layers.Remove(layer);
        }
    }
}
