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
    /// Demonstrates performing a geocode by submitting values for multiple address fields.
    /// </summary>
	/// <category>Tasks</category>
	/// <subcategory>Geocoding</subcategory>
	public partial class GeocodeFullAddressInput : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #region IsOnline Property

        private bool _isOnline = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is online.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is online; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets the match score.
        /// </summary>
        /// <value>
        /// The match score.
        /// </value>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GeocodeFullAddressInput"/> class.
        /// </summary>
        public GeocodeFullAddressInput()
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

            try
            {
                LocatorServiceInfo locatorServiceInfo = await _locatorTask.GetInfoAsync();

                Dictionary<string, string> address = new Dictionary<string, string>();

                //Street, City, State, ZIP

                if (!string.IsNullOrEmpty(InputAddress.Text))
                {
                    string fieldName = _isOnline == true ? "Address" : "Street";
                    address.Add(fieldName, InputAddress.Text);
                }
                if (!string.IsNullOrEmpty(City.Text))
                {
                    string fieldName = _isOnline == true ? "City" : "City";
                    address.Add(fieldName, City.Text);
                }
                if (!string.IsNullOrEmpty(State.Text))
                {
                    string fieldName = _isOnline == true ? "Region" : "State";
                    address.Add(fieldName, State.Text);
                }
                if (!string.IsNullOrEmpty(Zip.Text))
                {
                    string fieldName = _isOnline == true ? "Postal" : "ZIP";
                    address.Add(fieldName, Zip.Text);
                }
                _candidateResults = await _locatorTask.GeocodeAsync(address, new List<string> { "Match_addr" }, mapView1.SpatialReference, CancellationToken.None);

                List<Graphic> candidategraphicsList = new List<Graphic>();

                Envelope resultsEnvelope = null;

                foreach (LocatorGeocodeResult candidate in _candidateResults)
                {
                    MapPoint mapPoint = candidate.Location;
                    Graphic graphic = new Graphic() { Geometry = mapPoint };
                    graphic.Attributes.Add("Match_addr", candidate.Attributes["Match_addr"]);
                    graphic.Attributes.Add("Score", candidate.Score);

                    string latlon = String.Format("{0}, {1}", candidate.Location.X, candidate.Location.Y);
                    graphic.Attributes.Add("LatLon", latlon);
                    candidategraphicsList.Add(graphic);

                    if (resultsEnvelope == null)
                        resultsEnvelope = new Envelope(candidate.Location, candidate.Location);

                    if (mapPoint.X < resultsEnvelope.XMin)
                        resultsEnvelope.XMin = mapPoint.X;
                    if (mapPoint.Y < resultsEnvelope.YMin)
                        resultsEnvelope.YMin = mapPoint.Y;
                    if (mapPoint.X > resultsEnvelope.XMax)
                        resultsEnvelope.XMax = mapPoint.X;
                    if (mapPoint.Y > resultsEnvelope.YMax)
                        resultsEnvelope.YMax = mapPoint.Y;

                }

                _candidateAddressesGraphicsLayer.Graphics.AddRange(candidategraphicsList);

                UpdateCandidateGraphics();

                Envelope newExtent = null;
                if (resultsEnvelope != null)
                    newExtent = resultsEnvelope.Expand(2);
                else
                    newExtent = _candidateResults.First(x => x.Score == 100).Extent.Expand(2);
                await mapView1.SetViewAsync(newExtent);
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
