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
    /// Example of using the GeometryEngine.Intersection method to intersect feature geometries with a given polygon.
    /// </summary>
    /// <title>Intersection</title>
    /// <category>Geometry</category>
    public partial class Intersection : Windows.UI.Xaml.Controls.Page
    {
        private const string GdbPath = @"maps\usa.geodatabase";

        private Symbol _fillSymbol;
        private FeatureLayer _statesLayer;
        private GraphicsOverlay _resultGraphics;

        /// <summary>Construct Intersection sample control</summary>
        public Intersection()
        {
            InitializeComponent();

            _fillSymbol = LayoutRoot.Resources["FillSymbol"] as Symbol;
			_resultGraphics = MyMapView.GraphicsOverlays["resultsOverlay"];
                
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

        // Intersects feature geometries with a user defined polygon.
        private async void IntersectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _resultGraphics.Graphics.Clear();

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

                _resultGraphics.Graphics.AddRange(intersectGraphics);
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Intersection Error: " + ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
