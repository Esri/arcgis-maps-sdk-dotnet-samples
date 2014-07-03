using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Geometry;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples.Symbology.AdvancedSymbology
{
	/// <summary>
	/// This sample shows how to use the the SearchAsync method to search an HydrographicS57Layer based on an input geometry, buffer value and object name.
	/// </summary>
	/// <title>S57 Search </title>
	/// <category>Symbology</category>
	/// <subcategory>Advanced</subcategory>
	public partial class S57SearchSample : UserControl, INotifyPropertyChanged
	{
		private DrawShape _currentDrawShape;
		private Geometry _searchGeometry;

		private GroupLayer _hydrographicLayers;
		private GraphicsLayer _resultGraphicsLayer;
		private ObservableCollection<S57FeatureObject> _searchResults;

		public S57SearchSample()
		{
			InitializeComponent();

			DataContext = this;
			_currentDrawShape = DrawShape.Point;
			_searchResults = new ObservableCollection<S57FeatureObject>();
			ResultList.ItemsSource = _searchResults;

			// Reference layers that are used
			_hydrographicLayers = mapView.Map.Layers.OfType<GroupLayer>().First();
			_resultGraphicsLayer = mapView.Map.Layers.OfType<GraphicsLayer>().First(x => x.ID == "resultGraphics");
			var _ = ZoomToHydrographicLayersAsync();
		}

		public DrawShape CurrentDrawShape
		{
			get { return _currentDrawShape; }
			set
			{
				_currentDrawShape = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("CurrentDrawShape"));
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
			SearchArea.IsEnabled = true;
		}

		// Show error if loading layers fail
		private void MapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			MessageBox.Show(
					string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
					"Error loading layer");
		}

		private async void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Clear previous selection
			_resultGraphicsLayer.Graphics.Clear();

			// When no results found, this is 0
			if (e.AddedItems.Count > 0)
			{
				// Using single mode so there is only one item
				var selectedFeatureObject = e.AddedItems[0] as S57FeatureObject;
				_resultGraphicsLayer.Graphics.Add(new Graphic(selectedFeatureObject.Geometry));
				await mapView.SetViewAsync(selectedFeatureObject.Geometry.Extent);
			}
		}

        // Cancel the current shape drawing (if in Editor.RequestShapeAsync) when the shape type has changed
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mapView.Editor.IsActive)
                mapView.Editor.Cancel.Execute(null);
        }

        // Cancel the current shape drawing (if in Editor.RequestShapeAsync)
        // and draw new geometry
        private async void AddSearchGeometry_Click(object sender, RoutedEventArgs e)
        {
            if (mapView.Editor.IsActive)
                mapView.Editor.Cancel.Execute(null);

			graphicsLayer.Graphics.Clear();

			try
			{
				Symbol symbol = null;
				switch (CurrentDrawShape)
				{
					case DrawShape.Point:
						symbol = LayoutGrid.Resources["BluePointSymbol"] as Symbol;
						break;

					case DrawShape.LineSegment:
					case DrawShape.Freehand:
					case DrawShape.Polyline:
						symbol = LayoutGrid.Resources["GreenLineSymbol"] as Symbol;
						break;

					case DrawShape.Arrow:
					case DrawShape.Circle:
					case DrawShape.Ellipse:
					case DrawShape.Polygon:
					case DrawShape.Rectangle:
					case DrawShape.Triangle:
						symbol = LayoutGrid.Resources["RedFillSymbol"] as Symbol;
						break;
				}

				// wait for user to draw the shape
				_searchGeometry = await mapView.Editor.RequestShapeAsync(CurrentDrawShape, symbol);

				// add the new graphic to the graphic layer
				var graphic = new Graphic(_searchGeometry, symbol);
				graphicsLayer.Graphics.Add(graphic);
			}
			catch (TaskCanceledException)
			{
				// Ignore cancelations from selecting new shape type
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error drawing graphic: " + ex.Message, "S57 Search Sample");
			}
        }

		// Execute search against hydrographic layers
		private async void Search_Click(object sender, RoutedEventArgs e)
		{
			// Clear previous results
			_searchResults.Clear();

			foreach (var layer in _hydrographicLayers.ChildLayers)
			{
				var hydroLayer = layer as HydrographicS57Layer;

				// Get buffer value
				double bufferDistance = 0;
				Double.TryParse(BufferValue.Text, out bufferDistance);

				// Search feature objects from layer based on geometry, buffer and object name
				var results = await hydroLayer.SearchAsync(mapView, _searchGeometry, bufferDistance, SearchText.Text);

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
				ResultList.SelectedIndex = 0;
				ResultsArea.Visibility = Visibility.Visible;
				NoResultsArea.Visibility = Visibility.Collapsed;
			}
			else
			{
				ResultsArea.Visibility = Visibility.Collapsed;
				NoResultsArea.Visibility = Visibility.Visible;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
