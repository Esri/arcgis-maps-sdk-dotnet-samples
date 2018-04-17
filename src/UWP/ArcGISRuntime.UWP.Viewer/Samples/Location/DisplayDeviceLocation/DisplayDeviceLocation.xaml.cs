// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.DisplayDeviceLocation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display device location",
        "Location",
        "This sample demonstrates how you can enable location services and switch between different types of auto pan modes.",
        "")]
    public partial class DisplayDeviceLocation
    {
        // String array to store the different device location options.
        private string[] _navigationTypes = new string[]
        {
            "On",
            "Re-Center",
            "Navigation",
            "Compass"
        };
        
        public DisplayDeviceLocation()
        {
            InitializeComponent();

            // Setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Set titles as a items source
            locationModes.ItemsSource = _navigationTypes;
        }

        private void OnStopClicked(object sender, RoutedEventArgs e)
        {
            //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
            if (MyMapView.LocationDisplay.IsEnabled)
                MyMapView.LocationDisplay.IsEnabled = false;

            locationModes.SelectedItem = null;
        }

        private void OnLocationModesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selection cleared, don't do anything.
            if (!e.AddedItems.Any()) return;

            var selectedBasemap = e.AddedItems[0].ToString();
            
            // Get index that is used to get the selected url
            var selectedIndex = _navigationTypes.ToList().IndexOf(locationModes.SelectedValue.ToString());

            switch (selectedIndex)
            {
                case 0:
                    // Starts location display with auto pan mode set to Off
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;

                    //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    if (!MyMapView.LocationDisplay.IsEnabled)
                        MyMapView.LocationDisplay.IsEnabled = true;
                    break;

                case 1:
                    // Starts location display with auto pan mode set to Re-center
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;

                    //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    if (!MyMapView.LocationDisplay.IsEnabled)
                        MyMapView.LocationDisplay.IsEnabled = true;
                    break;

                case 2:
                    // Starts location display with auto pan mode set to Navigation
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

                    //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    if (!MyMapView.LocationDisplay.IsEnabled)
                        MyMapView.LocationDisplay.IsEnabled = true;
                    break;

                case 3:
                    // Starts location display with auto pan mode set to Compass Navigation
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;

                    //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    if (!MyMapView.LocationDisplay.IsEnabled)
                        MyMapView.LocationDisplay.IsEnabled = true;
                    break;
            }
        }
    }
}
