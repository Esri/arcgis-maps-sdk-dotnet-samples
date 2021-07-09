// Copyright 2016 Esri.
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
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Google.Android.Material.Snackbar;
using System;
using System.Linq;

namespace ArcGISRuntime.Samples.DisplayDeviceLocation
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display device location with autopan modes",
        category: "Location",
        description: "Display your current position on the map, as well as switch between different types of auto pan Modes.",
        instructions: "Select an autopan mode, then use the buttons to start and stop location display.",
        tags: new[] { "GPS", "compass", "location", "map", "mobile", "navigation" })]
    public class DisplayDeviceLocation : Activity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        // Constant for tracking permission request.
        private const int LocationPermissionRequestCode = 99;

        // Create and hold reference to the used MapView
        private MapView _myMapView;

        // String array to store the different device location options.
        private readonly string[] _navigationTypes =
        {
            "On",
            "Re-Center",
            "Navigation",
            "Compass"
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display device location";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnStopButtonClicked(object sender, EventArgs e) => _myMapView.LocationDisplay.IsEnabled = false;

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            Button startButton = (Button)sender;

            // Create menu to show navigation options
            PopupMenu navigationMenu = new PopupMenu(this, startButton);
            navigationMenu.MenuItemClick += OnNavigationMenuItemClicked;

            // Create menu options
            foreach (string navigationType in _navigationTypes)
            {
                navigationMenu.Menu.Add(navigationType);
            }

            // Show menu in the view
            navigationMenu.Show();
        }

        private void OnNavigationMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Reset navigation display.
            _myMapView.LocationDisplay.IsEnabled = false;

            // Get title from the selected item
            string selectedNavigationType = e.Item.TitleCondensedFormatted.ToString();

            // Get index that is used to get the selected url
            int selectedIndex = _navigationTypes.ToList().IndexOf(selectedNavigationType);

            // Set location display automatic panning mode.
            switch (selectedIndex)
            {
                case 0:
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
                    break;

                case 1:
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    break;

                case 2:
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                    break;

                case 3:
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
                    break;
            }

            // Ask for location and enable location display when permission is granted.
            AskForLocationPermission();
        }

        private async void AskForLocationPermission()
        {
            // Only check if permission hasn't been granted yet.
            if (ContextCompat.CheckSelfPermission(this, LocationService) != Permission.Granted)
            {
                // The Fine location permission will be requested.
                var requiredPermissions = new[] { Manifest.Permission.AccessFineLocation };

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
                try
                {
                    // Explicit DataSource.LoadAsync call is used to surface any errors that may arise.
                    await _myMapView.LocationDisplay.DataSource.StartAsync();
                    _myMapView.LocationDisplay.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    ShowMessage(ex.Message, "Failed to start location display.");
                }
            }
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
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
                    // Explicit DataSource.LoadAsync call is used to surface any errors that may arise.
                    await _myMapView.LocationDisplay.DataSource.StartAsync();
                    _myMapView.LocationDisplay.IsEnabled = true;
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

        private void ShowMessage(string message, string title = "Error") => new AlertDialog.Builder(this).SetTitle(title).SetMessage(message).Show();

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible navigation options.
            Button startButton = new Button(this)
            {
                Text = "Start"
            };
            startButton.Click += OnStartButtonClicked;

            // Create button to stop navigation.
            Button stopButton = new Button(this)
            {
                Text = "Stop"
            };
            stopButton.Click += OnStopButtonClicked;

            // Add start button to the layout.
            layout.AddView(startButton);

            // Add stop button to the layout.
            layout.AddView(stopButton);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        protected override void OnDestroy()
        {
            // Stop the location data source.
            _myMapView?.LocationDisplay?.DataSource?.StopAsync();

            base.OnDestroy();
        }
    }
}