using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Network Analyst Tasks</category>
	public sealed partial class DrivingDirections : Page
    {
        public DrivingDirections()
        {
            this.InitializeComponent();
            mapView1.Map.InitialExtent = new Envelope(-123, 33, -115, 37);
        }

        private async void GetDirections_Click(object sender, RoutedEventArgs e)
        {
            //Reset
            DirectionsStackPanel.Children.Clear();
            var _stops = new List<Graphic>();
            var _locator = new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"), "");
            var routeLayer = mapView1.Map.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
            routeLayer.Graphics.Clear();
            try
            {
                var fields = new List<string> { "Loc_name" };
                //Geocode from address
                var fromLocation = await _locator.GeocodeAsync(ParseAddress(FromTextBox.Text), fields, CancellationToken.None);
                if (fromLocation != null && fromLocation.Count > 0)
                {
                    var result = fromLocation.FirstOrDefault();
                    Graphic graphicLocation = new Graphic() { Geometry = result.Location, Symbol = LayoutRoot.Resources["FromSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol };
                    graphicLocation.Attributes["address"] = result.Address;
                    graphicLocation.Attributes["score"] = result.Score;
                    _stops.Add(graphicLocation);
                    routeLayer.Graphics.Add(graphicLocation);
                }
                //Geocode to address
                var toLocation = await _locator.GeocodeAsync(ParseAddress(ToTextBox.Text), fields, CancellationToken.None);
                if (toLocation != null && toLocation.Count > 0)
                {
                    var result = toLocation.FirstOrDefault();
                    Graphic graphicLocation = new Graphic() { Geometry = result.Location, Symbol = LayoutRoot.Resources["ToSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol };
                    graphicLocation.Attributes["address"] = result.Address;
                    graphicLocation.Attributes["score"] = result.Score;
                    _stops.Add(graphicLocation);
                    routeLayer.Graphics.Add(graphicLocation);
                }

                var routeTask = new OnlineRouteTask(new Uri("http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route"));
                RouteParameters routeParams = await routeTask.GetDefaultParametersAsync();
                routeParams.ReturnRoutes = true;
                routeParams.ReturnDirections = true;
                routeParams.DirectionsLengthUnit = LinearUnits.Miles;
                routeParams.UseTimeWindows = false;
                routeParams.OutSpatialReference = mapView1.SpatialReference;
                routeParams.Stops = new FeaturesAsFeature(routeLayer.Graphics);

                var routeTaskResult = await routeTask.SolveAsync(routeParams);
                _directionsFeatureSet = routeTaskResult.Routes.FirstOrDefault();

                _directionsFeatureSet.RouteGraphic.Symbol = LayoutRoot.Resources["RouteSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol;
                routeLayer.Graphics.Add(_directionsFeatureSet.RouteGraphic);

                var totalLength = _directionsFeatureSet.GetTotalLength(LinearUnits.Miles);
                var calculatedLength = _directionsFeatureSet.RouteDirections.Sum(x => x.GetLength(LinearUnits.Miles));

                TotalDistanceTextBlock.Text = string.Format("Total Distance: {0:N3} miles", totalLength); 

                TotalTimeTextBlock.Text = string.Format("Total Time: {0}", FormatTime(_directionsFeatureSet.TotalTime.TotalMinutes));
                TitleTextBlock.Text = _directionsFeatureSet.RouteName;

                int i = 1;
                foreach (var item in _directionsFeatureSet.RouteDirections)
                {
                    TextBlock textBlock = new TextBlock() { Text = string.Format("{0:00} - {1}", i, item.Text), Tag = item.Geometry, Margin = new Thickness(0, 15,0,0), FontSize = 20 };
                    textBlock.Tapped += TextBlock_Tapped;
                    DirectionsStackPanel.Children.Add(textBlock);

                    var secondarySP = new StackPanel() { Orientation = Orientation.Horizontal };
                    if (item.Time.TotalMinutes > 0)
                    {
                        var timeTb = new TextBlock { Text = string.Format("Time : {0:N2} minutes", item.Time.TotalMinutes), Margin= new Thickness(45,0,0,0) };
                        secondarySP.Children.Add(timeTb);
                        var distTb = new TextBlock { Text = string.Format("Distance : {0:N2} miles", item.GetLength(LinearUnits.Miles)), Margin = new Thickness(25, 0, 0, 0) };
                        secondarySP.Children.Add(distTb);

                        DirectionsStackPanel.Children.Add(secondarySP);
                    }
                    i++;
                }

                mapView1.SetView(_directionsFeatureSet.RouteGraphic.Geometry.Extent.Expand(1.2));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        Route _directionsFeatureSet;
        Graphic _activeSegmentGraphic;
        private void TextBlock_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            TextBlock textBlock = sender as TextBlock;
            var geom = textBlock.Tag as Geometry;
            if (geom != null)
            {
                mapView1.SetView(geom.Extent.Expand(1.2));
                if (_activeSegmentGraphic == null)
                {
                    _activeSegmentGraphic = new Graphic() { Symbol = LayoutRoot.Resources["SegmentSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol };
                    GraphicsLayer graphicsLayer = mapView1.Map.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
                    graphicsLayer.Graphics.Add(_activeSegmentGraphic);
                }
                _activeSegmentGraphic.Geometry = geom;
            }
        }

        void StackPanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (_directionsFeatureSet != null)
            {
                mapView1.SetView(_directionsFeatureSet.RouteGraphic.Geometry.Extent.Expand(0.6));
            }
        }

        private IDictionary<string, string> ParseAddress(string addressStr)
        {
            string[] fromArray = addressStr.Split(new char[] { ',' });
            var address = new Dictionary<string, string>();
            address.Add("Address", fromArray[0]);
            address.Add("City", fromArray[1]);
            address.Add("State", fromArray[2]);
            address.Add("Zip", fromArray[3]);
            address.Add("Country", "USA");

            //param.OutFields.Add("Loc_name");

            return address;
        }

        private string FormatDistance(double dist, string units)
        {
            string result = "";
            double formatDistance = Math.Round(dist, 2);
            if (formatDistance != 0)
            {
                result = formatDistance + " " + units;
            }
            return result;
        }

        private string FormatTime(double minutes)
        {
            TimeSpan time = TimeSpan.FromMinutes(minutes);
            string result = "";
            int hours = (int)Math.Floor(time.TotalHours);
            if (hours > 1)
                result = string.Format("{0} hours ", hours);
            else if (hours == 1)
                result = string.Format("{0} hour ", hours);
            if (time.Minutes > 1)
                result += string.Format("{0} minutes ", time.Minutes);
            else if (time.Minutes == 1)
                result += string.Format("{0} minute ", time.Minutes);
            return result;
        }
    }
}
