using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// 
    /// </summary>
    /// <category>Geocode Tasks</category>
    public sealed partial class BatchGeocoding : Page
    {
        LocatorTask _locatorTask;
        ObservableCollection<IDictionary<string, string>> batchaddresses = new ObservableCollection<IDictionary<string, string>>();
        GraphicsLayer geocodedResults;
        Graphic MapTipGraphic = null;

        public BatchGeocoding()
        {
            this.InitializeComponent();

			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-15000000, 2000000, -7000000, 8000000, SpatialReferences.WebMercator));

            _locatorTask = new OnlineLocatorTask
                (new Uri("http://serverapps101.esri.com/arcgis/rest/services/USA_Geocode/GeocodeServer", UriKind.Absolute), "");

            geocodedResults = mapView1.Map.Layers["LocationGraphicsLayer"] as GraphicsLayer;

            //List of addresses to geocode
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "4409 Redwood Dr" }, { "Zip", "92501" } });
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "3758 Cedar St" }, { "Zip", "92501" } });
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "3486 Orange St" }, { "Zip", "92501" } });
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "2999 4th St" }, { "Zip", "92507" } });
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "3685 10th St" }, { "Zip", "92501" } });

            AddressListbox.ItemsSource = batchaddresses;

        }

        private async void BatchGeocodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (batchaddresses.Count > 0)
            {
                var addresses = batchaddresses.ToList();
                var fields = new List<string>() { "*" };
                foreach (var address in addresses)
                {
                    var result = await _locatorTask.GeocodeAsync(address, fields, mapView1.SpatialReference, CancellationToken.None);

                    if (result != null && result.Count() > 0)
                    {
                        geocodedResults.Graphics.Clear();
                        foreach (var candidate in result)
                        {
                            Graphic graphic = new Graphic() { Geometry = candidate.Location };
                            graphic.Attributes.Add("X", candidate.Attributes["X"]);
                            graphic.Attributes.Add("Y", candidate.Attributes["Y"]);
                            graphic.Attributes.Add("Match_addr", candidate.Attributes["Match_addr"]);
                            graphic.Attributes.Add("Score", candidate.Attributes["Score"]);
                            geocodedResults.Graphics.Add(graphic);
                        }
                    }
                }
                ZoomToExtent();
            }
        }

        private void ZoomToExtent()
        {
            Envelope extent = null;
            foreach (Graphic g in geocodedResults.Graphics)
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
        private void addtolist_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StreetTextBox.Text) || string.IsNullOrEmpty(ZipTextBox.Text))
            {
                //MessageBox.Show("Value is required for all inputs");
                return;
            }

            int number;
            bool result = Int32.TryParse(ZipTextBox.Text, out number);

            if (!result)
            {
                //MessageBox.Show("Incorrect Zip format");
                return;
            }
            batchaddresses.Add(new Dictionary<string, string> { { "Street", StreetTextBox.Text }, { "Zip", ZipTextBox.Text } });
        }

        private void ResetList_Click(object sender, RoutedEventArgs e)
        {
            batchaddresses.Clear();
            geocodedResults.Graphics.Clear();
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
                // var mp = GeometryEngine.Project(anchor, mapView1.SpatialReference) as MapPoint;
                //Convert anchor point to screen MapPoint
                var screen = mapView1.LocationToScreen(anchor);

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
            var hitGraphic = await geocodedResults.HitTestAsync(mapView1, e.Position);
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
