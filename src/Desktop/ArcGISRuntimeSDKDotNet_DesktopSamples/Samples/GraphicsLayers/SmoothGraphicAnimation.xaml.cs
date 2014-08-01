using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Animates a graphic smoothly between two user defined locations by moving MapPoint at regular intervals as defined by a DispatcherTimer. 
    /// The distance the point is moved each time is calculated by a quintic easing function.
    /// </summary>
    /// <title>Smooth Graphic Animation</title>
    /// <category>Layers</category>
    /// <subcategory>Graphics Layers</subcategory>
    public partial class SmoothGraphicAnimation : UserControl
    {
        private const int AnimationDuration = 5000;     // milliseconds (5 second animation)

		private GraphicsOverlay _animatingOverlay;
		private GraphicsOverlay _userInteractionOverlay;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmoothGraphicAnimation"/> class.
        /// </summary>
        public SmoothGraphicAnimation()
        {
            InitializeComponent();

            _userInteractionOverlay = new GraphicsOverlay()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleMarkerSymbol() { 
						Color = Colors.Green, 
						Size = 15, 
						Style = SimpleMarkerStyle.Circle 
					}
                }
            };

			_animatingOverlay = new GraphicsOverlay()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleMarkerSymbol() { 
						Color = Colors.Red, 
						Size = 15, 
						Style = SimpleMarkerStyle.Circle 
					}
                }
            };

            PropertyChangedEventHandler propertyChanged = null;
            propertyChanged += async (s, e) =>
            {
                if (e.PropertyName == "SpatialReference")
                {
                    MyMapView.PropertyChanged -= propertyChanged;
					AddOverlays();
                    await WaitforMapClick();
                }
            };
			MyMapView.PropertyChanged += propertyChanged;
        }

        private void AddOverlays()
        {
			MyMapView.GraphicsOverlays.Add(_userInteractionOverlay);
			MyMapView.GraphicsOverlays.Add(_animatingOverlay);
        }

        private async Task WaitforMapClick()
        {
			MapPoint m = await MyMapView.Editor.RequestPointAsync();

			_userInteractionOverlay.Graphics.Add(new Graphic(m));

			if (_userInteractionOverlay.Graphics.Count == 2)
            {
                AnimateGraphic();
                await Task.Delay(AnimationDuration);
				_userInteractionOverlay.Graphics.Clear();
				_animatingOverlay.Graphics.Clear();
            }

            await WaitforMapClick();
        }

        private void AnimateGraphic()
        {
			MapPoint startPoint = _userInteractionOverlay.Graphics[0].Geometry as MapPoint;
			MapPoint finishPoint = _userInteractionOverlay.Graphics[1].Geometry as MapPoint;

            var animatingGraphic = new Graphic(new MapPoint(startPoint.X, startPoint.Y));
			_animatingOverlay.Graphics.Add(animatingGraphic);

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
