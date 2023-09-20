// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Microsoft.Maui.ApplicationModel;
using System.ComponentModel;
using System.Diagnostics;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace ArcGIS.Samples.DisplayDeviceLocation
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display device location with autopan modes",
        category: "Location",
        description: "Display your current position on the map, as well as switch between different types of auto pan modes.",
        instructions: "Select an autopan mode, then use the button to start and stop location display.",
        tags: new[] { "GPS", "compass", "location", "map", "mobile", "navigation" })]
    public partial class DisplayDeviceLocation : ContentPage, IDisposable
    {
        // Dictionary to store the different auto pan modes.
        private readonly Dictionary<string, LocationDisplayAutoPanMode> _autoPanModes = new()
        {
            { "AutoPan Off", LocationDisplayAutoPanMode.Off },
            { "Re-Center", LocationDisplayAutoPanMode.Recenter },
            { "Navigation", LocationDisplayAutoPanMode.Navigation },
            { "Compass", LocationDisplayAutoPanMode.CompassNavigation }
        };

        public DisplayDeviceLocation()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            var myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Assign the map to the MapView.
            MyMapView.Map = myMap;

            // Populate the picker with different auto pan modes.
            AutoPanModePicker.ItemsSource = _autoPanModes.Keys.ToList();

            // Keep listening to MapView property changed events until the location display has been initialized.
            MyMapView.PropertyChanged += MyMapView_PropertyChanged;
        }

        private void MyMapView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // The map view's location display is initially null, so check for a location display property
            // change before subscribing to auto pan mode change events.
            if (e.PropertyName == nameof(LocationDisplay))
            {
                // Show in the UI that LocationDisplay.AutoPanMode is off by default.
                AutoPanModePicker.SelectedItem = "AutoPan Off";

                // Update the UI when the user pans the view, disabling the auto pan mode.
                MyMapView.LocationDisplay.AutoPanModeChanged += (sender, args) =>
                {
                    if (MyMapView.LocationDisplay.AutoPanMode == LocationDisplayAutoPanMode.Off)
                    {
                        AutoPanModePicker.SelectedItem = "AutoPan Off";
                    }
                };

                // No longer a need to listen for MapView property changes, just listen for auto pan mode changes.
                MyMapView.PropertyChanged -= MyMapView_PropertyChanged;
            }
        }

        private async Task DisplayDeviceLocationAsync()
        {
            try
            {
                // Request access to device location.
                // When deploying to Android, iOS, and MacCatalyst, a permission prompt may appear.
                PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                // Start the location display if access to device location has been authorized.
                // Permission status will be restricted if the user approximates the device location.
                if (status == PermissionStatus.Granted || status == PermissionStatus.Restricted)
                {
                    await MyMapView.LocationDisplay.DataSource.StartAsync();
                }
            }
            catch (Exception ex)
            {
                // Note for MacCatalyst: while on ethernet, without an external GPS device connected,
                // location will be unknown.
                Debug.WriteLine(ex);
                await Application.Current.MainPage.DisplayAlert("Couldn't start location data source", ex.Message, "OK");
            }
        }

        private void AutoPanModePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change the auto pan mode based on the new selection.
            MyMapView.LocationDisplay.AutoPanMode = _autoPanModes[AutoPanModePicker.SelectedItem.ToString()];
        }

        private async void StartStopButton_Clicked(object sender, EventArgs e)
        {
            // Enable or disable the location display.
            if (MyMapView.LocationDisplay.IsEnabled)
            {
                MyMapView.LocationDisplay.IsEnabled = false;
            }
            else
            {
                // Attempt to display device location on the map.
                // Await before updating the button text to handle the case where the start button was pressed
                // but location services weren't authorized.
                await DisplayDeviceLocationAsync();
            }

            // Flip the button text if the LocationDisplay.IsEnabled property was changed.
            StartStopButton.Text = MyMapView.LocationDisplay.IsEnabled ? "Stop" : "Start";
        }

        public void Dispose()
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}