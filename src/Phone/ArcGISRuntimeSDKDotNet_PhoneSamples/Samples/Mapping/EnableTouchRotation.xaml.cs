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
		}

		private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
		{
			if(mapView1 == null)
				return;
			var isOn = (sender as ToggleSwitch).IsOn;
			if (isOn)
				mapView1.ClearValue(UIElement.ManipulationModeProperty); //Set back to default setting
			else
				mapView1.ManipulationMode = ManipulationModes.All; //Enable all manipulation modes including rotation
		}
	}
}
