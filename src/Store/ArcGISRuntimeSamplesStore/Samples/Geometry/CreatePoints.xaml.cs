using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates how to create point geometries, attach them to graphics and display them on the map.
	/// MapPoint geometry objects are used to store geographic points.
	/// </summary>
	/// <title>Create Points</title>
	/// <category>Geometry</category>
	public partial class CreatePoints : Page
	{
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct Create Points sample control</summary>
		public CreatePoints()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
		}

		// Create four point graphics on the map in the center of four equal quadrants
		private void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			try
			{
				// Get current viewpoints extent from the MapView
				var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
				var viewpointExtent = currentViewpoint.TargetGeometry.Extent;
				var height = viewpointExtent.Height / 4;
				var width = viewpointExtent.Width / 4;
				var center = viewpointExtent.GetCenter();

				var topLeft = new MapPoint(center.X - width, center.Y + height, MyMapView.SpatialReference);
				var topRight = new MapPoint(center.X + width, center.Y + height, MyMapView.SpatialReference);
				var bottomLeft = new MapPoint(center.X - width, center.Y - height, MyMapView.SpatialReference);
				var bottomRight = new MapPoint(center.X + width, center.Y - height, MyMapView.SpatialReference);

				var symbol = new SimpleMarkerSymbol() { Color = Colors.Red, Size = 15, Style = SimpleMarkerStyle.Diamond };

				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = topLeft, Symbol = symbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = topRight, Symbol = symbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = bottomLeft, Symbol = symbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = bottomRight, Symbol = symbol });

				_graphicsOverlay.Graphics.Add(new Graphic()
				{
					Geometry = new MapPoint(0, 0),
					Symbol = new SimpleMarkerSymbol() { Size = 15, Color = Colors.Blue }
				});
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Error occurred : " + ex.Message, "Sample Error").ShowAsync();
			}
		}
	}
}
