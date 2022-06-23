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
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
#if XAMARIN_ANDROID
using ArcGISRuntime.Droid;

#endif

namespace ArcGISRuntime.Samples.DisplayDeviceLocation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display device location with autopan modes",
        category: "Location",
        description: "Display your current position on the map, as well as switch between different types of auto pan Modes.",
        instructions: "Select an autopan mode, then use the buttons to start and stop location display.",
        tags: new[] { "GPS", "compass", "location", "map", "mobile", "navigation" })]
    public partial class DisplayDeviceLocation : ContentPage, IDisposable
    {
        // String array to store the different device location options.
        private string[] _navigationTypes =
        {
            "On",
            "Re-Center",
            "Navigation",
            "Compass"
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
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            MyMapView.LocationDisplay.IsEnabled = false;
        }

        private void OnStartClicked(object sender, EventArgs e)
        {
            _ = OnStartClickedTask(sender, e);
        }

        private async Task OnStartClickedTask(object sender, EventArgs e)
        {
            // Show sheet and get title from the selection.
            string selectedMode =
                await ((Page) Parent).DisplayActionSheet("Select navigation mode", "Cancel", null, _navigationTypes);

            // If selected cancel do nothing.
            if (selectedMode == "Cancel") return;

            // Get index that is used to get the selected url.
            int selectedIndex = _navigationTypes.ToList().IndexOf(selectedMode);

            switch (selectedIndex)
            {
                case 0:
                    // Starts location display with auto pan mode set to Off.
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
                    break;

                case 1:
                    // Starts location display with auto pan mode set to Re-center.
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    break;

                case 2:
                    // Starts location display with auto pan mode set to Navigation.
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                    break;

                case 3:
                    // Starts location display with auto pan mode set to Compass Navigation.
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
                    break;
            }

            try
            {
                // Permission request only needed on Android.
#if XAMARIN_ANDROID
                // See implementation in MainActivity.cs in the Android platform project.
                MainActivity.Instance.AskForLocationPermission(MyMapView);
#else
                await MyMapView.LocationDisplay.DataSource.StartAsync();
                MyMapView.LocationDisplay.IsEnabled = true;
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
            }
        }

        public void Dispose()
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}