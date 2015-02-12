using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates using a Query Filter to filter features from a feature layer
    /// </summary>
    /// <title>Query Filter</title>
    /// <category>Feature Layers</category>
    public sealed partial class QueryFilterSample : Page
	{
        private FeatureLayer _featureLayer;
        private GraphicsOverlay _queryResultsOverlay;

        public QueryFilterSample()
		{
			this.InitializeComponent();

            _featureLayer = MyMapView.Map.Layers["FeatureLayer"] as FeatureLayer;
            ((ServiceFeatureTable)_featureLayer.FeatureTable).OutFields = OutFields.All;

			_queryResultsOverlay = MyMapView.GraphicsOverlays[0];
        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var features = await _featureLayer.FeatureTable.QueryAsync(new QueryFilter() { WhereClause = where.Text });
                _queryResultsOverlay.GraphicsSource = features.Select(f => new Graphic(f.Geometry));
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Query Error").ShowAsync();
            }
        }
    }
}
