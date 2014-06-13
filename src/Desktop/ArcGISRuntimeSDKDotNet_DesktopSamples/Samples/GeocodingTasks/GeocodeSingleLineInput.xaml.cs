using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Performs a single line geocode using either an online ArcGIS Locator service or a local Locator.
    /// </summary>
	/// <category>Tasks</category>
	/// <subcategory>Geocoding</subcategory>
	public partial class GeocodeSingleLineInput : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region IsOnline Property

        private bool _isOnline = true;

        public bool IsOnline
        {
            get
            {
                return _isOnline;
            }
            set
            {
                _isOnline = value;

                SetupLocator();

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsOnline"));
                }
            }
        }

        #endregion

        #region MatchScore Property

        private double _matchScore = 100;

        public double MatchScore
        {
            get
            {
                return _matchScore;
            }
            set
            {
                _matchScore = value;

                UpdateCandidateGraphics();

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MatchScore"));
                }
            }
        }

        #endregion

        LocatorTask _locatorTask;
        IList<LocatorGeocodeResult> _candidateResults;
        GraphicsLayer _candidateAddressesGraphicsLayer;
        SpatialReference wgs84 = new SpatialReference(4326);
        SpatialReference webMercator = new SpatialReference(102100);
      
        public GeocodeSingleLineInput()
        {
            InitializeComponent();

            DataContext = this;

            // Min X,Y = -13,044,000  3,855,000 Meters
            // Max X,Y = -13,040,000  3,858,000 Meters
            map1.InitialExtent = new Envelope(-13044000, 3855000, -13040000, 3858000, webMercator);

            SetupLocator();

            _candidateAddressesGraphicsLayer = map1.Layers["CandidateAddressesGraphicsLayer"] as GraphicsLayer;

            var _ = SetSimpleRendererSymbols();
        }

        private async Task SetSimpleRendererSymbols()
        {
            PictureMarkerSymbol findResultMarkerSymbol = new PictureMarkerSymbol()
            {
                Width = 48,
                Height = 48,
                YOffset = 24
            };

            await findResultMarkerSymbol.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedStickpin.png"));
            SimpleRenderer findResultRenderer = new SimpleRenderer()
            {
                Symbol = findResultMarkerSymbol,
            };
            _candidateAddressesGraphicsLayer.Renderer = findResultRenderer;
        }

        private void SetupLocator()
        {
            if (!IsOnline)
            {
                _locatorTask = new LocalLocatorTask(@"..\..\..\..\..\samples-data\locators\san-diego\san-diego-locator.loc");
            }
            else
            {
                _locatorTask = new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"), string.Empty);
            }
        }

        private async void FindButton_Click(object sender, RoutedEventArgs e)
        {
            _candidateAddressesGraphicsLayer.Graphics.Clear();

            string text = SearchTextBox.Text;
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                LocatorServiceInfo locatorServiceInfo = await _locatorTask.GetInfoAsync();

                Dictionary<string, string> inputAddress = new Dictionary<string, string>() { { locatorServiceInfo.SingleLineAddressField.FieldName, text } };

                _candidateResults = await _locatorTask.GeocodeAsync(inputAddress, new List<string> { "Match_addr" }, mapView1.SpatialReference, CancellationToken.None);

                List<Graphic> candidategraphicsList = new List<Graphic>();

                foreach (LocatorGeocodeResult candidate in _candidateResults)
                {

                    Graphic graphic = new Graphic() { Geometry = candidate.Location };
                    graphic.Attributes.Add("Match_addr", candidate.Attributes["Match_addr"]);
                    graphic.Attributes.Add("Score", candidate.Score);

                    string latlon = String.Format("{0}, {1}", candidate.Location.X, candidate.Location.Y);
                    graphic.Attributes.Add("LatLon", latlon);
                    candidategraphicsList.Add(graphic);

                }

                _candidateAddressesGraphicsLayer.Graphics.AddRange(candidategraphicsList);

                UpdateCandidateGraphics();
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
        }

        private void UpdateCandidateGraphics()
        {
            foreach (Graphic g in _candidateAddressesGraphicsLayer.Graphics)
            {
                if (Convert.ToDouble(g.Attributes["Score"]) < MatchScore)
                {
                    g.IsVisible = false;
                }
                else
                    g.IsVisible = true;
            }
        }
    }
}
