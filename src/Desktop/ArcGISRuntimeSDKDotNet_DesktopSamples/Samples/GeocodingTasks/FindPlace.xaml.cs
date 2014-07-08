using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to use the FindAsync method of an OnlineLocatorTask object to find places by name.
    /// </summary>
    /// <title>Find a Place</title>
    /// <category>Tasks</category>
    /// <subcategory>Geocoding</subcategory>
    public partial class FindPlace : UserControl
    {
        private const string OnlineLocatorUrl = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";

        private GraphicsLayer _addressGraphicsLayer;
        private OnlineLocatorTask _locatorTask;

        /// <summary>Construct find place sample control</summary>
        public FindPlace()
        {
            InitializeComponent();

            var ext = new Envelope(-117.207, 32.686, -117.079, 32.739, SpatialReferences.Wgs84);
            mapView.Map.InitialViewpoint = ext;

            _addressGraphicsLayer = mapView.Map.Layers["AddressGraphicsLayer"] as GraphicsLayer;

            _locatorTask = new OnlineLocatorTask(new Uri(OnlineLocatorUrl));
            _locatorTask.AutoNormalize = true;

            listResults.ItemsSource = _addressGraphicsLayer.Graphics;

            var task = SetSimpleRendererSymbols();
        }

        // Setup the pin graphic and graphics layer renderer
        private async Task SetSimpleRendererSymbols()
        {
            var markerSymbol = new PictureMarkerSymbol() { Width = 48, Height = 48, YOffset = 24 };
            await markerSymbol.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedStickpin.png"));
            var renderer = new SimpleRenderer() { Symbol = markerSymbol };

            _addressGraphicsLayer.Renderer = renderer;
        }

        // Find matching places, create graphics and add them to the UI
        private async void FindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                listResults.Visibility = Visibility.Collapsed;
                _addressGraphicsLayer.Graphics.Clear();

                var param = new OnlineLocatorFindParameters(SearchTextBox.Text)
                {
                    SearchExtent = mapView.Extent,
                    Location = mapView.Extent.GetCenter(),
                    MaxLocations = 5,
                    OutSpatialReference = mapView.SpatialReference,
                    OutFields = new string[] { "Place_addr" }
                };

                var candidateResults = await _locatorTask.FindAsync(param, CancellationToken.None);

                if (candidateResults == null || candidateResults.Count == 0)
                    throw new Exception("No candidates found in the current map extent.");

                foreach (var candidate in candidateResults)
                    AddGraphicFromLocatorCandidate(candidate);

                var extent = GeometryEngine.Union(_addressGraphicsLayer.Graphics.Select(g => g.Geometry)).Extent.Expand(1.1);
                await mapView.SetViewAsync(extent);

                listResults.Visibility = Visibility.Visible;
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

        private void AddGraphicFromLocatorCandidate(LocatorFindResult candidate)
        {
            var location = GeometryEngine.Project(candidate.Feature.Geometry, SpatialReferences.Wgs84) as MapPoint;

            var graphic = new Graphic(location);
            graphic.Attributes["Name"] = candidate.Name;
            graphic.Attributes["Address"] = candidate.Feature.Attributes["Place_addr"];
            graphic.Attributes["LocationDisplay"] = string.Format("{0:0.000}, {1:0.000}", location.X, location.Y);

            _addressGraphicsLayer.Graphics.Add(graphic);
        }
    }
}
