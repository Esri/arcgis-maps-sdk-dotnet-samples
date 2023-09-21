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
using System.Collections.Generic;
using System.Windows;

namespace ArcGIS.WPF.Samples.DisplayDeviceLocation
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
        private readonly Dictionary<string, LocationDisplayAutoPanMode> _autoPanModes =
            new Dictionary<string, LocationDisplayAutoPanMode>
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

            // Create new Map with basemap.
            var myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Provide used Map to the MapView.
            MyMapView.Map = myMap;

            // Set navigation types as items source and set default value.
            AutoPanModeComboBox.ItemsSource = _autoPanModes.Keys;

            // Show in the UI that LocationDisplay.AutoPanMode is off by default.
            AutoPanModeComboBox.SelectedItem = "AutoPan Off";

            // Update the UI when the user pans the view, changing the location mode.
            MyMapView.LocationDisplay.AutoPanModeChanged += (sender, args) =>
            {
                if (MyMapView.LocationDisplay.AutoPanMode == LocationDisplayAutoPanMode.Off)
                {
                    AutoPanModeComboBox.SelectedItem = "AutoPan Off";
                }
            };
        }

        private void AutoPanModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Change the auto pan mode based on the new selection.
            MyMapView.LocationDisplay.AutoPanMode = _autoPanModes[AutoPanModeComboBox.SelectedItem.ToString()];
        }

        private async void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            // Try to start or stop the location display data source.
            try
            {
                if (MyMapView.LocationDisplay.IsEnabled)
                {
                    await MyMapView.LocationDisplay.DataSource.StopAsync();
                }
                else
                {
                    await MyMapView.LocationDisplay.DataSource.StartAsync();
                }
            }
            // An exception will be thrown on if location is turned off on your Windows device.
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                // Flip the button text if the LocationDisplay.IsEnabled property was changed.
                // Button text won't change if start button was pressed but location access wasn't authorized.
                StartStopButton.Content = MyMapView.LocationDisplay.IsEnabled ? "Stop" : "Start";
            }
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}