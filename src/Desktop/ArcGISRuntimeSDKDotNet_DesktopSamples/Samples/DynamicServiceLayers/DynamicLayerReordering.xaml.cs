using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Linq;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples.DynamicLayers
{
    /// <summary>
    /// This sample demonstrates changing the order of dynamic layers.  Clicking the 'Change Layer Order' button uses the DynamicLayerInfoCollection to move the top layer to the bottom.
    /// </summary>
    /// <title>Dynamic Layer Reordering</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class DynamicLayerReordering : UserControl
    {
        public DynamicLayerReordering()
        {
            InitializeComponent();

			// Create initial extend and set it
			var envelopeBuilder = new EnvelopeBuilder(SpatialReference.Create(102009));
			envelopeBuilder.XMin = -3548912;
			envelopeBuilder.YMin = -1847469;
			envelopeBuilder.XMax = 2472012;
			envelopeBuilder.YMax = 1742990;

			mapView.Map.InitialExtent = envelopeBuilder.ToGeometry();
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
