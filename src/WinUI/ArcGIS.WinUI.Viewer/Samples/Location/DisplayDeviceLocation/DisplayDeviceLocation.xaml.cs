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
using System.Collections.Generic;

namespace ArcGIS.WinUI.Samples.DisplayDeviceLocation
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display device location with autopan modes",
        category: "Location",
        description: "Display your current position on the map, as well as switch between different types of auto pan modes.",
        instructions: "Select an autopan mode, then use the button to start and stop location display.",
        tags: new[] { "GPS", "compass", "location", "map", "mobile", "navigation" })]
    public partial class DisplayDeviceLocation
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
            // Add event handler for when this sample is unloaded.
            Unloaded += SampleUnloaded;

            // Assign the map to the MapView.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);

            // Populate the combo box with auto pan modes.
            AutoPanModeComboBox.ItemsSource = _autoPanModes.Keys;

            // Update the UI when the user pans the view, changing the location mode.
            MyMapView.LocationDisplay.AutoPanModeChanged += (sender, args) =>
            {
                if (MyMapView.LocationDisplay.AutoPanMode == LocationDisplayAutoPanMode.Off)
                {
                    AutoPanModeComboBox.SelectedItem = "AutoPan Off";
                }
            };
        }

        private void AutoPanModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Change the auto pan mode based on the new selection.
            MyMapView.LocationDisplay.AutoPanMode = _autoPanModes[AutoPanModeComboBox.SelectedItem.ToString()];
        }

        private void StartStopButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Enable or disable the location display.
            MyMapView.LocationDisplay.IsEnabled = !MyMapView.LocationDisplay.IsEnabled;

            StartStopButton.Content = MyMapView.LocationDisplay.IsEnabled ? "Stop" : "Start";
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}