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

            mapView.Map.InitialExtent = new Envelope(-13279585.9811197, 4010136.34579502,
                -12786146.5545795, 4280849.94238526, SpatialReferences.WebMercator);
        }

        private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement).DataContext as Layer;
            mapView.Map.Layers.Remove(layer);
        }
    }
}
