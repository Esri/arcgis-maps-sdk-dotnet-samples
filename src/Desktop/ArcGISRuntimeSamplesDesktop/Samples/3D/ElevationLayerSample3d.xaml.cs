using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Diagnostics;
using System.Windows;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates adding an Elevation layer to a Scene in XAML.
	/// </summary>
	/// <title>3D Elevation Layer</title>
	/// <category>3D</category>
	/// <subcategory>Layers</subcategory>
	public partial class ElevationLayerSample3d
	{
		public ElevationLayerSample3d()
		{
			InitializeComponent();
			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			try
			{
				// Set viewpoint and navigate to it
				var viewpoint = new Viewpoint3D(
						new MapPoint(
							-89.029982661438169,
							34.200611952095031,
							3525722.6715643629),
						8.6483856492844726,
						0.59166619557758571);
				await MySceneView.SetViewAsync(viewpoint, 1, true);
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
