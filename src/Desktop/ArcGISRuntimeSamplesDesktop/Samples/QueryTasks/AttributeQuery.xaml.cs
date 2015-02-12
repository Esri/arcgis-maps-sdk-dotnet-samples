using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates performing an attribute query, adding the results to the map and UI, and zooming to the resulting geometry. To use the sample, select a state from the drop-down menu. A QueryTask is used to query a map service layer based on the selected state name. The result is then added as a Graphic to the map and the attributes displayed in a list control.
    /// </summary>
    /// <title>Attribute Query</title>
	/// <category>Tasks</category>
	/// <subcategory>Query</subcategory>
	public partial class AttributeQuery : UserControl
    {
		private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Attribute Query sample control</summary>
        public AttributeQuery()
        {
            InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			InitializeComboBox();
        }

        // Load the combobox with state data from the map service
        private async void InitializeComboBox()
        {
            try
            {
                QueryTask queryTask = new QueryTask(
                    new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));
                Query query = new Query("1=1") { ReturnGeometry = false };
                query.OutFields.Add("STATE_NAME");

                var result = await queryTask.ExecuteAsync(query);

                QueryComboBox.ItemsSource = result.FeatureSet.Features.OrderBy(g => g.Attributes["STATE_NAME"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Attribute Query Sample");
            }
        }

        // Query the map service for the selected state information
        private async void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                resultsGrid.Visibility = Visibility.Collapsed;
                resultsGrid.ItemsSource = null;
				_graphicsOverlay.Graphics.Clear();

                QueryTask queryTask = new QueryTask(
                    new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));

                var stateName = (string)(QueryComboBox.SelectedItem as Graphic).Attributes["STATE_NAME"];
                Query query = new Query(string.Format("STATE_NAME = '{0}'", stateName))
                {
                    OutFields = OutFields.All,
                    ReturnGeometry = true,
                    OutSpatialReference = MyMapView.SpatialReference
                };

                var result = await queryTask.ExecuteAsync(query);

                var featureSet = result.FeatureSet;
                if (featureSet != null && featureSet.Features.Count > 0)
                {
                    var graphic = featureSet.Features.First();
					_graphicsOverlay.Graphics.Add(graphic as Graphic);

                    var selectedFeatureExtent = graphic.Geometry.Extent;
                    Envelope displayExtent = selectedFeatureExtent.Expand(1.3);
                    MyMapView.SetView(displayExtent);

                    resultsGrid.ItemsSource = graphic.Attributes;
                    resultsGrid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Attribute Query Sample");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
