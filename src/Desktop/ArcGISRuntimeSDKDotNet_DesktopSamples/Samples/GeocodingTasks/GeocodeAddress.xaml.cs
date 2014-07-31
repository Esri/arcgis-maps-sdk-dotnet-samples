using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to geocode an address using the OnlineLocatorTask.
    /// </summary>
    /// <title>Geocode an Address</title>
    /// <category>Tasks</category>
    /// <subcategory>Geocoding</subcategory>
    public partial class GeocodeAddress : UserControl
    {
        private const string OnlineLocatorUrl = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";

        private GraphicsLayer _addressGraphicsLayer;
        private LocatorServiceInfo _locatorServiceInfo;
        private OnlineLocatorTask _locatorTask;

        public GeocodeAddress()
        {
            InitializeComponent();

            var ext = new Envelope(-122.554, 37.615, -122.245, 37.884, SpatialReferences.Wgs84);
			MyMapView.Map.InitialViewpoint = new Viewpoint(ext);

            _addressGraphicsLayer = MyMapView.Map.Layers["AddressGraphicsLayer"] as GraphicsLayer;

            _locatorTask = new OnlineLocatorTask(new Uri(OnlineLocatorUrl));
            _locatorTask.AutoNormalize = true;

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

        private async void GeocodeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                listResults.Visibility = Visibility.Collapsed;
                _addressGraphicsLayer.Graphics.Clear();

                if (_locatorServiceInfo == null)
                    _locatorServiceInfo = await _locatorTask.GetInfoAsync();

                var candidateResults = await _locatorTask.GeocodeAsync(
                    GetInputAddressFromUI(), new List<string> { "Addr_type","Score","X","Y" }, MyMapView.SpatialReference, CancellationToken.None);

                if (candidateResults == null || candidateResults.Count == 0)
                    throw new Exception("No candidates found.");

                foreach (var candidate in candidateResults)
                    AddGraphicFromLocatorCandidate(candidate);

                listResults.ItemsSource = _addressGraphicsLayer.Graphics.Where(g => g.IsVisible);
                listResults.Visibility = Visibility.Visible;

                var extent = GeometryEngine.Union(_addressGraphicsLayer.Graphics.Select(g => g.Geometry)).Extent.Expand(1.1);
                await MyMapView.SetViewAsync(extent);
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

        private Dictionary<string, string> GetInputAddressFromUI()
        {
            Dictionary<string, string> address = new Dictionary<string, string>();

            if (btnSingleLine.IsChecked == true)
            {
                address[_locatorServiceInfo.SingleLineAddressField.FieldName] = txtSingleLine.Text;
            }
            else
            {
                if (!string.IsNullOrEmpty(InputAddress.Text))
                {
                    string fieldName = "Address";
                    address.Add(fieldName, InputAddress.Text);
                }
                if (!string.IsNullOrEmpty(City.Text))
                {
                    string fieldName = "City";
                    address.Add(fieldName, City.Text);
                }
                if (!string.IsNullOrEmpty(State.Text))
                {
                    string fieldName = "Region";
                    address.Add(fieldName, State.Text);
                }
                if (!string.IsNullOrEmpty(Zip.Text))
                {
                    string fieldName = "Postal";
                    address.Add(fieldName, Zip.Text);
                }
            }

            return address;
        }

        private void AddGraphicFromLocatorCandidate(LocatorGeocodeResult candidate)
        {
            var graphic = new Graphic(new MapPoint(candidate.Location.X, candidate.Location.Y, MyMapView.SpatialReference));
            graphic.Attributes["Address"] = candidate.Address;
            graphic.Attributes["Score"] = candidate.Score;
            graphic.Attributes["MatchType"] = candidate.Attributes["Addr_type"];

            double x = 0.0, y = 0.0;
            double.TryParse(candidate.Attributes["X"], out x);
            double.TryParse(candidate.Attributes["Y"], out y);
            graphic.Attributes["LocationDisplay"] = string.Format("{0:0.000}, {1:0.000}", x, y);

            graphic.IsVisible = (candidate.Score >= MatchScoreSlider.Value);
            _addressGraphicsLayer.Graphics.Add(graphic);
        }

        private void btnMultipleLine_Checked(object sender, RoutedEventArgs e)
        {
            if (listResults != null)
                listResults.Visibility = Visibility.Collapsed;
        }

        private void MatchScoreSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_addressGraphicsLayer == null)
                return;

            foreach (var graphic in _addressGraphicsLayer.Graphics)
            {
                graphic.IsVisible = (Convert.ToInt32(graphic.Attributes["Score"]) >= e.NewValue);
            }

            listResults.ItemsSource = _addressGraphicsLayer.Graphics.Where(g => g.IsVisible);
        }
    }
}
