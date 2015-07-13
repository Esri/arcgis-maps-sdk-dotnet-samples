using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Diagnostics;
using System.Windows;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates adding an 2d ArcGIS dynamic map service to a Scene in XAML.
	/// </summary>
	/// <title>3D ArcGIS Dynamic Map Service Layer</title>
	/// <category>Scene</category>
	/// <subcategory>Layers</subcategory>
	public partial class ArcGISDynamicMapServiceLayerSample3d
	{
		public ArcGISDynamicMapServiceLayerSample3d()
		{
			InitializeComponent();
			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			try
			{
				// Set camera and navigate to it
				var viewpoint = new Camera(
					location:new MapPoint(x: -99,y: 34,z: 3500000),
					heading:0,
					pitch:5);
				await MySceneView.SetViewAsync(camera: viewpoint, velocity: 10, liftOff: true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occured while navigating to the target viewpoint",
					"An error occured");
				Debug.WriteLine(ex.ToString());
			}
		}
	}
}
