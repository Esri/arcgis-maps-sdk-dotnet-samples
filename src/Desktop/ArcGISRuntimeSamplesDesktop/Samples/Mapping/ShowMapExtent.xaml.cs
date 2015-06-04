using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Controls;
using System;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample includes a Map and a ArcGISTiledMapServiceLayer and demonstrates how to handle the ExtentChanged event on the MapView.  The extent parameters of the map are displayed in a textblock as they change at runtime.  Since the Map is in wrap-around mode, absolute values represent the map extent on an infinite continuous grid where values on the X axis increase or decrease as you pan.  Normalized values represent the map extent in the real-world and take the dateline (or current central meridian) into account.
	/// </summary>
	/// <title>Show Map Extent</title>
	/// <category>Mapping</category>
	public partial class ShowMapExtent : UserControl
	{
		public ShowMapExtent()
		{
			InitializeComponent();
		}

		private void MapView_ExtentChanged(object sender, EventArgs e)
		{
			Envelope newExtent = null;

			// Get current viewpoints extent from the MapView
			var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
			var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

			if (MyMapView.WrapAround)
			{
				Geometry normalizedExtent = GeometryEngine.NormalizeCentralMeridian(viewpointExtent);
				if (normalizedExtent is Polygon)
				{
					var normalizedPolygon = (Polygon)normalizedExtent;

					if (normalizedPolygon.Parts.Count == 1)
						newExtent = normalizedPolygon.Extent;
					else
					{
						var newExtentBuilder = new EnvelopeBuilder(MyMapView.SpatialReference);

						foreach (var p in normalizedPolygon.Parts[0].GetPoints())
						{
							if (p.X < newExtentBuilder.XMin || double.IsNaN(newExtentBuilder.XMin))
								newExtentBuilder.XMin = p.X;
							if (p.Y < newExtentBuilder.YMin || double.IsNaN(newExtentBuilder.YMin))
								newExtentBuilder.YMin = p.Y;
						}

						foreach (var p in normalizedPolygon.Parts[1].GetPoints())
						{
							if (p.X > newExtentBuilder.XMax || double.IsNaN(newExtentBuilder.XMax))
								newExtentBuilder.XMax = p.X;
							if (p.Y > newExtentBuilder.YMax || double.IsNaN(newExtentBuilder.YMax))
								newExtentBuilder.YMax = p.Y;
						}
						newExtent = newExtentBuilder.ToGeometry();
					}
				}
				else if (normalizedExtent is Envelope)
					newExtent = normalizedExtent as Envelope;
			}
			else
				newExtent = viewpointExtent;

			MinXNormalized.Text = newExtent.XMin.ToString("0.000");
			MinYNormalized.Text = newExtent.YMin.ToString("0.000");
			MaxXNormalized.Text = newExtent.XMax.ToString("0.000");
			MaxYNormalized.Text = newExtent.YMax.ToString("0.000");
		}
	}
}
