using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// This sample shows how to rotate a map using the MapView.Rotation property.
    /// </summary>
    /// <title>Map Rotation</title>
    /// <category>Mapping</category>
	public sealed partial class MapRotation : Page
    {
        public MapRotation()
        {
            this.InitializeComponent();
        }

        private void EnableTouchRotateButton_Click(object sender, RoutedEventArgs e)
        {
            var toggle = e.OriginalSource as AppBarToggleButton;
            if (toggle == null)
                return;

			if (toggle.IsChecked == true)
				MyMapView.InteractionOptions.RotationOptions.IsEnabled = true;
			else
				MyMapView.InteractionOptions.RotationOptions.IsEnabled = false;
		}

		private void rotationSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
		{
			MyMapView.SetRotation(e.NewValue);
		}
    }
}
