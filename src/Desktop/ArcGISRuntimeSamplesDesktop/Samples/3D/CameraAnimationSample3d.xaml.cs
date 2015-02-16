using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates how to animate camera to follow predefined viewpoints in 3d space.
	/// </summary>
	/// <title>3D Camera animation</title>
	/// <category>3D</category>
	/// <subcategory>Mapping</subcategory>
	public partial class CameraAnimationSample3d : UserControl
	{
		private List<Viewpoint3D> _animationViewpoints;

		public CameraAnimationSample3d()
		{
			InitializeComponent();
			CreateAnimationViewpoints();
			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private void CreateAnimationViewpoints()
		{
			_animationViewpoints = new List<Viewpoint3D>();

			// Create set of viepoints that we want to use as a navigation points when running the animation
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.41213238640989, 37.78073901800655, 80.497554714791477), 53.719780233659428, 73.16171159612496));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40861154299266, 37.783557613586339, 80.497554662637413), 53.721937317536742, 73.161711596124661));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40839269413901, 37.7842379355343, 50.870197203010321), 136.25583345601152, 72.630747530989183));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40588743811081, 37.782161668316185, 82.6258536670357), 128.61749698574343, 72.63074401929839));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40597363202352, 37.781873299685593, 81.9174535041675), 97.363424873052821, 72.807733197737008));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40697814112256, 37.7818719178375, 263.01900420058519), 97.710076330347789, 64.489432411992638));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40526704234077, 37.780624127886668, 530.14858964923769), 47.936205042384259, 28.562500130463196));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40338310429655, 37.78266709008588, 137.86560893058777), 47.93619233260236, 28.562500130462677));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40311789569392, 37.782907119168541, 27.716264456510544), 48.514816815983046, 70.686131774471832));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40082480923844, 37.784551001333554, 50.412653733044863), 19.577314993376579, 72.633026848587079));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40008739234563, 37.784724002596619, 52.155146343633533), 317.99577324063046, 70.686129241825867));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40100302401369, 37.785351317535131, 52.155146314762533), 317.995212231342, 70.686129241825967));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40149434979705, 37.785370629198638, 48.046838515438139), 44.81163145670557, 72.63301968085598));
			_animationViewpoints.Add(new Viewpoint3D(
				new MapPoint(-122.40181664486568, 37.785056929551139, 193.81390982400626), 44.232655850881628, 43.9610566420593));
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			try
			{
				// Wait that all layers are loaded
				var results = await MySceneView.LayersLoadedAsync();

				// Set navigation in the order we want to animate the camera
				await MySceneView.SetViewAsync(_animationViewpoints[0], 1, true);
				await MySceneView.SetViewAsync(_animationViewpoints[1], 0.2, true);
				await MySceneView.SetViewAsync(_animationViewpoints[2], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[3], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[4], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[5], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[6], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[7], 0.4, false);
				await MySceneView.SetViewAsync(_animationViewpoints[8], 0.3, false);
				await MySceneView.SetViewAsync(_animationViewpoints[9], 0.3, false);
				await MySceneView.SetViewAsync(_animationViewpoints[10], 0.3, false);
				await MySceneView.SetViewAsync(_animationViewpoints[11], 0.3, false);
				await MySceneView.SetViewAsync(_animationViewpoints[12], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[13], 0.2, false);
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
