using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
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
    /// Demonstrates performing an attribute query, adding the results to the map and UI, and zooming to the resulting geometry.
    /// </summary>
    /// <title>Attribute Query</title>
    /// <category>Query Tasks</category>
    public sealed partial class AttributeQuery : Page
    {
        private GraphicsOverlay _graphicsOverlay;

        public AttributeQuery()
        {
            this.InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
            InitializeComboBox();
        }

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
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                results.Visibility = Visibility.Collapsed;
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
                    var graphic = featureSet.Features.First() as Graphic;
                    _graphicsOverlay.Graphics.Add(graphic);

                    var selectedFeatureExtent = graphic.Geometry.Extent;
                    Envelope displayExtent = selectedFeatureExtent.Expand(1.3);
                    MyMapView.SetView(displayExtent);

                    resultsGrid.ItemsSource = graphic.Attributes
                        .Select(attr => new Tuple<string, string>(attr.Key, attr.Value.ToString()));
                    results.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}