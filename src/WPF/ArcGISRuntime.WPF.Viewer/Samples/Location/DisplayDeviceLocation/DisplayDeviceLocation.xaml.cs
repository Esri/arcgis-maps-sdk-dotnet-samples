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
using System;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.DisplayDeviceLocation
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
        private string[] _navigationTypes = Enum.GetNames(typeof(LocationDisplayAutoPanMode));

        public DisplayDeviceLocation()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Add event handler for when this sample is unloaded.
            Unloaded += SampleUnloaded;

            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Set navigation types as items source and set default value
            ModeChooser.ItemsSource = _navigationTypes;
            ModeChooser.SelectedIndex = 0;

            // Update the UI when the user pans the view, changing the location mode
            MyMapView.LocationDisplay.AutoPanModeChanged += (sender, args) =>
            {
                switch (MyMapView.LocationDisplay.AutoPanMode)
                {
                    case LocationDisplayAutoPanMode.Off:
                        ModeChooser.SelectedIndex = 0;
                        break;
                    case LocationDisplayAutoPanMode.Recenter:
                        ModeChooser.SelectedIndex = 1;
                        break;
                    case LocationDisplayAutoPanMode.Navigation:
                        ModeChooser.SelectedIndex = 2;
                        break;
                    case LocationDisplayAutoPanMode.CompassNavigation:
                        ModeChooser.SelectedIndex = 3;
                        break;
                }
            };
        }

        private void OnStartButtonClicked(object sender, RoutedEventArgs e)
        {
            MyMapView.LocationDisplay.IsEnabled = true;
        }
        private void OnStopButtonClicked(object sender, RoutedEventArgs e)
        {
            MyMapView.LocationDisplay.IsEnabled = false;
        }

        private void OnModeChooserSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get index that is used to get the selected URL
            int selectedIndex = ModeChooser.SelectedIndex;

            switch (selectedIndex)
            {
                case 0:
                    // Starts location display with auto pan mode set to Off
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
                    MyMapView.LocationDisplay.IsEnabled = true;
                    break;

                case 1:
                    // Starts location display with auto pan mode set to Re-center
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    MyMapView.LocationDisplay.IsEnabled = true;
                    break;

                case 2:
                    // Starts location display with auto pan mode set to Navigation
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                    MyMapView.LocationDisplay.IsEnabled = true;
                    break;

                case 3:
                    // Starts location display with auto pan mode set to Compass Navigation
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
                    MyMapView.LocationDisplay.IsEnabled = true;
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
