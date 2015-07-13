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
    /// Example of using the GeometryEngine.Intersection method to intersect feature geometries with a given polygon. To use this sample, the user draws a polygon over the feature polygons and the system then retrieves the intersection of the feature geometries and the new polygon. Resulting polygons are shown in a graphics layer on the map.
    /// </summary>
    /// <title>Intersection</title>
	/// <category>Geometry</category>
	public partial class Intersection : UserControl
    {
        private const string GDB_PATH = @"..\..\..\samples-data\maps\usa.geodatabase";

        private Symbol _fillSymbol;
        private FeatureLayer _statesLayer;
		private GraphicsOverlay _resultsOverlay;

        /// <summary>Construct Intersection sample control</summary>
        public Intersection()
        {
            InitializeComponent();

            _fillSymbol = layoutGrid.Resources["FillSymbol"] as Symbol;
			_resultsOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];

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

        // Intersects feature geometries with a user defined polygon.
        private async void IntersectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				_resultsOverlay.Graphics.Clear();

                // wait for user to draw a polygon
                Polygon userpoly = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon) as Polygon;

				Polygon poly = GeometryEngine.NormalizeCentralMeridian(userpoly) as Polygon;

                // get intersecting features from the feature layer
                SpatialQueryFilter filter = new SpatialQueryFilter();
                filter.Geometry = GeometryEngine.Project(poly, _statesLayer.FeatureTable.SpatialReference);
                filter.SpatialRelationship = SpatialRelationship.Intersects;
                filter.MaximumRows = 52;
                var stateFeatures = await _statesLayer.FeatureTable.QueryAsync(filter);

                // Intersect the feature geometries and add to graphics layer
                var states = stateFeatures.Select(feature => feature.Geometry);
                var intersectGraphics = states
                    .Select(state => GeometryEngine.Intersection(state, poly))
                    .Select(geo => new Graphic(geo, _fillSymbol));

				_resultsOverlay.Graphics.AddRange(intersectGraphics);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Intersection Error: " + ex.Message, "Intersection Sample");
            }
        }
    }
}
