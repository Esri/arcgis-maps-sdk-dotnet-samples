// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ShowCallout
{
    [Register("ShowCallout")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show callout",
        "MapView",
        "This sample illustrates how to show callouts on a map in response to user interaction.",
        "Tap on the map to show that point's coordinates.")]
    public class ShowCallout : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();

        private readonly UILabel _helpLabel = new UILabel
        {
            Text = "Tap to show a callout.",
            AdjustsFontSizeToFitWidth = true,
            TextAlignment = UITextAlignment.Center,
            Lines = 1
        };

        public ShowCallout()
        {
            Title = "Show callout";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Show a streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Respond to taps on the map.
            _myMapView.GeoViewTapped += MapView_GeoViewTapped;

            // Add the controls to the view.
            View.AddSubviews(_myMapView, _toolbar);

            // Add the help label to the toolbar.
            _toolbar.AddSubview(_helpLabel);
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = 40;
                nfloat margin = 5;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin + toolbarHeight, 0, 0, 0);
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, toolbarHeight);

                // Reposition the label within the toolbar.
                _helpLabel.Frame = new CGRect(margin, margin, _toolbar.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the user-tapped location.
            MapPoint mapLocation = e.Location;

            // Project the map point to WGS84 for latitude/longitude display.
            MapPoint projectedLocation = (MapPoint) GeometryEngine.Project(mapLocation, SpatialReferences.Wgs84);

            // Format the display callout string based upon the projected map point (example: "Lat: 100.123, Long: 100.234").
            string mapLocationDescription = $"Lat: {projectedLocation.Y:F3} Long:{projectedLocation.X:F3}";

            // Create a new callout definition using the formatted string.
            CalloutDefinition myCalloutDefinition = new CalloutDefinition("Location:", mapLocationDescription);

            // Display the callout.
            _myMapView.ShowCalloutAt(mapLocation, myCalloutDefinition);
        }
    }
}