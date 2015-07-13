using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates changing the order of dynamic layers.
	/// </summary>
	/// <title>Dynamic Layer Reordering</title>
	/// <category>Dynamic Service Layers</category>
	public sealed partial class DynamicLayerReordering : Page
	{
		public DynamicLayerReordering()
		{
			this.InitializeComponent();
			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;
		}

		private void ChangeLayerOrderClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var dynamicLayer = MyMapView.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;

				if (dynamicLayer.DynamicLayerInfos == null)
					dynamicLayer.DynamicLayerInfos = dynamicLayer.CreateDynamicLayerInfosFromLayerInfos();

				// Move the bottom layer to the top
				var layerInfo = dynamicLayer.DynamicLayerInfos[0];
				dynamicLayer.DynamicLayerInfos.RemoveAt(0);
				dynamicLayer.DynamicLayerInfos.Add(layerInfo);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Sample Error: " + ex.Message).ShowAsync();
			}
		}
	}
}
