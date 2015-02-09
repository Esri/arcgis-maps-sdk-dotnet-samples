using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
	/// <summary>
	/// This sample demonstrates how to load dynamic map service layers in xaml. 
	/// </summary>
	/// <title>Dynamic Layers in XAML</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class DynamicLayersInXAML : UserControl
	{
		public DynamicLayersInXAML()
		{
			InitializeComponent();

			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-3170138, -1823795, 2850785, 1766663, SpatialReference.Create(102009)));

			(MyMapView.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer).VisibleLayers = new System.Collections.ObjectModel.ObservableCollection<int> { 0, 2, 4 };

		}
	}
}
