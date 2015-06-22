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
			_timer.Tick += _timer_Tick;

			MySceneView.GraphicsOverlays["TargetOverlay"].Graphics.Add(new Graphic(_point));
		}

		void _timer_Tick(object sender, EventArgs e)
		{
			_point = new MapPoint(_point.X + sldSpeed.Value / 10000, _point.Y, _point.Z);
			MySceneView.GraphicsOverlays["TargetOverlay"].Graphics[0].Geometry = _point;
			FollowGraphic(_point, sldHeading.Value, sldPitch.Value, sldDistance.Value);
		}

		private void sldInputs_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (sldHeading != null && sldPitch != null && sldDistance != null)
				FollowGraphic(_point, sldHeading.Value, sldPitch.Value, sldDistance.Value);
		}

		private async void FollowGraphic(MapPoint point, double heading, double pitch, double distance)
		{
			try
			{
				var cam = new Camera(new MapPoint(point.X, point.Y, point.Z + distance), 0, 0);
				if (pitch > 7.5)
				{
					cam = cam.RotateAround(point, heading, pitch - 7.5);
					cam = new Camera(cam.Location, cam.Heading, cam.Pitch + 7.5);
				}
				else
				{
					cam = cam.RotateAround(point, heading, 0);
					cam = new Camera(cam.Location, cam.Heading, cam.Pitch + pitch);
				}

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
