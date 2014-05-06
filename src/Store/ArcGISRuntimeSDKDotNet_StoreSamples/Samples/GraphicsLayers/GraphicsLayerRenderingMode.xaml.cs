using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstates the use of the GraphicsLayer.RenderingMode property to control how a GraphicsLayer draws its graphics.
    /// </summary>
    /// <title>Rendering Mode</title>
    /// <category>Graphics Layers</category>
    public sealed partial class GraphicsLayerRenderingMode : Page
    {
        private Random _random = new Random();

        private Envelope _maxExtent;
        private List<Graphic> _graphics = new List<Graphic>();

        public GraphicsLayerRenderingMode()
        {
            this.InitializeComponent();

            graphicCountSlider.Value = 1000;

            renderingModeCombo.DisplayMemberPath = "Item1";
            renderingModeCombo.SelectedValuePath = "Item2";
            renderingModeCombo.ItemsSource = Enum.GetValues(typeof(GraphicsRenderingMode)).Cast<int>()
                .Select(n => new Tuple<string, int>(Enum.GetName(typeof(GraphicsRenderingMode), n), n));
            renderingModeCombo.SelectedIndex = 0;

            // Create the minimum set of graphics
            var task = CreateGraphics(1000);
        }

        // Creates a new graphics layer with the specified graphics count and rendering mode
        private async void CreateGraphicsLayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (mapView.Map.Layers.Count > 1)
                mapView.Map.Layers.RemoveAt(1);

            var graphicsLayer = new GraphicsLayer() { RenderingMode = (GraphicsRenderingMode)renderingModeCombo.SelectedValue };
            mapView.Map.Layers.Add(graphicsLayer);

            // Add new graphics if needed
            var numGraphics = (int)graphicCountSlider.Value;
            if (_graphics.Count < numGraphics)
            {
                await CreateGraphics(numGraphics - _graphics.Count);
            }
            graphicsLayer.Graphics.AddRange(_graphics.Take(numGraphics));
        }

        // Add new random graphics to the graphics layer
        private async Task CreateGraphics(int numGraphics)
        {
            await mapView.LayersLoadedAsync();

            if (_maxExtent == null)
                _maxExtent = mapView.Extent;

            for (int n = 0; n < numGraphics; ++n)
            {
                _graphics.Add(CreateRandomGraphic());
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
                    Color = GetRandomColor(),
                    Size = 12
                }
            };
        }

        // Utility: Generate a random MapPoint within the current extent
        private MapPoint GetRandomMapPoint()
        {
            double x = _maxExtent.XMin + (_random.NextDouble() * _maxExtent.Width);
            double y = _maxExtent.YMin + (_random.NextDouble() * _maxExtent.Height);
            return new MapPoint(x, y, mapView.SpatialReference);
        }

        // Utility: Generate a random System.Windows.Media.Color
        private Color GetRandomColor()
        {
            var colorBytes = new byte[3];
            _random.NextBytes(colorBytes);
            return Color.FromArgb(0xFF, colorBytes[0], colorBytes[1], colorBytes[2]);
        }
    }
}
