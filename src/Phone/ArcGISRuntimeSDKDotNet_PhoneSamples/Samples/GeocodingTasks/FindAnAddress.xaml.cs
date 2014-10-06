using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geocode Tasks</category>
	public sealed partial class FindAnAddress : Page
    {
        LocatorTask _locatorTask;
        GraphicsLayer _candidateGraphicsLayer;
        Graphic MapTipGraphic = null;

        public FindAnAddress()
        {
            this.InitializeComponent();
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-122.554, 37.615, -122.245, 37.884, SpatialReferences.Wgs84));
			_candidateGraphicsLayer = mapView1.Map.Layers["CandidateGraphicsLayer"] as GraphicsLayer;
        }

        private async void FindAddressButton_Click(object sender, RoutedEventArgs e)
        {
            _locatorTask = new OnlineLocatorTask
                (new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer", UriKind.Absolute), "");



            Dictionary<string, string> address = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(InputAddress.Text))
                address.Add("Address", InputAddress.Text);
            if (!string.IsNullOrEmpty(City.Text))
                address.Add("City", City.Text);
            if (!string.IsNullOrEmpty(State.Text))
                address.Add("Region", State.Text);
            if (!string.IsNullOrEmpty(Zip.Text))
                address.Add("Postal", Zip.Text);

            var candidateFields = new List<string> {"Address", "City","Region", "Postal" };


            try
            {
                var results = await _locatorTask.GeocodeAsync(address, candidateFields, new CancellationTokenSource().Token);

                if (_candidateGraphicsLayer.Graphics != null && _candidateGraphicsLayer.Graphics.Count > 0)
                    _candidateGraphicsLayer.Graphics.Clear();

                if (results != null)
                {
                    if (results.Count == 0)
                        await new MessageDialog("Cannot find this address").ShowAsync();
                    else
                    {
                        foreach (var candidate in results)
                        {
                            if (candidate.Score >= 80)
                            {
                                Graphic graphic = new Graphic()
                                {
                                    Geometry = new MapPoint(candidate.Location.X, candidate.Location.Y, SpatialReferences.Wgs84)
                                };

                                graphic.Attributes.Add("Address", candidate.Address);

                                string latlon = String.Format("{0}, {1}", candidate.Location.X, candidate.Location.Y);
                                graphic.Attributes.Add("LatLon", latlon);
                                _candidateGraphicsLayer.Graphics.Add(graphic);
                            }
                        }
                        //ZoomToExtent();
                    }
                    
                }
            }
            catch (Exception)
            {
            }
        }

        private void ZoomToExtent()
        {
            Envelope extent = null;
            foreach (Graphic g in _candidateGraphicsLayer.Graphics)
            {
                Envelope tempEnv = GetDisplayExtent(g.Geometry as MapPoint, mapView1.ActualHeight, mapView1.ActualWidth);
                if (extent == null)
                    extent = tempEnv;
                else
                    extent = extent.Union(GetDisplayExtent(g.Geometry as MapPoint, mapView1.ActualHeight, mapView1.ActualWidth));
            }
            if (extent != null)
                mapView1.SetView(extent);

        }

        private Envelope GetDisplayExtent(MapPoint point, double mapHeight, double mapWidth)
        {
            double halfWidth = 0.29858214173896908 * mapWidth / 2;
            double halfHeight = 0.29858214173896908 * mapHeight / 2;
            Envelope newExtent = new Envelope(point.X - halfWidth, point.Y - halfHeight,
                point.X + halfWidth, point.Y + halfHeight);
            return newExtent;

        }
       

        private void RenderMapTip()
        {
            MapPoint anchor = MapTipGraphic.Geometry as MapPoint;
            if (mapView1.SpatialReference != null)
            {
                if (MapTipGraphic != null)
                {
                    maptip.DataContext = MapTipGraphic.Attributes;
                }
                //Convert anchor point to the spatial reference of the map
                var mp = GeometryEngine.Project(anchor, mapView1.SpatialReference) as MapPoint;
                //Convert anchor point to screen MapPoint
                var screen = mapView1.LocationToScreen(mp);

                if (screen.X >= 0 && screen.Y >= 0 &&
                    screen.X < mapView1.ActualWidth && screen.Y < mapView1.ActualHeight)
                {
                    //Update location of map
                    MapTipTranslate.X = screen.X;
                    MapTipTranslate.Y = screen.Y - maptip.ActualHeight;
                    maptip.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else //Anchor is outside the display so close maptip
                {
                    maptip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

     

        private void maptip_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            RenderMapTip();
        }

        private async void mapView1_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            var hitGraphic = await _candidateGraphicsLayer.HitTestAsync(mapView1, e.Position);
            if (hitGraphic != null)
            {
                if (maptip.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                {
                    MapTipGraphic = hitGraphic;
                    RenderMapTip();
                }
                else
                {
                    maptip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    maptip.DataContext = null;
                    MapTipGraphic = null;
                }
            }
        }
    }
}
