using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples.Symbology.Hydrographic
{
	/// <summary>
	/// This sample demonstrates using the global static HydrographicS52DisplayProperties object to control the display properties of an HydrographicS57Layer, such as the Color Scheme or Safety Contour Depth. 
	/// </summary>
	/// <title>S57 Display Properties</title>
	/// <category>Symbology</category>
	/// <subcategory>Hydrographic</subcategory>
	/// <requiresSymbols>true</requiresSymbols>
	public sealed partial class S57DisplayPropertiesSample : Page
	{
		private const string LAYER_1_PATH = @"symbology\s57-electronic-navigational-charts\us1wc01m\us1wc01m.000";
		private const string LAYER_2_PATH = @"symbology\s57-electronic-navigational-charts\us1wc07m\us1wc07m.000";

		private GroupLayer _hydrographicGroupLayer;

		public S57DisplayPropertiesSample()
		{
			this.InitializeComponent();

			colorSchemes.ItemsSource = Enum.GetValues(typeof(S52ColorScheme)).Cast<S52ColorScheme>();
			dislayDepthUnits.ItemsSource = Enum.GetValues(typeof(S52DisplayDepthUnits)).Cast<S52DisplayDepthUnits>();
			areaSymbolizationTypes.ItemsSource = Enum.GetValues(typeof(S52AreaSymbolizationType)).Cast<S52AreaSymbolizationType>();
			pointSymbolizationTypes.ItemsSource = Enum.GetValues(typeof(S52PointSymbolizationType)).Cast<S52PointSymbolizationType>();
			displayCategory.ItemsSource = Enum.GetValues(typeof(S52DisplayCategory)).Cast<S52DisplayCategory>();
			depthShades.ItemsSource = Enum.GetValues(typeof(S52DepthShades)).Cast<S52DepthShades>();

			// Create default instance of display properties and set that to DataContext for binding
			DataContext = HydrographicS52DisplayProperties.Default;
			MyMapView.ExtentChanged += MyMapView_ExtentChanged;
		}

		// Load data - enable functionality after layers are loaded.
		private async void MyMapView_ExtentChanged(object sender, EventArgs e)
		{
			try
			{
				MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

				// Get group layer from Map and set list items source
				_hydrographicGroupLayer = MyMapView.Map.Layers.OfType<GroupLayer>().First();

				// Check that sample data is downloaded to the client
				await CreateHydrographicLayerAsync(LAYER_1_PATH);
				await CreateHydrographicLayerAsync(LAYER_2_PATH);

				// Wait until all layers are loaded
				var layers = await MyMapView.LayersLoadedAsync();

				Envelope extent = _hydrographicGroupLayer.ChildLayers.First().FullExtent;

				// Create combined extent from child hydrographic layers
				foreach (var layer in _hydrographicGroupLayer.ChildLayers)
					extent = extent.Union(layer.FullExtent);


				// Zoom to full extent
				await MyMapView.SetViewAsync(extent);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "S57 Display Properties Sample").ShowAsync();
			}
		}

		// Show error if loading layers fail
		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			var _x = new MessageDialog(
				string.Format("Error when loading layer. {0}", e.LoadError.ToString()), "S57 Cell Info Sample").ShowAsync();
		}

		private async Task CreateHydrographicLayerAsync(string path)
		{
			var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(path);
			if (file == null)
				throw new Exception("Local hydrographic data not found. Please download sample data from 'Sample Data Settings'");

			// Create hydrographic layer from sample data
			var hydroLayer = new HydrographicS57Layer()
			{
				Path = file.Path,
				ID = Path.GetFileNameWithoutExtension(file.Name)
			};
			_hydrographicGroupLayer.ChildLayers.Add(hydroLayer);
		}
	}
}
