// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using UIKit;
using Foundation;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;

namespace ArcGISRuntimeXamarin.Samples.DisplayDeviceLocation
{    
    [Register("DisplayDeviceLocation")]
    public class DisplayDeviceLocation : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public DisplayDeviceLocation()
        {
            Title = "Display Device Location";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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
            try
            {
                UIAlertController actionAlert = UIAlertController.Create(
                    "Select device location option", "", UIAlertControllerStyle.Alert);

                    // Add actions to alert. Selecting an option displays different option for auto pan modes.
                    actionAlert.AddAction(UIAlertAction.Create("On", UIAlertActionStyle.Default, (action) =>
                    {
                        // Starts location display with auto pan mode set to Off
                        _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
                        
                        //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                        if (!_myMapView.LocationDisplay.IsStarted)
                            _myMapView.LocationDisplay.Start();
                    }));
                    actionAlert.AddAction(UIAlertAction.Create("Re-center", UIAlertActionStyle.Default, (action) =>
                    {
                        // Starts location display with auto pan mode set to Default
                        _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;

                        //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                        if (!_myMapView.LocationDisplay.IsStarted)
                            _myMapView.LocationDisplay.Start();
                    }));
                    actionAlert.AddAction(UIAlertAction.Create("Navigation", UIAlertActionStyle.Default, (action) =>
                    {
                        // Starts location display with auto pan mode set to Navigation
                        _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

                        //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                        if (!_myMapView.LocationDisplay.IsStarted)
                            _myMapView.LocationDisplay.Start();
                    }));
                    actionAlert.AddAction(UIAlertAction.Create("Compass", UIAlertActionStyle.Default, (action) =>
                    {
                        // Starts location display with auto pan mode set to Compass Navigation
                        _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;

                        //TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                        if (!_myMapView.LocationDisplay.IsStarted)
                            _myMapView.LocationDisplay.Start();
                    }));
                //present alert
                PresentViewController(actionAlert, true, null);                                    
            }
            catch (Exception ex)
            {
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                PresentViewController(alert, true, null);
            }
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView = new MapView()
            {
                Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height)
            };

            // Create a button to start the location
            var startButton = new UIBarButtonItem() { Title = "Start", Style = UIBarButtonItemStyle.Plain };
            startButton.Clicked += OnStartButtonClicked;

            // Create a button to apply new renderer
            var stopButton = new UIBarButtonItem() { Title = "Stop", Style = UIBarButtonItemStyle.Plain };
            stopButton.Clicked += OnStopButtonClicked; ;

            // Add the buttons to the toolbar
            SetToolbarItems(new UIBarButtonItem[] {startButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null),
                stopButton}, false);

            // Show the toolbar
            NavigationController.ToolbarHidden = false;

            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }
    }
}