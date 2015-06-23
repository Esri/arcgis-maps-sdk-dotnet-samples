using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Sample shows how to project a coordinate from the current map projection (in this case Web Mercator) to a different projection.
	/// </summary>
	/// <title>Project</title>
	/// <category>Geometry</category>
	public partial class ProjectCoordinate : Windows.UI.Xaml.Controls.Page
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
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Accept user map clicks and add points to the graphics layer with the selected symbol
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

				// Convert from web mercator to WGS84
				var projectedPoint = GeometryEngine.Project(normalizedPt, SpatialReferences.Wgs84);

				gridXY.Visibility = gridLatLon.Visibility = Visibility.Visible;
				gridXY.DataContext = point;
				gridLatLon.DataContext = projectedPoint;
			}
		}
	}
}
