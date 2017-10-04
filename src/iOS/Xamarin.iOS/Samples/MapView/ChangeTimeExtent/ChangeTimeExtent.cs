// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChangeTimeExtent
{
    [Register("ChangeTimeExtent")]
    public class ChangeTimeExtent : UIViewController
    {
        // Create and hold buttons for changing the time extent
        private UIButton _intervalButton1 = new UIButton();
        private UIButton _intervalButton2 = new UIButton();

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public ChangeTimeExtent()
        {
            Title = "Change time extent";
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Hold height of buttons
            int height = 60;

            // Setup the visual frame for the buttons
            _intervalButton1.Frame = new CoreGraphics.CGRect(0, NavigationController.TopLayoutGuide.Length + height, View.Bounds.Width / 2, height);
            _intervalButton2.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2, NavigationController.TopLayoutGuide.Length + height, View.Bounds.Width / 2, height);

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Create the buttons
            _intervalButton1.SetTitle("Interval 1", UIControlState.Normal);
            _intervalButton2.SetTitle("Interval 2", UIControlState.Normal);

            // Set a more visible color
            _intervalButton1.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _intervalButton2.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Add MapView and buttons to the page
            View.AddSubviews(_myMapView, _intervalButton1, _intervalButton2);

            // Add event handlers for button clicked events
            _intervalButton1.TouchUpInside += _intervalButton1_Clicked;
            _intervalButton2.TouchUpInside += _intervalButton2_Clicked;
        }

        private void _intervalButton1_Clicked(object sender, EventArgs e)
        {
        }

        private void _intervalButton2_Clicked(object sender, EventArgs e)
        {
        }
    }
}