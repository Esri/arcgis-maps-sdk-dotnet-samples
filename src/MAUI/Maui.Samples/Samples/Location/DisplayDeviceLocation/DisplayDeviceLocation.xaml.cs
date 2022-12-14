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
        description: "Display your current position on the map, as well as switch between different types of auto pan Modes.",
        instructions: "Select an autopan mode, then use the buttons to start and stop location display.",
        tags: new[] { "GPS", "compass", "location", "map", "mobile", "navigation" })]
    public partial class DisplayDeviceLocation : ContentPage, IDisposable
    {
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
        }

        private async Task StartDeviceLocationTask()
        {
            try
            {
                var status = Microsoft.Maui.ApplicationModel.PermissionStatus.Unknown;

                status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();

                if (status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                {
                    await MyMapView.LocationDisplay.DataSource.StartAsync();
                    MyMapView.LocationDisplay.IsEnabled = true;

                    // Enable the stop device location button.
                    StopButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
            }
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            MyMapView.LocationDisplay.IsEnabled = false;
            StopButton.IsEnabled = false;
        }

        public void Dispose()
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }

        private void RecenterButton_Clicked(object sender, EventArgs e)
        {
            // Starts location display with auto pan mode set to Re-Center.
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;

            _ = StartDeviceLocationTask();
        }

        private void NavigationButton_Clicked(object sender, EventArgs e)
        {
            // Starts location display with auto pan mode set to Navigation.
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

            _ = StartDeviceLocationTask();
        }

        private void CompassNavigationButton_Clicked(object sender, EventArgs e)
        {
            // Starts location display with auto pan mode set to Compass Navigation.
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;

            _ = StartDeviceLocationTask();
        }

        private void ShowDeviceLocationButtons_Clicked(object sender, EventArgs e)
        {
            // Show/Hide the device location buttons.
            DeviceLocationButtonsGrid.IsVisible = !DeviceLocationButtonsGrid.IsVisible;
        }
    }
}