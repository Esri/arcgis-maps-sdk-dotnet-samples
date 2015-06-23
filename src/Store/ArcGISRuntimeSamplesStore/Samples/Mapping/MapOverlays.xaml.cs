using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates how to display map overlay elements in the MapView.
	/// </summary>
	/// <title>Map Overlays</title>
	/// <category>Mapping</category>
	public sealed partial class MapOverlays : Page
	{
		private FrameworkElement _clickOverlay;
		private FrameworkElement _centerOverlay;

		public MapOverlays()
		{
			this.InitializeComponent();
			(MyMapView.Overlays.Items[0] as Grid).DataContext = new MapPoint(-117.19568, 34.056601, SpatialReferences.Wgs84);
			
			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;

			_clickOverlay = FindName("clickOverlay") as FrameworkElement;
			_centerOverlay = FindName("centerOverlay") as FrameworkElement;

            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

			var normalizedPoint = GeometryEngine.NormalizeCentralMeridian(viewpointExtent.GetCenter());
			var projectedCenter = GeometryEngine.Project(normalizedPoint, SpatialReferences.Wgs84) as MapPoint;

			if (!(_clickOverlay.DataContext is MapPoint))
				_clickOverlay.DataContext = projectedCenter;

			_centerOverlay.DataContext = projectedCenter;
		}

		private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			var normalizedPoint = GeometryEngine.NormalizeCentralMeridian(e.Location);
			var projectedCenter = GeometryEngine.Project(normalizedPoint, SpatialReferences.Wgs84) as MapPoint;
			_clickOverlay.DataContext = projectedCenter;
		}
	}
}
