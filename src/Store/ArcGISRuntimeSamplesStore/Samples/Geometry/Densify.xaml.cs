using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates using GeometryEngine.Densify to take an input polygon and return a densified polygon.
	/// </summary>
	/// <title>Densify</title>
	/// <category>Geometry</category>
	public partial class Densify : Windows.UI.Xaml.Controls.Page
	{
		private Symbol _polySymbol;
		private Symbol _origVertexSymbol;
		private Symbol _newVertexSymbol;

		private GraphicsOverlay _inputOverlay;
		private GraphicsOverlay _resultOverlay;

		/// <summary>Construct Densify sample control</summary>
		public Densify()
		{
			InitializeComponent();

			_polySymbol = LayoutRoot.Resources["PolySymbol"] as Symbol;
			_origVertexSymbol = LayoutRoot.Resources["OrigVertexSymbol"] as Symbol;
			_newVertexSymbol = LayoutRoot.Resources["NewVertexSymbol"] as Symbol;

			_inputOverlay = MyMapView.GraphicsOverlays["inputOverlay"];
			_resultOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];
		}

		// Draw and densify a user defined polygon
		private async void DensifyButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_inputOverlay.Graphics.Clear();
				_resultOverlay.Graphics.Clear();

				// Request polygon from the user
				var poly = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon, _polySymbol) as Polygon;

				// Add original polygon and vertices to input graphics layer
				_inputOverlay.Graphics.Add(new Graphic(poly, _polySymbol));
				foreach (var coord in poly.Parts.First().GetPoints())
				{
					_inputOverlay.Graphics.Add(new Graphic(coord, _origVertexSymbol));
				}

                // Get current viewpoints extent from the MapView
                var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				// Densify the polygon
                var densify = GeometryEngine.Densify(poly, viewpointExtent.Width / 100) as Polygon;

				// Add new vertices to result graphics layer
				foreach (var coord in densify.Parts.First().GetPoints())
				{
					_resultOverlay.Graphics.Add(new Graphic(coord, _newVertexSymbol));
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Densify Error: " + ex.Message, "Sample Error").ShowAsync();
			}
		}
	}
}
