using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to position map tips on graphics in a graphics layer.
    /// </summary>
    /// <title>Map Tips</title>
    /// <category>Graphics Layers</category>
    public sealed partial class GraphicsMapTips : Page
    {
        private const int MAX_GRAPHICS = 50;

        private Random _random = new Random();
        private GraphicsLayer _graphicsLayer;
        private bool _isHitTesting;

        public GraphicsMapTips()
        {
            this.InitializeComponent();

            _graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

            mapView.PointerMoved += mapView_PointerMoved;
            
            _isHitTesting = false;
            CreateGraphics();
        }

        // HitTest the graphics and position the map tip
        private async void mapView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isHitTesting)
                return;

            try
            {
                _isHitTesting = true;

                Point screenPoint = e.GetCurrentPoint(mapView).Position;
                var graphic = await _graphicsLayer.HitTestAsync(mapView, screenPoint);
                if (graphic != null)
                {
                    maptipTransform.X = screenPoint.X + 4;
                    maptipTransform.Y = screenPoint.Y - mapTip.ActualHeight;
                    mapTip.DataContext = graphic;
                    mapTip.Visibility = Visibility.Visible;
                }
                else
                    mapTip.Visibility = Visibility.Collapsed;
            }
            catch
            {
                mapTip.Visibility = Visibility.Collapsed;
            }
            finally
            {
                _isHitTesting = false;
            }
        }

        // Create three List<Graphic> objects with random graphics to serve as layer GraphicsSources
        private async void CreateGraphics()
        {
            await mapView.LayersLoadedAsync();

            for (int n = 1; n <= MAX_GRAPHICS; ++n)
            {
                _graphicsLayer.Graphics.Add(CreateRandomGraphic(n));
            }
        }

        // Create a random graphic
        private Graphic CreateRandomGraphic(int id)
        {
            var symbol = new CompositeSymbol();
            symbol.Symbols.Add(new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Circle, Color = Colors.Red, Size = 16 });
            symbol.Symbols.Add(new TextSymbol()
            {
                Text = id.ToString(),
                Color = Colors.White,
                VerticalTextAlignment = VerticalTextAlignment.Middle,
                HorizontalTextAlignment = HorizontalTextAlignment.Center,
                YOffset = -1
            });

            var graphic = new Graphic()
            {
                Geometry = GetRandomMapPoint(),
                Symbol = symbol
            };

            graphic.Attributes["ID"] = id;

            return graphic;
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
