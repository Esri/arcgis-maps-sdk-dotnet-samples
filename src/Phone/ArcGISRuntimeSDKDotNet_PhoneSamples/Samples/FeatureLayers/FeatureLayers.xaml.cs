using Esri.ArcGISRuntime.Controls;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Feature Layers</category>
	public sealed partial class FeatureLayers : Page
	{
		public FeatureLayers()
		{
			this.InitializeComponent();
		}

		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			var _x = new MessageDialog(
				string.Format("Error when loading layer. {0}", e.LoadError.ToString()), "Sample error").ShowAsync();
		}
	}
}
