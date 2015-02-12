using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using symbols = Esri.ArcGISRuntime.Symbology;

namespace ArcGISRuntime.Samples.Store.Samples.Symbology.Hydrographic
{
    /// <summary>
    /// This sample shows how to use the SearchAsync method to search an HydrographicS57Layer based on an input geometry, buffer value and object name.
    /// </summary>
    /// <title>S57 Search </title>
    /// <category>Symbology</category>
    /// <subcategory>Hydrographic</subcategory>
    /// <requiresSymbols>true</requiresSymbols>
    public sealed partial class S57SearchSample : Page
    {
        private const string LAYER_1_PATH = @"symbology\s57-electronic-navigational-charts\us1wc01m\us1wc01m.000";
        private const string LAYER_2_PATH = @"symbology\s57-electronic-navigational-charts\us1wc07m\us1wc07m.000";

        private GroupLayer _hydrographicGroupLayer;
        private Geometry _searchGeometry;

        private GroupLayer _hydrographicLayers;
        private GraphicsOverlay _pointResultGraphicsOverlay;
        private GraphicsOverlay _lineResultGraphicsOverlay;
        private GraphicsOverlay _polygonResultGraphicsOverlay;
        private GraphicsOverlay _drawGraphicsOverlay;
        private ObservableCollection<S57FeatureObject> _searchResults;

        public S57SearchSample()
        {
            this.InitializeComponent();
            MyMapView.ExtentChanged += MyMapView_ExtentChanged;

            _searchResults = new ObservableCollection<S57FeatureObject>();
            resultList.ItemsSource = _searchResults;

            // Reference layers that are used
            _hydrographicLayers = MyMapView.Map.Layers.OfType<GroupLayer>().First();
            _drawGraphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
            _polygonResultGraphicsOverlay = MyMapView.GraphicsOverlays["polygonResultsOverlay"];
            _lineResultGraphicsOverlay = MyMapView.GraphicsOverlays["lineResultsOverlay"];
            _pointResultGraphicsOverlay = MyMapView.GraphicsOverlays["pointResultsOverlay"];
        }

        // Load data - enable functionality after layers are loaded.
        private async void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

                // Get group layer from Map and set list items source
                _hydrographicGroupLayer = MyMapView.Map.Layers.OfType<GroupLayer>().First();

                // Check that sample data is downloaded to the client
                await CreateHydrographicLayerAsync(LAYER_1_PATH);
                await CreateHydrographicLayerAsync(LAYER_2_PATH);

                // Wait until all layers are loaded
                var layers = await MyMapView.LayersLoadedAsync();

                Envelope extent = _hydrographicGroupLayer.ChildLayers.First().FullExtent;

                // Create combined extent from child hydrographic layers
                foreach (var layer in _hydrographicGroupLayer.ChildLayers)
                    extent = extent.Union(layer.FullExtent);

                // Zoom to full extent
                await MyMapView.SetViewAsync(extent);

                searchParamBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "S57 Display Properties Sample").ShowAsync();
            }
        }

        // Show error if loading layers fail
        private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError == null)
                return;

            var _x = new MessageDialog(
                string.Format("Error when loading layer. {0}", e.LoadError.ToString()), "S57 Cell Info Sample").ShowAsync();
        }

        private async Task CreateHydrographicLayerAsync(string path)
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(path);
            if (file == null)
                throw new Exception("Local hydrographic data not found. Please download sample data from 'Sample Data Settings'");

            // Create hydrographic layer from sample data
            var hydroLayer = new HydrographicS57Layer()
            {
                Path = file.Path,
                ID = Path.GetFileNameWithoutExtension(file.Name)
            };
            _hydrographicGroupLayer.ChildLayers.Add(hydroLayer);
        }

        private async void resultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            // Hide flyout from the UI
            drawFlyout.Hide();
            BottomAppBar.IsOpen = false;

            try
            {
                var selectedDrawShape = DrawShape.Polygon;
                var symbolType = (sender as Button).Tag.ToString();
                symbols.Symbol symbol = null;
                switch (symbolType)
                {
                    case "line":
                        selectedDrawShape = DrawShape.Polyline;
                        symbol = Resources["GreenLineSymbol"] as symbols.Symbol;
                        break;
                    case "polygon":
                        selectedDrawShape = DrawShape.Polygon;
                        symbol = Resources["RedFillSymbol"] as symbols.Symbol;
                        break;
                    case "circle":
                        selectedDrawShape = DrawShape.Circle;
                        symbol = Resources["RedFillSymbol"] as symbols.Symbol;
                        break;
                    case "rectangle":
                        selectedDrawShape = DrawShape.Rectangle;
                        symbol = Resources["RedFillSymbol"] as symbols.Symbol;
                        break;
                }

                // wait for user to draw the shape
                _searchGeometry = await MyMapView.Editor.RequestShapeAsync(selectedDrawShape, symbol);

                // add the new graphic to the graphic layer
                var graphic = new Graphic(_searchGeometry, symbol);
                _drawGraphicsOverlay.Graphics.Add(graphic);
                searchBtn.IsEnabled = true;
                clearBtn.IsEnabled = true;
            }
            catch (TaskCanceledException)
            {
                // Ignore cancelations from selecting new shape type
                searchBtn.IsEnabled = false;
                clearBtn.IsEnabled = false;
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Error drawing graphic: " + ex.Message, "S57 Search Sample").ShowAsync();
            }

            BottomAppBar.IsOpen = true;
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

            // Show results if there were any, otherwise show no results
            if (_searchResults.Count > 0)
            {
                // Select first one
                resultList.SelectedIndex = 0;
                resultsArea.Visibility = Visibility.Visible;
                noResultsArea.Visibility = Visibility.Collapsed;
            }
            else
            {
                resultsArea.Visibility = Visibility.Collapsed;
                noResultsArea.Visibility = Visibility.Visible;
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            // Cancel the Editor and clear graphics and search results from the map
            if (MyMapView.Editor.IsActive)
                MyMapView.Editor.Cancel.Execute(null);

            _drawGraphicsOverlay.Graphics.Clear();
            _searchResults.Clear();

            SearchText.Text = "";
            BufferValue.Text = "";

            resultsArea.Visibility = Visibility.Collapsed;
            noResultsArea.Visibility = Visibility.Visible;

            searchBtn.IsEnabled = false;
            clearBtn.IsEnabled = false;
        }
    }
}
