using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Controls;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Shows how to handle the ExtentChanged event on the MapView and display extent properties at runtime.
	/// Since the Map is in wrap-around mode, absolute values represent the map extent on an infinite continuous grid where values on the X axis increase or decrease as you pan.
	/// Normalized values represent the map extent in the real-world and take the dateline (or current central meridian) into account.
	/// </summary>
	/// <title>Show Map Extent</title>
	/// <category>Mapping</category>
	public sealed partial class ShowMapExtent : Page
	{
		public ShowMapExtent()
		{
			this.InitializeComponent();

			MyMapView.ExtentChanged += MapView_ExtentChanged;
		}

		private void MapView_ExtentChanged(object sender, System.EventArgs e)
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
							if (Geometry.IsNullOrEmpty(newExtent) || p.X < newExtent.XMin || double.IsNaN(newExtent.XMin))
								newExtentBuilder.XMin = p.X;
							if (Geometry.IsNullOrEmpty(newExtent) || p.Y < newExtent.YMin || double.IsNaN(newExtent.YMin))
								newExtentBuilder.YMin = p.Y;
						}

						foreach (var p in normalizedPolygon.Parts[1].GetPoints())
						{
							if (Geometry.IsNullOrEmpty(newExtent) || p.X > newExtent.XMax || double.IsNaN(newExtent.XMax))
								newExtentBuilder.XMax = p.X;
							if (Geometry.IsNullOrEmpty(newExtent) || p.Y > newExtent.YMax || double.IsNaN(newExtent.YMax))
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

			MinXAbsolute.Text = viewpointExtent.XMin.ToString("0.000");
			MinYAbsolute.Text = viewpointExtent.YMin.ToString("0.000");
			MaxXAbsolute.Text = viewpointExtent.XMax.ToString("0.000");
			MaxYAbsolute.Text = viewpointExtent.YMax.ToString("0.000");
		}
	}
}
