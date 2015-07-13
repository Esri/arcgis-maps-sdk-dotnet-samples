using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// Shows how to swipe one map over another.
	/// </summary>
	/// <title>Swipe</title>
	/// <category>Mapping</category>
	public sealed partial class Swipe : Page
	{
        public Swipe()
        {
            this.InitializeComponent();

            thumb.RenderTransform = new TranslateTransform() { X = 0, Y = 0 };
            mapImagery.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, 0, 0) };

            mapStreets.ExtentChanged += mapStreets_ExtentChanged;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                var transform = (TranslateTransform)thumb.RenderTransform;
                transform.X = Math.Max(0, Math.Min(transform.X + e.HorizontalChange, this.ActualWidth - thumb.ActualWidth));
                mapImagery.Clip.Rect = new Rect(0, 0, transform.X, this.ActualHeight);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void mapStreets_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                if (mapImagery.Extent != null)
                    mapImagery.SetView(mapStreets.Extent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
	}
}
