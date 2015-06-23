using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// This sample demonstrates how to add the ArcGIS Traffic service to a map.
	/// </summary>
	/// <title>Traffic</title>
	/// <category>ArcGIS Online Services</category>
	public partial class Traffic : Page
	{
		private ArcGISDynamicMapServiceLayer _trafficLayer;
		private FrameworkElement _trafficOverlay;

		public Traffic()
		{
			this.InitializeComponent();
			IdentityManager.Current.OAuthAuthorizeHandler = new OAuthAuthorizeHandler();
			IdentityManager.Current.ChallengeHandler = new ChallengeHandler(PortalSecurity.Challenge);

			_trafficLayer = MyMapView.Map.Layers["Traffic"] as ArcGISDynamicMapServiceLayer;
			_trafficLayer.VisibleLayers = new ObservableCollection<int>() { 2, 3, 4, 6, 7 };

			_trafficOverlay = MyMapView.Overlays.Items[0] as FrameworkElement;

			MyMapView.LayerLoaded += MyMapView_LayerLoaded;
		}

		// Populate layer legend with north america traffic sublayer names
		private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.Layer == _trafficLayer)
			{
				var legendLayer = _trafficLayer as ILegendSupport;
				var layerLegendInfo = await legendLayer.GetLegendInfosAsync();
				legendTree.ItemsSource = layerLegendInfo.LayerLegendInfos.First().LayerLegendInfos;
			}
		}

		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			try
			{
				_trafficOverlay.Visibility = Visibility.Collapsed;
				_trafficOverlay.DataContext = null;

				var identifyTask = new IdentifyTask(new Uri(_trafficLayer.ServiceUri));
				
				// Get current viewpoints extent from the MapView
				var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
				var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				IdentifyParameters identifyParams = new IdentifyParameters(e.Location, viewpointExtent, 5, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth)
				{
					LayerIDs = new int[] { 2, 3, 4 },
					LayerOption = LayerOption.Top,
					SpatialReference = MyMapView.SpatialReference,
				};

				var result = await identifyTask.ExecuteAsync(identifyParams);

				if (result != null && result.Results != null && result.Results.Count > 0)
				{
					_trafficOverlay.DataContext = result.Results.First();
					_trafficOverlay.Visibility = Visibility.Visible;
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}
	}
}
