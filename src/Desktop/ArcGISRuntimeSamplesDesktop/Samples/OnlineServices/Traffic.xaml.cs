using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample shows how to add the ArcGIS Traffic service to a map.
    /// </summary>
    /// <title>Traffic</title>
    /// <category>ArcGIS Online Services</category>
    public partial class Traffic : UserControl
    {
        private ArcGISDynamicMapServiceLayer _trafficLayer;

        public Traffic()
        {
            InitializeComponent();
            IdentityManager.Current.OAuthAuthorizeHandler = new OAuthAuthorizeHandler();
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(PortalSecurity.Challenge);

            _trafficLayer = MyMapView.Map.Layers["Traffic"] as ArcGISDynamicMapServiceLayer;

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
                incidentOverlay.Visibility = Visibility.Collapsed;
                incidentOverlay.DataContext = null;

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
                    incidentOverlay.DataContext = result.Results.First();
                    incidentOverlay.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Identify Error");
            }
        }
    }
}
