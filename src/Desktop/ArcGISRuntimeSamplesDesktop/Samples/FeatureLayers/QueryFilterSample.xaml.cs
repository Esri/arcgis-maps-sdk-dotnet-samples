using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates how to apply a filter to a feature layer using the FeatureLayer.Where property and to further filter the result set using a QueryFilter. The FeatureLayer.Where property allows you to retrieve a subset of records from a feature service that match an attribute query.  The ArcGISFeatureTable.QueryAsync method takes a QueryFilter object and uses it to filter the current feature set.  In this example, the features returned by the query are converted to graphics and displayed in a GraphicsOverlay on the map.
    /// </summary>
    /// <title>Query Filter</title>
	/// <category>Layers</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class QueryFilterSample : UserControl
    {
        /// <summary>Construct QueryFilterSample control</summary>
        public QueryFilterSample()
        {
            InitializeComponent();
        }

        private async void QueryButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var features = await cities.FeatureTable.QueryAsync(new QueryFilter() { WhereClause = where.Text});
				MyMapView.GraphicsOverlays["queryResults"].GraphicsSource = features.Select(f => new Graphic(f.Geometry));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Query Error: " + ex.Message, "Query Filter Sample");
            }
        }
    }
}
