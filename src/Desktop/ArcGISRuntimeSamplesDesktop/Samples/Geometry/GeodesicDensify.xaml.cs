using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates using GeometryEngine.GeodesicDensify to take an input shape and return a geodesic densified shape. Original vertices to create the original polygon or polyline are shown in red. The returned polygon shows the additional densified vertices in green. To use the sample, click the 'Densify' button and create a shape on the map. Double-click to end the polygon sketch and densify the shape and see the original and densified vertices.
	/// </summary>
	/// <title>Geodesic Densify</title>
	/// <category>Geometry</category>
	public partial class GeodesicDensify : UserControl
	{
		private const double METERS_TO_MILES = 0.000621371192;
		private const double SQUARE_METERS_TO_MILES = 3.86102159e-7;

		private Symbol _lineSymbol;
		private Symbol _fillSymbol;
		private Symbol _origVertexSymbol;
		private Symbol _newVertexSymbol;

		private GraphicsOverlay _inputOverlay;
		private GraphicsOverlay _resultsOverlay;

		/// <summary>Construct Densify sample control</summary>
		public GeodesicDensify()
		{
			InitializeComponent();

			_lineSymbol = layoutGrid.Resources["LineSymbol"] as Symbol;
			_fillSymbol = layoutGrid.Resources["FillSymbol"] as Symbol;
			_origVertexSymbol = layoutGrid.Resources["OrigVertexSymbol"] as Symbol;
			_newVertexSymbol = layoutGrid.Resources["NewVertexSymbol"] as Symbol;

			_inputOverlay = MyMapView.GraphicsOverlays["inputOverlay"];
			_resultsOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];
		}

		// Draw and densify a user defined polygon
		private async void DensifyButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				resultsPanel.Visibility = Visibility.Collapsed;
				_inputOverlay.Graphics.Clear();
				_resultsOverlay.Graphics.Clear();

				// Request polygon or polyline from the user
				DrawShape drawShape = (DrawShape)comboShapeType.SelectedValue;

				// Use polyline as default
				Symbol symbolToUse = _lineSymbol;
				if (drawShape == DrawShape.Polygon)
					symbolToUse = _fillSymbol;

				var original = await MyMapView.Editor.RequestShapeAsync(drawShape, symbolToUse);

				// Account for WrapAround
				var normalized = GeometryEngine.NormalizeCentralMeridian(original);

				// Add original shape vertices to input graphics layer
				var coordsOriginal = (normalized as Multipart).Parts.First().GetPoints();
				foreach (var mapPoint in coordsOriginal)
					_inputOverlay.Graphics.Add(new Graphic(mapPoint, _origVertexSymbol));

                // Get current viewpoints extent from the MapView
                var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				// Densify the shape
				var densify = GeometryEngine.GeodesicDensify(normalized, viewpointExtent.Width / 100, LinearUnits.Meters);

				if (densify.GeometryType == GeometryType.Polygon)
					_inputOverlay.Graphics.Add(new Graphic(densify, _fillSymbol));
				else
					_inputOverlay.Graphics.Add(new Graphic(densify, _lineSymbol));

				// Add new vertices to result graphics layer
				var coordsDensify = (densify as Multipart).Parts.First().GetPoints();
				foreach (var mapPoint in coordsDensify)
					_resultsOverlay.Graphics.Add(new Graphic(mapPoint, _newVertexSymbol));

				// Results
				Dictionary<string, object> results = new Dictionary<string, object>();
				results["Length"] = GeometryEngine.GeodesicLength(densify) * METERS_TO_MILES;
				if (normalized is Polygon)
					results["Area"] = GeometryEngine.GeodesicArea(densify) * SQUARE_METERS_TO_MILES;
				else
					results["Area"] = "N/A";
				results["Vertices Before"] = coordsOriginal.Count();
				results["Vertices After"] = coordsDensify.Count();

				resultsListView.ItemsSource = results;
				resultsPanel.Visibility = Visibility.Visible;
			}
			catch (TaskCanceledException) { }
			catch (Exception ex)
			{
				MessageBox.Show("Densify Error: " + ex.Message, "Geodesic Densify Sample");
			}
		}
	}
}
