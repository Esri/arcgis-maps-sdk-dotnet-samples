using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates how to use a QueryTask to get statistics from a map service. A statistics query is executed against a US states map service that gets statistics about population in sub-regions. This is combined with another query that retrieves all state features and the result is added to a graphicslayer and a tabular list view.
    /// </summary>
    /// <title>Statistics</title>
	/// <category>Tasks</category>
	/// <subcategory>Query</subcategory>
	public partial class Statistics : UserControl
    {
        private const string LAYER_URL = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2";

		private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Statistics sample control</summary>
        public Statistics()
        {
            InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];

			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
        }

		private async void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			await SetupSymbology();
			await RunQuery();
		}

        // Create a unique value renderer by state sub-region name
		private async Task SetupSymbology()
        {
            try
            {
                // Generate a unique value renderer on the server
                GenerateRendererTask generateRendererTask = new GenerateRendererTask(new Uri(LAYER_URL));

                UniqueValueDefinition uvDef = new UniqueValueDefinition() { Fields = new List<string> { "sub_region" } };
                uvDef.ColorRamps.Add(new ColorRamp() { From = Colors.Purple, To = Colors.Yellow, Algorithm = Algorithm.LabLch });
                GenerateRendererParameters rendererParams = new GenerateRendererParameters() { ClassificationDefinition = uvDef };

                var rendererResult = await generateRendererTask.GenerateRendererAsync(rendererParams);
				_graphicsOverlay.Renderer = rendererResult.Renderer;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Statistics Sample");
            }
        }

        // Query states the for graphics and statistics
        private async Task RunQuery()
        {
            try
            {
                progress.Visibility = Visibility.Visible;
				_graphicsOverlay.Graphics.Clear();
                resultsGrid.ItemsSource = null;

                QueryTask queryTask = new QueryTask(new Uri(LAYER_URL));
                Query query = new Query("1=1")
                {
                    GroupByFieldsForStatistics = new List<string> { "sub_region" },
                    OutStatistics = new List<OutStatistic> 
                    { 
                        new OutStatistic() 
                        {
                            OnStatisticField = "pop2000",
                            OutStatisticFieldName = "SubRegionPopulation",
                            StatisticType = StatisticType.Sum
                        },
                        new OutStatistic()
                        {
                            OnStatisticField = "sub_region",
                            OutStatisticFieldName = "NumberOfStates",
                            StatisticType = StatisticType.Count
                        }
                    }
                };

                var result = await queryTask.ExecuteAsync(query);
                if (result.FeatureSet.Features != null && result.FeatureSet.Features.Count > 0)
                {
                    await CreateSubRegionLayerGraphics(result.FeatureSet.Features.OfType<Graphic>());
					resultsGrid.ItemsSource = _graphicsOverlay.Graphics;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Statistics Sample");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }

        private async Task CreateSubRegionLayerGraphics(IEnumerable<Graphic> statistics)
        {
            QueryTask queryTask = new QueryTask(new Uri(LAYER_URL));
            Query query = new Query("1=1")
            {
                ReturnGeometry = true,
                OutSpatialReference = MyMapView.SpatialReference,
                OutFields = new OutFields(new List<string> { "sub_region" })
            };

            var states = await queryTask.ExecuteAsync(query);

            // Create unioned graphics from state geometries for each region
            var regions = states.FeatureSet.Features
                .GroupBy(g => g.Attributes["sub_region"], g => g.Geometry)
                .Select(grp => new Graphic(GeometryEngine.Union(grp), statistics.First(stat => grp.Key.Equals(stat.Attributes["sub_region"])).Attributes));

			_graphicsOverlay.Graphics.Clear();
			_graphicsOverlay.Graphics.AddRange(regions);
        }

        private void resultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			_graphicsOverlay.ClearSelection();

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var graphic = e.AddedItems[0] as Graphic;
                if (graphic != null)
                    graphic.IsSelected = true;
            }
        }
    }
}
