using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates performing a geocode by submitting values for multiple address fields to a local locator.
    /// </summary>
    /// <title>Geocoding</title>
    /// <category>Offline</category>
    public partial class OfflineGeocoding : Windows.UI.Xaml.Controls.Page
    {
        private const string LOCATOR_PATH = @"locators\san-diego\san-diego-locator.loc";

        private LocalLocatorTask _locatorTask;
        private GraphicsLayer _graphicsLayer;

        /// <summary>Construct Offline Geocoding sample control</summary>
        public OfflineGeocoding()
        {
            InitializeComponent();

            mapView.Map.InitialExtent = new Envelope(-13044000, 3855000, -13040000, 3858000, SpatialReferences.WebMercator);
            _graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

            var _ = SetupRendererSymbols();
        }

        // Setup marker symbol and renderer
        private async Task SetupRendererSymbols()
        {
            var markerSymbol = new PictureMarkerSymbol() { Width = 48, Height = 48, YOffset = 24 };
            await markerSymbol.SetSourceAsync(
                new Uri("ms-appx:///Assets/RedStickpin.png"));

            _graphicsLayer.Renderer = new SimpleRenderer() { Symbol = markerSymbol, };
        }

        // Geocode input address and add result graphics to the map
        private async void FindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                listResults.Visibility = Visibility.Collapsed;
                _graphicsLayer.GraphicsSource = null;

                // Street, City, State, ZIP
                Dictionary<string, string> address = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(InputAddress.Text))
                    address.Add("Street", InputAddress.Text);
                if (!string.IsNullOrEmpty(City.Text))
                    address.Add("City", City.Text);
                if (!string.IsNullOrEmpty(State.Text))
                    address.Add("State", State.Text);
                if (!string.IsNullOrEmpty(Zip.Text))
                    address.Add("ZIP", Zip.Text);

                if (_locatorTask == null)
                {
                    var path = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, LOCATOR_PATH);
                    _locatorTask = await Task.Run<LocalLocatorTask>(() => new LocalLocatorTask(path));
                }

                var candidateResults = await _locatorTask.GeocodeAsync(
                    address, new List<string> { "Match_addr" }, mapView.SpatialReference, CancellationToken.None);

                _graphicsLayer.GraphicsSource = candidateResults
                    .Select(result => new Graphic(result.Location, new Dictionary<string, object> { { "Locator", result } }));

                await mapView.SetViewAsync(ExtentFromGraphics());
            }
            catch (AggregateException ex)
            {
                var innermostExceptions = ex.Flatten().InnerExceptions;
                if (innermostExceptions != null && innermostExceptions.Count > 0)
                {
                    var _ = new MessageDialog(string.Join(" > ", innermostExceptions.Select(i => i.Message).ToArray())).ShowAsync();
                }
                else
                {
                    var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
                }
            }
            catch (System.Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
                if (_graphicsLayer.GraphicsSource != null)
                    listResults.Visibility = Visibility.Visible;
            }
        }

        // Helper method to retrieve an extent from graphics in the graphics layer
        private Envelope ExtentFromGraphics()
        {
            var graphics = _graphicsLayer.GraphicsSource;
            if (graphics == null || graphics.Count() == 0)
                return mapView.Extent;

            var extent = graphics.First().Geometry.Extent;
            foreach (var graphic in graphics)
            {
                MapPoint point = graphic.Geometry as MapPoint;
                if (point == null)
                    continue;

                if (point.X < extent.XMin)
                    extent.XMin = point.X;
                if (point.Y < extent.YMin)
                    extent.YMin = point.Y;
                if (point.X > extent.XMax)
                    extent.XMax = point.X;
                if (point.Y > extent.YMax)
                    extent.YMax = point.Y;
            }

            return extent.Expand(2);
        }

        private void listResults_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            _graphicsLayer.ClearSelection();

            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                var graphic = e.AddedItems[0] as Graphic;
                graphic.IsSelected = true;
            }
        }
    }

    /// <summary>
    /// MapPoint to formatted string value converter
    /// </summary>
    internal class PointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MapPoint)
            {
                var point = value as MapPoint;
                return string.Format("{0:0.000},{1:0.000}", point.X, point.Y);
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
