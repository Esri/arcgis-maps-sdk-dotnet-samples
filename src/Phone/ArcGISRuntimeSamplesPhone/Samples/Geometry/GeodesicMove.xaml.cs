using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates use of the GeometryEngine.GeodesicMove method to move a geometry be a specified distance.
	/// </summary>
	/// <title>Geodesic Move</title>
	/// <category>Geometry</category>
	public partial class GeodesicMove : Windows.UI.Xaml.Controls.Page
	{
		private Symbol _origSymbol;
		private GraphicsOverlay _originalOverlay;
		private GraphicsOverlay _movedOverlay;

		/// <summary>Construct Geodesic Move sample control</summary>
		public GeodesicMove()
		{
			InitializeComponent();

			_origSymbol = LayoutRoot.Resources["OriginalSymbol"] as Symbol;
			_originalOverlay = MyMapView.GraphicsOverlays["originalOverlay"];
			_movedOverlay = MyMapView.GraphicsOverlays["movedOverlay"];

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		// Start map interaction once the mapview extent is set
		private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;

			MyMapView.Editor.EditorConfiguration.MidVertexSymbol = null;
			MyMapView.Editor.EditorConfiguration.VertexSymbol = null;
			MyMapView.Editor.EditorConfiguration.SelectedVertexSymbol = new SimpleMarkerSymbol() { Color = Colors.Blue, Size = 6 };

			await AcceptUserPolygonAsync();
		}

		// Digitize a new original polygon to move
		private async void DigitizeButton_Click(object sender, RoutedEventArgs e)
		{
			await AcceptUserPolygonAsync();
		}

		// Get the polygon from the user
		private async Task AcceptUserPolygonAsync()
		{
			try
			{
				_movedOverlay.Graphics.Clear();
				_originalOverlay.Graphics.Clear();

				var polygon = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon, _origSymbol);
        
        //to accomodate for wraparound, otherwise movedpoints returned from geodesicmove are NaN
        polygon = GeometryEngine.NormalizeCentralMeridian(polygon);
				_originalOverlay.Graphics.Add(new Graphic(polygon));
			}
			catch (TaskCanceledException) { }
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Move the polygon the specified distance and angle
		private void GeodesicMoveButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (_originalOverlay.Graphics.Count == 0)
					throw new Exception("Digitize a polygon to move.");

				var coords = _originalOverlay.Graphics[0].Geometry as Multipart;
				if (coords == null)
					throw new Exception("Digitize a polygon to move.");

				var points = coords.Parts.First().GetPoints();
				var distance = (double)comboDistance.SelectedItem;
				var azimuth = (double)sliderAngle.Value;
				var movedPoints = GeometryEngine.GeodesicMove(points, distance, LinearUnits.Miles, azimuth);

				Polygon movedPoly = new PolygonBuilder(movedPoints, MyMapView.SpatialReference).ToGeometry();
				_movedOverlay.Graphics.Clear();
				_movedOverlay.Graphics.Add(new Graphic(movedPoly));
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}
	}
}
