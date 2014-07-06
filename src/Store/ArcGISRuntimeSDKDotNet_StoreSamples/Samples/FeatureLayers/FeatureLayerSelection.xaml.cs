using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates feature selection functionality in a Feature Layer
    /// </summary>
    /// <title>Feature Layer Selection</title>
    /// <category>Feature Layers</category>
    public sealed partial class FeatureLayerSelection : Page
	{
        private FeatureLayer _featureLayer;

        public FeatureLayerSelection()
		{
			this.InitializeComponent();

            mapView.Map.InitialViewpoint = new Envelope(-14675766.3566695, 2695407.73380258, -6733121.86117095, 6583994.1013904);

            _featureLayer = mapView.Map.Layers["FeatureLayer"] as FeatureLayer;
            ((GeodatabaseFeatureServiceTable)_featureLayer.FeatureTable).OutFields = OutFields.All;

            panelPrompt.DataContext = mapView;
            SetSelectionCountUI();
        }

        private async void AddSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var featureIds = await FindIntersectingFeaturesAsync();
                _featureLayer.SelectFeatures(featureIds);
                SetSelectionCountUI();
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Selection Error: " + ex.Message, "Feature Layer Selection Sample").ShowAsync();
            }
        }

        private async void RemoveSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var featureIds = await FindIntersectingFeaturesAsync();
                _featureLayer.UnselectFeatures(featureIds);
                SetSelectionCountUI();
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Selection Error: " + ex.Message, "Feature Layer Selection Sample").ShowAsync();
            }
        }

        private void ClearSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _featureLayer.ClearSelection();
                SetSelectionCountUI();
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Selection Error: " + ex.Message, "Feature Layer Selection Sample").ShowAsync();
            }
        }

        private async Task<long[]> FindIntersectingFeaturesAsync()
        {
            var rect = await mapView.Editor.RequestShapeAsync(DrawShape.Rectangle);

            SpatialQueryFilter filter = new SpatialQueryFilter();
            filter.Geometry = GeometryEngine.Project(rect, _featureLayer.FeatureTable.SpatialReference);
            filter.SpatialRelationship = SpatialRelationship.Intersects;
            filter.MaximumRows = 1000;
            var features = await _featureLayer.FeatureTable.QueryAsync(filter);

            return features
                .Select(f => Convert.ToInt64(f.Attributes[_featureLayer.FeatureTable.ObjectIDField]))
                .ToArray();
        }

        private void SetSelectionCountUI()
        {
            txtSelectionCount.Text = string.Format("Selection Count: {0}", _featureLayer.SelectedFeatureIDs.Count());
        }
    }
}
