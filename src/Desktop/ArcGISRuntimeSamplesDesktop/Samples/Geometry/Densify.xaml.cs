using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates using GeometryEngine.Densify to take an input polygon and return a densified polygon. Original vertices to create the original polygon are shown in red. The returned polygon shows the additional densified vertices in green. To use the sample, click the 'Densify Polygon' button and create a polygon on the map. Double-click to end the polygon sketch and densify the polygon and see the original and densified vertices.
	/// </summary>
	/// <title>Densify</title>
	/// <category>Geometry</category>
	public partial class Densify : UserControl
	{
		private Symbol _polySymbol;
		private Symbol _origVertexSymbol;
		private Symbol _newVertexSymbol;

		private GraphicsOverlay _inputOverlay;
		private GraphicsOverlay _resultsOverlay;

		/// <summary>Construct Densify sample control</summary>
		public Densify()
		{
			InitializeComponent();

			_polySymbol = layoutGrid.Resources["PolySymbol"] as Symbol;
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
				_inputOverlay.Graphics.Clear();
				_resultsOverlay.Graphics.Clear();

				// Request polygon from the user
				var poly = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon, _polySymbol) as Polygon;

				// Add original polygon and vertices to input graphics layer
				_inputOverlay.Graphics.Add(new Graphic(poly, _polySymbol));
				foreach (var mapPoint in poly.Parts.First().GetPoints())
				{
					_inputOverlay.Graphics.Add(new Graphic(mapPoint, _origVertexSymbol));
				}

                // Get current viewpoints extent from the MapView
                var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				// Densify the polygon
				var densify = GeometryEngine.Densify(poly, viewpointExtent.Width / 100) as Polygon;

				// Add new vertices to result graphics layer
				foreach (var mapPoint in densify.Parts.First().GetPoints())
				{
					_resultsOverlay.Graphics.Add(new Graphic(mapPoint, _newVertexSymbol));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Densify Error: " + ex.Message, "Densify Geometry Sample");
			}
		}
	}
}
