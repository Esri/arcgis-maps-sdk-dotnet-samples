using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Globalization;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Network Analyst Tasks</category>
	public sealed partial class Routing : Page
    {
        public Routing()
        {
            this.InitializeComponent();
        }

        private async void mapView1_Tapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            var mp = e.Location;
            Graphic stop = new Graphic() { Geometry = mp };
            var stopsGraphicsLayer = mapView1.Map.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            stopsGraphicsLayer.Graphics.Add(stop);

            if (stopsGraphicsLayer.Graphics.Count > 1)
            {
                try
                {
                    var routeTask = new OnlineRouteTask(
						new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route"));
                    var routeParams = await routeTask.GetDefaultParametersAsync();

                    routeParams.SetStops(stopsGraphicsLayer.Graphics);
                    routeParams.UseTimeWindows = false;
                    routeParams.OutSpatialReference = mapView1.SpatialReference;
                    routeParams.DirectionsLengthUnit = LinearUnits.Miles;
					routeParams.DirectionsLanguage = new CultureInfo("en-Us"); // CultureInfo.CurrentCulture;

                    var result = await routeTask.SolveAsync(routeParams);
                    if (result != null)
                    {
                        if (result.Routes != null && result.Routes.Count > 0)
                        {
                            var firstRoute = result.Routes.FirstOrDefault();
                            var direction = firstRoute.RouteDirections.FirstOrDefault();

                            if (direction != null)
                            {
                                int totalMins = 0;
                                foreach (RouteDirection dir in firstRoute.RouteDirections)
                                    totalMins = totalMins + dir.Time.Minutes;

                                await new MessageDialog(string.Format("{0:N2} minutes", totalMins)).ShowAsync();
                            }

                            var routeLayer = mapView1.Map.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
                            routeLayer.Graphics.Add(firstRoute.RouteFeature as Graphic);
                        }
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
