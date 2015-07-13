using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Geometry = Esri.ArcGISRuntime.Geometry;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates the GeometryEngine.Boundary method to calculate the geometric boundary of a given geometry. Here, boundaries are calculated for a multi-part polygon (returning a boundary polyline) and a multi-part polyline (returning multiple points).
	/// </summary>
	/// <title>Boundary</title>
	/// <category>Geometry</category>
	public partial class Boundary : UserControl
	{
		private GraphicsOverlay _testGraphics;
		private GraphicsOverlay _boundaryGraphics;

		/// <summary>Construct Boundary sample control</summary>
		public Boundary()
		{
			InitializeComponent();

			_testGraphics = MyMapView.GraphicsOverlays["TestGraphics"];
			_boundaryGraphics = MyMapView.GraphicsOverlays["BoundaryGraphics"];

			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
		}

		private void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			CreateGraphics();
		}

		// Setup graphic layers with test graphics and calculated boundaries of each
		private async void CreateGraphics()
		{
			try
			{
				await MyMapView.LayersLoadedAsync();

				CreateTestGraphics();
				CalculateBoundaries();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occured : " + ex.Message, "Boundary Sample");
			}
		}

		// Creates a two-part polygon and a four-part polyline to use as test graphics for the Boundary method
		private void CreateTestGraphics()
		{
			// Get current viewpoints extent from the MapView
			var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
			var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

			var center = viewpointExtent.GetCenter();
			var width = viewpointExtent.Width / 4;
			var left = new MapPoint(center.X - width, center.Y, MyMapView.SpatialReference);
			var right = new MapPoint(center.X + width, center.Y, MyMapView.SpatialReference);

			var fillSymbol = new SimpleFillSymbol() { Color = Colors.Red, Style = SimpleFillStyle.Solid };
			var lineSymbol = new SimpleLineSymbol() { Color = Colors.Red, Style = SimpleLineStyle.Solid, Width = 2 };

			_testGraphics.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(left, width), Symbol = fillSymbol });
			_testGraphics.Graphics.Add(new Graphic() { Geometry = CreatePolylineBox(right, width), Symbol = lineSymbol });
		}

		// Calculates the geometric boundaries for each test graphic
		private void CalculateBoundaries()
		{
			var lineSymbol = (Symbol)new SimpleLineSymbol() { 
				Color = Colors.Blue, 
				Style = SimpleLineStyle.Solid, 
				Width = 2 
			};

			var pointSymbol = (Symbol)new SimpleMarkerSymbol() { 
				Color = Colors.Blue, 
				Style = SimpleMarkerStyle.Circle, 
				Size = 12 
			};

			foreach (var testGraphic in _testGraphics.Graphics)
			{
				var boundary = GeometryEngine.Boundary(testGraphic.Geometry);
				var graphic = new Graphic(boundary, (boundary.GeometryType == GeometryType.Polyline) ? lineSymbol : pointSymbol);
				_boundaryGraphics.Graphics.Add(graphic);
			}
		}

		// Creates a square polygon with a hole centered at the given point
		private Polygon CreatePolygonBox(MapPoint center, double length)
		{
			var halfLen = length / 2.0;

			Geometry.PointCollection coords = new Geometry.PointCollection();
			coords.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));
			coords.Add(new MapPoint(center.X + halfLen, center.Y + halfLen));
			coords.Add(new MapPoint(center.X + halfLen, center.Y - halfLen));
			coords.Add(new MapPoint(center.X - halfLen, center.Y - halfLen));
			coords.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));

			halfLen /= 3;
			Geometry.PointCollection coordsHole = new Geometry.PointCollection();
			coordsHole.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));
			coordsHole.Add(new MapPoint(center.X - halfLen, center.Y - halfLen));
			coordsHole.Add(new MapPoint(center.X + halfLen, center.Y - halfLen));
			coordsHole.Add(new MapPoint(center.X + halfLen, center.Y + halfLen));
			coordsHole.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));

			return new Polygon(new List<Geometry.PointCollection> { coords, coordsHole }, MyMapView.SpatialReference);
		}

		// Creates a polyline with four paths in the shape of a box centered at the given point
		private Polyline CreatePolylineBox(MapPoint center, double length)
		{
			var halfLen = length / 2.0;
			var spacer = length / 10.0;

			PolylineBuilder polylineBuilder = new PolylineBuilder(MyMapView.SpatialReference);
			
			// First path
			polylineBuilder.AddPart(new Geometry.PointCollection()
				{
					new MapPoint(center.X - halfLen + spacer, center.Y + halfLen),
					new MapPoint(center.X + halfLen - spacer, center.Y + halfLen)
				});

			// Second path
			polylineBuilder.AddPart(new Geometry.PointCollection()
				{
					new MapPoint(center.X + halfLen, center.Y + halfLen - spacer),
					new MapPoint(center.X + halfLen, center.Y - halfLen + spacer)
				});

			// Third path
			polylineBuilder.AddPart(new Geometry.PointCollection()
				{
					new MapPoint(center.X + halfLen - spacer, center.Y - halfLen),
					new MapPoint(center.X - halfLen + spacer, center.Y - halfLen)
				});

			// Forth path
			polylineBuilder.AddPart(new Geometry.PointCollection()
				{
					new MapPoint(center.X - halfLen, center.Y - halfLen + spacer),
					new MapPoint(center.X - halfLen, center.Y + halfLen - spacer)
				});

			return polylineBuilder.ToGeometry();
		}
	}
}
