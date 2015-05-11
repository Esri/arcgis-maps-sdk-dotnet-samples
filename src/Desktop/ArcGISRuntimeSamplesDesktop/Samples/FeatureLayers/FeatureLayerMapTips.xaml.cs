using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample shows how to display a map tip for a feature layer.  In this example, the MouseMove event of the MapView is handled with code that performs a FeatureLayer HitTest / Query combination which returns a single feature for display in the mapTip element defined in the XAML.
	/// </summary>
	/// <title>Map Tips</title>
	/// <category>Layers</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class FeatureLayerMapTips : UserControl
	{
		private bool _isMapReady;

		/// <summary>Construct Map Tips sample</summary>
		public FeatureLayerMapTips()
		{
			InitializeComponent();

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		private async void MyMapView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			await MyMapView.LayersLoadedAsync();
			_isMapReady = true;
		}

		private async void MyMapView_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMapReady)
				return;

			try
			{
				_isMapReady = false;

				Point screenPoint = e.GetPosition(MyMapView);
				var rows = await shelters.HitTestAsync(MyMapView, screenPoint);
				if (rows != null && rows.Length > 0)
				{
					var features = await shelters.FeatureTable.QueryAsync(rows);
					mapTip.DataContext = features.FirstOrDefault();
					mapTip.Visibility = System.Windows.Visibility.Visible;
				}
				else
					mapTip.Visibility = System.Windows.Visibility.Collapsed;
			}
			catch
			{
				mapTip.Visibility = System.Windows.Visibility.Collapsed;
			}
			finally
			{
				_isMapReady = true;
			}
		}
	}
}
