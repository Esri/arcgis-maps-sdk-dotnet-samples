using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geocode Tasks</category>
	public sealed partial class FindAnAddressUsingArcGISOnline : Page
    {
        GraphicsLayer _candidateGraphicsLayer;
        Graphic _mapTipGraphic = null;
        private LocatorFindParameters _locatorFindParameter;
        private string _emptySearchText = "Enter an address or place name...";

        public FindAnAddressUsingArcGISOnline()
        {
            this.InitializeComponent();
            var ext = new Envelope(-122.554, 37.615, -122.245, 37.884, SpatialReferences.Wgs84);

			mapView1.Map.InitialViewpoint = new Viewpoint(ext);

            _candidateGraphicsLayer = mapView1.Map.Layers["CandidateGraphicsLayer"] as GraphicsLayer;
        }

        private async void SingleLineAddressButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnlineLocatorTask locatorTask = 
                    new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer", UriKind.Absolute), "");

                var text = SingleLineAddressText.Text;
                if (string.IsNullOrEmpty(text))
                    return;

                _locatorFindParameter = new OnlineLocatorFindParameters(text)
                {
                    Text = text,
                    Location = mapView1.Extent.GetCenter(),
                    Distance = mapView1.Extent.Width / 2,
                    MaxLocations = 5,
                    OutSpatialReference = mapView1.SpatialReference,
                    OutFields = new List<string>() { "*" }
                };

                CancellationToken cancellationToken = new CancellationTokenSource().Token;
                var results = await locatorTask.FindAsync(_locatorFindParameter, cancellationToken);
                if (_candidateGraphicsLayer.Graphics != null && _candidateGraphicsLayer.Graphics.Count > 0)
                    _candidateGraphicsLayer.Graphics.Clear();

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        var geom = result.Feature.Geometry;
                        Graphic graphic = new Graphic()
                        {
                            Geometry = geom
                        };

                        graphic.Attributes.Add("Name", result.Feature.Attributes["Match_addr"]);
                        if (geom.GeometryType == GeometryType.Point)
                        {
                            var pt = geom as MapPoint;
                            string latlon = String.Format("{0}, {1}", pt.X, pt.Y);
                            graphic.Attributes.Add("LatLon", latlon);
                        }

                        _candidateGraphicsLayer.Graphics.Add(graphic);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        //private void GraphicsLayer_PointerEntered_1(object sender, GraphicPointerRoutedEventArgs e)
        //{
        //    _mapTipGraphic = e.Graphic;
        //    RenderMapTip();
        //}

        private void RenderMapTip()
        {
            MapPoint anchor = _mapTipGraphic.Geometry as MapPoint;
            if (mapView1.SpatialReference != null)
            {
                if (_mapTipGraphic != null)
                {
                    mapView1tip.DataContext = _mapTipGraphic.Attributes;
                }
                //Convert anchor point to the spatial reference of the map
                var mp = GeometryEngine.Project(anchor, mapView1.SpatialReference) as MapPoint;

                //Convert anchor point to screen coordinate
                var screen = mapView1.LocationToScreen(mp);

                if (screen.X >= 0 && screen.Y >= 0 &&
                    screen.X < mapView1.ActualWidth && screen.Y < mapView1.ActualHeight)
                {
                    //Update location of map
                    MapTipTranslate.X = screen.X;
                    MapTipTranslate.Y = screen.Y - mapView1tip.ActualHeight;
                    mapView1tip.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else //Anchor is outside the display so close mapView1tip
                {
                    mapView1tip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        //private void GraphicsLayer_PointerExited_1(object sender, GraphicPointerRoutedEventArgs e)
        //{
        //    mapView1tip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        //    mapView1tip.DataContext = null;
        //    _mapTipGraphic = null;
        //}

        private void maptip_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            RenderMapTip();
        }



        private void SingleLineAddressText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SingleLineAddressText.Text == _emptySearchText)
            {
                SingleLineAddressText.Text = "";
                SingleLineAddressButton.IsEnabled = true;
            }
        }

        private void SingleLineAddressText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SingleLineAddressText.Text))
            {
                SingleLineAddressText.Text = _emptySearchText;
                SingleLineAddressButton.IsEnabled = false;
            }

        }

        private void SingleLineAddressText_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SingleLineAddressButton_Click(sender, e);
        }


        private async void mapView1_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            var hitGraphic = await _candidateGraphicsLayer.HitTestAsync(mapView1, e.Position);
            if (hitGraphic != null)
            {
                if (mapView1tip.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                {
                    _mapTipGraphic = hitGraphic;
                    RenderMapTip();
                }
                else
                {
                    mapView1tip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    mapView1tip.DataContext = null;
                    _mapTipGraphic = null;
                }
            }
        }
    }
}
