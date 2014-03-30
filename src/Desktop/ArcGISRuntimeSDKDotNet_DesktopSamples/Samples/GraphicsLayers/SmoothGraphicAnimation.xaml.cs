using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Animates a graphic smoothly between two user defined locations by calling the MapPoint.MoveTo method at regular intervals as defined by a DispatcherTimer. 
    /// The distance the point is moved each time is calculated by a quintic easing function.
    /// </summary>
    /// <title>Smooth Graphic Animation</title>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class SmoothGraphicAnimation : UserControl
    {
        GraphicsLayer _animatingLayer;
        GraphicsLayer _userInteractionLayer;
        private DateTime animateStartTime;
        private DispatcherTimer animationTimer;
        private int animationDuration = 5000;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmoothGraphicAnimation"/> class.
        /// </summary>
        public SmoothGraphicAnimation()
        {
            InitializeComponent();

            _userInteractionLayer = new GraphicsLayer()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleMarkerSymbol() { Color = Colors.Green, Size = 12, Style = SimpleMarkerStyle.Circle }
                }
            };

            _animatingLayer = new GraphicsLayer()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleMarkerSymbol() { Color = Colors.Red, Size = 12, Style = SimpleMarkerStyle.Circle }
                }
            };

            PropertyChangedEventHandler propertyChanged = null;
            propertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SpatialReference")
                {
                    mapView1.PropertyChanged -= propertyChanged;
                    AddLayers();
                    WaitforMapClick();                    
                }
            };
            mapView1.PropertyChanged += propertyChanged;
        }

        private void AddLayers() 
        { 
            map1.Layers.Add(_userInteractionLayer);
            map1.Layers.Add(_animatingLayer);      
        }

        private async Task WaitforMapClick() 
        {
            
            MapPoint m = await mapView1.Editor.RequestPointAsync();

            _userInteractionLayer.Graphics.Add(new Graphic(m));

            if (_userInteractionLayer.Graphics.Count == 2)
            {
                await AnimateGraphic();
                await Task.Delay(animationDuration);
                _userInteractionLayer.Graphics.Clear();
                _animatingLayer.Graphics.Clear();     
            }

            WaitforMapClick();   
        }

        private async Task AnimateGraphic() 
        {
            MapPoint startPoint = _userInteractionLayer.Graphics[0].Geometry as MapPoint;
            MapPoint finishPoint = _userInteractionLayer.Graphics[1].Geometry as MapPoint;

            MapPoint animatingPoint = new MapPoint(startPoint.X, startPoint.Y);
            _animatingLayer.Graphics.Add(new Graphic() { Geometry = animatingPoint});

            animationTimer= new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(33);
            animateStartTime = DateTime.Now;
            animationTimer.Tick += (s, ex) => 
            {
                double fraction = (DateTime.Now - animateStartTime).TotalMilliseconds / animationDuration;
                fraction = QuinticEasingInOut(fraction, 0, 1, 1);
                var x = (finishPoint.X - startPoint.X) * fraction + startPoint.X;
                var y = (finishPoint.Y - startPoint.Y) * fraction + startPoint.Y;
                animatingPoint.MoveTo(x, y);
            };
            animationTimer.Start();
        }

        public double QuinticEasingInOut(double t, double b, double c, double d)
        {
            t /= d / 2;
            if (t < 1) return c / 2 * t * t * t * t * t + b;
            t -= 2;
            return c / 2 * (t * t * t * t * t + 2) + b;
        }
    }
}
