using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates use of the GeometryEngine.Simplify method to simplify a polygon, and demonstrates the importance of simplification. To use the sample, click the Query with Original Polygon button. Observe the results include parcels that are not wholly within the red polygon. Then click the Simplify Polygon, then Query button. Observe the resulting parcels are now wholly within the red polygon.
	/// </summary>
	/// <title>Simplify</title>
	/// <category>Geometry</category>
	public partial class Simplify : UserControl
	{
		private Polygon _unsimplifiedPolygon;
		private GraphicsOverlay _parcelOverlay;
		private GraphicsOverlay _polygonOverlay;

		/// <summary>Construct Geodesic Move sample control</summary>
		public Simplify()
		{
			InitializeComponent();

			_parcelOverlay = MyMapView.GraphicsOverlays["parcelOverlay"];
			_polygonOverlay = MyMapView.GraphicsOverlays["polygonOverlay"];

			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
		}

		// Start map interaction once the mapview finishes navigation to initial viewpoint
		private void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			DrawPolygon();
		}

		// Query without simplifying original geometry
		private async void QueryOnlyButton_Click(object sender, RoutedEventArgs e)
		{
			await ParcelQuery(_unsimplifiedPolygon);
		}

		// Simplify and then query
		private async void SimplifyAndQueryButton_Click(object sender, RoutedEventArgs e)
		{
			var simplifiedPolygon = GeometryEngine.Simplify(_unsimplifiedPolygon);
			await ParcelQuery(simplifiedPolygon);
		}

		// Draw the unsimplified polygon
		private void DrawPolygon()
		{
			// Get current viewpoints extent from the MapView
			var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
			var viewpointExtent = currentViewpoint.TargetGeometry.Extent; 

			MapPoint center = viewpointExtent.GetCenter();
			double lat = center.Y;
			double lon = center.X + 300;
			double latOffset = 300;
			double lonOffset = 300;

			var points = new PointCollection()
			{
				new MapPoint(lon - lonOffset, lat),
				new MapPoint(lon, lat + latOffset),
				new MapPoint(lon + lonOffset, lat),
				new MapPoint(lon, lat - latOffset),
				new MapPoint(lon - lonOffset, lat),
				new MapPoint(lon - 2 * lonOffset, lat + latOffset),
				new MapPoint(lon - 3 * lonOffset, lat),
				new MapPoint(lon - 2 * lonOffset, lat - latOffset),
				new MapPoint(lon - 1.5 * lonOffset, lat + latOffset),
				new MapPoint(lon - lonOffset, lat)
			};
			_unsimplifiedPolygon = new Polygon(points, MyMapView.SpatialReference);

			_polygonOverlay.Graphics.Clear();
			_polygonOverlay.Graphics.Add(new Graphic(_unsimplifiedPolygon));
		}

		// Query the parcel service with the given geometry (Contains)
		private async Task ParcelQuery(Geometry geometry)
		{
			try
			{
				QueryTask queryTask = new QueryTask(
					new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2"));
				var query = new Query(geometry)
				{
					ReturnGeometry = true,
					OutSpatialReference = MyMapView.SpatialReference,
					SpatialRelationship = SpatialRelationship.Contains,
					OutFields = OutFields.All
				};
				var result = await queryTask.ExecuteAsync(query);

				_parcelOverlay.Graphics.Clear();
				_parcelOverlay.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Simplify Geometry Sample");
			}
		}
	}
}
