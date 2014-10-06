using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples.Symbology.Hydrographic
{
	/// <summary>
	/// This sample demonstrates adding and removing S-57 cells from an HydrographicS57Layer in the Map and accessing the Dataset Identification (DSID) metadata of those S-57 cells. The DSID metadata Provides information regarding the dataset's source and the product specification it is part of.
	/// </summary>
	/// <title>S57 Cell Information</title>
	/// <category>Symbology</category>
	/// <subcategory>Hydrographic</subcategory>
	public sealed partial class S57CellInfoSample : Page
	{
		private const string LAYER_1_PATH = @"samples-data\symbology\s57-electronic-navigational-charts\us1wc01m\us1wc01m.000";
		private const string LAYER_2_PATH = @"samples-data\symbology\s57-electronic-navigational-charts\us1wc07m\us1wc07m.000";

		private GroupLayer _hydrographicGroupLayer;

		public S57CellInfoSample()
		{
			this.InitializeComponent();
			mapView.ExtentChanged += mapView_ExtentChanged;
		}

		// Load data - enable functionality after layers are loaded.
		private async void mapView_ExtentChanged(object sender, EventArgs e)
		{
			try
			{
				mapView.ExtentChanged -= mapView_ExtentChanged;

				// Get group layer from Map and set list items source
				_hydrographicGroupLayer = mapView.Map.Layers.OfType<GroupLayer>().First();

				// Check that sample data is downloaded to the client
				await CreateHydrographicLayerAsync(LAYER_1_PATH);
				await CreateHydrographicLayerAsync(LAYER_2_PATH);

				// Wait until all layers are loaded
				var layers = await mapView.LayersLoadedAsync();

				// Set item sources
				s57CellList.ItemsSource = _hydrographicGroupLayer.ChildLayers;
				s57CellList.SelectedIndex = 0;

				// Zoom to hydrographic layer
				await mapView.SetViewAsync(_hydrographicGroupLayer.FullExtent);
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.Message, "S57 Cell Info Sample").ShowAsync();
			}
		}

		// Show error if loading layers fail
		private void mapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			var _ = new MessageDialog(
				string.Format("Error when loading layer. {0}", e.LoadError.ToString()), "S57 Cell Info Sample").ShowAsync();
		}

		private async void ZoomToSelectedButtom_Click(object sender, RoutedEventArgs e)
		{
			var selectedLayer = s57CellList.SelectedItem as HydrographicS57Layer;
			if (selectedLayer == null)
				return;

			ZoomToCell(await selectedLayer.GetCellAsync(mapView));
		}

		private void ZoomToCell(S57Cell currentCell)
		{
			if (currentCell == null)
				return;

			if (currentCell.Extent != null)
			{
				mapView.SetView(currentCell.Extent);
			}
		}

		private async void s57CellList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedLayer = s57CellList.SelectedItem as HydrographicS57Layer;
			if (selectedLayer == null)
				return;

			cellInfoDisplay.DataContext = await selectedLayer.GetCellAsync(mapView);
		}

		private async Task CreateHydrographicLayerAsync(string path)
		{
			StorageFile file = null;
			try
			{
				file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
			}
			catch (FileNotFoundException)
			{
				throw new Exception("Local hydrographic data not found. Please download sample data from 'Sample Data Settings'");
			}
			
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
