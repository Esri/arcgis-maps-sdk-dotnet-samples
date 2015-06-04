using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Sample shows how to project a coordinate from the current map projection (in this case Web Mercator) to a different projection.
	/// </summary>
	/// <title>Project</title>
	/// <category>Geometry</category>
	public partial class ProjectCoordinate : UserControl
	{
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct Project sample control</summary>
		public ProjectCoordinate()
		{
			InitializeComponent();
		
			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"]; 
			MyMapView.ExtentChanged += MyMapView_ExtentChanged;
		}

		// Start map interaction
		private async void MyMapView_ExtentChanged(object sender, EventArgs e)
		{
			try
			{
				MyMapView.ExtentChanged -= MyMapView_ExtentChanged;
				await AcceptPointsAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message, "Project Sample");
			}
		}

		// Accept user map clicks and add points to the graphics overlay with the selected symbol
		// - collected point is in the coordinate system of the current map
		private async Task AcceptPointsAsync()
		{
			while (MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent != null)
			{
				var point = await MyMapView.Editor.RequestPointAsync();

				_graphicsOverlay.Graphics.Clear();
				_graphicsOverlay.Graphics.Add(new Graphic(point));

				// Take account of WrapAround
				var normalizedPt = GeometryEngine.NormalizeCentralMeridian(point) as MapPoint;

				// Convert from Web Mercator to WGS84
				var projectedPoint = GeometryEngine.Project(normalizedPt, SpatialReferences.Wgs84);

				gridXY.Visibility = gridLatLon.Visibility = Visibility.Visible;
				gridXY.DataContext = normalizedPt;
				gridLatLon.DataContext = projectedPoint;
			}
		}
	}
}
