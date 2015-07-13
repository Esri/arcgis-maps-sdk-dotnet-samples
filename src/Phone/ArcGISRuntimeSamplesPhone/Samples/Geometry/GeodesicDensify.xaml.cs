using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates using GeometryEngine.GeodesicDensify to take an input shape and return a geodesic densified shape.
	/// </summary>
	/// <title>Geodesic Densify</title>
	/// <category>Geometry</category>
	public partial class GeodesicDensify : Windows.UI.Xaml.Controls.Page
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

			_lineSymbol = LayoutRoot.Resources["LineSymbol"] as Symbol;
			_fillSymbol = LayoutRoot.Resources["FillSymbol"] as Symbol;
			_origVertexSymbol = LayoutRoot.Resources["OrigVertexSymbol"] as Symbol;
			_newVertexSymbol = LayoutRoot.Resources["NewVertexSymbol"] as Symbol;

			_inputOverlay = MyMapView.GraphicsOverlays["inputOverlay"];
			_resultsOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];
		}

		// Draw and densify a user defined polygon
		private async void DensifyButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DensifyButton.IsEnabled = false;
				resultsPanel.Visibility = Visibility.Collapsed;
				_inputOverlay.Graphics.Clear();
				_resultsOverlay.Graphics.Clear();

				DrawShape drawShape;

				drawShape = (RadioPolyline.IsChecked.Value) ? drawShape = DrawShape.Polyline : drawShape = DrawShape.Polygon;

				// Use polyline as default
				Symbol symbolToUse = _lineSymbol;
				if (drawShape == DrawShape.Polygon)
					symbolToUse = _fillSymbol;

				var original = await MyMapView.Editor.RequestShapeAsync(drawShape, symbolToUse);

				// Account for WrapAround
				var normalized = GeometryEngine.NormalizeCentralMeridian(original);

				// Add original shape vertices to input graphics layer
				var coordsOriginal = (normalized as Multipart).Parts.First().GetPoints();
				foreach (var coord in coordsOriginal)
					_inputOverlay.Graphics.Add(new Graphic(coord, _origVertexSymbol));

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
				foreach (var coord in coordsDensify)
					_resultsOverlay.Graphics.Add(new Graphic(coord, _newVertexSymbol));

				// Results
				var results = new List<Tuple<string, object>>()
				{
					new Tuple<string, object>("Length", GeometryEngine.GeodesicLength(densify) * METERS_TO_MILES),
					new Tuple<string, object>("Area", 
						(normalized is Polygon) ? (GeometryEngine.GeodesicArea(densify) * SQUARE_METERS_TO_MILES).ToString("0.000") : "N/A"),
					new Tuple<string, object>("Vertices Before", coordsOriginal.Count()),
					new Tuple<string, object>("Vertices After", coordsDensify.Count())
				};

				resultsListView.ItemsSource = results;
				resultsPanel.Visibility = Visibility.Visible;
			}
			catch (TaskCanceledException) { }
			catch (Exception ex)
			{
				var _x = new MessageDialog("Densify Error: " + ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				DensifyButton.IsEnabled = true;
			}
		}
	}
}
