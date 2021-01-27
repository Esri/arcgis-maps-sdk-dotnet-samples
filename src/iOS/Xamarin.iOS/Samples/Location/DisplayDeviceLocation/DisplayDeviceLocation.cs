// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Linq;
using UIKit;

namespace ArcGISRuntime.Samples.DisplayDeviceLocation
{
    [Register("DisplayDeviceLocation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display device location",
        category: "Location",
        description: "Display your current position on the map, as well as switch between different types of auto pan Modes.",
        instructions: "Select an autopan mode, then use the buttons to start and stop location display.",
        tags: new[] { "GPS", "compass", "location", "map", "mobile", "navigation" })]
    public class DisplayDeviceLocation : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _startButton;
        private UIBarButtonItem _stopButton;

        public DisplayDeviceLocation()
        {
            Title = "Display Device Location";
        }

        private void Initialize()
        {
            // Show an imagery basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);
        }

        private void OnStopButtonClicked(object sender, EventArgs e)
        {
            _myMapView.LocationDisplay.IsEnabled = false;
        }

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            try
            {
                UIAlertController actionAlert = UIAlertController.Create("Select device location option", "", UIAlertControllerStyle.Alert);

                // Add actions to alert. Selecting an option displays different option for auto pan modes.
                actionAlert.AddAction(UIAlertAction.Create("On", UIAlertActionStyle.Default, action =>
                {
                    // Starts location display with auto pan mode set to Off.
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
                    _myMapView.LocationDisplay.IsEnabled = true;
                }));
                actionAlert.AddAction(UIAlertAction.Create("Re-center", UIAlertActionStyle.Default, action =>
                {
                    // Starts location display with auto pan mode set to Default.
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    _myMapView.LocationDisplay.IsEnabled = true;
                }));
                actionAlert.AddAction(UIAlertAction.Create("Navigation", UIAlertActionStyle.Default, action =>
                {
                    // Starts location display with auto pan mode set to Navigation.
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                    _myMapView.LocationDisplay.IsEnabled = true;
                }));
                actionAlert.AddAction(UIAlertAction.Create("Compass", UIAlertActionStyle.Default, action =>
                {
                    // Starts location display with auto pan mode set to Compass Navigation.
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
                    _myMapView.LocationDisplay.IsEnabled = true;
                }));

                // Show alert.
                PresentViewController(actionAlert, true, null);
            }
            catch (Exception ex)
            {
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                PresentViewController(alert, true, null);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _startButton = new UIBarButtonItem();
            _startButton.Title = "Start";

            _stopButton = new UIBarButtonItem();
            _stopButton.Title = "Stop";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _startButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _stopButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _startButton.Clicked += OnStartButtonClicked;
            _stopButton.Clicked += OnStopButtonClicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _startButton.Clicked -= OnStartButtonClicked;
            _stopButton.Clicked -= OnStopButtonClicked;

            // Check if sample is being closed.
            if (NavigationController?.ViewControllers == null)
            {
                // Stop the location data source.
                _myMapView.LocationDisplay?.DataSource?.StopAsync();
            }
        }
    }
}