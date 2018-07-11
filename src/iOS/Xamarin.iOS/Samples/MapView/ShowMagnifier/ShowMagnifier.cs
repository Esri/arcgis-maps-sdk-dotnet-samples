// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ShowMagnifier
{
    [Register("ShowMagnifier")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show magnifier",
        "MapView",
        "This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.",
        "")]
    public class ShowMagnifier : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();

        private readonly UILabel _helpLabel = new UILabel
        {
            Text = "Tap and hold to show the magnifier.",
            AdjustsFontSizeToFitWidth = true,
            TextAlignment = UITextAlignment.Center,
            Lines = 1
        };

        public ShowMagnifier()
        {
            Title = "Show magnifier";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
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
                _helpLabel.Frame = new CGRect(margin, margin, _toolbar.Bounds.Width - (2 * margin), controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location.
            Map myMap = new Map(BasemapType.Topographic, 34.056295, -117.195800, 10);

            // Enable magnifier.
            _myMapView.InteractionOptions = new MapViewInteractionOptions
            {
                IsMagnifierEnabled = true
            };

            // Show the map in the view.
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Add the controls to the view.
            View.AddSubviews(_myMapView, _toolbar);

            // Add the help label to the toolbar.
            _toolbar.AddSubview(_helpLabel);
        }
    }
}