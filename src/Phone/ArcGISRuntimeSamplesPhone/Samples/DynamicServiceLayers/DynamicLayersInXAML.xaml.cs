using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to load dynamic map service layers in xaml. 
	/// </summary>
	/// <title>Dynamic Layers in XAML</title>
	/// <category>Dynamic Service Layers</category>
	public sealed partial class DynamicLayersInXAML : Page
	{
		public DynamicLayersInXAML()
		{
			this.InitializeComponent();
			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-3170138, -1823795, 2850785, 1766663, SpatialReference.Create(102009)));

			(MyMapView.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer).VisibleLayers = new System.Collections.ObjectModel.ObservableCollection<int> { 0, 2, 4 };
		}
	}
}
