using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Diagnostics;
using System.Windows;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates adding an ElevationSource to a Scene in XAML.
	/// </summary>
	/// <title>3D Elevation Source</title>
	/// <category>Scene</category>
	/// <subcategory>Elevation</subcategory>
	public partial class ElevationSourceSample3d
	{
		public ElevationSourceSample3d()
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
					new MapPoint(
						-122.41213238640989, 
						37.78073901800655, 
						80.497554714791477),
						 53.719780233659428, 
						 73.16171159612496);
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
