using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples.Symbology.AdvancedSymbology
{
	/// <summary>
	/// This sample demostrates how to perform a point-based identify style operation on an HydrographicS57Layer using the SearchLayer method.
	/// </summary>
	/// <title>S57 Identify</title>
	/// <category>Symbology</category>
	/// <subcategory>Advanced</subcategory>
	public partial class S57IdentifySample : UserControl
	{
		GroupLayer _hydrographicLayers;
		GraphicsLayer _resultGraphicsLayer;
		ObservableCollection<S57FeatureObject> _searchResults;
		bool _isLoaded;

		public S57IdentifySample()
		{
			InitializeComponent();
			_searchResults = new ObservableCollection<S57FeatureObject>();
			resultList.ItemsSource = _searchResults;

			// Reference layers that are used
			_hydrographicLayers = mapView.Map.Layers.OfType<GroupLayer>().First();
			_resultGraphicsLayer = mapView.Map.Layers.OfType<GraphicsLayer>().First();
			var _ = ZoomToHydrographicLayersAsync();
		}

		// When user clicks/taps the map, execute search to all hydrographic layers and set results to view
		private async void mapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			if (!_isLoaded)
				return;

			// Clear previous results
			_searchResults.Clear();

			foreach (var layer in _hydrographicLayers.ChildLayers)
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

		// Zoom to combined extent of the group layer that contains all hydrographic layers
		private async Task ZoomToHydrographicLayersAsync()
		{
			// wait until all layers are loaded
			await mapView.LayersLoadedAsync();

			Polygon extent = new Polygon();

			// Create combined extent from child hydrographic layers
			foreach (var layer in _hydrographicLayers.ChildLayers)
				extent = GeometryEngine.Union(extent, layer.FullExtent) as Polygon;

			// Zoom to full extent
			await mapView.SetViewAsync(extent);
			_isLoaded = true;
		}

		// Show error if loading layers fail
		private void mapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			MessageBox.Show(
					string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
					"Error loading layer");
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
