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

        public FeatureLayerMapTips()
		{
			this.InitializeComponent();

            _featureLayer = mapView.Map.Layers["FeatureLayer"] as FeatureLayer;
            ((GeodatabaseFeatureServiceTable)_featureLayer.FeatureTable).OutFields = OutFields.All;

            mapView.PointerMoved += mapView_PointerMoved;
        }

        private async void mapView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                Point screenPoint = e.GetCurrentPoint(mapView).Position;
                var rows = await _featureLayer.HitTestAsync(mapView, screenPoint);
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
