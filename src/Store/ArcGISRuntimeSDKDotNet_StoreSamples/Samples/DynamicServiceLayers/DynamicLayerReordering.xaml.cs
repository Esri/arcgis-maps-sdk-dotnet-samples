using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates changing the order of dynamic layers.
    /// </summary>
    /// <title>Dynamic Layer Reordering</title>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class DynamicLayerReordering : Page
    {
        public DynamicLayerReordering()
        {
            this.InitializeComponent();

			// Create initial extend and set it
			var envelopeBuilder = new EnvelopeBuilder(SpatialReference.Create(102009));
			envelopeBuilder.XMin = -3548912;
			envelopeBuilder.YMin = -1847469;
			envelopeBuilder.XMax = 2472012;
			envelopeBuilder.YMax = 1742990;

			mapView.Map.InitialExtent = envelopeBuilder.ToGeometry();
        }

        private void ChangeLayerOrderClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dynamicLayer = mapView.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;

                if (dynamicLayer.DynamicLayerInfos == null)
                    dynamicLayer.DynamicLayerInfos = dynamicLayer.CreateDynamicLayerInfosFromLayerInfos();

                // Move the bottom layer to the top
                var layerInfo = dynamicLayer.DynamicLayerInfos[0];
                dynamicLayer.DynamicLayerInfos.RemoveAt(0);
                dynamicLayer.DynamicLayerInfos.Add(layerInfo);
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Sample Error: " + ex.Message).ShowAsync();
            }
        }
    }
}
