using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates GraphicLayer selection management. Graphics are set in a selected or unselected state by using the Graphic.IsSelected property. Here, the user selects or unselects graphics by clicking on the graphic.
    /// </summary>
    /// <title>Selection</title>
    /// <category>Layers</category>
    /// <subcategory>Graphics Layers</subcategory>
    public partial class GraphicsLayerSelection : UserControl
    {
        private const int MAX_GRAPHICS = 50;

        private Random _random = new Random();

        /// <summary>Construct Graphics Layer Selection sample control</summary>
        public GraphicsLayerSelection()
        {
            InitializeComponent();
            CreateGraphics();
        }

        // Remove selected graphics from graphics layer selection
        private async void AddSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var graphics = await FindIntersectingGraphicsAsync();
                foreach (var graphic in graphics)
                {
                    graphic.IsSelected = true;
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selection Error: " + ex.Message, "Graphics Layer Selection Sample");
            }
        }

        // Remove selected graphics from graphics layer selection
        private async void RemoveSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var graphics = await FindIntersectingGraphicsAsync();
                foreach (var graphic in graphics)
                {
                    graphic.IsSelected = false;
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selection Error: " + ex.Message, "Graphics Layer Selection Sample");
            }
        }

        // Clear graphics layer selection
        private void ClearSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                graphicsLayer.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selection Error: " + ex.Message, "Graphics Layer Selection Sample");
            }
        }

        // Retrieve a user click point and return hit tested graphics
        private async Task<IEnumerable<Graphic>> FindIntersectingGraphicsAsync()
        {
            var mapRect = await mapView.Editor.RequestShapeAsync(DrawShape.Envelope) as Envelope;

            Rect winRect = new Rect(
                mapView.LocationToScreen(new MapPoint(mapRect.XMin, mapRect.YMax, mapView.SpatialReference)),
                mapView.LocationToScreen(new MapPoint(mapRect.XMax, mapRect.YMin, mapView.SpatialReference)));

            return await graphicsLayer.HitTestAsync(mapView, winRect, MAX_GRAPHICS);
        }

        // Add new random graphics to the graphics layer
        private async void CreateGraphics()
        {
            await mapView.LayersLoadedAsync();

            for (int n = 1; n <= MAX_GRAPHICS; ++n)
            {
                graphicsLayer.Graphics.Add(CreateRandomGraphic());
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
