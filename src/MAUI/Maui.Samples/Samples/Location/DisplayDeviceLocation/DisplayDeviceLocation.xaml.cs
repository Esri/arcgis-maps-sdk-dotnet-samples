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
using System.Diagnostics;

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
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Assign the map to the MapView.
            MyMapView.Map = myMap;

            // Populate the picker with different auto pan modes.
            AutoPanModePicker.ItemsSource = _autoPanModes.Keys.ToList();
        }

        private async Task StartDeviceLocationTask()
        {
            try
            {
                // Check if location permission granted.
                var status = Microsoft.Maui.ApplicationModel.PermissionStatus.Unknown;
                status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();

                // Request location permission if not granted.
                if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                {
                    status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
                }

                // Start the location display if permission is granted.
                MyMapView.LocationDisplay.IsEnabled = status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;

                // Update the UI when the user pans the view, changing the location mode.
                MyMapView.LocationDisplay.AutoPanModeChanged += (sender, args) =>
                {
                    if (MyMapView.LocationDisplay.AutoPanMode == LocationDisplayAutoPanMode.Off)
                    {
                        AutoPanModePicker.SelectedItem = "AutoPan Off";
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
            }
        }

        private void AutoPanModePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change the auto pan mode based on the new selection.
            MyMapView.LocationDisplay.AutoPanMode = _autoPanModes[AutoPanModePicker.SelectedItem.ToString()];
        }

        private void StartStopButton_Clicked(object sender, EventArgs e)
        {
            // Enable or disable the location display.
            if (MyMapView.LocationDisplay.IsEnabled)
            {
                MyMapView.LocationDisplay.IsEnabled = false;
                StartStopButton.Text = "Start";
            }
            else
            {
                _ = StartDeviceLocationTask();
                StartStopButton.Text = "Stop";
            }
        }

        public void Dispose()
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}