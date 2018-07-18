// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.DisplayDeviceLocation
{
    [Register("DisplayDeviceLocation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display Device Location",
        "Location",
        "This sample demonstrates how you can enable location services and switch between different types of auto pan modes.",
        "")]
    public class DisplayDeviceLocation : UIViewController
    {
        // Create and hold a reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        public DisplayDeviceLocation()
        {
            Title = "Display Device Location";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization.
            Initialize();
            CreateLayout();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.ToolbarHidden = true;
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Show an imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());
        }

        private void OnStopButtonClicked(object sender, EventArgs e)
        {
            _myMapView.LocationDisplay.IsEnabled = false;
        }

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            try
            {
                UIAlertController actionAlert = UIAlertController.Create("Select device location option", "", UIAlertControllerStyle.Alert);

                // Add actions to alert. Selecting an option displays different option for auto pan modes.
                actionAlert.AddAction(UIAlertAction.Create("On", UIAlertActionStyle.Default, action =>
                {
                    // Starts location display with auto pan mode set to Off.
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
                    _myMapView.LocationDisplay.IsEnabled = true;
                }));
                actionAlert.AddAction(UIAlertAction.Create("Re-center", UIAlertActionStyle.Default, action =>
                {
                    // Starts location display with auto pan mode set to Default.
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                    _myMapView.LocationDisplay.IsEnabled = true;
                }));
                actionAlert.AddAction(UIAlertAction.Create("Navigation", UIAlertActionStyle.Default, action =>
                {
                    // Starts location display with auto pan mode set to Navigation.
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                    _myMapView.LocationDisplay.IsEnabled = true;
                }));
                actionAlert.AddAction(UIAlertAction.Create("Compass", UIAlertActionStyle.Default, action =>
                {
                    // Starts location display with auto pan mode set to Compass Navigation.
                    _myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation;
                    _myMapView.LocationDisplay.IsEnabled = true;
                }));

                // Show alert.
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
            // Create a button to start the location.
            UIBarButtonItem startButton = new UIBarButtonItem {Title = "Start", Style = UIBarButtonItemStyle.Plain};
            startButton.Clicked += OnStartButtonClicked;

            // Create a button to apply new renderer.
            UIBarButtonItem stopButton = new UIBarButtonItem {Title = "Stop", Style = UIBarButtonItemStyle.Plain};
            stopButton.Clicked += OnStopButtonClicked;

            // Add the buttons to the toolbar.
            SetToolbarItems(new[] {startButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null), stopButton}, false);

            // Show the toolbar.
            NavigationController.ToolbarHidden = false;

            // Add MapView to the page.
            View.AddSubviews(_myMapView);
        }
    }
}