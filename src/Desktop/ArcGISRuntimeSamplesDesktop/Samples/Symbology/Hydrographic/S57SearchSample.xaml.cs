using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop.Symbology.Hydrographic
{
    /// <summary>
    /// This sample shows how to use the SearchAsync method to search an HydrographicS57Layer based on an input geometry, buffer value and object name.
    /// </summary>
    /// <title>S57 Search </title>
    /// <category>Symbology</category>
    /// <subcategory>Hydrographic</subcategory>
    /// <requiresSymbols>true</requiresSymbols>
    public partial class S57SearchSample : UserControl, INotifyPropertyChanged
    {
        private DrawShape _currentDrawShape;
        private Geometry _searchGeometry;

        private GroupLayer _hydrographicLayers;
        private GraphicsOverlay _pointResultGraphicsOverlay;
        private GraphicsOverlay _lineResultGraphicsOverlay;
        private GraphicsOverlay _polygonResultGraphicsOverlay;
        private GraphicsOverlay _drawGraphicsOverlay;
        private ObservableCollection<S57FeatureObject> _searchResults;

        public S57SearchSample()
        {
            InitializeComponent();

            DataContext = this;
            _currentDrawShape = DrawShape.Point;
            _searchResults = new ObservableCollection<S57FeatureObject>();
            ResultList.ItemsSource = _searchResults;

            // Reference layers that are used
            _hydrographicLayers = MyMapView.Map.Layers.OfType<GroupLayer>().First();
            _drawGraphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
            _polygonResultGraphicsOverlay = MyMapView.GraphicsOverlays["polygonResultsOverlay"];
            _lineResultGraphicsOverlay = MyMapView.GraphicsOverlays["lineResultsOverlay"];
            _pointResultGraphicsOverlay = MyMapView.GraphicsOverlays["pointResultsOverlay"];
            ZoomToHydrographicLayers();
        }

        public DrawShape CurrentDrawShape
        {
            get { return _currentDrawShape; }
            set
            {
                _currentDrawShape = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentDrawShape"));
            }
        }

        // Zoom to combined extent of the group layer that contains all hydrographic layers
        private async void ZoomToHydrographicLayers()
        {
            try
            {
                // wait until all layers are loaded
                await MyMapView.LayersLoadedAsync();

                var extent = _hydrographicLayers.ChildLayers.First().FullExtent;

                // Create combined extent from child hydrographic layers
                foreach (var layer in _hydrographicLayers.ChildLayers)
                    extent = extent.Union(layer.FullExtent);

                // Zoom to full extent
                await MyMapView.SetViewAsync(extent);
                SearchArea.IsEnabled = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        // Show error if loading layers fail
        private void MapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError == null)
                return;

            MessageBox.Show(
                    string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
                    "Error loading layer");
        }

        private async void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear previous selection
            _polygonResultGraphicsOverlay.Graphics.Clear();
            _lineResultGraphicsOverlay.Graphics.Clear();
            _pointResultGraphicsOverlay.Graphics.Clear();

            // When no results found, this is 0
            if (e.AddedItems.Count > 0)
            {
                // Using single mode so there is only one item
                var selectedFeatureObject = e.AddedItems[0] as S57FeatureObject;

                var selectedGeometry = selectedFeatureObject.Geometry;
                if (selectedGeometry is Polygon)
                {
                    _polygonResultGraphicsOverlay.Graphics.Add(new Graphic(selectedFeatureObject.Geometry));
                    _polygonResultGraphicsOverlay.Graphics[0].IsSelected = true;
                }
                else if (selectedGeometry is Polyline)
                {
                    _lineResultGraphicsOverlay.Graphics.Add(new Graphic(selectedFeatureObject.Geometry));
                    _lineResultGraphicsOverlay.Graphics[0].IsSelected = true;
                }
                else if (selectedGeometry is MapPoint)
                {
                    _pointResultGraphicsOverlay.Graphics.Add(new Graphic(selectedFeatureObject.Geometry));
                    _pointResultGraphicsOverlay.Graphics[0].IsSelected = true;
                }

                await MyMapView.SetViewAsync(selectedFeatureObject.Geometry.Extent);
            }
        }

        // Cancel the current shape drawing (if in Editor.RequestShapeAsync) when the shape type has changed
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyMapView.Editor.IsActive)
                MyMapView.Editor.Cancel.Execute(null);
        }

        // Cancel the current shape drawing (if in Editor.RequestShapeAsync)
        // and draw new geometry
        private async void AddSearchGeometry_Click(object sender, RoutedEventArgs e)
        {
            if (MyMapView.Editor.IsActive)
                MyMapView.Editor.Cancel.Execute(null);

            _drawGraphicsOverlay.Graphics.Clear();
            searchBtn.IsEnabled = false;

            try
            {
                Symbol symbol = null;
                switch (CurrentDrawShape)
                {
                    case DrawShape.Point:
                        symbol = LayoutGrid.Resources["BluePointSymbol"] as Symbol;
                        break;

                    case DrawShape.LineSegment:
                    case DrawShape.Freehand:
                    case DrawShape.Polyline:
                        symbol = LayoutGrid.Resources["GreenLineSymbol"] as Symbol;
                        break;

                    case DrawShape.Arrow:
                    case DrawShape.Circle:
                    case DrawShape.Ellipse:
                    case DrawShape.Polygon:
                    case DrawShape.Rectangle:
                    case DrawShape.Triangle:
                        symbol = LayoutGrid.Resources["RedFillSymbol"] as Symbol;
                        break;
                }

                // wait for user to draw the shape
                _searchGeometry = await MyMapView.Editor.RequestShapeAsync(CurrentDrawShape, symbol);

                // add the new graphic to the graphic layer
                var graphic = new Graphic(_searchGeometry, symbol);
                _drawGraphicsOverlay.Graphics.Add(graphic);
                searchBtn.IsEnabled = true;
                clearSearchBtn.IsEnabled = true;
            }
            catch (TaskCanceledException)
            {
                searchBtn.IsEnabled = false;
                clearSearchBtn.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error drawing graphic: " + ex.Message, "S57 Search Sample");
            }
        }

        // Execute search against hydrographic layers
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous results
            _searchResults.Clear();

            foreach (var layer in _hydrographicLayers.ChildLayers)
            {
                var hydroLayer = layer as HydrographicS57Layer;

                // Get buffer value
                double bufferDistance = 0;
                Double.TryParse(BufferValue.Text, out bufferDistance);

                // Search feature objects from layer based on geometry, buffer and object name
                var results = await hydroLayer.SearchAsync(MyMapView, _searchGeometry, bufferDistance, SearchText.Text);

                // Add results to results list
                if (results != null && results.Count > 0)
                {
                    foreach (var result in results)
                        _searchResults.Add(result);
                }
            }

            // Show results if there were any, other vice show no results
            if (_searchResults.Count > 0)
            {
                // Select first one
                ResultList.SelectedIndex = 0;
                ResultsArea.Visibility = Visibility.Visible;
                NoResultsArea.Visibility = Visibility.Collapsed;
            }
            else
            {
                ResultsArea.Visibility = Visibility.Collapsed;
                NoResultsArea.Visibility = Visibility.Visible;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Cancel the Editor and clear graphics and search results from the map
        private void clearSearchBtn_Click(object sender, RoutedEventArgs e)
        {

            if (MyMapView.Editor.IsActive)
                MyMapView.Editor.Cancel.Execute(null);

            _drawGraphicsOverlay.Graphics.Clear();
            _searchResults.Clear();

            SearchText.Clear();
            BufferValue.Clear();

            ResultsArea.Visibility = Visibility.Collapsed;
            NoResultsArea.Visibility = Visibility.Visible;

            searchBtn.IsEnabled = false;
            clearSearchBtn.IsEnabled = false;

        }
    }
}
