using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
	/// This sample demostrates how to perform a point-based identify style operation on an HydrographicS57Layer using the SearchLayer method.
	/// </summary>
	/// <title>S57 Identify</title>
	/// <category>Symbology</category>
	/// <subcategory>Hydrographic</subcategory>
	public sealed partial class S57IdentifySample : Page
	{
		private const string LAYER_1_PATH = @"samples-data\symbology\s57-electronic-navigational-charts\us1wc01m\us1wc01m.000";
		private const string LAYER_2_PATH = @"samples-data\symbology\s57-electronic-navigational-charts\us1wc07m\us1wc07m.000";

		private GroupLayer _hydrographicGroupLayer;
		private GraphicsLayer _resultGraphicsLayer;
		private ObservableCollection<S57FeatureObject> _searchResults;
		private bool _isLoaded;

		public S57IdentifySample()
		{
			InitializeComponent();
			_searchResults = new ObservableCollection<S57FeatureObject>();
			resultList.ItemsSource = _searchResults;

			// Reference layers that are used
			_hydrographicGroupLayer = mapView.Map.Layers.OfType<GroupLayer>().First();
			_resultGraphicsLayer = mapView.Map.Layers.OfType<GraphicsLayer>().First();
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

				Envelope extent = _hydrographicGroupLayer.ChildLayers.First().FullExtent;

				// Create combined extent from child hydrographic layers
				foreach (var layer in _hydrographicGroupLayer.ChildLayers)
					extent = extent.Union(layer.FullExtent);

				// Zoom to full extent
				await mapView.SetViewAsync(extent);
				_isLoaded = true;
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.Message, "S57 Display Properties Sample").ShowAsync();
			}
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

		// When user clicks/taps the map, execute search to all hydrographic layers and set results to view
		private async void mapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			if (!_isLoaded)
				return;

			// Clear previous results
			_searchResults.Clear();

			foreach (var layer in _hydrographicGroupLayer.ChildLayers)
			{
				var hydroLayer = layer as HydrographicS57Layer;

				// Identify feature objects from layer
				var results = await hydroLayer.HitTestAsync(mapView, e.Position, 10, 3);

				// Add results to results list
				if (results != null && results.Count > 0)
				{
					foreach (var result in results)
						_searchResults.Add(result);
				}
			}

			// Show results if there were any, other vice show no results
			if (_searchResults.Count > 0)
			{
				// Select first one
				resultList.SelectedIndex = 0;
				resultsArea.Visibility = Visibility.Visible;
				noResultsArea.Visibility = Visibility.Collapsed;
			}
			else
			{
				resultsArea.Visibility = Visibility.Collapsed;
				noResultsArea.Visibility = Visibility.Visible;
			}
		}

		// Show error if loading layers fail
		private void mapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			var _ = new MessageDialog(
				string.Format("Error when loading layer. {0}", e.LoadError.ToString()), "S57 Identify Sample").ShowAsync();
		}

		private void resultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Clear previous selection
			_resultGraphicsLayer.Graphics.Clear();

			// When no results found, this is 0
			if (e.AddedItems.Count > 0)
			{
				// Using single mode so there is only one item
				var selectedFeatureObject = e.AddedItems[0] as S57FeatureObject;
				_resultGraphicsLayer.Graphics.Add(new Graphic(selectedFeatureObject.Geometry));
			}
		}
	}
}
