using Esri.ArcGISRuntime.Layers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Shows how to access layers in the map.
	/// </summary>
	/// <title>Layer List</title>
	/// <category>Mapping</category>
	public sealed partial class LayerList : Page
	{
		private ObservableCollection<Layer> _legendLayers;
		public ObservableCollection<Layer> LegendLayers
		{
			get { return _legendLayers; }
		}

		public LayerList()
		{
			this.InitializeComponent();

			// Create legend layers (reverse order) from initial map layers (defined in XAML)
			_legendLayers = new ObservableCollection<Layer>(MyMapView.Map.Layers.Reverse());
			_legendLayers.CollectionChanged += LegendLayers_CollectionChanged;

			DataContext = this;
		}

		private Layer _movingLayer;

		private void LegendLayers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Handle moving a layer in the legend (Remove and Add combination)

			if (e.Action == NotifyCollectionChangedAction.Remove)
				_movingLayer = e.OldItems[0] as Layer;
			else if ((e.Action == NotifyCollectionChangedAction.Add) && (_movingLayer != null))
			{
				int old_idx = MyMapView.Map.Layers.IndexOf(_movingLayer);
				int new_idx = (MyMapView.Map.Layers.Count - 1) - e.NewStartingIndex;
				MyMapView.Map.Layers.Move(old_idx, new_idx);
			}
		}

		private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
		{
			var layer = (sender as FrameworkElement).DataContext as Layer;
			MyMapView.Map.Layers.Remove(layer);
			LegendLayers.Remove(layer);
		}
	}
}
