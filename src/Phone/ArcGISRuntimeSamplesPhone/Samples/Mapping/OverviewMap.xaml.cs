using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates displaying an overview map to indicate the extent of the parent map.
	/// </summary>
	/// <title>Overview Map</title>
	/// <category>Mapping</category>
	public sealed partial class OverviewMap : Page
	{
		public OverviewMap()
		{
			this.InitializeComponent();

			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-5, 20, 50, 65, SpatialReferences.Wgs84));
			
		}

		private async void MyMapView_ExtentChanged(object sender, System.EventArgs e)
		{
			var graphicslayer = overviewMap.Map.Layers.OfType<GraphicsLayer>().FirstOrDefault();
			Graphic g = graphicslayer.Graphics.FirstOrDefault();
			if (g == null) //first time
			{
				g = new Graphic();
				graphicslayer.Graphics.Add(g);
			}
            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;
            g.Geometry = viewpointExtent;

			// Adjust overview map scale
			await overviewMap.SetViewAsync(viewpointExtent.GetCenter(), MyMapView.Scale * 15);

		}
	}
}
