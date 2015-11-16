using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates Querying the elevation of a local file elevation source
	/// </summary>
	/// <title>3D Query Elevation</title>
	/// <category>Scene</category>
	/// <subcategory>Elevation</subcategory>
	public partial class QueryElevation3d : UserControl
	{
		private bool _isSceneReady;
		private FileElevationSource _fileElevationSource;

		public QueryElevation3d()
		{
			InitializeComponent();
			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			List<string> fileNames = new List<string>();
			fileNames.Add(@"..\..\..\samples-data\elevation\SRTM-Void-Filled-3-arc-second\n18_w156_3arc_v2.dt1");
			fileNames.Add(@"..\..\..\samples-data\elevation\SRTM-Void-Filled-3-arc-second\n19_w155_3arc_v2.dt1");
			fileNames.Add(@"..\..\..\samples-data\elevation\SRTM-Void-Filled-3-arc-second\n19_w156_3arc_v2.dt1");
			fileNames.Add(@"..\..\..\samples-data\elevation\SRTM-Void-Filled-3-arc-second\n19_w157_3arc_v2.dt1");
			fileNames.Add(@"..\..\..\samples-data\elevation\SRTM-Void-Filled-3-arc-second\n20_w156_3arc_v2.dt1");

			foreach (var item in fileNames)
			{
				if (!File.Exists(item))
				{
					MessageBox.Show("Sample data not found");
					return;
				}
			}

			_fileElevationSource = new FileElevationSource(fileNames);
			MySceneView.Scene.Surface.Add(_fileElevationSource);

			MySceneView.SetViewAsync(new Camera(new MapPoint(-156.277, 18.356, 58877.626), 20.091, 70.160), new TimeSpan(0, 0, 5));
			await MySceneView.LayersLoadedAsync();
			_isSceneReady = true;

			MySceneView.MouseMove += MySceneView_MouseMove;
		}

		void MySceneView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			System.Windows.Point screenPoint = e.GetPosition(MySceneView);
			MapPoint mapPoint = MySceneView.ScreenToLocation(screenPoint);

			if (mapPoint == null)
				return;

			QueryElevation(mapPoint);
		}

		private async Task QueryElevation(MapPoint location)
		{
			if (MySceneView.GetCurrentViewpoint(ViewpointType.BoundingGeometry) == null)
				return;

			if (!_isSceneReady)
				return;

			try
			{
				_isSceneReady = false;

				double elevation = await _fileElevationSource.GetElevationAsync(location);

				if (elevation.ToString() == "NaN")
				{
					mapTip.Visibility = System.Windows.Visibility.Hidden;
					return;
				}

				MapView.SetViewOverlayAnchor(mapTip, location);
				mapTip.Visibility = System.Windows.Visibility.Visible;
				txtElevation.Text = String.Format("Elevation: {0} meters", elevation.ToString());

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error retrieving elevation values: " + ex.Message, "Sample Error");
			}
			finally
			{
				_isSceneReady = true;
			}
		}
	}
}
