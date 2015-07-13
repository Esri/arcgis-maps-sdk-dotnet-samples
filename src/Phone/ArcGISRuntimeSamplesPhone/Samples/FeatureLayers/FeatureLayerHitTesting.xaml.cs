using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates Feature Layer hit testing
	/// </summary>
	/// <title>Hit Testing</title>
	/// <category>Feature Layers</category>
	public sealed partial class FeatureLayerHitTesting : Page
	{
		private FeatureLayer _featureLayer;

		public FeatureLayerHitTesting()
		{
			this.InitializeComponent();

			MyMapView.MapViewTapped += MyMapView_MapViewTapped;

			_featureLayer = MyMapView.Map.Layers["FeatureLayer"] as FeatureLayer;
			((ServiceFeatureTable)_featureLayer.FeatureTable).OutFields = OutFields.All;
		}

		/// <summary>
		/// On each mouse click:
		/// - HitTest the feature layer
		/// - Query the feature table for the returned row
		/// - Set the result feature for the UI
		/// </summary>
		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			try
			{
				var rows = await _featureLayer.HitTestAsync(MyMapView, e.Position);
				if (rows != null && rows.Length > 0)
				{
                    // Forcing query to be executed against local cache
                    var features = await (_featureLayer.FeatureTable as ServiceFeatureTable).QueryAsync(rows, true);
                    var feature = features.FirstOrDefault();
					if (feature != null)
						listHitFeature.ItemsSource = feature.Attributes.Select(attr => new Tuple<string, string>(attr.Key, attr.Value.ToString()));
					else
						listHitFeature.ItemsSource = null;
				}
				else
					listHitFeature.ItemsSource = null;
			}
			catch (Exception ex)
			{
				listHitFeature.ItemsSource = null;
				var _x = new MessageDialog("HitTest Error: " + ex.Message, "Feature Layer Hit Testing Sample").ShowAsync();
			}
		}
	}
}
