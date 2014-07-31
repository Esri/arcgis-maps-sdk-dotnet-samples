using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Linq;
using Esri.ArcGISRuntime.Controls;
namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Geocode Tasks</category>
	public sealed partial class AddressOfLocation : Page
    {
        GraphicsLayer _locationGraphicsLayer;
        SpatialReference wgs84 = new SpatialReference(4326);
        SpatialReference mercator = new SpatialReference(102100);
        Graphic MapTipGraphic = null;

        public AddressOfLocation()
        {
            this.InitializeComponent();

            Envelope initial_extent = new Envelope(-117.387, 33.97, -117.355, 33.988, wgs84);

			mapView1.Map.InitialViewpoint = new Viewpoint(initial_extent);

            _locationGraphicsLayer = mapView1.Map.Layers["LocationGraphicsLayer"] as GraphicsLayer;

        }

        private async void mapView_Tapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {

            var hitGraphic = await _locationGraphicsLayer.HitTestAsync(mapView1, e.Position);
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
            else
            {
				maptip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				var mp = mapView1.ScreenToLocation(e.Position);

                Graphic g = new Graphic() { Geometry = mp };

                var layer = mapView1.Map.Layers.OfType<GraphicsLayer>().First();
                layer.Graphics.Add(g);

                var token = "";
                var locatorTask =
                    new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer", UriKind.Absolute), token);

                try
                {
                    var result = await locatorTask.ReverseGeocodeAsync(mp, 30, mapView1.SpatialReference, CancellationToken.None);

                    Graphic graphic = new Graphic() { Geometry = mp };

                    string latlon = String.Format("{0}, {1}", result.Location.X, result.Location.Y);
                    string address1 = result.AddressFields["Address"].ToString();
                    string address2 = String.Format("{0}, {1} {2}", result.AddressFields["City"], result.AddressFields["Region"], result.AddressFields["Postal"]);

                    graphic.Attributes.Add("LatLon", latlon);
                    graphic.Attributes.Add("Address1", address1);
                    graphic.Attributes.Add("Address2", address2);

                    _locationGraphicsLayer.Graphics.Add(graphic);
					MapTipGraphic = graphic;
					RenderMapTip();

                }
                catch (Exception)
                {
                }
            }
        }

        //private void GraphicsLayer_PointerEntered_1(object sender, GraphicPointerRoutedEventArgs e)
        //{
        //    MapTipGraphic = e.Graphic;
        //    RenderMapTip();
        //}

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

        private void mapView1_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {

        }


    }
}
