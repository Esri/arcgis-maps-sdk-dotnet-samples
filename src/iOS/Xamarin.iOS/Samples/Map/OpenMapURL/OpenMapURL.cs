// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
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
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();
        private UIButton _mapsButton;

        // String array to hold urls to publicly available web maps
        private string[] itemURLs = new string[] 
        {
            "https://www.arcgis.com/home/item.html?id=2d6fa24b357d427f9c737774e7b0f977",
            "https://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3",
            "https://www.arcgis.com/home/item.html?id=92ad152b9da94dee89b9e387dfe21acd"
        };

        // String array to store titles for the webmaps specified above. These titles are in the same order as the urls above
        private string[] titles = new string[]
        {
            "Housing with Mortgages",
            "USA Tapestry Segmentation",
            "Geology of United States"
        };

        public OpenMapURL()
        {
            Title = "Open map (URL)";
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();
			CreateLayout();
			Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Create the UI, setup the control references and execute initialization 
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _mapsButton.Frame = new CoreGraphics.CGRect(
                0, View.Bounds.Height - 40, View.Bounds.Width, 40);
        }

        private void Initialize()
        {
            // Create a new Map instance with url of the webmap that is displayed by default
            Map myMap = new Map(new Uri(itemURLs[0]));

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnMapsButtonTouch(object sender, EventArgs e)
        {
            // Initialize an UIAlertController with a title and style of an alert
            UIAlertController actionSheetAlert = UIAlertController.Create(
                "Select a map to open", "", UIAlertControllerStyle.Alert);

            // Add actions to alert. Selecting an option re-initializes the Map 
            // with selected webmap url and assigns it to MapView.
            actionSheetAlert.AddAction(UIAlertAction.Create(titles[0], UIAlertActionStyle.Default, (action) =>
            {
                _myMapView.Map = new Map(new Uri(itemURLs[0]));
            }));
            actionSheetAlert.AddAction(UIAlertAction.Create(titles[1], UIAlertActionStyle.Default, (action) =>
            {
                _myMapView.Map = new Map(new Uri(itemURLs[1]));
            }));
            actionSheetAlert.AddAction(UIAlertAction.Create(titles[2], UIAlertActionStyle.Default, (action) =>
            {
                _myMapView.Map = new Map(new Uri(itemURLs[2]));
            }));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Add a button at the bottom to show webmap choices
            _mapsButton = new UIButton(UIButtonType.Custom)
            {
                Frame = new CoreGraphics.CGRect(
                    0, View.Bounds.Height - 40, View.Bounds.Width, 40),
                BackgroundColor = UIColor.White
            };

            // Create button to show map options
            _mapsButton.SetTitle("Maps", UIControlState.Normal);
            _mapsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _mapsButton.TouchUpInside += OnMapsButtonTouch;

            // Add MapView to the page
            View.AddSubviews(_myMapView, _mapsButton);
        }
    }
}