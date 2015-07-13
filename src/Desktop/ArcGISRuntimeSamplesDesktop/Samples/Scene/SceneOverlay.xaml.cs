using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates how to use a scene overlay as a label
	/// </summary>
	/// <title>3D Scene Overlay as Label</title>
	/// <category>Scene</category>
	/// <subcategory>Graphics</subcategory>
	public partial class SceneOverlay : UserControl
	{
		public SceneOverlay()
		{
			InitializeComponent();
		}

		private async void MySceneView_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
		{
			if (e.LoadError == null && e.Layer.ID == "AGOLayer")
			{
				var camera = new Esri.ArcGISRuntime.Controls.Camera(new MapPoint(2.2950, 48.8738, 3000000), 0, 0);
				MySceneView.SetViewAsync(camera);
				
				// Find the overlay element from the MapView using its name
				var triompheTip = this.MySceneView.FindName("triompheOverlay") as FrameworkElement;

				// If the overlay element is found, set its position and make it visible
				if (triompheTip != null)
				{
					var overlayLocation = new MapPoint(2.2950, 48.8738, SpatialReferences.Wgs84);
					MapView.SetViewOverlayAnchor(triompheTip, overlayLocation);
					triompheTip.Visibility = Visibility.Visible;
				}
			}
		}
	}
}
