using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates GraphicLayer selection management.
    /// </summary>
    /// <title>Selection</title>
    /// <category>Graphics Layers</category>
    public sealed partial class GraphicsLayerSelection : Page
    {
        private const int MAX_GRAPHICS = 50;

        private Random _random = new Random();
        private GraphicsLayer _graphicsLayer;

        public GraphicsLayerSelection()
        {
            this.InitializeComponent();

            _graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

            SetGraphicsCountUI();
            CreateGraphics();
        }

        // Add selected graphics to graphics layer selection
        private async void AddSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var graphics = await FindIntersectingGraphicsAsync();
                foreach (var graphic in graphics)
                {
                    graphic.IsSelected = true;
                }

                SetGraphicsCountUI();
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Selection Error: " + ex.Message, "Graphics Layer Selection Sample").ShowAsync();
            }
        }

        // Remove selected graphics from graphics layer selection
        private async void RemoveSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var graphics = await FindIntersectingGraphicsAsync();
                foreach (var graphic in graphics)
                {
                    graphic.IsSelected = false;
                }

                SetGraphicsCountUI();
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Selection Error: " + ex.Message, "Graphics Layer Selection Sample").ShowAsync();
            }
        }

        // Clear graphics layer selection
        private void ClearSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _graphicsLayer.ClearSelection();
                SetGraphicsCountUI();
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Selection Error: " + ex.Message, "Graphics Layer Selection Sample").ShowAsync();
            }
        }

        // Retrieve a user click point and return hit tested graphics
        private async Task<IEnumerable<Graphic>> FindIntersectingGraphicsAsync()
        {
            var mapRect = await mapView.Editor.RequestShapeAsync(DrawShape.Rectangle) as Envelope;

            Rect winRect = new Rect(
                mapView.LocationToScreen(new MapPoint(mapRect.XMin, mapRect.YMin, mapView.SpatialReference)),
                mapView.LocationToScreen(new MapPoint(mapRect.XMax, mapRect.YMax, mapView.SpatialReference)));

            return await _graphicsLayer.HitTestAsync(mapView, winRect, MAX_GRAPHICS);
        }

        private void SetGraphicsCountUI()
        {
            txtSelectionCount.Text = _graphicsLayer.SelectedItems.Count().ToString();
        }

        // Add new random graphics to the graphics layer
        private async void CreateGraphics()
        {
            await mapView.LayersLoadedAsync();

            for (int n = 1; n <= MAX_GRAPHICS; ++n)
            {
                _graphicsLayer.Graphics.Add(CreateRandomGraphic());
            }
        }

        // Create a random graphic
        private Graphic CreateRandomGraphic()
        {
            return new Graphic()
            {
                Geometry = GetRandomMapPoint(),
                Symbol = new SimpleMarkerSymbol()
                {
                    Style = (SimpleMarkerStyle)_random.Next(0, 6),
                    Color = Colors.Red,
                    Size = 15
                }
            };
        }

        // Utility: Generate a random MapPoint within the current extent
        private MapPoint GetRandomMapPoint()
        {
            double x = mapView.Extent.XMin + (_random.NextDouble() * mapView.Extent.Width);
            double y = mapView.Extent.YMin + (_random.NextDouble() * mapView.Extent.Height);
            return new MapPoint(x, y, mapView.SpatialReference);
        }
    }
}
