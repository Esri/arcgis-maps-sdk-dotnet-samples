// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISRuntime.WinUI.Samples.DisplayDeviceLocation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display device location",
        category: "Location",
        description: "Display your current position on the map, as well as switch between different types of auto pan Modes.",
        instructions: "Select an autopan mode, then use the buttons to start and stop location display.",
        tags: new[] { "GPS", "compass", "location", "map", "mobile", "navigation" })]
    public partial class DisplayDeviceLocation
    {
        // String array to store the different device location options.
        private readonly string[] _navigationTypes =
        {
            "Off",
            "Re-Center",
            "Navigation",
            "Compass"
        };

        public DisplayDeviceLocation()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Add event handler for when this sample is unloaded.
            Unloaded += SampleUnloaded;

            // Assign the map to the MapView.
            MyMapView.Map = new Map(Basemap.CreateTopographic());

            // Populate the list of options and select a default.
            LocationModes.ItemsSource = _navigationTypes;
            LocationModes.SelectedIndex = 0;
        }

        private void OnStopClicked(object sender, RoutedEventArgs e)
        {
            // Disable location display.
            MyMapView.LocationDisplay.IsEnabled = false;
        }

        private void OnStartClicked(object sender, RoutedEventArgs e)
        {
            // Enable location display.
            MyMapView.LocationDisplay.IsEnabled = true;
        }

        private void LocationModes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set the autopan mode.
            switch (LocationModes.SelectedValue.ToString())
            {
                case "Off":
                    // Starts location display with auto pan mode set to Off.
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
                    break;
                case "Re-Center":
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    break;
                case "Navigation":
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                    break;
                case "Compass":
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
                    break;
            }
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}