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
using System.Collections.Generic;
using System.Windows;

namespace ArcGIS.WPF.Samples.DisplayDeviceLocation
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display device location with autopan modes",
        category: "Location",
        description: "Display your current position on the map, as well as switch between different types of auto pan Modes.",
        instructions: "Select an autopan mode, then use the buttons to start and stop location display.",
        tags: new[] { "GPS", "compass", "location", "map", "mobile", "navigation" })]
    public partial class DisplayDeviceLocation
    {
        // Dictionary to store the different autopan modes.
        private readonly Dictionary<string, LocationDisplayAutoPanMode> _autoPanModes = new()
        {
            { "Autopan Off", LocationDisplayAutoPanMode.Off },
            { "Re-Center", LocationDisplayAutoPanMode.Recenter },
            { "Navigation", LocationDisplayAutoPanMode.Navigation },
            { "Compass", LocationDisplayAutoPanMode.CompassNavigation }
        };

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
            Map myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Set navigation types as items source and set default value
            AutopanModeComboBox.ItemsSource = _autoPanModes.Keys;
        }

        private void AutopanModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Change the autopan mode based on the new selection.
            MyMapView.LocationDisplay.AutoPanMode = _autoPanModes[AutopanModeComboBox.SelectedItem.ToString()];
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
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