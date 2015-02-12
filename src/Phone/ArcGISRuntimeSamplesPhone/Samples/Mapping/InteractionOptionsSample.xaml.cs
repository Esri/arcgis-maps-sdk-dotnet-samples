using Esri.ArcGISRuntime.Controls;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample shows how to change interaction settings that controls the MapView.
	/// </summary>
	/// <title>Interaction Options</title>
	/// <category>Mapping</category>
	public sealed partial class InteractionOptionsSample : Page
    {
		public InteractionOptionsSample()
        {
            this.InitializeComponent();

			PanOptions.DataContext = MyMapView.InteractionOptions.PanOptions;
			ZoomOptions.DataContext = MyMapView.InteractionOptions.ZoomOptions;
			RotateToggle.DataContext = MyMapView.InteractionOptions.RotationOptions;
			OptionsToggle.DataContext = MyMapView.InteractionOptions;
        }

		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			var _x = new MessageDialog(
				string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
					"Interaction Options Sample").ShowAsync();
		}

		private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			Debug.WriteLine("MapViewTapped");
		}

		private void MyMapView_MapViewDoubleTapped(object sender, MapViewInputEventArgs e)
		{
			Debug.WriteLine("MapViewDoubleTapped");
		}

		private void MyMapView_MapViewHolding(object sender, MapViewInputEventArgs e)
		{
			Debug.WriteLine("Holding");
		}
    }
}
