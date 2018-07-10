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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.OpenMapURL
{
    [Register("OpenMapURL")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open map (URL)",
        "Map",
        "This sample demonstrates how to open an existing map from a portal. The sample opens with a map displayed by default. You can change the shown map by selecting a new one from the populated list.",
        "")]
    public class OpenMapURL : UIViewController
    {
        // Create and hold references to the controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UIButton _mapsButton = new UIButton();

        // String array to hold URLs to publicly available web maps.
        private readonly string[] _itemUrLs =
        {
            "https://www.arcgis.com/home/item.html?id=392451c381ad4109bf04f7bd442bc038",
            "https://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3",
            "https://www.arcgis.com/home/item.html?id=92ad152b9da94dee89b9e387dfe21acd"
        };

        // String array to store titles for the webmaps specified above. These titles are in the same order as the URLs above.
        private readonly string[] _titles =
        {
            "Population Pressure",
            "USA Tapestry Segmentation",
            "Geology of the United States"
        };

        public OpenMapURL()
        {
            Title = "Open map (URL)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _mapsButton.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Show the first webmap by default.
            _myMapView.Map = new Map(new Uri(_itemUrLs[0]));
        }

        private void OnMapsButtonTouch(object sender, EventArgs e)
        {
            // Initialize an UIAlertController with a title and style of an alert.
            UIAlertController actionSheetAlert = UIAlertController.Create("Select a map to open", "", UIAlertControllerStyle.Alert);

            // Add actions to alert. Selecting an option re-initializes the Map
            // with selected webmap URL and assigns it to MapView.
            actionSheetAlert.AddAction(UIAlertAction.Create(_titles[0], UIAlertActionStyle.Default,
                action => _myMapView.Map = new Map(new Uri(_itemUrLs[0]))));
            actionSheetAlert.AddAction(UIAlertAction.Create(_titles[1], UIAlertActionStyle.Default,
                action => _myMapView.Map = new Map(new Uri(_itemUrLs[1]))));
            actionSheetAlert.AddAction(UIAlertAction.Create(_titles[2], UIAlertActionStyle.Default,
                action => _myMapView.Map = new Map(new Uri(_itemUrLs[2]))));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void CreateLayout()
        {
            // Create button to show map options.
            _mapsButton.SetTitle("Select a map", UIControlState.Normal);
            _mapsButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _mapsButton.TouchUpInside += OnMapsButtonTouch;

            // Add MapView to the page.
            View.AddSubviews(_myMapView, _toolbar, _mapsButton);
        }
    }
}