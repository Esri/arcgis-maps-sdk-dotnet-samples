using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates hit testing in the PointerMoved event of the map view to display feature map tips.
	/// </summary>
	/// <title>Map Tips</title>
	/// <category>Feature Layers</category>
	public sealed partial class FeatureLayerMapTips : Page
	{
		private FeatureLayer _featureLayer;
		private bool _isMapReady;
		private FrameworkElement _mapTip;

		public FeatureLayerMapTips()
		{
			this.InitializeComponent();

			_featureLayer = MyMapView.Map.Layers["FeatureLayer"] as FeatureLayer;
			((ServiceFeatureTable)_featureLayer.FeatureTable).OutFields = OutFields.All;

			_mapTip = MyMapView.Overlays.Items[0] as FrameworkElement;

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
			MyMapView.PointerMoved += MyMapView_PointerMoved;
		}

		private async void MyMapView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			await MyMapView.LayersLoadedAsync();
			_isMapReady = true;
		}

		private async void MyMapView_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isMapReady)
				return;
			
			try
			{
				_isMapReady = false;

				Point screenPoint = e.GetCurrentPoint(MyMapView).Position;
				var rows = await _featureLayer.HitTestAsync(MyMapView, screenPoint);
				if (rows != null && rows.Length > 0)
				{
					var features = await _featureLayer.FeatureTable.QueryAsync(rows);
					_mapTip.DataContext = features.FirstOrDefault();
					_mapTip.Visibility = Visibility.Visible;
				}
				else
					_mapTip.Visibility = Visibility.Collapsed;
			}
			catch
			{
				_mapTip.Visibility = Visibility.Collapsed;
			}
			finally
			{
				_isMapReady = true;
			}
		}
	}
}
