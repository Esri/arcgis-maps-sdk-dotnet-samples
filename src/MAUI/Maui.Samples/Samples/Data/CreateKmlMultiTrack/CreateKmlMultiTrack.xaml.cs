// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Ogc;
using ArcGIS.Samples.Managers;
using Microsoft.Maui.ApplicationModel;
using Map = Esri.ArcGISRuntime.Mapping.Map;
using Location = Esri.ArcGISRuntime.Location.Location;
using System.ComponentModel;

namespace ArcGIS.Samples.CreateKmlMultiTrack
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Create KML multi-track",
        "Data",
        "Create, save and preview a KML multi-track, captured from a location data source.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class CreateKmlMultiTrack
    {
        private const string KML_PATH = "C:\\arcgis-maps-sdk-dotnet-samples\\src\\WPF\\WPF.Viewer\\Resources\\coastal_trail.json";

        // Flag to indicate whether the app is recording a KML track.
        private bool _isRecordingTrack = false;

        // Lists for storing KML track elements and KML tracks.
        private List<KmlTrackElement> _kmlTrackElements = new List<KmlTrackElement>();
        private List<KmlTrack> _kmlTracks = new List<KmlTrack>();

        // Graphics overlay indexes for the KML track elements and the KML tracks.
        private int _elementsIndex = 0;
        private int _tracksIndex = 1;

        // Symbology to differentiate the KML track elements and the KML tracks.
        private SimpleMarkerSymbol _locationSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);
        private SimpleLineSymbol _routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 10);

        // Route geometry for the simulation.
        private Polyline _routeGeometry;

        // File path for the .kmz file.
        private string _filePath;

        // Dictionary to store the KML tracks and their names.
        private Dictionary<string, Geometry> _tracksDictionary = new Dictionary<string, Geometry>();

        public CreateKmlMultiTrack()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a street basemap style.
            MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

            // Get the hiking path geometry.
            string coastalTrailJson = File.ReadAllText(KML_PATH);
            _routeGeometry = Geometry.FromJson(coastalTrailJson) as Polyline;

            // Set the map's initial viewpoint.
            MyMapView.SetViewpoint(new Viewpoint(_routeGeometry, 25));

            MyMapView.PropertyChanged += MyMapView_PropertyChanged;

            // Add graphics overlays for the KML track elements and the KML tracks.
            MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());
            MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Define the save file path.
            _filePath = Path.Combine(DataManager.GetDataFolder(), "SampleData", "CreateKmlMultiTrack", "HikingTracks", "HikingTracks.kmz");
        }

        /// <summary>
        /// // The map view's location display is initially null, so check for a location display property change before enabling it, configuring the simulated data source, and subscribing to location changes.
        /// </summary>
        private void MyMapView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocationDisplay))
            {
                // Create a simulated location data source from json data with simulation parameters to set a consistent velocity.
                var simulatedLocationDataSource = new SimulatedLocationDataSource();
                simulatedLocationDataSource.SetLocationsWithPolyline(
                    _routeGeometry,
                    new SimulationParameters(DateTimeOffset.Now, 25, 0, 0));

                // Set the simulated location data source as the location data source for this app.
                MyMapView.LocationDisplay.DataSource = simulatedLocationDataSource;

                // Enable the recenter button when autopan mode isn't navigation.
                MyMapView.LocationDisplay.AutoPanModeChanged += (s, e) =>
                {
                    RecenterButton.IsEnabled = MyMapView.LocationDisplay.AutoPanMode != LocationDisplayAutoPanMode.Navigation;
                };

                StartNavigation();

                // Unsubscribe now that the location display has been initialized.
                MyMapView.PropertyChanged -= MyMapView_PropertyChanged;
            }
        }

        #region KML track recording state

        /// <summary>
        /// Toggles the recording state and updates the UI accordingly. A KML track is created from the recorded elements when recording is stopped.
        /// </summary>
        private void RecordTrackButton_Clicked(object sender, EventArgs e)
        {
            // Toggle the recording state.
            _isRecordingTrack = !_isRecordingTrack;

            // Update the UI based on the recording state.
            if (!_isRecordingTrack)
            {
                RecordTrackButton.Text = "Start Recording";
                ElementsCountLabel.Text = "Clicked record to capture KML track elements.";
                SaveButton.IsEnabled = true;
                DisplayKmlTrack();
            }
            else
            {
                RecordTrackButton.Text = "Stop Recording";
                ElementsCountLabel.Text = "Recording KML track. Elements added: 0";
                SaveButton.IsEnabled = false;
                ElementsCountLabel.IsVisible = true;
                RecordTrackButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Creates a KML track and displays it on the map.
        /// </summary>
        private void DisplayKmlTrack()
        {
            // Remove the red location markers from the map.
            MyMapView.GraphicsOverlays[_elementsIndex].Graphics.Clear();

            // Create a new KML track from the recorded elements.
            var kmlTrack = new KmlTrack(_kmlTrackElements, KmlAltitudeMode.RelativeToGround);
            _kmlTracks.Add(kmlTrack);

            // Set the KML geometry to use the same projection as the map.
            var multipoint = kmlTrack.Geometry as Multipoint;

            // Add the KML track as a graphic to the map.
            var graphic = new Graphic(new Polyline(multipoint.Points), _routeSymbol);
            MyMapView.GraphicsOverlays[_tracksIndex].Graphics.Add(graphic);

            // Clear the list of track elements.
            _kmlTrackElements.Clear();

            // Increment the track count in the UI.
            TracksCountLabel.Text = $"Number of tracks in MultiTrack: {_kmlTracks.Count}";
        }

        /// <summary>
        /// When recording is enabled, add the given location to the list
        /// of KML track elements and display the graphic on the map.
        /// </summary>
        private void LocationDisplay_LocationChanged(object sender, Location e)
        {
            if (_isRecordingTrack)
            {
                // Ensure the code runs on the UI thread.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Add a new element to the KMl track.
                    _kmlTrackElements.Add(new KmlTrackElement(DateTime.Now, e.Position));

                    // Add a graphic at the location's position.
                    MyMapView.GraphicsOverlays[_elementsIndex].Graphics.Add(new Graphic(e.Position, _locationSymbol));

                    ElementsCountLabel.Text = $"Recording KML track. Elements added: {_kmlTrackElements.Count}";

                    // KML tracks require at least two elements to be valid, so recording can only be stopped when this requirement is met.
                    if (_kmlTrackElements.Count >= 2)
                    {
                        RecordTrackButton.IsEnabled = true;
                    }
                });
            }
        }

        /// <summary>
        /// Recenters the map view on the simulated location. This is helpful after panning away from the location.
        /// </summary>
        private void RecenterButton_Clicked(object sender, EventArgs e)
        {
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
        }

        #endregion KML track recording state

        #region KML track viewing state

        /// <summary>
        /// Saves the KML muli-track to a file, loads it back and displays it on the map. Transitions the UI to the KML track viewing state.
        /// </summary>
        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            ToggleUIVisibility();
            await ExportKlmMultiTrack();
            _kmlTracks.Clear();
            StopNavigation();
            await LoadLocalKmlFile();
        }

        /// <summary>
        /// Exports the KML multi-track to a .kmz file.
        /// </summary>
        private async Task ExportKlmMultiTrack()
        {
            // Create a default KML document which will export file to device.
            var kmlDocument = new KmlDocument();

            // Create a KML multi track using the current list of tracks.
            var multiTrack = new KmlMultiTrack(_kmlTracks);

            // Add the multi track as a placemark KML node to the KML document.
            kmlDocument.ChildNodes.Add(new KmlPlacemark(multiTrack));

            // Delete the file if it already exists.
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }

            // Save the KML document to the file path.
            await kmlDocument.SaveAsAsync(_filePath);
        }

        /// <summary>
        /// Loads a local KML file and displays the KML tracks on the map. Adds the KML tracks to the combo box for selection.
        /// </summary>
        private async Task LoadLocalKmlFile()
        {
            // Create a KML dataset using the local file path.
            var kmlDataset = new KmlDataset(new Uri(_filePath));

            // Load the KML dataset.
            await kmlDataset.LoadAsync();

            // Get the document's node which contains the placemark.
            var kmlDocument = kmlDataset.RootNodes.OfType<KmlDocument>().FirstOrDefault();
            var kmlPlacemark = kmlDocument.ChildNodes.OfType<KmlPlacemark>().FirstOrDefault();

            // Get the multi-track geometry from the placemark.
            var kmlMultiTrack = kmlPlacemark.KmlGeometry as KmlMultiTrack;

            // Obtain all the KML tracks from the multi-track.
            var trackGeometries = kmlMultiTrack.Tracks.Select(track => track.Geometry);

            // Calculate the union of all the KML tracks.
            var allTracksGeometry = GeometryEngine.Union(trackGeometries);

            // Clear track graphics from the previous UI state.
            MyMapView.GraphicsOverlays[_tracksIndex].Graphics.Clear();

            // Project the KML tracks to the map's spatial reference and add them as graphics to the map.
            // Populate a dictionary with the track names as keys and the track geometries as values.
            _tracksDictionary.Add("Show all KLM tracks", allTracksGeometry);
            for (int i = 0; i < kmlMultiTrack.Tracks.Count; i++)
            {
                var trackGeometry = kmlMultiTrack.Tracks[i].Geometry as Multipoint;
                MyMapView.GraphicsOverlays[_tracksIndex].Graphics.Add(new Graphic(new Polyline(trackGeometry.Points), _routeSymbol));
                _tracksDictionary.Add($"Track {i + 1}", trackGeometry);
            }

            // Set the combo box's item source to the dictionary.
            TracksPicker.ItemsSource = _tracksDictionary.Keys.ToList();

            // Set the viewpoint to the union geometry by invoking the selection changed event.
            TracksPicker.SelectedIndex = 0;
        }

        /// <summary>
        /// Zooms to the selected KML track.
        /// </summary>
        private void TracksPicker_SelectionChanged(object sender, EventArgs e)
        {
            if (TracksPicker.ItemsSource != null)
            {
                // TODO: implement tracks picker item lookup.
                Geometry selectedGeometry = _tracksDictionary[TracksPicker.SelectedItem.ToString()];

                // Set the viewpoint to the selected geometry.
                MyMapView.SetViewpointGeometryAsync(selectedGeometry, 25);
            }
        }

        #endregion KML track viewing state

        #region Start and stop navigation

        /// <summary>
        /// Configures and starts the location display for navigation.
        /// </summary>
        private void StartNavigation()
        {
            // Start the location data source.
            MyMapView.LocationDisplay.DataSource.StartAsync();

            // Set the location display to auto pan mode.
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

            // Listen for changes in location.
            MyMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

            // Enable the location display.
            MyMapView.LocationDisplay.IsEnabled = true;
        }

        /// <summary>
        /// Turns off the location display and stops listening for location changes.
        /// </summary>
        private void StopNavigation()
        {
            MyMapView.LocationDisplay.IsEnabled = false;
            MyMapView.LocationDisplay.LocationChanged -= LocationDisplay_LocationChanged;
            MyMapView.LocationDisplay.DataSource.StopAsync();
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
        }

        #endregion Start and stop navigation

        #region Change UI state

        /// <summary>
        /// Resets the state for KML track recording.
        /// </summary>
        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            ToggleUIVisibility();
            StartNavigation();
            TracksCountLabel.Text = "Number of tracks in MultiTrack: 0";
            SaveButton.IsEnabled = false;
            TracksPicker.ItemsSource = null;
            _tracksDictionary.Clear();
            MyMapView.GraphicsOverlays[_tracksIndex].Graphics.Clear();
        }

        /// <summary>
        /// Toggles the visibility of the recording and viewing UIs.
        /// </summary>
        private void ToggleUIVisibility()
        {
            RecordingUI.IsVisible = !RecordingUI.IsVisible;
            ViewingUI.IsVisible = !ViewingUI.IsVisible;
        }

        #endregion Change UI state
    }
}