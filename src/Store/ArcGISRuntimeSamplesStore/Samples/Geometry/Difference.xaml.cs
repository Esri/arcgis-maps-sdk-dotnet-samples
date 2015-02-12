using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Example of using the GeometryEngine.Difference or GeometryEngine.SymmetricDifference methods to calculate the geometric difference between feature geometries and a user defined geometry.
    /// </summary>
    /// <title>Difference</title>
    /// <category>Geometry</category>
    public partial class Difference : Windows.UI.Xaml.Controls.Page
    {
        private const string GdbPath = @"maps\usa.geodatabase";

        private Symbol _fillSymbol;
        private FeatureLayer _statesLayer;
        private GraphicsOverlay _differenceGraphics;

        /// <summary>Construct Difference sample control</summary>
        public Difference()
        {
            InitializeComponent();

            _fillSymbol = LayoutRoot.Resources["FillSymbol"] as Symbol;
			_differenceGraphics = MyMapView.GraphicsOverlays["resultsOverlay"];
            CreateFeatureLayers();
        }

        // Creates a feature layer from a local .geodatabase file
		private async void CreateFeatureLayers()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(GdbPath);
                if (file == null)
                    throw new Exception("Local geodatabase not found. Please download sample data from 'Sample Data Settings'");

                var gdb = await Geodatabase.OpenAsync(file.Path);
                var table = gdb.FeatureTables.First(ft => ft.Name == "US-States");
                _statesLayer = new FeatureLayer() { ID = table.Name, FeatureTable = table };
                MyMapView.Map.Layers.Insert(1, _statesLayer);
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Error creating feature layer: " + ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Calculates a geometric difference between features and user defined geometry
        private async void DifferenceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _differenceGraphics.Graphics.Clear();

				// wait for user to draw difference polygon
				Polygon userpoly = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon) as Polygon;

				// Take account of WrapAround
				Polygon poly = GeometryEngine.NormalizeCentralMeridian(userpoly) as Polygon;

				// Adjust user polygon for backward digitization
				poly = GeometryEngine.Simplify(poly) as Polygon;

                // get intersecting features from the feature layer
                SpatialQueryFilter filter = new SpatialQueryFilter();
                filter.Geometry = GeometryEngine.Project(poly, _statesLayer.FeatureTable.SpatialReference);
                filter.SpatialRelationship = SpatialRelationship.Intersects;
                filter.MaximumRows = 52;
                var stateFeatures = await _statesLayer.FeatureTable.QueryAsync(filter);

                // Calc difference between feature geometries and user polygon and add results to graphics layer
                var states = stateFeatures.Select(feature => feature.Geometry);

                var diffGraphics = states
                    .Select(state => ((bool)useSymmetricDifference.IsChecked)
                        ? GeometryEngine.SymmetricDifference(state, poly)
                        : GeometryEngine.Difference(state, poly))
                    .Select(geo => new Graphic(geo, _fillSymbol));

                _differenceGraphics.Graphics.AddRange(diffGraphics);
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Difference Error: " + ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
