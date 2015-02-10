using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
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

			// Since this is Phone, always have touch rotation enabled.
			MyMapView.InteractionOptions.RotationOptions.IsEnabled = true;	
		}

		private void rotationSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
		{
			MyMapView.SetRotation(e.NewValue);
		}
	}
}
