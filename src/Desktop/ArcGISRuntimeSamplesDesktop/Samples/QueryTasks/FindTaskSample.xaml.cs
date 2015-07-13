using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates how to search your data using the find task. The sample displays the results on the map and in tabular list view.
    /// </summary>
    /// <title>Find</title>
	/// <category>Tasks</category>
	/// <subcategory>Query</subcategory>
	public partial class FindTaskSample : UserControl
    {
        private Symbol _markerSymbol;
        private Symbol _lineSymbol;
        private Symbol _fillSymbol;
		private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Find sample control</summary>
        public FindTaskSample()
        {
            InitializeComponent();
            _markerSymbol = layoutGrid.Resources["MarkerSymbol"] as Symbol;
            _lineSymbol = layoutGrid.Resources["LineSymbol"] as Symbol;
            _fillSymbol = layoutGrid.Resources["FillSymbol"] as Symbol;

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
        }

        // Find map service items with entered information in given fields
        private async void FindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                resultsGrid.Visibility = Visibility.Collapsed;
                resultsGrid.ItemsSource = null;
				_graphicsOverlay.Graphics.Clear();

                FindTask findTask = new FindTask(
                    new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer"));

                var param = new FindParameters()
                {
                    LayerIDs = new List<int> { 0, 1, 2 },
                    SearchFields = new List<string> { "CITY_NAME", "NAME", "SYSTEM", "STATE_ABBR", "STATE_NAME" },
                    ReturnGeometry = true,
                    SpatialReference = MyMapView.SpatialReference,
                    SearchText = txtFind.Text
                };

                var findResults = await findTask.ExecuteAsync(param);
                if (findResults != null && findResults.Results.Count > 0)
                {
                    resultsGrid.ItemsSource = findResults.Results;
                    resultsGrid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Find Sample");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }

        // Highlight the selected grid view item on the map
        private void resultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			_graphicsOverlay.Graphics.Clear();

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var findItem = e.AddedItems.OfType<FindItem>().FirstOrDefault();
                if (findItem != null)
					_graphicsOverlay.Graphics.Add(new Graphic(findItem.Feature.Geometry, ChooseGraphicSymbol(findItem.Feature.Geometry)));
            }
        }

        // Select a marker / line / fill symbol based on geometry type
        private Symbol ChooseGraphicSymbol(Geometry geometry)
        {
            if (geometry == null)
                return null;

            Symbol symbol = null;
            switch (geometry.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    symbol = _markerSymbol;
                    break;

                case GeometryType.Polyline:
                    symbol = _lineSymbol;
                    break;

                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    symbol = _fillSymbol;
                    break;
            }

            return symbol;
        }
    }
}
