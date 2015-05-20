using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// This sample demonstrates how to access layers in the map.
    /// </summary>
    /// <title>Layer List</title>
    /// <category>Mapping</category>
    public sealed partial class LayerList : Page
    {
        public LayerList()
        {
            this.InitializeComponent();

            // Set datacontext for flyout
            layerList.DataContext = MyMapView.Map;

            MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-13279586, 4010136,
                -12786147, 4280850, SpatialReferences.WebMercator));
        }

        private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement).DataContext as Layer;
            MyMapView.Map.Layers.Remove(layer);
        }

        private void EnableReordering_Click(object sender, RoutedEventArgs e)
        {
            if (lstContents.ReorderMode == ListViewReorderMode.Disabled)
                lstContents.ReorderMode = ListViewReorderMode.Enabled;
            else
                lstContents.ReorderMode = ListViewReorderMode.Disabled;
        }
    }
}
