using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Example of using the GeometryEngine.Clip method to clip feature geometries with a given envelope.
    /// </summary>
    /// <title>Clip</title>
	/// <category>Geometry</category>
	public partial class ClipGeometry : Windows.UI.Xaml.Controls.Page
    {
        private const string GdbPath = @"maps\usa.geodatabase";

        private GraphicsOverlay _clippedGraphicsOverlay;
        private Symbol _clipSymbol;
        private FeatureLayer _statesLayer;

        /// <summary>Construct Clip Geometry sample control</summary>S
        public ClipGeometry()
        {
            InitializeComponent();

			_clippedGraphicsOverlay = MyMapView.GraphicsOverlays["clippedGraphicsOverlay"];
            _clipSymbol = layoutGrid.Resources["ClipRectSymbol"] as Symbol;

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
                var _x = new MessageDialog("Error creating feature layer: " + ex.Message, "Clip Geometry").ShowAsync();
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
                var _x = new MessageDialog("Clip Error: " + ex.Message, "Clip Geometry").ShowAsync();
            }
        }
    }
}
