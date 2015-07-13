using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop.Symbology.Hydrographic
{
	/// <summary>
	/// This sample demonstrates how to perform a point-based identify style operation on an HydrographicS57Layer using the SearchLayer method.
	/// </summary>
	/// <title>S57 Identify</title>
	/// <category>Symbology</category>
	/// <subcategory>Hydrographic</subcategory>
	/// <requiresSymbols>true</requiresSymbols>
	public partial class S57IdentifySample : UserControl
	{
		private GroupLayer _hydrographicLayers;
		private GraphicsOverlay _pointResultGraphicsOverlay;
		private GraphicsOverlay _lineResultGraphicsOverlay;
		private GraphicsOverlay _polygonResultGraphicsOverlay;
		private ObservableCollection<S57FeatureObject> _searchResults;
		private bool _isLoaded;

		public S57IdentifySample()
		{
			InitializeComponent();
			_searchResults = new ObservableCollection<S57FeatureObject>();
			resultList.ItemsSource = _searchResults;

			// Reference layers that are used
			_hydrographicLayers = MyMapView.Map.Layers.OfType<GroupLayer>().First();
			_polygonResultGraphicsOverlay = MyMapView.GraphicsOverlays["polygonResultsOverlay"];
			_lineResultGraphicsOverlay = MyMapView.GraphicsOverlays["lineResultsOverlay"];
			_pointResultGraphicsOverlay = MyMapView.GraphicsOverlays["pointResultsOverlay"];
			ZoomToHydrographicLayers();
		}

		// When user clicks/taps the map, execute search to all hydrographic layers and set results to view
		private async void MyMapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			if (!_isLoaded)
				return;

			// Clear previous results
			_searchResults.Clear();

			foreach (var layer in _hydrographicLayers.ChildLayers)
			{
				var hydroLayer = layer as HydrographicS57Layer;

				// Identify feature objects from layer
				var results = await hydroLayer.HitTestAsync(MyMapView, e.Position, 10, 3);
			
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
		private async void ZoomToHydrographicLayers()
		{
			try
			{
				// wait until all layers are loaded
				await MyMapView.LayersLoadedAsync();

				Envelope extent = _hydrographicLayers.ChildLayers.First().FullExtent;

				// Create combined extent from child hydrographic layers
				foreach (var layer in _hydrographicLayers.ChildLayers)
					extent = extent.Union(layer.FullExtent);

				// Zoom to full extent
				await MyMapView.SetViewAsync(extent);
				_isLoaded = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		// Show error if loading layers fail
		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
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
			_polygonResultGraphicsOverlay.Graphics.Clear();
			_lineResultGraphicsOverlay.Graphics.Clear();
			_pointResultGraphicsOverlay.Graphics.Clear();

			// When no results found, this is 0
			if (e.AddedItems.Count > 0)
			{
				// Using single mode so there is only one item
				var selectedFeatureObject = e.AddedItems[0] as S57FeatureObject;

				var selectedGeometry = selectedFeatureObject.Geometry;
				if (selectedGeometry is Polygon)
				{
					_polygonResultGraphicsOverlay.Graphics.Add(new Graphic(selectedFeatureObject.Geometry));
					_polygonResultGraphicsOverlay.Graphics[0].IsSelected = true;
				}
				else if (selectedGeometry is Polyline)
				{
					_lineResultGraphicsOverlay.Graphics.Add(new Graphic(selectedFeatureObject.Geometry));
					_lineResultGraphicsOverlay.Graphics[0].IsSelected = true;
				}
				else if (selectedGeometry is MapPoint)
				{
					_pointResultGraphicsOverlay.Graphics.Add(new Graphic(selectedFeatureObject.Geometry));
					_pointResultGraphicsOverlay.Graphics[0].IsSelected = true;
				}
			}
		}
	}
}
