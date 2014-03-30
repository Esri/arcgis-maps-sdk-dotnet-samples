using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Network Analyst Tasks</category>
	public sealed partial class RoutingClosestFacility : Page
    {
        private static readonly string NETWORKSERVICE = "http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route";
        OnlineRouteTask _onlineRouteTask;
        private GraphicsLayer _facilitiesGraphicsLayer;
        private GraphicsLayer _stopsGraphicsLayer;
        private GraphicsLayer _routeGraphicsLayer;
        private GraphicsLayer _barrierGraphicsLayer;

        List<Graphic> _stops;
        List<Graphic> _polylineBarriers;
        List<Graphic> _polygonBarriers;
        Random random;
        public RoutingClosestFacility()
        {
            this.InitializeComponent();
            _facilitiesGraphicsLayer = mapView1.Map.Layers["MyFacilitiesGraphicsLayer"] as GraphicsLayer;
            _stopsGraphicsLayer = mapView1.Map.Layers["MyIncidentsGraphicsLayer"] as GraphicsLayer;
            _routeGraphicsLayer = mapView1.Map.Layers["MyBarriersGraphicsLayer"] as GraphicsLayer;
            _barrierGraphicsLayer = mapView1.Map.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;

            _stops = new List<Graphic>();
            _polylineBarriers = new List<Graphic>();
            _polygonBarriers = new List<Graphic>();
            random = new Random();
            _onlineRouteTask= new OnlineRouteTask(new Uri(NETWORKSERVICE));
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void AddPointButton_Click(object sender, RoutedEventArgs e)
        {
            var stop = await mapView1.Editor.RequestShapeAsync(DrawShape.Point) as MapPoint;
            _stopsGraphicsLayer.Graphics.Add(new Graphic() { Geometry = stop });
            _stops.Add(new Graphic() { Geometry = stop });
        }

        private async void AddLineButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cutPolyLine = await mapView1.Editor.RequestShapeAsync(DrawShape.Polyline) as Polyline;
                _barrierGraphicsLayer.Graphics.Add(new Graphic() { Geometry = cutPolyLine });
            }
            catch (OperationCanceledException) { }
        }

        private async void AddPolygonButton_Click(object sender, RoutedEventArgs e)
        {
			try
			{
				var cutPolyLine = await mapView1.Editor.RequestShapeAsync(DrawShape.Polygon) as Polygon;
				_barrierGraphicsLayer.Graphics.Add(new Graphic() { Geometry = cutPolyLine });
			}
			catch (OperationCanceledException) { }
        }

        private async void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_stops.Count == 0)
                    return;

                RouteParameters routeParams = await _onlineRouteTask.GetDefaultParametersAsync();
                GenerateBarriers();

                  FeaturesAsFeature stopsFeatures = new FeaturesAsFeature();
            stopsFeatures.Features = _stops;
            routeParams.Stops = stopsFeatures;
                
                
                if (_polylineBarriers.Count > 0)
                {
                    FeaturesAsFeature polylineBarrierFeatures = new FeaturesAsFeature();
                    polylineBarrierFeatures.Features = _polylineBarriers;
                    routeParams.PolylineBarriers = polylineBarrierFeatures;
                }
                if (_polygonBarriers.Count > 0)
                {
                    FeaturesAsFeature polygonBarrierFeatures = new FeaturesAsFeature();
                    polygonBarrierFeatures.Features = _polygonBarriers;
                    routeParams.PolygonBarriers = polygonBarrierFeatures;
                }
             
                List<AttributeParameterValue> aps = new List<AttributeParameterValue>();
                AttributeParameterValue apv = GetAttributeParameterValue(AttributeParameter2.SelectionBoxItem.ToString().Trim());
                if (apv != null)
                    aps.Add(apv);
                //routeParams.AttributeParameterValues = aps;
                //routeParams.ReturnDirections = ReturnDirections2.IsChecked.HasValue ? ReturnDirections2.IsChecked.Value : false;
                routeParams.DirectionsLanguage = String.IsNullOrEmpty(DirectionsLanguage2.Text) ? new System.Globalization.CultureInfo("en-US") : new System.Globalization.CultureInfo(DirectionsLanguage2.Text);
                routeParams.DirectionsLengthUnit = GetDirectionsLengthUnits(DirectionsLengthUnits2.SelectionBoxItem.ToString().Trim());
                
                routeParams.ReturnRoutes = ReturnRoutes2.IsChecked.HasValue ? ReturnRoutes2.IsChecked.Value : false;
                routeParams.ReturnStops = ReturnFacilities2.IsChecked.HasValue ? ReturnFacilities2.IsChecked.Value : false;
                routeParams.ReturnPointBarriers = ReturnBarriers2.IsChecked.HasValue ? ReturnBarriers2.IsChecked.Value : false;
                routeParams.ReturnPolygonBarriers = ReturnPolygonBarriers2.IsChecked.HasValue ? ReturnPolygonBarriers2.IsChecked.Value : false;
                routeParams.ReturnPolylineBarriers = ReturnPolylineBarriers2.IsChecked.HasValue ? ReturnPolylineBarriers2.IsChecked.Value : false;

                routeParams.OutputLines = GetOutputLines(OutputLines2.SelectionBoxItem.ToString().Trim());
                routeParams.OutSpatialReference = string.IsNullOrEmpty(OutputSpatialReference2.Text) ? mapView1.SpatialReference : new SpatialReference(int.Parse(OutputSpatialReference2.Text));

                //routeParams.AccumulateAttributeNames = AccumulateAttributeNames2.Text.Split(',');
                routeParams.ImpedanceAttributeName = ImpedanceAttributeName2.Text;
                routeParams.RestrictionAttributeNames = RestrictionAttributeNames2.Text.Split(',');
                routeParams.RestrictUTurns = GetRestrictUTurns(RestrictUTurns2.SelectionBoxItem.ToString().Trim());
                routeParams.UseHierarchy = UseHierarchy2.IsChecked.HasValue ? UseHierarchy2.IsChecked.Value : false;
                routeParams.OutputGeometryPrecision = string.IsNullOrEmpty(OutputGeometryPrecision2.Text) ? 0 : double.Parse(OutputGeometryPrecision2.Text);
                routeParams.OutputGeometryPrecisionUnit = GetGeometryPrecisionUnits(OutputGeometryPrecisionUnits2.SelectionBoxItem.ToString().Trim());

               RouteResult result= await _onlineRouteTask.SolveAsync(routeParams);
               _routeGraphicsLayer.Graphics.Clear();

               foreach (Route route in result.Routes)
               {
                   Graphic g = route.RouteGraphic;
                   g.Symbol = LayoutRoot.Resources["RouteSymbol"] as SimpleLineSymbol;
                   _routeGraphicsLayer.Graphics.Add(g);
               }

             
            }
			catch (Exception ex)
			{
				var dlg = new MessageDialog(ex.Message, "Solve Failed!");
				var _ = dlg.ShowAsync();
			}
        }

        private OutputLine GetOutputLines(string outputLines)
        {
            OutputLine result = OutputLine.None;
            if (outputLines.Equals(string.Empty))
                return OutputLine.None;

            switch (outputLines.ToLower())
            {
                case "none":
                    result = OutputLine.None;
                    break;
                case "straight":
                    result = OutputLine.Straight;
                    break;
                case "true shape":
                    result = OutputLine.TrueShape;
                    break;
                default:
                    break;
            }
            return result;
        }

        private UTurnRestriction GetRestrictUTurns(string restrictUTurns)
        {
            UTurnRestriction result = UTurnRestriction.AllowBacktrack;
            switch (restrictUTurns.ToLower())
            {
                case "allow backtrack":
                    result = UTurnRestriction.AllowBacktrack;
                    break;
                case "at dead ends only":
                    result = UTurnRestriction.AtDeadEndsOnly;
                    break;
                case "no backtrack":
                    result = UTurnRestriction.NoBacktrack;
                    break;
                default:
                    break;
            }
            return result;
        }

        private LinearUnit GetDirectionsLengthUnits(string directionsLengthUnits)
        {
            switch (directionsLengthUnits.ToLower())
            {
                case "kilometers":
                    return LinearUnits.Kilometers;
                case "meters":
                    return LinearUnits.Meters;
                case "miles":
                    return LinearUnits.Miles;
                case "nautical miles":
                    return LinearUnits.NauticalMiles;
                default:
                    break;
            }
            throw new NotImplementedException();
        }

        private LinearUnit GetGeometryPrecisionUnits(string outputGeometryPrecisionUnits)
        {
            
            switch (outputGeometryPrecisionUnits.ToLower())
            {
                case "kilometers":
                    return LinearUnits.Kilometers;
                case "meters":
                    return LinearUnits.Meters;
                case "miles":
                    return LinearUnits.Miles;
                case "nautical miles":
                    return LinearUnits.NauticalMiles;
                case "inches":
                    return LinearUnits.Inches;
                case "feet":
                    return LinearUnits.Feet;
                case "yards":
                    return LinearUnits.Yards;
                case "millimeters":
                    return LinearUnits.Millimeters;
                case "centimeters":
                    return (LinearUnit)LinearUnit.Create(109006);
                default:
                    break;
            }
            throw new NotSupportedException();
        }


        private AttributeParameterValue GetAttributeParameterValue(string attributeParamSelection)
        {
            if (attributeParamSelection.Equals("None"))
                return null;

            if (attributeParamSelection.Equals("Other Roads"))
                return new AttributeParameterValue("Time","OtherRoads","5");

            return new AttributeParameterValue("Time", attributeParamSelection, attributeParamSelection.Replace(" MPH", ""));
        }


        public void GenerateBarriers()
        {
            foreach (Graphic g in _barrierGraphicsLayer.Graphics)
            {
                Type gType = g.Geometry.GetType();
if (gType == typeof(Polyline))
                    _polylineBarriers.Add(g);
                else if (gType == typeof(Polygon) || gType == typeof(Envelope))
                    _polygonBarriers.Add(g);
            }
        }

    }
}
