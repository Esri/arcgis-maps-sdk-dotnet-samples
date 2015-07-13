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
    /// Example of using the GeometryEngine.Cut method to cut feature geometries with a given polyline. To use this sample, the user draws a cut polyline intersecting the feature polygons and the system then cuts the intersecting feature geometries and displays the resulting polygons in a graphics overlay on the map.
    /// </summary>
    /// <title>Cut</title>
	/// <category>Geometry</category>
	public partial class CutGeometry : UserControl
    {
        private const string GDB_PATH = @"..\..\..\samples-data\maps\usa.geodatabase";

        private Symbol _cutLineSymbol;
        private Symbol _cutFillSymbol;
        private FeatureLayer _statesLayer;
		private GraphicsOverlay _resultGraphicsOverlay;

        /// <summary>Construct Cut Geometry sample control</summary>
        public CutGeometry()
        {
            InitializeComponent();

            _cutLineSymbol = layoutGrid.Resources["CutLineSymbol"] as Symbol;
            _cutFillSymbol = layoutGrid.Resources["CutFillSymbol"] as Symbol;
			_resultGraphicsOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];

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
                MyMapView.Map.Layers.Add(_statesLayer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating feature layer: " + ex.Message, "Samples");
            }
        }

        // Cuts feature geometries with a user defined cut polyline.
        private async void CutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				_resultGraphicsOverlay.Graphics.Clear();

                // wait for user to draw cut line
                var cutLine = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polyline, _cutLineSymbol) as Polyline;

				Polyline polyline = GeometryEngine.NormalizeCentralMeridian(cutLine) as Polyline;

                // get intersecting features from the feature layer
                SpatialQueryFilter filter = new SpatialQueryFilter();
                filter.Geometry = GeometryEngine.Project(polyline, _statesLayer.FeatureTable.SpatialReference);
                filter.SpatialRelationship = SpatialRelationship.Crosses;
                filter.MaximumRows = 52;
                var stateFeatures = await _statesLayer.FeatureTable.QueryAsync(filter);

                // Cut the feature geometries and add to graphics layer
                var states = stateFeatures.Select(feature => feature.Geometry);
                var cutGraphics = states
                    .Where(geo => !GeometryEngine.Within(polyline, geo))
                    .SelectMany(state => GeometryEngine.Cut(state, polyline))
                    .Select(geo => new Graphic(geo, _cutFillSymbol));

				_resultGraphicsOverlay.Graphics.AddRange(cutGraphics);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show("Cut Error: " + ex.Message, "Cut Geometry");
            }
        }
    }
}
