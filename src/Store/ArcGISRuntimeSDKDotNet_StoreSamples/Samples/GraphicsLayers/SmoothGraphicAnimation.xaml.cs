using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Animates a graphic smoothly between two user defined locations by calling the MapPoint.MoveTo method at regular intervals as defined by a DispatcherTimer. 
    /// The distance the point is moved each time is calculated by a quintic easing function.
    /// </summary>
    /// <title>Smooth Graphic Animation</title>
    /// <category>Graphics Layers</category>
    public partial class SmoothGraphicAnimation : Windows.UI.Xaml.Controls.Page
    {
        private const int AnimationDuration = 5000;     // milliseconds (5 second animation)

        private GraphicsLayer _animatingLayer;
        private GraphicsLayer _userInteractionLayer;

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
                    Symbol = new SimpleMarkerSymbol() { Color = Colors.Green, Size = 15, Style = SimpleMarkerStyle.Circle }
                }
            };

            _animatingLayer = new GraphicsLayer()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleMarkerSymbol() { Color = Colors.Red, Size = 15, Style = SimpleMarkerStyle.Circle }
                }
            };

            PropertyChangedEventHandler propertyChanged = null;
            propertyChanged += async (s, e) =>
            {
                if (e.PropertyName == "SpatialReference")
                {
                    MyMapView.PropertyChanged -= propertyChanged;
                    AddLayers();
                    await WaitforMapClick();
                }
            };
            MyMapView.PropertyChanged += propertyChanged;
        }

        private void AddLayers()
        {
            MyMapView.Map.Layers.Add(_userInteractionLayer);
			MyMapView.Map.Layers.Add(_animatingLayer);
        }

        private async Task WaitforMapClick()
        {
            MapPoint point = await MyMapView.Editor.RequestPointAsync();

            _userInteractionLayer.Graphics.Add(new Graphic(point));

            if (_userInteractionLayer.Graphics.Count == 2)
            {
                AnimateGraphic();
                await Task.Delay(AnimationDuration);
                _userInteractionLayer.Graphics.Clear();
                _animatingLayer.Graphics.Clear();
            }

            await WaitforMapClick();
        }

        private void AnimateGraphic()
        {
			MapPoint userStartPoint = _userInteractionLayer.Graphics[0].Geometry as MapPoint;
			MapPoint userFinishPoint = _userInteractionLayer.Graphics[1].Geometry as MapPoint;

			MapPoint startPoint = GeometryEngine.NormalizeCentralMeridian(userStartPoint) as MapPoint;
			MapPoint finishPoint = GeometryEngine.NormalizeCentralMeridian(userFinishPoint) as MapPoint;

			var animatingGraphic = new Graphic(new MapPoint(startPoint.X, startPoint.Y));
			_animatingLayer.Graphics.Add(animatingGraphic);

            // Framework easing objects may be used to calculate progressive values
            // - i.e. QuinticEase, BackEase, BounceEase, ElasticEase, etc.
            var easing = new QuinticEase() { EasingMode = EasingMode.EaseInOut };

            var animateStartTime = DateTime.Now;
            var animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(33);
            animationTimer.Tick += (s, ex) =>
            {
                double fraction = easing.Ease((DateTime.Now - animateStartTime).TotalMilliseconds / AnimationDuration);
                var x = (finishPoint.X - startPoint.X) * fraction + startPoint.X;
                var y = (finishPoint.Y - startPoint.Y) * fraction + startPoint.Y;
				animatingGraphic.Geometry = new MapPoint(x, y);
			};

            animationTimer.Start();
        }
    }
}
