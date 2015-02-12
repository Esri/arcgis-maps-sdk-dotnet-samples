using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates how to select and unselect features in a feature layer using the SelectFeatures, UnSelectFeatures and ClearSelecteion methods of the FeatureLayer class.  The sample uses the MapView.RequestShapeAsync method to allow the user to draw a selection rectangle intersecting features that he would like to manage.  Once the selection rectangle is returned, a SpatialQueryFilter is used to spatially select features in the feature layer using FeatureLayer.QueryAsync.  When the features are identified the selected features list of the feature layer is managed using the FeatureLayer selection management methods.
    /// </summary>
    /// <title>Feature Layer Selection</title>
	/// <category>Layers</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class FeatureLayerSelection : UserControl
    {
        /// <summary>Construct Feature Layer Selection sample control</summary>
        public FeatureLayerSelection()
        {
            InitializeComponent();
        }

        private async void AddSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var featureIds = await FindIntersectingFeaturesAsync();
                cities.SelectFeatures(featureIds);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selection Error: " + ex.Message, "Feature Layer Selection Sample");
            }
        }

        private async void RemoveSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var featureIds = await FindIntersectingFeaturesAsync();
                cities.UnselectFeatures(featureIds);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selection Error: " + ex.Message, "Feature Layer Selection Sample");
            }
        }

        private void ClearSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                cities.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selection Error: " + ex.Message, "Feature Layer Selection Sample");
            }
        }

        private async Task<long[]> FindIntersectingFeaturesAsync()
        {
			var rect = await MyMapView.Editor.RequestShapeAsync(DrawShape.Rectangle);

            SpatialQueryFilter filter = new SpatialQueryFilter();
            filter.Geometry = GeometryEngine.Project(rect, cities.FeatureTable.SpatialReference);
            filter.SpatialRelationship = SpatialRelationship.Intersects;
            filter.MaximumRows = 1000;
            var features = await cities.FeatureTable.QueryAsync(filter);

            return features
                .Select(f => Convert.ToInt64(f.Attributes[cities.FeatureTable.ObjectIDField]))
                .ToArray();
        }
    }
}
