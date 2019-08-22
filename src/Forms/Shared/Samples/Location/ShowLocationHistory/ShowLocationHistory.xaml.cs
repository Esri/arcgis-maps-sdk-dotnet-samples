// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using Xamarin.Forms;
using Color = System.Drawing.Color;

namespace ArcGISRuntimeXamarin.Samples.ShowLocationHistory
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show location history",
        "Location",
        "Display your location history on the map.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("FakeLocationDataSource.cs")]
    public partial class ShowLocationHistory : ContentPage
    {
        // URL to the raster dark gray canvas basemap.
        private const string BasemapUrl = "https://www.arcgis.com/home/item.html?id=1970c1995b8f44749f4b9b6e81b5ba45";

        // Track whether location tracking is enabled.
        private bool _isTrackingEnabled;

        // Location data source provides location data updates.
        private LocationDataSource _locationDataSource;

        // Graphics overlay to display the location history (points).
        private GraphicsOverlay _locationHistoryOverlay;

        // Graphics overlay to display the line created by the location points.
        private GraphicsOverlay _locationHistoryLineOverlay;

        // Polyline builder to more efficiently manage large location history graphic.
        private PolylineBuilder _polylineBuilder;

        // Track previous location to ensure the route line appears behind the animating location symbol.
        private MapPoint _lastPosition;

        public ShowLocationHistory()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(new Uri(BasemapUrl));

            // Display the map.
            MyMapView.Map = myMap;

            // Create and add graphics overlay for displaying the trail.
            _locationHistoryLineOverlay = new GraphicsOverlay();
            SimpleLineSymbol locationLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Lime, 2);
            _locationHistoryLineOverlay.Renderer = new SimpleRenderer(locationLineSymbol);
            MyMapView.GraphicsOverlays.Add(_locationHistoryLineOverlay);

            // Create and add graphics overlay for showing points.
            _locationHistoryOverlay = new GraphicsOverlay();
            SimpleMarkerSymbol locationPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 3);
            _locationHistoryOverlay.Renderer = new SimpleRenderer(locationPointSymbol);
            MyMapView.GraphicsOverlays.Add(_locationHistoryOverlay);

            // Create the polyline builder.
            _polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);

            MyMapView.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(MyMapView.LocationDisplay) && MyMapView.LocationDisplay != null)
                {
                    // Configure location.
                    HandleLocationReady();
                }
            };
        }

        private async void HandleLocationReady()
        {
            // Create the location data source.
            _locationDataSource = new FakeLocationDataSource();
            MyMapView.LocationDisplay.DataSource = _locationDataSource;
            // Use this instead if you want real location: _locationDataSource = new SystemLocationDataSource();

            try
            {
#if XAMARIN_ANDROID
                // See implementation in MainActivity.cs in the Android platform project.
                ArcGISRuntime.Droid.MainActivity.Instance.AskForLocationPermission(MyMapView);
#endif
                // Start the data source.
                await _locationDataSource.StartAsync();

                if (_locationDataSource.IsStarted)
                {
                    // Set the location display data source and enable location display.
                    MyMapView.LocationDisplay.IsEnabled = true;
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    MyMapView.LocationDisplay.InitialZoomScale = 10000;

                    // Enable the button to start location tracking.
                    LocationTrackingButton.IsEnabled = true;
                }
                else
                {
                    ShowMessage("There was a problem enabling location", "Error");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                ShowMessage("There was a problem enabling location", "Error");
            }
        }

        private void ToggleLocationTracking()
        {
            // Toggle location tracking.
            _isTrackingEnabled = !_isTrackingEnabled;

            // Apply new configuration.
            if (_isTrackingEnabled)
            {
                // Configure new symbology first.
                _locationDataSource.LocationChanged += LocationDataSourceOnLocationChanged;

                // Update the UI.
                LocationTrackingButton.Text = "Stop tracking";
            }
            else
            {
                // Stop updating.
                _locationDataSource.LocationChanged -= LocationDataSourceOnLocationChanged;

                // Update the UI.
                LocationTrackingButton.Text = "Start tracking";
            }
        }

        private void LocationDataSourceOnLocationChanged(object sender, Location e)
        {
            // Remove the old line.
            _locationHistoryLineOverlay.Graphics.Clear();

            // Add any previous position to the history.
            if (_lastPosition != null)
            {
                _polylineBuilder.AddPoint(_lastPosition);
                _locationHistoryOverlay.Graphics.Add(new Graphic(_lastPosition));
            }

            // Store the current position.
            _lastPosition = e.Position;

            // Add the updated line.
            _locationHistoryLineOverlay.Graphics.Add(new Graphic(_polylineBuilder.ToGeometry()));
        }

        private void StartTracking_Tapped(object sender, EventArgs e) => ToggleLocationTracking();

        private async void ShowMessage(string title, string detail)
        {
            await Application.Current.MainPage.DisplayAlert(title, detail, "OK");
        }
    }
}