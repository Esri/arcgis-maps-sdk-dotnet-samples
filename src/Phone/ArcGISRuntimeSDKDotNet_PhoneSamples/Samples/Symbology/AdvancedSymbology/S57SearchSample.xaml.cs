using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Windows.UI.Xaml.Navigation;

using symbols = Esri.ArcGISRuntime.Symbology;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples.Symbology.AdvancedSymbology
{
	/// <summary>
	/// This sample shows how to use the the SearchAsync method to search an HydrographicS57Layer based on an input geometry, buffer value and object name.
	/// </summary>
	/// <title>S57 Search </title>
	/// <category>Symbology</category>
	/// <subcategory>Advanced</subcategory>
	public sealed partial class S57SearchSample : Page, INotifyPropertyChanged
	{
		private const string LAYER_1_PATH = @"symbology\s57-electronic-navigational-charts\us1wc01m\us1wc01m.000";
		private const string LAYER_2_PATH = @"symbology\s57-electronic-navigational-charts\us1wc07m\us1wc07m.000";

		private GroupLayer _hydrographicGroupLayer;
		private DrawShape _currentDrawShape;
		private Geometry _searchGeometry;

		private GroupLayer _hydrographicLayers;
		private GraphicsLayer _resultGraphicsLayer;
		private GraphicsLayer _drawGraphicsLayer;
		private ObservableCollection<S57FeatureObject> _searchResults;

		public S57SearchSample()
		{
			this.InitializeComponent();
			mapView.ExtentChanged += mapView_ExtentChanged;
			
			DataContext = this;
			_currentDrawShape = DrawShape.Point;
			_searchResults = new ObservableCollection<S57FeatureObject>();
			resultList.ItemsSource = _searchResults;

			// Reference layers that are used
			_hydrographicLayers = mapView.Map.Layers.OfType<GroupLayer>().First();
			_resultGraphicsLayer = mapView.Map.Layers.OfType<GraphicsLayer>().First(x => x.ID == "resultGraphics");
			_drawGraphicsLayer = mapView.Map.Layers.OfType<GraphicsLayer>().First(x => x.ID == "drawGraphics");
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

				Polygon extent = new Polygon();

				// Create combined extent from child hydrographic layers
				foreach (var layer in _hydrographicGroupLayer.ChildLayers)
					extent = GeometryEngine.Union(extent, layer.FullExtent) as Polygon;

				// Zoom to full extent
				await mapView.SetViewAsync(extent);
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.Message, "S57 Display Properties Sample").ShowAsync();
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

		private async Task CreateHydrographicLayerAsync(string path)
		{
			StorageFile file = null;
			try
			{
				file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
			}
			catch (FileNotFoundException notFoundException)
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

		private async void resultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

			_drawGraphicsLayer.Graphics.Clear();

			try
			{
				symbols.Symbol symbol = null;
				switch (CurrentDrawShape)
				{
					case DrawShape.Point:
						symbol = Resources["BluePointSymbol"] as symbols.Symbol;
						break;

					case DrawShape.LineSegment:
					case DrawShape.Freehand:
					case DrawShape.Polyline:
						symbol = Resources["GreenLineSymbol"] as symbols.Symbol;
						break;

					case DrawShape.Arrow:
					case DrawShape.Circle:
					case DrawShape.Ellipse:
					case DrawShape.Polygon:
					case DrawShape.Rectangle:
					case DrawShape.Triangle:
						symbol = Resources["RedFillSymbol"] as symbols.Symbol;
						break;
				}

				// wait for user to draw the shape
				_searchGeometry = await mapView.Editor.RequestShapeAsync(CurrentDrawShape, symbol);

				// add the new graphic to the graphic layer
				var graphic = new Graphic(_searchGeometry, symbol);
				_drawGraphicsLayer.Graphics.Add(graphic);
			}
			catch (TaskCanceledException)
			{
				// Ignore cancelations from selecting new shape type
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog("Error drawing graphic: " + ex.Message, "S57 Search Sample").ShowAsync();
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

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
