using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Globalization;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Network Analyst Tasks</category>
	public sealed partial class RoutingWithBarriers : Page
    {
        public RoutingWithBarriers()
        {
            this.InitializeComponent();
        }

        private void mapView1_Tapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            var mp = mapView1.ScreenToLocation(e.Position);
            Graphic g = new Graphic() { Geometry = mp };
            var stopsLayer = mapView1.Map.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            var barriersLayer = mapView1.Map.Layers["MyBarriersGraphicsLayer"] as GraphicsLayer;
            if (StopsRadioButton.IsChecked.Value)
            {
                stopsLayer.Graphics.Add(g);
            }
            else if (BarriersRadioButton.IsChecked.Value)
            {
                barriersLayer.Graphics.Add(g);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var layer in mapView1.Map.Layers)
                if (layer is GraphicsLayer)
                    (layer as GraphicsLayer).Graphics.Clear();
        }

        private async void OnSolveRouteClicked(object sender, RoutedEventArgs e)
        {

            var stopsLayer = mapView1.Map.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            var barriersLayer = mapView1.Map.Layers["MyBarriersGraphicsLayer"] as GraphicsLayer;

            if (stopsLayer.Graphics.Count > 1)
            {
                try
                {
                    OnlineRouteTask routeTask = new OnlineRouteTask
						(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route"));
                    RouteParameters routeParams = await routeTask.GetDefaultParametersAsync();
					routeParams.SetStops(stopsLayer.Graphics);
                    routeParams.UseTimeWindows = false;
                    routeParams.OutSpatialReference = mapView1.SpatialReference;
                    routeParams.SetPointBarriers(barriersLayer.Graphics);
                    routeParams.OutputGeometryPrecision = 1;
                    routeParams.DirectionsLengthUnit = LinearUnits.Miles;
					routeParams.DirectionsLanguage = new CultureInfo("en-Us"); // CultureInfo.CurrentCulture;

                    var result = await routeTask.SolveAsync(routeParams);

                    if (result != null)
                    {
                        GraphicsLayer routeLayer = mapView1.Map.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
                        routeLayer.Graphics.Clear();


                        foreach (var route in result.Routes)
                            routeLayer.Graphics.Add(route.RouteFeature as Graphic);

                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        
    }
}
