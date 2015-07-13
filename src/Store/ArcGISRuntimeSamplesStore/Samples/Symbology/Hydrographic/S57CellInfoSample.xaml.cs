using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples.Symbology.Hydrographic
{
	/// <summary>
	/// This sample demonstrates adding and removing S-57 cells from an HydrographicS57Layer in the Map and accessing the Dataset Identification (DSID) metadata of those S-57 cells. The DSID metadata Provides information regarding the dataset's source and the product specification it is part of.
	/// </summary>
	/// <title>S57 Cell Information</title>
	/// <category>Symbology</category>
	/// <subcategory>Hydrographic</subcategory>
	/// <requiresSymbols>true</requiresSymbols>
	public sealed partial class S57CellInfoSample : Page
	{
		private const string LAYER_1_PATH = @"symbology\s57-electronic-navigational-charts\us1wc01m\us1wc01m.000";
		private const string LAYER_2_PATH = @"symbology\s57-electronic-navigational-charts\us1wc07m\us1wc07m.000";

		private GroupLayer _hydrographicGroupLayer;

		public S57CellInfoSample()
		{
			this.InitializeComponent();
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

				// Set item sources
				s57CellList.ItemsSource = _hydrographicGroupLayer.ChildLayers;
				s57CellList.SelectedIndex = 0;

				// Zoom to hydrographic layer
				await MyMapView.SetViewAsync(_hydrographicGroupLayer.FullExtent);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "S57 Cell Info Sample").ShowAsync();
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

		private async void ZoomToSelectedButtom_Click(object sender, RoutedEventArgs e)
		{
			var selectedLayer = s57CellList.SelectedItem as HydrographicS57Layer;
			if (selectedLayer == null)
				return;

			ZoomToCell(await selectedLayer.GetCellAsync(MyMapView));
		}

		private void ZoomToCell(S57Cell currentCell)
		{
			if (currentCell == null)
				return;

			if (currentCell.Extent != null)
			{
				MyMapView.SetView(currentCell.Extent);
			}
		}

		private async void s57CellList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedLayer = s57CellList.SelectedItem as HydrographicS57Layer;
			if (selectedLayer == null)
				return;

			cellInfoDisplay.DataContext = await selectedLayer.GetCellAsync(MyMapView);
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
