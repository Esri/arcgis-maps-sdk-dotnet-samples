using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates use of the GeometryEngine.GeodesicEllipse to calculate a geodesic ellipse. 
	/// Also shows how to calculate a geodesic sector using GeometryEngine.GeodesicSector to create a geodesic sector emanating from point.
	/// </summary>
	/// <title>Geodesic Ellipse</title>
	/// <category>Geometry</category>
	public partial class GeodesicEllipse : Windows.UI.Xaml.Controls.Page
	{
		private Symbol _pinSymbol;
		private Symbol _sectorSymbol;
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct Geodesic Ellipse sample control</summary>
		public GeodesicEllipse()
		{
			InitializeComponent();

			_pinSymbol = LayoutRoot.Resources["PointSymbol"] as Symbol;
			_sectorSymbol = LayoutRoot.Resources["SectorSymbol"] as Symbol;
			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;

			await DrawEllipse();
		}

		private async Task DrawEllipse()
		{
			try
			{
				while (MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent != null)
				{
					// Accept user point
					MapPoint userpoint = await MyMapView.Editor.RequestPointAsync() as MapPoint;

					MapPoint point = GeometryEngine.NormalizeCentralMeridian(userpoint) as MapPoint;

					// create the geodesic ellipse
					var radius1 = (double)comboRadius1.SelectedItem;
					var radius2 = (double)comboRadius2.SelectedItem;
					var axis = sliderAxis.Value;
					var maxLength = (double)comboSegmentLength.SelectedItem;
					var param = new GeodesicEllipseParameters(point, radius1, radius2, LinearUnits.Miles)
					{
						AxisDirection = axis,
						MaxPointCount = 10000,
						MaxSegmentLength = maxLength
					};
					var ellipse = GeometryEngine.GeodesicEllipse(param);

					//show geometries on map
					_graphicsOverlay.Graphics.Clear();
					_graphicsOverlay.Graphics.Add(new Graphic(point, _pinSymbol));
					_graphicsOverlay.Graphics.Add(new Graphic(ellipse));

					// geodesic sector
					if ((bool)chkSector.IsChecked)
					{
						var sectorParams = new GeodesicSectorParameters(point, radius1, radius2, LinearUnits.Miles)
						{
							AxisDirection = axis,
							MaxPointCount = 10000,
							MaxSegmentLength = maxLength,
							SectorAngle = sliderSectorAxis.Value,
							StartDirection = sliderSectorStart.Value
						};
						var sector = GeometryEngine.GeodesicSector(sectorParams);

						_graphicsOverlay.Graphics.Add(new Graphic(sector, _sectorSymbol));
					}
				}
			}
			catch (TaskCanceledException) { }
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Error").ShowAsync();
			}
		}
	}
}
