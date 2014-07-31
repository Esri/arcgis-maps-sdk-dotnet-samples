using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates how to position map tips on graphics in a graphics layer. In this example, a random set of graphics is added to a graphics layer on the map and the MapView.MouseMove event is handled to hit test the graphics layer and position the map tip.
    /// </summary>
    /// <title>Map Tips</title>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class GraphicsMapTips : UserControl
    {
        private Random _random = new Random();

        private bool _isHitTesting;

        /// <summary>Construct Graphics Map Tips sample control</summary>
        public GraphicsMapTips()
        {
            InitializeComponent();

            _isHitTesting = false;
            CreateGraphics();
        }

        // HitTest the graphics and position the map tip
        private async void MyMapView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isHitTesting)
                return;

            try
            {
                _isHitTesting = true;

                System.Windows.Point screenPoint = e.GetPosition(MyMapView);
                var graphic = await graphicsLayer.HitTestAsync(MyMapView, screenPoint);
                if (graphic != null)
                {
                    maptipTransform.X = screenPoint.X + 4;
                    maptipTransform.Y = screenPoint.Y - mapTip.ActualHeight;
                    mapTip.DataContext = graphic;
                    mapTip.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    mapTip.Visibility = System.Windows.Visibility.Hidden;
            }
            catch
            {
                mapTip.Visibility = System.Windows.Visibility.Hidden;
            }
            finally
            {
                _isHitTesting = false;
            }
        }

        // Create three List<Graphic> objects with random graphics to serve as layer GraphicsSources
        private async void CreateGraphics()
        {
            await MyMapView.LayersLoadedAsync();

            for (int n = 1; n <= 20; ++n)
            {
                graphicsLayer.Graphics.Add(CreateRandomGraphic(n));
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
            double x = MyMapView.Extent.XMin + (_random.NextDouble() * MyMapView.Extent.Width);
            double y = MyMapView.Extent.YMin + (_random.NextDouble() * MyMapView.Extent.Height);
            return new MapPoint(x, y, MyMapView.SpatialReference);
        }
    }
}
