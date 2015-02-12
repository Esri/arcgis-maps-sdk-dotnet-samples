using Esri.ArcGISRuntime.Geometry;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates changing the order of dynamic layers. Clicking the 'Change Layer Order' button uses the DynamicLayerInfoCollection to move the top layer to the bottom.
    /// </summary>
    /// <title>Dynamic Layer Reordering</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class DynamicLayerReordering : UserControl
    {
        public DynamicLayerReordering()
        {
            InitializeComponent();
			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;
        }

        private void ChangeLayerOrderClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dynamicLayer.DynamicLayerInfos == null)
                dynamicLayer.DynamicLayerInfos = dynamicLayer.CreateDynamicLayerInfosFromLayerInfos();

            // Move the bottom layer to the top
            var layerInfo = dynamicLayer.DynamicLayerInfos[0];
            dynamicLayer.DynamicLayerInfos.RemoveAt(0);
            dynamicLayer.DynamicLayerInfos.Add(layerInfo);
        }
    }
}
