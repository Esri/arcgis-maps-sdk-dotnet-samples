using Esri.ArcGISRuntime.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
