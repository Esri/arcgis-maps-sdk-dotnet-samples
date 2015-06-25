using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates how to follow a target in the SceneView
	/// </summary>
	/// <title>3D Follow Target</title>
	/// <category>Scene</category>
	/// <subcategory>Mapping</subcategory>
	public partial class FollowTarget : UserControl
	{
		private DispatcherTimer _timer;
		private MapPoint _point = new MapPoint(2.3522, 48.856, 1, SpatialReferences.Wgs84);

		public FollowTarget()
		{
			InitializeComponent();

			_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(sldRefresh.Value) };
			_timer.Tick += timerTick;

			MySceneView.GraphicsOverlays["TargetOverlay"].Graphics.Add(new Graphic(_point));
		}

		void timerTick(object sender, EventArgs e)
		{
			_point = new MapPoint(_point.X + sldSpeed.Value / 10000, _point.Y, _point.Z);
			MySceneView.GraphicsOverlays["TargetOverlay"].Graphics[0].Geometry = _point;
			FollowGraphic(_point, sldHeading.Value, sldPitch.Value, sldDistance.Value);
		}

		private void sldInputs_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			// Update the FollowGraphic method as the slider inputs are changed
			if (sldHeading != null && sldPitch != null && sldDistance != null)
				FollowGraphic(_point, sldHeading.Value, sldPitch.Value, sldDistance.Value);
		}

		private async void FollowGraphic(MapPoint point, double heading, double pitch, double distance)
		{
			try
			{
				// Create a new Camera with the specified point and distance values
				var cam = new Camera(new MapPoint(point.X, point.Y, point.Z + distance), 0, 0);

				// Set the camera to RotateAround the specified point with the user-specified values.
				// RotateAround returns a new Camera with changes centered at the specified location.
				cam = cam.RotateAround(point, heading, pitch);

				// Set the SceneView to center the view on the camera's perspective
				await MySceneView.SetViewAsync(cam, TimeSpan.FromMilliseconds(1), false);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Sample Error");
			}
		}

		private void sldRefresh_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_timer != null)
				_timer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
		}

		private void FollowTargetChecked(object sender, RoutedEventArgs e)
		{
			_timer.Start();
		}

		private void FollowTargetUnchecked(object sender, RoutedEventArgs e)
		{
			if (_timer.IsEnabled)
				_timer.Stop();
		}
	}
}
