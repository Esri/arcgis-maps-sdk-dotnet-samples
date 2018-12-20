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
        // Hold a reference to the MapView.
        private MapView _myMapView;

        public DisplayDeviceLocation()
        {
            Title = "Display Device Location";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
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

        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            
            View.AddSubviews(_myMapView, toolbar);

            toolbar.Items = new[]
            {
                new UIBarButtonItem("Start", UIBarButtonItemStyle.Plain, OnStartButtonClicked),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Stop", UIBarButtonItemStyle.Plain, OnStopButtonClicked)
            };

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor).Active = true;

            toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }
    }
}