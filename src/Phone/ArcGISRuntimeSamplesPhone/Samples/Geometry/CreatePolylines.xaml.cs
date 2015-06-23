﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to create polyline geometries, attach them to graphics, and display them on the map.
	/// Polyline geometry objects are used to store geographic lines.
	/// </summary>
	/// <title>Create Polylines</title>
	/// <category>Geometry</category>
	public partial class CreatePolylines : Page
	{
		private GraphicsOverlay _graphicsOverlay;

		public CreatePolylines()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		// Create polyline graphics on the map in the center and the center of four equal quadrants
		void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;

			try
			{
				var height = MyMapView.Extent.Height / 4;
				var width = MyMapView.Extent.Width / 4;
				var length = width / 4;
				var center = MyMapView.Extent.GetCenter();
				var topLeft = new MapPoint(center.X - width, center.Y + height, MyMapView.SpatialReference);
				var topRight = new MapPoint(center.X + width, center.Y + height, MyMapView.SpatialReference);
				var bottomLeft = new MapPoint(center.X - width, center.Y - height, MyMapView.SpatialReference);
				var bottomRight = new MapPoint(center.X + width, center.Y - height, MyMapView.SpatialReference);

				var redSymbol = new SimpleLineSymbol() { Color = Colors.Red, Width = 4, Style = SimpleLineStyle.Solid };
				var blueSymbol = new SimpleLineSymbol() { Color = Colors.Blue, Width = 4, Style = SimpleLineStyle.Solid };

				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(center, length), Symbol = blueSymbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(topLeft, length), Symbol = redSymbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(topRight, length), Symbol = redSymbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(bottomLeft, length), Symbol = redSymbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolylineX(bottomRight, length), Symbol = redSymbol });
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Error occurred : " + ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Creates a polyline with two paths in the shape of an 'X' centered at the given point
		private Polyline CreatePolylineX(MapPoint center, double length)
		{
			var halfLen = length / 2.0;

			PointCollection coords1 = new PointCollection();
			coords1.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));
			coords1.Add(new MapPoint(center.X + halfLen, center.Y - halfLen));

			PointCollection coords2 = new PointCollection();
			coords2.Add(new MapPoint(center.X + halfLen, center.Y + halfLen));
			coords2.Add(new MapPoint(center.X - halfLen, center.Y - halfLen));

			return new Polyline(new List<PointCollection> { coords1, coords2 }, MyMapView.SpatialReference);
		}
	}
}
