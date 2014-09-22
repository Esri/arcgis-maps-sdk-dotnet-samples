using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Mapping</category>
	public sealed partial class EnableTouchRotation : Page
	{
		public EnableTouchRotation()
		{
			this.InitializeComponent();
            mapView1.InteractionOptions.RotationOptions.IsEnabled = true;
		}

		private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
		{
			if(mapView1 == null)
				return;
			var isOn = (sender as ToggleSwitch).IsOn;
            mapView1.InteractionOptions.RotationOptions.IsEnabled = isOn ? false : true;            
		}
	}
}
