using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to use the FindAsync method of an OnlineLocatorTask object to find places by name.  This sample adds places to the map and list view that match the place name and are above the specified score range.
    /// </summary>
    /// <title>Find a Place</title>
	/// <category>Tasks</category>
	/// <subcategory>Geocoding</subcategory>
	public partial class FindPlace : UserControl
    {
        private OnlineLocatorTask _locatorTask;

        /// <summary>Construct find place sample control</summary>
        public FindPlace()
        {
            InitializeComponent();

            mapView.Map.InitialExtent = new Envelope(-13043302, 3856091, -13040394, 3857406, SpatialReferences.WebMercator);

            _locatorTask = new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"));
            _locatorTask.AutoNormalize = true;

            var task = SetSimpleRendererSymbols();
        }

        // Setup the pin graphic and graphics layer renderer
        private async Task SetSimpleRendererSymbols()
        {
            var markerSymbol = new PictureMarkerSymbol() { Width = 48, Height = 48, YOffset = 24 };
            await markerSymbol.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedStickpin.png"));
            var renderer = new SimpleRenderer() { Symbol = markerSymbol };

            addressesGraphicsLayer.Renderer = renderer;
        }

        // Find matching places, create graphics and add them to the UI
        private async void FindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                addressesGraphicsLayer.Graphics.Clear();

                var param = new OnlineLocatorFindParameters(SearchTextBox.Text)
                {
                    SearchExtent = mapView.Extent,
                    Location = mapView.Extent.GetCenter(),
                    Distance = mapView.Extent.Width / 2,
                    MaxLocations = 5,
                    OutSpatialReference = mapView.SpatialReference,
                    OutFields = OutFields.All
                };

                var candidateResults = await _locatorTask.FindAsync(param, CancellationToken.None);

                var graphics = candidateResults
                    .Select(result => new Graphic(result.Feature.Geometry, new Dictionary<string, object> { { "Locator", result } }));
                
                foreach (var graphic in graphics)
                    addressesGraphicsLayer.Graphics.Add(graphic);
            }
            catch (AggregateException ex)
            {
                var innermostExceptions = ex.Flatten().InnerExceptions;
                if (innermostExceptions != null && innermostExceptions.Count > 0)
                    MessageBox.Show(string.Join(" > ", innermostExceptions.Select(i => i.Message).ToArray()));
                else
                    MessageBox.Show(ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
