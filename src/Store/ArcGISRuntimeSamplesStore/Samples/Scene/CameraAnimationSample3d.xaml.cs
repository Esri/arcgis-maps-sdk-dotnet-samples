using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates how to animate camera to follow predefined viewpoints in 3d space.
	/// </summary>
	/// <title>3D Camera Animation</title>
	/// <category>Scene</category>
	public sealed partial class CameraAnimationSample3d : Page
	{
		private List<Camera> _animationViewpoints;

		public CameraAnimationSample3d()
		{
			this.InitializeComponent();
			CreateAnimationViewpoints();
			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private void CreateAnimationViewpoints()
		{
			_animationViewpoints = new List<Camera>();

			// Create set of viewpoints that we want to use as a navigation points when running the animation
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.114867093837, 49.2638368778531, 340.3367222948), 7.374728906848, 69.8626679976746));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.114965292071, 49.2717251063816, 89.4152405494824), 7.3746354649862, 69.8626679976747));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.115875364452, 49.2738362856553, 47.3339174203575), 58.3560195341804, 84.379160503014));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.108207747363, 49.2739366964438, 161.416419573128), 333.460145032915, 64.7491138927415));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.106397410767, 49.2755549744347, 255.560161876492), 301.384894480396, 58.0403593524606));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.110753487062, 49.2777895041816, 49.4048086255789), 315.245367181591, 98.8467517341441));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.113667344201, 49.2796475948698, 57.1119953114539), 314.711862150522, 83.7545479170797));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.117795544198, 49.2814476204395, 64.6341089177877), 49.43800242478, 82.4632096505297));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.114661919624, 49.283457382915, 60.3068332597613), 38.281643690211, 82.5829177202596));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.108683730623, 49.2866845667142, 8.81717556901276), 234.297965820692, 95.6078872295047));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.10993747893, 49.2860677171583, 85.5259443288669), 226.969682773948, 105.037684376599));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.111263519792, 49.2857483082594, 268.379973833449), 211.160304925221, 34.4122534933856));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.113991384417, 49.2839562358968, 225.773985985667), 76.9587991589986, 58.5224006620205));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.125265335238, 49.2884082699029, 483.856628921814), 111.082681508211, 54.468550947838));
			_animationViewpoints.Add(new Camera(
				new MapPoint(-123.144807433184, 49.2940492084871, 932.409413537942), 111.073466603409, 54.4685509478381));

		}

		private async void MySceneView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			try
			{
				// Set first one to starting point
				MySceneView.SetView(_animationViewpoints[0]);

				await MySceneView.LayersLoadedAsync();

				// Set navigation in the order we want to animate the camera
				await MySceneView.SetViewAsync(_animationViewpoints[1], 0.2, true);
				await MySceneView.SetViewAsync(_animationViewpoints[2], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[3], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[4], 0.4, false);
				await MySceneView.SetViewAsync(_animationViewpoints[5], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[6], 0.3, false);
				await MySceneView.SetViewAsync(_animationViewpoints[7], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[8], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[9], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[10], 0.3, false);
				await MySceneView.SetViewAsync(_animationViewpoints[11], 0.3, false);
				await MySceneView.SetViewAsync(_animationViewpoints[12], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[13], 0.2, false);
				await MySceneView.SetViewAsync(_animationViewpoints[14], 0.2, false);

			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Error occured while navigating to the target viewpoint", "Sample Error").ShowAsync();
			}
		}
	}
}
