using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Example of using the GeometryEngine.Clip method to clip feature geometries with a given envelope. To use this sample, the user draws a clipping rectangle over the feature polygons and the system then clips the intersecting feature geometries and displays the resulting polygons in a graphics layer on the map.
    /// </summary>
    /// <title>Clip</title>
	/// <category>Geometry</category>
	public partial class ClipGeometry : UserControl
    {
        private const string GDB_PATH = @"..\..\..\samples-data\maps\usa.geodatabase";

        private Symbol _clipSymbol;
        private FeatureLayer _statesLayer;

		private GraphicsOverlay _clippedGraphicsOverlay;

        /// <summary>Construct Clip Geometry sample control</summary>
        public ClipGeometry()
        {
            InitializeComponent();

            _clipSymbol = layoutGrid.Resources["ClipRectSymbol"] as Symbol;
			_clippedGraphicsOverlay = MyMapView.GraphicsOverlays["clippedGraphicsOverlay"];
            CreateFeatureLayers();
        }

        // Creates a feature layer from a local .geodatabase file
		private async void CreateFeatureLayers()
        {
            try
            {
                var gdb = await Geodatabase.OpenAsync(GDB_PATH);

                var table = gdb.FeatureTables.First(ft => ft.Name == "US-States");
                _statesLayer = new FeatureLayer() { ID = table.Name, FeatureTable = table };
                MyMapView.Map.Layers.Insert(1, _statesLayer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating feature layer: " + ex.Message, "Samples");
            }
        }

        // Clips feature geometries with a user defined clipping rectangle.
        private async void ClipButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				_clippedGraphicsOverlay.Graphics.Clear();

                // wait for user to draw clip rect
                var rect = await MyMapView.Editor.RequestShapeAsync(DrawShape.Rectangle);

				Polygon polygon = GeometryEngine.NormalizeCentralMeridian(rect) as Polygon;

                // get intersecting features from the feature layer
                SpatialQueryFilter filter = new SpatialQueryFilter();
				filter.Geometry = GeometryEngine.Project(polygon, _statesLayer.FeatureTable.SpatialReference);
                filter.SpatialRelationship = SpatialRelationship.Intersects;
                filter.MaximumRows = 52;
                var stateFeatures = await _statesLayer.FeatureTable.QueryAsync(filter);

                // Clip the feature geometries and add to graphics layer
                var states = stateFeatures.Select(feature => feature.Geometry);
                var clipGraphics = states
					.Select(state => GeometryEngine.Clip(state, polygon.Extent))
                    .Select(geo => new Graphic(geo, _clipSymbol));

				_clippedGraphicsOverlay.Graphics.AddRange(clipGraphics);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show("Clip Error: " + ex.Message, "Clip Geometry");
            }
        }
    }
}
