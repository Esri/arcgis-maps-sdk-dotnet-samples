using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
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

        public FeatureLayerMapTips()
		{
			this.InitializeComponent();

            _featureLayer = MyMapView.Map.Layers["FeatureLayer"] as FeatureLayer;
            ((ServiceFeatureTable)_featureLayer.FeatureTable).OutFields = OutFields.All;

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
                Point screenPoint = e.GetCurrentPoint(MyMapView).Position;
                var rows = await _featureLayer.HitTestAsync(MyMapView, screenPoint);
                if (rows != null && rows.Length > 0)
                {
                    var features = await _featureLayer.FeatureTable.QueryAsync(rows);
                    var feature = features.FirstOrDefault();

                    maptipTransform.X = screenPoint.X + 4;
                    maptipTransform.Y = screenPoint.Y - mapTip.ActualHeight;
                    mapTip.DataContext = feature;
                    mapTip.Visibility = Visibility.Visible;
                }
                else
                    mapTip.Visibility = Visibility.Collapsed;
            }
            catch
            {
                mapTip.Visibility = Visibility.Collapsed;
            }
        }
    }
}
