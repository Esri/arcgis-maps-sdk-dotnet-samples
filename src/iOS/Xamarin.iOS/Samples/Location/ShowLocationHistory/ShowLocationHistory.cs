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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ShowLocationHistory
{
    [Register("ShowLocationHistory")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show location history",
        "Location",
        "Display your location history on the map.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("FakeLocationDataSource.cs")]
    public class ShowLocationHistory : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _trackingToggleButton;

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
            Title = "Show location history";
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateDarkGrayCanvasVector());

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

            // Start location services.
            HandleLocationReady();
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
                _trackingToggleButton.Title = "Stop tracking";
            }
            else
            {
                // Stop updating.
                _locationDataSource.LocationChanged -= LocationDataSourceOnLocationChanged;

                // Update the UI.
                _trackingToggleButton.Title = "Start tracking";
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

        private void TrackingToggleButtonOnClicked(object sender, EventArgs e) => ToggleLocationTracking();

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _trackingToggleButton = new UIBarButtonItem("Start tracking", UIBarButtonItemStyle.Plain, null);
            _trackingToggleButton.Enabled = false;

            toolbar.Items = new []
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), 
                _trackingToggleButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new []{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _trackingToggleButton.Clicked += TrackingToggleButtonOnClicked;
        }
        
        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _trackingToggleButton.Clicked -= TrackingToggleButtonOnClicked;

            // Stop the location data source.
            _myMapView.LocationDisplay?.DataSource?.StopAsync();
        }

        private void ShowMessage(string title, string detail)
        {
            new UIAlertView(title, detail, (IUIAlertViewDelegate)null, "OK", null).Show();
        }
    }
}
