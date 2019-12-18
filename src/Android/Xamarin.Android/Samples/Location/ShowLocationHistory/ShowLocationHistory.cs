// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;

namespace ArcGISRuntimeXamarin.Samples.ShowLocationHistory
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show location history",
        "Location",
        "Display your location history on the map.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("FakeLocationDataSource.cs")]
    public class ShowLocationHistory : Activity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Button _trackingToggleButton;

        // Constant for tracking permission request.
        private const int LocationPermissionRequestCode = 99;

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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Show location history";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(new Uri(BasemapUrl));

            // Display the map.
            _myMapView.Map = myMap;

            // Create and add graphics overlay for displaying the trail.
            _locationHistoryLineOverlay = new GraphicsOverlay();
            SimpleLineSymbol locationLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Lime, 2);
            _locationHistoryLineOverlay.Renderer = new SimpleRenderer(locationLineSymbol);
            _myMapView.GraphicsOverlays.Add(_locationHistoryLineOverlay);

            // Create and add graphics overlay for showing points.
            _locationHistoryOverlay = new GraphicsOverlay();
            SimpleMarkerSymbol locationPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 3);
            _locationHistoryOverlay.Renderer = new SimpleRenderer(locationPointSymbol);
            _myMapView.GraphicsOverlays.Add(_locationHistoryOverlay);

            // Create the polyline builder.
            _polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);

            // Get permissions.
            AskForLocationPermission();
        }

        private async void HandleLocationReady()
        {
            // Create the location data source.
            _locationDataSource = new FakeLocationDataSource();
            // Use this instead if you want real location: _locationDataSource = new SystemLocationDataSource();

            try
            {
                // Start the data source.
                await _locationDataSource.StartAsync();

                if (_locationDataSource.IsStarted)
                {
                    // Set the location display data source and enable location display.
                    _myMapView.LocationDisplay.DataSource = _locationDataSource;
                    _myMapView.LocationDisplay.IsEnabled = true;
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    _myMapView.LocationDisplay.InitialZoomScale = 10000;

                    // Enable the button to start location tracking.
                    _trackingToggleButton.Enabled = true;
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
                _trackingToggleButton.Text = "Stop tracking";
            }
            else
            {
                // Stop updating.
                _locationDataSource.LocationChanged -= LocationDataSourceOnLocationChanged;

                // Update the UI.
                _trackingToggleButton.Text = "Start tracking";
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

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Create and add the button.
            _trackingToggleButton = new Button(this) {Text = "Start tracking", Enabled = false};
            _trackingToggleButton.Click += TrackingToggleButtonOnClick;
            layout.AddView(_trackingToggleButton);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private void AskForLocationPermission()
        {
            // Only check if permission hasn't been granted yet.
            if (ContextCompat.CheckSelfPermission(this, LocationService) != Permission.Granted)
            {
                // The Fine location permission will be requested.
                var requiredPermissions = new[] {Manifest.Permission.AccessFineLocation};

                // Only prompt the user first if the system says to.
                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
                {
                    // A snackbar is a small notice that shows on the bottom of the view.
                    Snackbar.Make(_myMapView,
                            "Location permission is needed to display location on the map.",
                            Snackbar.LengthIndefinite)
                        .SetAction("OK",
                            delegate
                            {
                                // When the user presses 'OK', the system will show the standard permission dialog.
                                // Once the user has accepted or denied, OnRequestPermissionsResult is called with the result.
                                ActivityCompat.RequestPermissions(this, requiredPermissions, LocationPermissionRequestCode);
                            }
                        ).Show();
                }
                else
                {
                    // When the user presses 'OK', the system will show the standard permission dialog.
                    // Once the user has accepted or denied, OnRequestPermissionsResult is called with the result.
                    this.RequestPermissions(requiredPermissions, LocationPermissionRequestCode);
                }
            }
            else
            {
                HandleLocationReady();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            // Ignore other location requests.
            if (requestCode != LocationPermissionRequestCode)
            {
                return;
            }

            // If the permissions were granted, enable location.
            if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
            {
                System.Diagnostics.Debug.WriteLine("User affirmatively gave permission to use location. Enabling location.");
                try
                {
                    HandleLocationReady();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    ShowMessage(ex.Message, "Failed to start location display.");
                }
            }
            else
            {
                ShowMessage("Location permissions not granted.", "Failed to start location display.");
            }
        }

        private void TrackingToggleButtonOnClick(object sender, EventArgs e) => ToggleLocationTracking();

        private void ShowMessage(string message, string title) => new AlertDialog.Builder(this).SetTitle(title).SetMessage(message).Show();

        protected override void OnDestroy()
        {
            // Stop the location data source.
            _myMapView?.LocationDisplay?.DataSource?.StopAsync();

            base.OnDestroy();
        }
    }
}