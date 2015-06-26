using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ArcGISRuntime.Samples.Desktop
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
                    Symbol = new SimpleMarkerSymbol() { 
						Color = Colors.Green, 
						Size = 15, 
						Style = SimpleMarkerStyle.Circle 
					}
                }
            };

            _animatingLayer = new GraphicsLayer()
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

            MyMapView.Map.Layers.Add(_userInteractionLayer);
            MyMapView.Map.Layers.Add(_animatingLayer);

            MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
        }

        private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
        {
            MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;

            try
            {
                await WaitforMapClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Smooth Graphic animation sample");
            }
        }

        private async Task WaitforMapClick()
        {
			MapPoint m = await MyMapView.Editor.RequestPointAsync();

			_userInteractionLayer.Graphics.Add(new Graphic(m));

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
