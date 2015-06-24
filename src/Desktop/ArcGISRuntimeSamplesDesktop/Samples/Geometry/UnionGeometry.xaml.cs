using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
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
    /// Example of using the GeometryEngine.Union method to calculate the geometric union of feature geometries and a given polygon. To use this sample, the user draws a polygon over the feature polygons and the system then retrieves the union of the feature geometries and the new polygon. Resulting polygons are shown in a graphics overlay on the map.
    /// </summary>
    /// <title>Union</title>
    /// <category>Geometry</category>
    public partial class UnionGeometry : UserControl
    {
        private const string GDB_PATH = @"..\..\..\samples-data\maps\usa.geodatabase";

        private Symbol _fillSymbol;
        private FeatureLayer _statesLayer;
        private GraphicsOverlay _resultGraphics;

        /// <summary>Construct Union sample control</summary>
        public UnionGeometry()
        {
            InitializeComponent();

            _fillSymbol = layoutGrid.Resources["FillSymbol"] as Symbol;
            _resultGraphics = MyMapView.GraphicsOverlays["resultsOverlay"];

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

        // Unions feature geometries with a user defined polygon.
        private async void UnionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _resultGraphics.Graphics.Clear();

                // wait for user to draw a polygon
                var poly = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon);

                // Take account of WrapAround
                var normalizedPoly = GeometryEngine.NormalizeCentralMeridian(poly) as Polygon;

                // get intersecting features from the feature layer
                SpatialQueryFilter filter = new SpatialQueryFilter();
                filter.Geometry = GeometryEngine.Project(normalizedPoly, _statesLayer.FeatureTable.SpatialReference);
                filter.SpatialRelationship = SpatialRelationship.Intersects;
                filter.MaximumRows = 52;
                var stateFeatures = await _statesLayer.FeatureTable.QueryAsync(filter);

                // Union the geometries and add to graphics layer
                var states = stateFeatures.Select(feature => feature.Geometry);
                var unionPolys = states.ToList();


                var unionPoly = GeometryEngine.Union(unionPolys);
                var unionGraphic = new Graphic(unionPoly, _fillSymbol);

                _resultGraphics.Graphics.Add(unionGraphic);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Union Error: " + ex.Message, "Union Sample");
            }
        }
    }
}
