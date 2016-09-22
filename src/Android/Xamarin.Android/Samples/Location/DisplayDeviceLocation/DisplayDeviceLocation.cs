// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.DisplayDeviceLocation
{
    [Activity]
    public class DisplayDeviceLocation : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // String array to store the different device location options.
        private string[] _navigationTypes = new string[]
        {
            "On",
            "Re-Center",
            "Navigation",
            "Compass" 
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display device location";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnStopButtonClicked(object sender, EventArgs e)
        {
            //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
            if (_myMapView.LocationDisplay.IsStarted)
                _myMapView.LocationDisplay.Stop();
        }

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            var startButton = sender as Button;

            // Create menu to show navigation options
            var navigationMenu = new PopupMenu(this, startButton);
            navigationMenu.MenuItemClick += OnNavigationMenuItemClicked;

            // Create menu options
            foreach (var navigationType in _navigationTypes)
                navigationMenu.Menu.Add(navigationType);

            // Show menu in the view
            navigationMenu.Show();
        }

        private void OnNavigationMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get title from the selected item
            var selectedNavigationType = e.Item.TitleCondensedFormatted.ToString();

            // Get index that is used to get the selected url
            var selectedIndex = _navigationTypes.ToList().IndexOf(selectedNavigationType);

            switch (selectedIndex)
            {
                case 0:
                    // Starts location display with auto pan mode set to Off
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;

                    //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    if (!_myMapView.LocationDisplay.IsStarted)
                        _myMapView.LocationDisplay.Start();
                    break;

                case 1:
                    // Starts location display with auto pan mode set to Re-center
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;

                    //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    if (!_myMapView.LocationDisplay.IsStarted)
                        _myMapView.LocationDisplay.Start();
                    break;

                case 2:
                    // Starts location display with auto pan mode set to Navigation
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

                    //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    if (!_myMapView.LocationDisplay.IsStarted)
                        _myMapView.LocationDisplay.Start();
                    break;

                case 3:
                    // Starts location display with auto pan mode set to Compass Navigation
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;

                    //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    if (!_myMapView.LocationDisplay.IsStarted)
                        _myMapView.LocationDisplay.Start();
                    break;
            }
        }

        private void CreateLayout()
        {

            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible navigation options
            var startButton = new Button(this);
            startButton.Text = "Start";
            startButton.Click += OnStartButtonClicked;

            // Create button to stop navigation
            var stopButton = new Button(this);
            stopButton.Text = "Stop";
            stopButton.Click += OnStopButtonClicked;

            // Add start button to the layout
            layout.AddView(startButton);

            // Add stop button to the layout
            layout.AddView(stopButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);          
        }
    }
}